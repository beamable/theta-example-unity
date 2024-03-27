using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Microservices.ThetaFederation.Features.Accounts;
using Beamable.Microservices.ThetaFederation.Features.Accounts.Storage;
using Beamable.Microservices.ThetaFederation.Features.EthRpc;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts
{
    public class InitializeNonceService : IService
    {
        private readonly AccountsService _accountsService;
        private readonly EthRpcClient _ethRpcClient;
        private readonly NonceCollection _nonceCollection;

        public InitializeNonceService(AccountsService accountsService, EthRpcClient ethRpcClient, NonceCollection nonceCollection)
        {
            _accountsService = accountsService;
            _ethRpcClient = ethRpcClient;
            _nonceCollection = nonceCollection;
        }

        public async Task InitializeNonce(string? publicKey = null)
        {
            if (string.IsNullOrEmpty(publicKey))
            {
                publicKey = (await _accountsService.GetOrCreateRealmAccount()).Address;
            }

            var currentCount = await _ethRpcClient.GetTransactionCountAsync(publicKey);
            var counterValue = (long)currentCount.Value - 1;
            BeamableLogger.Log("Setting nonce {k} to {N}", publicKey, counterValue);
            await _nonceCollection.Set(publicKey, counterValue);
        }
    }
}