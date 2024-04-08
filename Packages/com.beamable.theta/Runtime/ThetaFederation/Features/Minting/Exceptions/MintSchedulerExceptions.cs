using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.ThetaFederation.Features.Minting.Exceptions
{
    public class MintSchedulerExceptions : MicroserviceException
    {
        public MintSchedulerExceptions(string message) : base((int)HttpStatusCode.BadRequest, "MintSchedulerExceptions",
            message)
        {
        }
    }
}