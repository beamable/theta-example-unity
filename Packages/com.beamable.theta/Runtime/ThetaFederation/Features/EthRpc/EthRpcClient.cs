using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Numerics;
using System.Threading.Tasks;
using Assets.Beamable.Microservices.ThetaFederation.Features.EthRpc;
using Beamable.Common;
using Beamable.Microservices.ThetaFederation.Extensions;
using Beamable.Microservices.ThetaFederation.Features.Accounts;
using Beamable.Microservices.ThetaFederation.Features.EthRpc.Exceptions;
using Beamable.Microservices.ThetaFederation.Features.Transactions;
using Beamable.Microservices.ThetaFederation.Features.Transactions.Storage.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Account = Nethereum.Web3.Accounts.Account;

namespace Beamable.Microservices.ThetaFederation.Features.EthRpc
{
    public class EthRpcClient : IService
{
	private readonly AccountsService _accountsService;
	private readonly TransactionManager _transactionManager;
	private readonly ConcurrentDictionary<string, Web3> _web3Cache = new();
	private readonly AccountOfflineTransactionSigner _transactionSigner;
	private readonly MemoryCache _chainIdCache = new(Options.Create(new MemoryCacheOptions()));
	private readonly MemoryCache _gasPriceCache = new(Options.Create(new MemoryCacheOptions()));

	public EthRpcClient(AccountsService accountsService, TransactionManager transactionManager)
	{
		_accountsService = accountsService;
		_transactionManager = transactionManager;
		_transactionSigner = new AccountOfflineTransactionSigner();
	}

	private async Task<Web3> GetClient()
	{
		var realmAccount = await _accountsService.GetOrCreateRealmAccount();
		var endpoint = Configuration.RPCEndpoint;

		if (string.IsNullOrEmpty(endpoint))
			throw new ConfigurationException("Missing RPCEndpoint configuration");

		var cacheKey = $"{realmAccount.Address}.{endpoint}";
		return _web3Cache.GetOrAdd(cacheKey, key =>
			{
				BeamableLogger.Log("Creating new web3 client for {key}", key);
				var client = new Web3(realmAccount, endpoint)
				{
					TransactionManager =
					{
						UseLegacyAsDefault = true
					}
				};
				return client;
			}
		);
	}

	public async Task<HexBigInteger> GetTransactionCountAsync(string address)
	{
		using (new Measure("GetTransactionCountAsync"))
		{
			var web3Client = await GetClient();
			var count = await RunWithRetry(_ => web3Client.Eth.Transactions.GetTransactionCount.SendRequestAsync(address), "GetTransactionCount");
			BeamableLogger.Log("Address {a} has {N} transactions", address, count);
			return count;
		}
	}

	public async Task<HexBigInteger> EstimateContractGasAsync(Account realmAccount, string abi, string contractByteCode)
	{
		using (new Measure("EstimateContractGasAsync"))
		{
			var web3Client = await GetClient();
			var gas = await web3Client.Eth.DeployContract.EstimateGasAsync(abi, contractByteCode, realmAccount.Address);
			BeamableLogger.Log("Gas is {g}", gas);
			return gas;
		}
	}

	public async Task<TransactionReceipt> DeployContractAsync(Account realmAccount, string abi, string contractByteCode, HexBigInteger gas)
	{
		using (new Measure("DeployContractAsync"))
		{
			var web3Client = await GetClient();
			string transactionHash;
			try
			{
				transactionHash = await web3Client.Eth.DeployContract.SendRequestAsync(abi, contractByteCode, realmAccount.Address, gas);
			}
			catch (Exception ex)
			{
				BeamableLogger.LogWarning("Resetting nonce due to error: {error}", ex.Message);
				await web3Client.TransactionManager.Account.NonceService.ResetNonceAsync();
				throw;
			}

			BeamableLogger.Log("Transaction hash is {transactionHash}", transactionHash);
			var receipt = await FetchReceiptAsync(transactionHash);
			BeamableLogger.Log("Response: {@response}", receipt);
			if (!receipt.Succeeded()) throw new ContractDeployException("Contract deployment failed. Check microservice logs.");
			return receipt;
		}
	}

	public async Task<string> SendTransactionAsync<TContractMessage>(string contractAddress, TContractMessage functionMessage, bool waitForReceipt = false) where TContractMessage : FunctionMessage, new()
	{
		var maxRetries = Configuration.TransactionRetries;
		var retryCount = 0;
		var initialDelayMs = 300;
		var maxNonceTooLow = 50;
		var nonceTooLowRetryCount = 0;

		var nonce = functionMessage.Nonce;

		while (true)
		{
			try
			{
				return await SendTransactionInternalAsync(nonce, contractAddress, functionMessage, waitForReceipt);
			}
			catch (RpcResponseException ex)
			{
				throw new ContractException(ex.Message);
			}
			catch (NonceTooLowException)
			{
				// If this is a retry of a dropped transaction and nonce is too low, transaction is already processed
				if (nonce is not null)
				{
					BeamableLogger.LogError("Error processing transaction with nonce {n}. The transaction is already processed.", nonce);
					return "0x0000000000000000000000000000000000000000";
				}

				// Ignore and retry without a delay and incrementing retryCount

				// Make sure we don't end up in an infinite loop
				nonceTooLowRetryCount++;
				if (nonceTooLowRetryCount >= maxNonceTooLow)
				{
					throw;
				}
			}
			catch (Exception ex)
			{
				retryCount++;
				if (retryCount >= maxRetries)
				{
					throw new SendTransactionException($"Maximum retry count exceeded for transaction {_transactionManager.GetCurrentTransactionId()}. ERROR: {ex.Message}");
				}

				var delayMs = (int)Math.Pow(2, retryCount - 1) * initialDelayMs;

				BeamableLogger.LogWarning("Retrying [{retry}] transaction {txId} in {N} milliseconds. Error: {error}", retryCount, _transactionManager.GetCurrentTransactionId(), delayMs, ex.ToLogFormat());

				await Task.Delay(delayMs);
			}
		}
	}

	private async Task<string> SendTransactionInternalAsync<TContractMessage>(BigInteger? nonce, string contractAddress, TContractMessage functionMessage, bool waitForReceipt = false) where TContractMessage : FunctionMessage, new()
	{
		using (new Measure(functionMessage))
		{
			var web3Client = await GetClient();

			var newTransaction = nonce is null;
			if (newTransaction)
			{
				nonce = await ((MongoNonceService)web3Client.Eth.TransactionManager.Account.NonceService).GetNextNonceAsync();
			}
			functionMessage.Nonce = nonce;

			string transactionHash;

			functionMessage.FromAddress = web3Client.Eth.TransactionManager.Account.Address;

			try
			{
				await SetTransactionGas(functionMessage, contractAddress, web3Client, newTransaction);

				var rawTransaction = functionMessage.CreateTransactionInput(contractAddress);

				var chainId = await GetChainId(web3Client);
				rawTransaction.ChainId = chainId;
				var signedTransaction = _transactionSigner.SignTransaction((Account)web3Client.Eth.TransactionManager.Account, rawTransaction, chainId);

				transactionHash = await RunWithRetry(_ => web3Client.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTransaction), "SendTransaction");

				await _transactionManager.AddChainTransaction(new ChainTransaction
				{
					TransactionInput = rawTransaction,
					FunctionMessage = functionMessage,
					Nonce = (ulong)nonce!.Value,
					Function = functionMessage.GetFunctionName(),
					TransactionHash = transactionHash
				});
			}
			catch (Exception ex)
			{
				BeamableLogger.LogError("Transaction with nonce {N} failed with error: {e}", nonce, ex.ToLogFormat());

				await _transactionManager.AddChainTransaction(new ChainTransaction
				{
					Function = functionMessage.GetFunctionName(),
					Nonce = (ulong)nonce!.Value,
					Error = ex.Message
				});

				if (ex.Message.StartsWith("nonce too low"))
				{
					throw new NonceTooLowException();
				}

				if (newTransaction)
				{
					// We want to reuse this nonce since the transaction failed
					await ((MongoNonceService)web3Client.Eth.TransactionManager.Account.NonceService).PushErrorAsync((long)nonce.Value);
				}

				throw;
			}

			BeamableLogger.Log("Transaction hash is {transactionHash}", transactionHash);

			if (waitForReceipt)
			{
				TransactionReceipt? receipt;
				try
				{
					receipt = await FetchReceiptAsync(transactionHash);
					if (!receipt.Succeeded())
					{
						BeamableLogger.Log("Transaction failed. Response: {@response}", receipt);
					}
				}
				catch (TaskCanceledException)
				{
					BeamableLogger.LogWarning("Timeout waiting for a receipt for transaction {tx}", transactionHash);
				}
				catch (OperationCanceledException)
				{
					BeamableLogger.LogWarning("Timeout waiting for a receipt for transaction {tx}", transactionHash);
				}
				catch (Exception ex)
				{
					BeamableLogger.LogError("Error fetching the receipt: {ex}", ex.ToLogFormat());
				}
			}

			return transactionHash;
		}
	}

	private async Task<HexBigInteger?> GetChainId(Web3 web3Client)
	{
		var chainId = await _chainIdCache
			.GetOrCreateAsync(Configuration.RPCEndpoint, async _ => await RunWithRetry(_ => web3Client.Eth.ChainId.SendRequestAsync(), "GetChainId"));
		return chainId;
	}

	private async Task SetTransactionGas<TContractMessage>(TContractMessage functionMessage, string contractAddress, Web3 web3Client, bool newTransaction) where TContractMessage : FunctionMessage, new()
	{
		functionMessage.Gas = null;
		functionMessage.MaxFeePerGas = null;
		functionMessage.MaxPriorityFeePerGas = null;
		functionMessage.TransactionType = null;

		// Execute gas estimation to force simulation and early failure
		var callInput = functionMessage.CreateCallInput(contractAddress);
		_ = await RunWithRetry(_ => web3Client.Eth.Transactions.EstimateGas.SendRequestAsync(callInput), "EstimateGas");

		try
		{
			functionMessage.Gas = Configuration.MaximumGas;

			var gasPrice = await _gasPriceCache.GetOrCreateAsync("gas", async entry =>
			{
				entry.SetAbsoluteExpiration(TimeSpan.FromSeconds(Configuration.GasPriceCacheTtlSec));
				return await RunWithRetry(_ => web3Client.Eth.GasPrice.SendRequestAsync(), "GetGasPrice");
			});

			functionMessage.GasPrice = gasPrice!.Value;
			var extraGasPercent = Configuration.GasExtraPercent;
			if (extraGasPercent > 0)
			{
				var extraGas = gasPrice.Value * extraGasPercent / 100;
				functionMessage.GasPrice += extraGas;
			}

			if (!newTransaction)
			{
				var extraGas = functionMessage.GasPrice!.Value * 30 / 100;
				BeamableLogger.LogError("Adding extra {extraGas} gas to the replacement transaction with nonce {n}", extraGas, functionMessage.Nonce);
				functionMessage.GasPrice += extraGas;
			}
		}
		catch (Exception ex)
		{
			BeamableLogger.LogError("Error calculating gas: {ex}", ex.ToLogFormat());
			throw new GasCalculationException(ex.Message);
		}
	}

	public async Task<TFunctionOutput> SendFunctionQueryAsync<TContractMessage, TFunctionOutput>(string contractAddress, TContractMessage functionMessage) where TContractMessage : FunctionMessage, new() where TFunctionOutput : IFunctionOutputDTO, new()
	{
		using (new Measure(functionMessage))
		{
			var web3Client = await GetClient();
			var handler = web3Client.Eth.GetContractQueryHandler<TContractMessage>();

			try
			{
				return await RunWithRetry(_ => handler.QueryDeserializingToObjectAsync<TFunctionOutput>(functionMessage, contractAddress), functionMessage.GetFunctionName());
			}
			catch (Exception ex)
			{
				throw new ContractException(ex.Message);
			}
		}
	}

	private async Task<TransactionReceipt> FetchReceiptAsync(string? transactionHash)
	{
		using (new Measure("FetchReceiptAsync"))
		{
			var web3Client = await GetClient();
			var receipt = await web3Client.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);

			var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(Configuration.ReceiptPoolTimeoutMs));
			while (receipt == null)
			{
				tokenSource.Token.ThrowIfCancellationRequested();
				await Task.Delay(Configuration.ReceiptPoolIntervalMs, tokenSource.Token);
				receipt = await RunWithRetry(_ => web3Client.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash), "GetTransactionReceipt");
			}

			return receipt;
		}
	}

	private async Task<T> RunWithRetry<T>(Func<int, Task<T>> func, string operationName)
	{
		var maxRetries = 8;
		var retryCount = 0;
		var initialDelayMs = 300;

		while (true)
		{
			try
			{
				return await func(retryCount);
			}
			catch (RpcClientUnknownException ex)
			{
				BeamableLogger.LogWarning("Rate limited: {op}", operationName);

				// Probably rate limited
				retryCount++;
				if (retryCount >= maxRetries)
				{
					throw new RpcCallException($"Maximum retry count exceeded for operation: {operationName}, Tx: {_transactionManager.GetCurrentTransactionId()}. ERROR: {ex.Message}");
				}

				var delayMs = (int)Math.Pow(2, retryCount - 1) * initialDelayMs;

				await Task.Delay(delayMs);
			}
		}
	}
}
}