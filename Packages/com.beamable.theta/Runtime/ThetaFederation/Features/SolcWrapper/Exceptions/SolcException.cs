﻿using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.ThetaFederation.Features.SolcWrapper.Exceptions
{
    internal class SolcException : MicroserviceException

    {
        public SolcException(string message) : base((int)HttpStatusCode.BadRequest, "SolcError", message)
        {
        }
    }
}