using Beamable.Microservices.ThetaFederation.Features.Security;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Security.Models
{
    [Function("setTransferSecurityLevelOfCollection")]
    public class SetTransferSecurityLevelOfCollectionFunctionMessage : FunctionMessage
    {
        [Parameter("uint8", "level")]
        public virtual TransferSecurityLevels Level { get; set; }
    }
}