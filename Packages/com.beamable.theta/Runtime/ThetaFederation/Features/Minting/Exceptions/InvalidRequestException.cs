using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.ThetaFederation.Features.Minting.Exceptions
{
    public class InvalidRequestException : MicroserviceException
    {
        public InvalidRequestException(string message) : base((int)HttpStatusCode.BadRequest, "InvalidRequestError", message)
        {
        }
    }
}