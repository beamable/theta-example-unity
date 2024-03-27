using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Common.Api.Inventory;
using Beamable.Microservices.ThetaFederation.Features.Contracts;
using Beamable.Microservices.ThetaFederation.Features.Inventory;
using Beamable.Microservices.ThetaFederation.Features.Minting;
using Beamable.Microservices.ThetaFederation.Features.Minting.Exceptions;
using Beamable.Microservices.ThetaFederation.Features.Minting.Storage;
using Beamable.Microservices.ThetaFederation.Features.Transactions;

namespace Beamable.Microservices.ThetaFederation.Endpoints
{
    public class StartInventoryTransactionEndpoint : IEndpoint
    {
        private readonly ContractProxy _contractProxy;
        private readonly MetadataService _metadataService;
        private readonly MintCollection _mintCollection;
        private readonly MintingService _mintingService;
        private readonly TransactionManager _transactionManager;
        private readonly InventoryService _inventoryService;

        public StartInventoryTransactionEndpoint(ContractProxy contractProxy, MintingService mintingService,
            MintCollection mintCollection, MetadataService metadataService, TransactionManager transactionManager,
            InventoryService inventoryService)
        {
            _contractProxy = contractProxy;
            _mintingService = mintingService;
            _mintCollection = mintCollection;
            _metadataService = metadataService;
            _transactionManager = transactionManager;
            _inventoryService = inventoryService;
        }

        public async Promise<FederatedInventoryProxyState> StartInventoryTransaction(string id, string transaction,
            Dictionary<string, long> currencies, List<FederatedItemCreateRequest> newItems,
            List<FederatedItemDeleteRequest> deleteItems, List<FederatedItemUpdateRequest> updateItems)
        {
            var transactionId = await _transactionManager.StartTransaction(id, nameof(StartInventoryTransaction),
                transaction, currencies, newItems, deleteItems, updateItems);
            _transactionManager.SetCurrentTransactionContext(transactionId);

            _ = _transactionManager.RunAsyncBlock(transactionId, transaction, async () =>
            {
                // NEW MINTS
                if (currencies.Any() || newItems.Any())
                {
                    var currencyMints = currencies.Select(c => new MintRequest
                    {
                        ContentId = c.Key,
                        Amount = (uint)c.Value,
                        Properties = new Dictionary<string, string>(),
                        IsNft = false
                    });

                    var itemMints = newItems.Select(i => new MintRequest
                    {
                        ContentId = i.contentId,
                        Amount = 1,
                        Properties = i.properties,
                        IsNft = true
                    });

                    await _mintingService.Mint(id, currencyMints.Union(itemMints).ToList());
                }

                // UPDATE ITEMS
                if (updateItems.Any())
                {
                    var requests = await Task.WhenAll(updateItems.Select(async x =>
                    {
                        var metadata =
                            await _metadataService.BuildMetadata(x.contentId, uint.Parse(x.proxyId), x.properties);
                        return new UpdateMetadataRequest
                        {
                            TokenId = uint.Parse(x.proxyId),
                            Metadata = metadata
                        };
                    }).ToList());

                    // Store new metadata
                    var newMetadataMessage = await _metadataService.StoreNewMetadata(requests);

                    // Bulk update contract metadata
                    await _contractProxy.SetTokenMetadataHashes(newMetadataMessage);

                    // Save the new metadata in DB
                    await _mintCollection.BulkSaveMetadata(requests);
                }
            });

            return await _inventoryService.GetLastKnownState(id);
        }

        public async Task ValidateRequest(string id, string transaction, Dictionary<string, long> currencies,
            List<FederatedItemCreateRequest> newItems, List<FederatedItemDeleteRequest> deleteItems,
            List<FederatedItemUpdateRequest> updateItems)
        {
            updateItems.ForEach(update =>
            {
                if (!uint.TryParse(update.proxyId, out _))
                    throw new InvalidRequestException($"{update.proxyId} is not a valid proxyId");
            });
        }
    }
}