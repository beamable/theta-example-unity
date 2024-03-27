using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.ThetaFederation.Features.EthRpc.Exceptions
{
    internal class GasCalculationException : MicroserviceException
    {
        public GasCalculationException(string message) : base((int)HttpStatusCode.BadRequest, "GasCalculationError", message)
        {
        }
    }
}