using System.Threading.Tasks;
using Beamable.Microservices.ThetaFederation.Features.Accounts;

namespace Beamable.Microservices.ThetaFederation.Endpoints
{
    public class GetRealmAccountEndpoint : IEndpoint
    {
        private readonly AccountsService _accountsService;

        public GetRealmAccountEndpoint(AccountsService accountsService)
        {
            _accountsService = accountsService;
        }

        public async Task<string> GetRealmAccount()
        {
            var realmAccount = await _accountsService.GetOrCreateRealmAccount();
            return realmAccount.Address;
        }
    }
}