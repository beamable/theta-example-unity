using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.ThetaFederation.Features.EthRpc.Exceptions
{
    internal class TransactionReceiptException : MicroserviceException
    {
        public TransactionReceiptException(string message) : base((int)HttpStatusCode.BadRequest, "TransactionReceiptException", message)
        {
        }
    }
}