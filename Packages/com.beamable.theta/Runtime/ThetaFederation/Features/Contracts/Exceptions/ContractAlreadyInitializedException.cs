using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Exceptions
{
    internal class ContractAlreadyInitializedException : MicroserviceException
    {
        public ContractAlreadyInitializedException() : base((int)HttpStatusCode.BadRequest, "ContractAlreadyInitializedError", "Contract is already initialized")
        {
        }
    }
}