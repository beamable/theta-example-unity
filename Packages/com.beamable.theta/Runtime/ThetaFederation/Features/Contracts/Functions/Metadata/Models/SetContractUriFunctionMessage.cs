using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Metadata.Models
{
    [Function("setContractURI")]
    public class SetContractUriFunctionMessage : FunctionMessage
    {
        [Parameter("string", "newUri")]
        public virtual string NewUri { get; set; } = null!;
    }
}