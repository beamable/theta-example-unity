using System.Threading.Tasks;
using Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Minting.Models;
using Nethereum.RPC.Eth.DTOs;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts
{
    public partial class ContractProxy
    {
        public async Task<string> BatchMint(BatchMintFunctionMessage request, bool waitForReceipt = false)
        {
            return await _ethRpcClient.SendTransactionAsync((await GetDefaultContract()).PublicKey, request, waitForReceipt);
        }

        public async Task<string> MintAndAttach(MintAndAttachFunctionMessage request, bool waitForReceipt = false)
        {
            return await _ethRpcClient.SendTransactionAsync((await GetDefaultContract()).PublicKey, request, waitForReceipt);
        }

        public async Task<string> MintAndAttachLocked(MintAndAttachLockedFunctionMessage request, bool waitForReceipt = false)
        {
            return await _ethRpcClient.SendTransactionAsync((await GetDefaultContract()).PublicKey, request, waitForReceipt);
        }

        public async Task<TransactionReceipt> FetchReceiptAsync(string? transactionHash)
        {
            return await _ethRpcClient.FetchReceiptAsync(transactionHash);
        }
    }
}