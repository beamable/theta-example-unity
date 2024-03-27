using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Microservices.ThetaFederation.Features.Contracts.Exceptions;
using Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Metadata.Models;
using Beamable.Microservices.ThetaFederation.Features.Contracts.Models;
using Beamable.Microservices.ThetaFederation.Features.Contracts.Storage;
using Beamable.Microservices.ThetaFederation.Features.Contracts.Storage.Models;
using Beamable.Microservices.ThetaFederation.Features.EthRpc;
using Beamable.Microservices.ThetaFederation.Features.Minting;
using Newtonsoft.Json;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts
{
    public partial class ContractProxy : IService
    {
        private readonly ContractCollection _contractCollection;
        private readonly EthRpcClient _ethRpcClient;
        private readonly MetadataService _metadataService;
        private Contract? _cachedDefaultContract;

        public ContractProxy(MetadataService metadataService, ContractCollection contractCollection,
            EthRpcClient ethRpcClient)
        {
            _metadataService = metadataService;
            _contractCollection = contractCollection;
            _ethRpcClient = ethRpcClient;
        }

        public async ValueTask<Contract?> GetDefaultContractOrDefault()
        {
            if (_cachedDefaultContract is null)
            {
                var persistedContract = await _contractCollection.GetContract(ContractService.DefaultContractName);
                _cachedDefaultContract = persistedContract;
            }

            return _cachedDefaultContract;
        }

        public async ValueTask<Contract> GetDefaultContract()
        {
            var contract = await GetDefaultContractOrDefault();
            if (contract is null)
                throw new ContractNotInitializedException();
            return contract;
        }

        public async Task InitializeDefaultContract(string publicKey)
        {
            _cachedDefaultContract = null;

            var baseUri = await _metadataService.GetBaseUri();

            BeamableLogger.Log("Setting the base uri to {baseUri}", baseUri);
            await SetBaseUri(new SetBaseUriFunctionMessage
            {
                NewBaseUri = baseUri
            }, publicKey);

            var collectionUri = await UpdateCollectionMetadata(publicKey);

            await _contractCollection.SaveContract(new Contract
            {
                Name = ContractService.DefaultContractName,
                PublicKey = publicKey,
                BaseMetadataUri = baseUri,
                CollectionMetadataUri = collectionUri
            });
        }

        private async Task<string> UpdateCollectionMetadata(string publicKey)
        {
            var metadata = CollectionMetadata.Construct();
            var metadataJson = JsonConvert.SerializeObject(metadata);

            BeamableLogger.Log("Uploading collection metadata");
            var collectionUri = await _metadataService.StoreExternalMetadata(metadataJson);

            BeamableLogger.Log("Setting the contract URI to {metadataUri}", collectionUri);
            await SetContractUri(new SetContractUriFunctionMessage
            {
                NewUri = collectionUri
            }, publicKey);

            return collectionUri;
        }
    }
}