using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Minting.Models
{
    [Function("mint")]
    public class MintFunctionMessage : FunctionMessage
    {
        [Parameter("address", "to")]
        public virtual string To { get; set; } = null!;

        [Parameter("uint256", "tokenId", 2)]
        public virtual uint TokenId { get; set; }

        [Parameter("uint256", "amount", 3)]
        public virtual uint Amount { get; set; }

        [Parameter("string", "metadataHash", 4)]
        public virtual string MetadataHash { get; set; } = null!;
    }
}