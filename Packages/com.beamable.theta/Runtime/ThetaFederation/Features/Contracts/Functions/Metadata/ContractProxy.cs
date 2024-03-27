using System.Threading.Tasks;
using Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Metadata.Models;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts
{
    public partial class ContractProxy
    {
        public async Task<GetTokenUriFunctionOutput> GetTokenUri(GetTokenUriFunctionMessage request)
        {
            return await _ethRpcClient.SendFunctionQueryAsync<GetTokenUriFunctionMessage, GetTokenUriFunctionOutput>((await GetDefaultContract()).PublicKey, request);
        }

        public async Task SetBaseUri(SetBaseUriFunctionMessage request, string contractAddress, bool waitForReceipt = true)
        {
            await _ethRpcClient.SendTransactionAsync(contractAddress, request, waitForReceipt);
        }

        public async Task SetContractUri(SetContractUriFunctionMessage request, string contractAddress, bool waitForReceipt = true)
        {
            await _ethRpcClient.SendTransactionAsync(contractAddress, request, waitForReceipt);
        }


        public async Task SetTokenMetadataHashes(SetTokenMetadataHashesFunctionMessage request, bool waitForReceipt = false)
        {
            await _ethRpcClient.SendTransactionAsync((await GetDefaultContract()).PublicKey, request, waitForReceipt);
        }
    }
}