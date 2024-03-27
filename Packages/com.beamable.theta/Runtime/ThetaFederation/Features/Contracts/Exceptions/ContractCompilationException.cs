using System.Collections.Generic;
using System.Linq;
using System.Net;
using Beamable.Microservices.ThetaFederation.Features.SolcWrapper.Models;
using Beamable.Server;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Exceptions
{
    internal class ContractCompilationException : MicroserviceException
    {
        public ContractCompilationException(IEnumerable<SolidityCompilerOutput.OutputError> errors) : base((int)HttpStatusCode.BadRequest, "ContractCompilationError",
            $"Compile errors: {string.Join(",", errors.Select(x => x.Message).ToList())}")
        {
        }
    }
}