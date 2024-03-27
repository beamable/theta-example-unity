using System;
using Beamable.Common;
using Beamable.Microservices.ThetaFederation.Extensions;
using Beamable.Microservices.ThetaFederation.Features.Accounts;
using Beamable.Microservices.ThetaFederation.Features.Accounts.Exceptions;
using Beamable.Server;
using Beamable.Theta.Common;
using Nethereum.Util;

namespace Beamable.Microservices.ThetaFederation.Endpoints
{
    public class AuthenticateEndpoint : IEndpoint
    {
        private readonly AccountsService _accountsService;
        private readonly AuthenticationService _authenticationService;
        private readonly RequestContext _requestContext;

        public AuthenticateEndpoint(AccountsService accountsService, AuthenticationService authenticationService,
            RequestContext requestContext)
        {
            _accountsService = accountsService;
            _authenticationService = authenticationService;
            _requestContext = requestContext;
        }

        public async Promise<FederatedAuthenticationResponse> Authenticate(string token, string challenge,
            string solution)
        {
            // Check if an external identity is already attached (from request context)
            if (_requestContext.UserId != 0L)
            {
                var microserviceInfo =
                    MicroserviceMetadataExtensions.GetMetadata<ThetaFederation, ThetaCloudIdentity>();
                var existingExternalIdentity = _requestContext.GetExternalIdentity(microserviceInfo);

                if (existingExternalIdentity is not null)
                {
                    return new FederatedAuthenticationResponse
                    {
                        user_id = existingExternalIdentity.userId
                    };
                }
            }

            if (Configuration.AllowManagedAccounts)
                if (string.IsNullOrEmpty(token) && _requestContext.UserId != 0L)
                {
                    // Create new account for player if token is empty
                    var account = await _accountsService.GetOrCreateAccount(_requestContext.UserId.ToString());
                    return new FederatedAuthenticationResponse
                    {
                        user_id = account.Address
                    };
                }

            // Challenge-based authentication
            if (!string.IsNullOrEmpty(challenge) && !string.IsNullOrEmpty(solution))
            {
                if (_authenticationService.IsSignatureValid(token, challenge, solution))
                    // User identity is confirmed
                    return new FederatedAuthenticationResponse
                    {
                        user_id = AddressUtil.Current.ConvertToChecksumAddress(token)
                    };

                // Signature is invalid, user identity isn't confirmed
                BeamableLogger.LogWarning(
                    "Invalid signature {signature} for challenge {challenge} and account {account}", solution,
                    challenge, token);
                throw new UnauthorizedException();
            }

            // Generate a challenge
            return new FederatedAuthenticationResponse
            {
                challenge = $"Please sign this random message to authenticate: {Guid.NewGuid()}",
                challenge_ttl = Configuration.AuthenticationChallengeTtlSec
            };
        }
    }
}