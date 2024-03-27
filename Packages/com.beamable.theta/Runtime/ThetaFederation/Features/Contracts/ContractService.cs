using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Microservices.ThetaFederation.Features.Accounts;
using Beamable.Microservices.ThetaFederation.Features.Contracts.Exceptions;
using Beamable.Microservices.ThetaFederation.Features.Contracts.Storage.Models;
using Beamable.Microservices.ThetaFederation.Features.EthRpc;
using Beamable.Microservices.ThetaFederation.Features.SolcWrapper;
using Beamable.Microservices.ThetaFederation.Features.SolcWrapper.Models;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts
{
    public class ContractService : IService
    {
        private const string DefaultContractFile = "GameToken.sol";
        public const string DefaultContractName = "default";

        private readonly ContractProxy _contractProxy;
        private readonly InitializeNonceService _initializeNonceService;
        private readonly AccountsService _accountsService;
        private readonly EthRpcClient _ethRpcClient;

        private static readonly SemaphoreSlim Semaphore = new(1);

        public ContractService(ContractProxy contractProxy, InitializeNonceService initializeNonceService, AccountsService accountsService, EthRpcClient ethRpcClient)
        {
            _contractProxy = contractProxy;
            _initializeNonceService = initializeNonceService;
            _accountsService = accountsService;
            _ethRpcClient = ethRpcClient;
        }

        public async ValueTask<Contract> GetOrCreateDefaultContract()
        {
            await Semaphore.WaitAsync();
            try
            {
                return await GetOrCreateContract(DefaultContractName, DefaultContractFile);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        private async Task<Contract> GetOrCreateContract(string name, string contractFile)
        {

            var persistedContract = await _contractProxy.GetDefaultContractOrDefault();
            if (persistedContract is not null)
            {
                return persistedContract;
            }

            var realmAccount = await _accountsService.GetOrCreateRealmAccount();
            await _initializeNonceService.InitializeNonce(realmAccount.Address);

            var compilerOutput = await Compile(contractFile);
            var contractOutput = compilerOutput.Contracts.Contract.First().Value;
            var abi = contractOutput.GetAbi();
            var contractByteCode = contractOutput.GetBytecode();

            var gas = await _ethRpcClient.EstimateContractGasAsync(realmAccount, abi, contractByteCode);
            var result = await _ethRpcClient.DeployContractAsync(realmAccount, abi, contractByteCode, gas);

            var contract = new Contract
            {
                Name = name,
                PublicKey = result.ContractAddress
            };

            await _contractProxy.InitializeDefaultContract(contract.PublicKey);
            persistedContract = await _contractProxy.GetDefaultContractOrDefault();

            if (persistedContract is not null)
            {
                BeamableLogger.Log("Contract {contractName} created successfully. Address: {contractAddress}", name, contract.PublicKey);
                return contract;
            }

            BeamableLogger.LogWarning("Contract {contractName} already created, fetching again", name);
            return await GetOrCreateContract(name, contractFile);
        }

        private async Task<SolidityCompilerOutput> Compile(string contractFile)
        {
            var compilerInput = new SolidityCompilerInput(contractFile, new[] { "abi", "evm.bytecode" });

            var compilerOutput = await Solc.Compile(compilerInput);

            if (compilerOutput.HasErrors)
            {
                BeamableLogger.LogError("Compile errors: {@compileErrors}", compilerOutput.Errors.Select(x => x.Message).ToList());
                throw new ContractCompilationException(compilerOutput.Errors);
            }

            return compilerOutput;
        }
    }
}