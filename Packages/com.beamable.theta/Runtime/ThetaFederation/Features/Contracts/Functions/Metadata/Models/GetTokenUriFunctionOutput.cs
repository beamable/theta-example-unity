using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Metadata.Models
{
    [FunctionOutput]
    public class GetTokenUriFunctionOutput : IFunctionOutputDTO
    {
        [Parameter("string", "tokenURI")]
        public virtual string TokenUri { get; set; } = null!;
    }
}