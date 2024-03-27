using System.Threading.Tasks;
using Beamable.Microservices.ThetaFederation.Features.Contracts;

namespace Beamable.Microservices.ThetaFederation.Endpoints
{
    public class GetContractAddressEndpoint : IEndpoint
    {
        private readonly ContractProxy _contractProxy;

        public GetContractAddressEndpoint(ContractProxy contractProxy)
        {
            _contractProxy = contractProxy;
        }

        public async Task<string> GetContractAddress()
        {
            var contract = await _contractProxy.GetDefaultContract();
            return contract.PublicKey;
        }
    }
}