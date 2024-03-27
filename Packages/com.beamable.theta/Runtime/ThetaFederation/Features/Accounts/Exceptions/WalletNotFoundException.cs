using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.ThetaFederation.Features.Accounts.Exceptions
{
    internal class WalletNotFoundException : MicroserviceException
    {
        public WalletNotFoundException(string walletAddress) : base((int)HttpStatusCode.NotFound, "WalletNotFound", $"Wallet {walletAddress} not found")
        {
        }
    }
}