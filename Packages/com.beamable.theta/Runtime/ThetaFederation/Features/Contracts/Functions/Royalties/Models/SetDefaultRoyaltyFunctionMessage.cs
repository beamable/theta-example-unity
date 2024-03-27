using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Royalties.Models
{
    [Function("setDefaultRoyalty")]
    public class SetDefaultRoyaltyFunctionMessage : FunctionMessage
    {
        [Parameter("address", "receiver")]
        public virtual string Receiver { get; set; } = null!;

        [Parameter("uint96", "feeNumerator", 2)]
        public virtual int FeeNumerator { get; set; }
    }
}