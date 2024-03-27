using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Microservices.ThetaFederation.Features.Contracts;
using Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Inventory.Models;
using Beamable.Microservices.ThetaFederation.Features.Inventory.Storage;
using Beamable.Microservices.ThetaFederation.Features.Inventory.Storage.Models;
using Beamable.Microservices.ThetaFederation.Features.Minting.Storage;

namespace Beamable.Microservices.ThetaFederation.Features.Inventory
{
    public class InventoryService : IService
{
    private readonly MintCollection _mintCollection;
    private readonly InventoryStateCollection _inventoryStateCollection;
    private readonly ContractProxy _contractProxy;

    public InventoryService(MintCollection mintCollection, InventoryStateCollection inventoryStateCollection, ContractProxy contractProxy)
    {
        _mintCollection = mintCollection;
        _inventoryStateCollection = inventoryStateCollection;
        _contractProxy = contractProxy;
    }

    public async Task<FederatedInventoryProxyState> GetInventoryState(string id)
    {
        var inventoryResponse = await _contractProxy.GetInventory(new GetInventoryFunctionMessage { Account = id });

        var existingMints = (await _mintCollection.GetTokenMints(ContractService.DefaultContractName, inventoryResponse.TokenIds)).ToDictionary(x => x.TokenId, x => x);

        var currencies = new Dictionary<string, long>();
        var items = new List<(string, FederatedItemProxy)>();

        for (var i = 0; i < inventoryResponse.TokenIds.Count; i++)
        {
            var tokenId = inventoryResponse.TokenIds[i];

            if (!existingMints.ContainsKey(tokenId))
            {
                BeamableLogger.LogWarning("Token {tokenId} for account {accountId} isn't present in the mint collection", tokenId, id);
                continue;
            }

            var existingMint = existingMints[tokenId];
            var amount = inventoryResponse.TokenAmounts[i];
            var contentId = existingMint.ContentId;

            if (contentId.StartsWith("currency.")) currencies.Add(contentId, amount);

            if (contentId.StartsWith("items."))
            {
                items.Add((contentId, new FederatedItemProxy
                {
                    proxyId = tokenId.ToString(),
                    properties = existingMint.Metadata.GetItemProperties().ToList()
                }));
            }
        }

        BeamableLogger.Log("Found {x} items", items.Count);

        var itemGroups = items.GroupBy(i => i.Item1).ToDictionary(g => g.Key, g => g.Select(i => i.Item2).ToList());

        var state = new FederatedInventoryProxyState
        {
            currencies = currencies,
            items = itemGroups
        };

        await _inventoryStateCollection.Save(new InventoryState
        {
            Id = id.ToLower(),
            Inventory = state
        });

        return state;
    }

    public async Task<FederatedInventoryProxyState> GetLastKnownState(string id)
    {
        var lastKnownState = await _inventoryStateCollection.Get(id.ToLower());

        return lastKnownState ?? new FederatedInventoryProxyState
        {
            currencies = new Dictionary<string, long>(),
            items = new Dictionary<string, List<FederatedItemProxy>>()
        };
    }
}
}