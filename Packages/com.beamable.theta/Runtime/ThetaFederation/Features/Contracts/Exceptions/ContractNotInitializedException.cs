using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Exceptions
{
    internal class ContractNotInitializedException : MicroserviceException
    {
        public ContractNotInitializedException() : base((int)HttpStatusCode.BadRequest, "ContractNotInitializedError", "Contract is not initialized")
        {
        }
    }
}