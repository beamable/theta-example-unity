using System.Collections.Generic;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Metadata.Models
{
    [Function("setTokenMetadataHashes")]
    public class SetTokenMetadataHashesFunctionMessage : FunctionMessage
    {
        [Parameter("uint256[]", "tokenIds")]
        public virtual List<uint> TokenIds { get; set; } = null!;

        [Parameter("string[]", "metadataHashes", 2)]
        public virtual List<string> MetadataHashes { get; set; } = null!;
    }
}