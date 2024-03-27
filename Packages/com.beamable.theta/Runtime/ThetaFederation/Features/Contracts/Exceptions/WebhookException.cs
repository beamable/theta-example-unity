using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Exceptions
{
    public class WebhookException : MicroserviceException
    {
        public WebhookException(string message) : base((int)HttpStatusCode.InternalServerError, "WebhookException", message)
        {
        }
    }
}