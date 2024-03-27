using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Common.Api.Inventory;
using Beamable.Microservices.ThetaFederation.Extensions;
using Beamable.Microservices.ThetaFederation.Features.Transactions.Exceptions;
using Beamable.Microservices.ThetaFederation.Features.Transactions.Storage;
using Beamable.Microservices.ThetaFederation.Features.Transactions.Storage.Models;
using Beamable.Server;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace Beamable.Microservices.ThetaFederation.Features.Transactions
{
    public class TransactionManager
    {
        private readonly InventoryTransactionCollection _inventoryTransactionCollection;
        private readonly TransactionLogCollection _transactionLogCollection;
        private readonly RequestContext _requestContext;

        private static int _inflightTransactions;
        private static bool _serviceInitialized;

        private static readonly AsyncLocal<ObjectId?> CurrentTransaction = new();

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Ignore
        };

        public static void SetupShutdownHook()
        {
            BeamableLogger.Log("Registering transaction shutdown hook");
            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                BeamableLogger.Log("Waiting for inflight transactions");
                var inflightTransactions = _inflightTransactions;

                while (inflightTransactions > 0)
                {
                    BeamableLogger.Log("{inflightTransactions} inflight transactions, waiting for 500ms",
                        inflightTransactions);
                    Thread.Sleep(500);
                    inflightTransactions = _inflightTransactions;
                }

                BeamableLogger.Log("Done waiting for inflight transactions");
            };
        }

        public TransactionManager(InventoryTransactionCollection inventoryTransactionCollection,
            TransactionLogCollection transactionLogCollection, RequestContext requestContext)
        {
            _inventoryTransactionCollection = inventoryTransactionCollection;
            _transactionLogCollection = transactionLogCollection;
            _requestContext = requestContext;
        }

        public void SetCurrentTransactionContext(ObjectId transactionId)
        {
            CurrentTransaction.Value = transactionId;
        }

        public async Task<ObjectId> StartTransaction<TRequest>(string walletAddress, string? inventoryTransaction,
            string operationName, TRequest request)
        {
            return await StartTransaction(walletAddress, inventoryTransaction, operationName, request,
                _requestContext.UserId, _requestContext.Path);
        }

        private async Task<ObjectId> StartTransaction<TRequest>(string walletAddress, string? inventoryTransaction,
            string operationName, TRequest request, long requesterUserId, string path)
        {
            if (inventoryTransaction is not null)
            {
                var isSuccess =
                    await _inventoryTransactionCollection.TryInsertInventoryTransaction(inventoryTransaction);
                if (!isSuccess)
                    throw new TransactionException(
                        $"Inventory transaction {inventoryTransaction} already processed or in-progress");
            }

            var transactionId = ObjectId.GenerateNewId();
            await _transactionLogCollection.Insert(new TransactionLog
            {
                Id = transactionId,
                InventoryTransactionId = inventoryTransaction,
                RequesterUserId = requesterUserId,
                Wallet = walletAddress,
                Request = request as string ?? JsonConvert.SerializeObject(request, JsonSerializerSettings),
                Path = path,
                OperationName = operationName
            });

            Interlocked.Increment(ref _inflightTransactions);

            return transactionId;
        }

        public async Task<ObjectId> StartTransaction(string walletAddress, string operationName,
            string inventoryTransaction, Dictionary<string, long> currencies, List<FederatedItemCreateRequest> newItems,
            List<FederatedItemDeleteRequest> deleteItems, List<FederatedItemUpdateRequest> updateItems)
        {
            return await StartTransaction(walletAddress, inventoryTransaction, operationName, new
            {
                currencies,
                newItems,
                deleteItems,
                updateItems
            });
        }

        public async Task AddChainTransaction(ChainTransaction chainTransaction)
        {
            if (CurrentTransaction.Value.HasValue)
                await _transactionLogCollection.AddChainTransaction(CurrentTransaction.Value!.Value, chainTransaction);
        }

        public ObjectId? GetCurrentTransactionId() => CurrentTransaction.Value;

        public Task<TransactionLog?> GetLastByWalletAndOperation(string wallet, string operationName)
        {
            return _transactionLogCollection.GetLastByWalletAndOperation(wallet, operationName);
        }

        public async Task TransactionError(ObjectId transactionId, string? inventoryTransactionId, Exception ex)
        {
            Interlocked.Decrement(ref _inflightTransactions);

            if (inventoryTransactionId is not null)
            {
                BeamableLogger.Log("Clearing the inventory transaction {transactionId} record to enable retries.",
                    inventoryTransactionId);
                await _inventoryTransactionCollection.DeleteInventoryTransaction(inventoryTransactionId);
            }

            await _transactionLogCollection.SetError(transactionId, ex.Message);
        }

        private async Task TransactionDone(ObjectId transactionId)
        {
            Interlocked.Decrement(ref _inflightTransactions);
            await _transactionLogCollection.SetDone(transactionId);
        }

        public async Task RunAsyncBlock(ObjectId transactionId, string? inventoryTransactionId, Func<Task> block)
        {
            try
            {
                await block();
                await TransactionDone(transactionId);
            }
            catch (Exception ex)
            {
                BeamableLogger.LogError("Error processing transaction {transaction}. Error: {e}", transactionId,
                    ex.ToLogFormat());
                await TransactionError(transactionId, inventoryTransactionId, ex);
                throw;
            }
        }

        public static bool IsServiceInitialized() => _serviceInitialized;

        public static void SetServiceInitialized()
        {
            _serviceInitialized = true;
        }
    }
}