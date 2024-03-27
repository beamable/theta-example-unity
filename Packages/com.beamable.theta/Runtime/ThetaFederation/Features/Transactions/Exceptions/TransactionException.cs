using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.ThetaFederation.Features.Transactions.Exceptions
{
    internal class TransactionException : MicroserviceException
    {
        public TransactionException(string message) : base((int)HttpStatusCode.BadRequest, "TransactionError", message)
        {
        }
    }
}