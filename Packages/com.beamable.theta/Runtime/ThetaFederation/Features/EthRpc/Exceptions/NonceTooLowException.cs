using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.ThetaFederation.Features.EthRpc.Exceptions
{
    public class NonceTooLowException : MicroserviceException
    {
        public NonceTooLowException() : base((int)HttpStatusCode.BadRequest, "NonceTooLow", "")
        {
        }
    }
}