using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Metadata.Models
{
    [Function("setBaseURI")]
    public class SetBaseUriFunctionMessage : FunctionMessage
    {
        [Parameter("string", "newBaseUri")]
        public virtual string NewBaseUri { get; set; } = null!;
    }
}