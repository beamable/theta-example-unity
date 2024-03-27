using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.ThetaFederation.Features.EthRpc.Exceptions
{
    internal class RpcCallException : MicroserviceException
    {
        public RpcCallException(string message) : base((int)HttpStatusCode.BadRequest, "RpcCallError", message)
        {
        }
    }
}