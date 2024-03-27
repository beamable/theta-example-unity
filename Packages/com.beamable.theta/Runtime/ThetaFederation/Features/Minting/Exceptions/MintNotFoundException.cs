using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.ThetaFederation.Features.Minting.Exceptions
{
    internal class MintNotFoundException : MicroserviceException
    {
        public MintNotFoundException(string message) : base((int)HttpStatusCode.BadRequest, "MintNotFoundError", message)
        {
        }
    }
}