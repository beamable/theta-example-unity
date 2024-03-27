using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Composition.Models
{
    [Function("getAttachmentsRecursive")]
    public class GetAttachmentsFunctionMessage : FunctionMessage
    {
        [Parameter("uint256", "tokenId")]
        public virtual uint TokenId { get; set; }
    }
}