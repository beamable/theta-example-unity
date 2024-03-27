using System.Threading.Tasks;
using Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Royalties.Models;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts
{
    public partial class ContractProxy
    {
        public async Task SetDefaultRoyalty(SetDefaultRoyaltyFunctionMessage request, bool waitForReceipt = false)
        {
            await _ethRpcClient.SendTransactionAsync((await GetDefaultContract()).PublicKey, request, waitForReceipt);
        }
    }
}