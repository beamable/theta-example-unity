using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.ThetaFederation.Features.Minting.Exceptions
{
    internal class MintingNotAllowed: MicroserviceException
    {
        public MintingNotAllowed(string message) : base((int)HttpStatusCode.BadRequest, "MintingNotAllowed", message)
        {
        }
    }
}