using Beamable.Common;
using Beamable.Microservices.ThetaFederation.Features.Inventory;

namespace Beamable.Microservices.ThetaFederation.Endpoints
{
    public class GetInventoryStateEndpoint : IEndpoint
    {
        private readonly InventoryService _inventoryService;

        public GetInventoryStateEndpoint(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public async Promise<FederatedInventoryProxyState> GetInventoryState(string id)
        {
            return await _inventoryService.GetInventoryState(id);
        }
    }
}