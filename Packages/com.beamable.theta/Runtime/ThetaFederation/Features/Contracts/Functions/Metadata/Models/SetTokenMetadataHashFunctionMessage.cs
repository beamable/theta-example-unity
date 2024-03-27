using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Metadata.Models
{
    [Function("setTokenMetadataHash")]
    public class SetTokenMetadataHashFunctionMessage : FunctionMessage
    {
        [Parameter("uint256", "tokenId")]
        public virtual uint TokenId { get; set; }

        [Parameter("string", "metadataHash", 2)]
        public virtual string MetadataHash { get; set; } = null!;
    }
}