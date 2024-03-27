using System.Collections.Generic;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Composition.Models
{
    [FunctionOutput]
    public class GetAttachmentsFunctionOutput : IFunctionOutputDTO
    {
        [Parameter("uint256[]", "tokenIds")]
        public virtual List<uint> TokenIds { get; set; } = null!;
    }
}