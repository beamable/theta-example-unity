using System.Collections.Generic;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Inventory.Models
{
    [FunctionOutput]
    public class GetInventoryFunctionOutput : IFunctionOutputDTO
    {
        [Parameter("uint256[]", "tokenIds")]
        public virtual List<uint> TokenIds { get; set; } = null!;

        [Parameter("uint256[]", "tokenAmounts", 2)]
        public virtual List<uint> TokenAmounts { get; set; } = null!;

        [Parameter("string[]", "metadataHashes", 3)]
        public virtual List<string> MetadataHashes { get; set; } = null!;
    }
}