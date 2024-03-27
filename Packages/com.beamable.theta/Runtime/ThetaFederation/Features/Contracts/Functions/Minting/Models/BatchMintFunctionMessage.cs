using System.Collections.Generic;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Minting.Models
{
    [Function("batchMint")]
    public class BatchMintFunctionMessage : FunctionMessage
    {
        [Parameter("address", "to")]
        public virtual string To { get; set; } = null!;

        [Parameter("uint256[]", "tokenIds", 2)]
        public virtual List<uint> TokenIds { get; set; } = null!;

        [Parameter("uint256[]", "amounts", 3)]
        public virtual List<uint> Amounts { get; set; } = null!;

        [Parameter("string[]", "metadataHashes", 4)]
        public virtual List<string> MetadataHashes { get; set; } = null!;
    }
}