using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Inventory.Models
{
    [Function("getInventory")]
    public class GetInventoryFunctionMessage : FunctionMessage
    {
        [Parameter("address", "account")]
        public virtual string Account { get; set; } = null!;
    }
}