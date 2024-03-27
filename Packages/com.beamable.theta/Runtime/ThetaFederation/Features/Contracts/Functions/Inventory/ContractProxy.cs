using System.Threading.Tasks;
using Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Inventory.Models;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts
{
    public partial class ContractProxy
    {
        public async Task<GetInventoryFunctionOutput> GetInventory(GetInventoryFunctionMessage request)
        {
            return await _ethRpcClient.SendFunctionQueryAsync<GetInventoryFunctionMessage, GetInventoryFunctionOutput>((await GetDefaultContract()).PublicKey, request);
        }
    }
}