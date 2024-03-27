using System;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Common.Dependencies;
using Beamable.Microservices.ThetaFederation.Features.Accounts.Storage;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.NonceServices;

namespace Beamable.Microservices.ThetaFederation.Features.Accounts
{
    public class MongoNonceService : INonceService
    {
        private readonly NonceCollection _nonceCollection;
        private readonly string _nonceKey;
        public IClient Client { get; set; } = null!;

        public MongoNonceService(IDependencyProvider dependencyProvider, string nonceKey)
        {
            _nonceCollection = dependencyProvider.GetService<NonceCollection>();
            _nonceKey = nonceKey;
        }

        public async Task<HexBigInteger> GetNextNonceAsync()
        {
            var nextValue = await _nonceCollection.GetNextIfNoErrors(_nonceKey);
            if (nextValue is not null)
                return new HexBigInteger(nextValue.Value);

            var errorNonce = await _nonceCollection.PopError(_nonceKey);
            BeamableLogger.LogWarning("Nonce has errors. Trying to reuse the error nonce value {n}.", errorNonce);
            if (errorNonce is not null)
                return new HexBigInteger(errorNonce.Value);

            BeamableLogger.LogWarning("Unable to fetch error nonce. Fetching nonce again.");
            return await GetNextNonceAsync();
        }

        public Task ResetNonceAsync()
        {
            return Task.CompletedTask;
        }

        public async Task PushErrorAsync(long error)
        {
            await _nonceCollection.PushError(_nonceKey, error);
        }
    }
}