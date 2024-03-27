using System.Threading.Tasks;
using Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Composition.Models;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts
{
    public partial class ContractProxy
    {
        public async Task<GetAttachmentsFunctionOutput> GetAttachments(GetAttachmentsFunctionMessage request)
        {
            return await _ethRpcClient.SendFunctionQueryAsync<GetAttachmentsFunctionMessage, GetAttachmentsFunctionOutput>((await GetDefaultContract()).PublicKey, request);
        }

        public async Task<string> Compose(ComposeFunctionMessage request, bool waitForReceipt = false)
        {
            return await _ethRpcClient.SendTransactionAsync((await GetDefaultContract()).PublicKey, request, waitForReceipt);
        }

        public async Task<string> ComposeAndUpdate(ComposeAndUpdateFunctionMessage request, bool waitForReceipt = false)
        {
            return await _ethRpcClient.SendTransactionAsync((await GetDefaultContract()).PublicKey, request, waitForReceipt);
        }
    }
}