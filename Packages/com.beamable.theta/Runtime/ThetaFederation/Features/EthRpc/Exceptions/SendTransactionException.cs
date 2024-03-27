using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.ThetaFederation.Features.EthRpc.Exceptions
{
    internal class SendTransactionException : MicroserviceException
    {
        public SendTransactionException(string message) : base((int)HttpStatusCode.BadRequest, "SendTransactionError", message)
        {
        }
    }
}