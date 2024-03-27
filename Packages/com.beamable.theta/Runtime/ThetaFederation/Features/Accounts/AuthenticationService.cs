using System;
using Beamable.Common;
using Beamable.Microservices.ThetaFederation.Features.Accounts.Exceptions;
using Nethereum.Signer;

namespace Beamable.Microservices.ThetaFederation.Features.Accounts
{
    public class AuthenticationService : IService
    {
        private readonly EthereumMessageSigner _signer = new();

        // Documentation: https://docs.nethereum.com/en/latest/Nethereum.Workbooks/docs/nethereum-signing-messages/
        public bool IsSignatureValid(string address, string challenge, string signature)
        {
            try
            {
                var recoveredAddress = _signer.EncodeUTF8AndEcRecover(challenge, signature);
                return string.Equals(recoveredAddress, address, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                BeamableLogger.LogError(ex);
                throw new UnauthorizedException();
            }
        }
    }
}