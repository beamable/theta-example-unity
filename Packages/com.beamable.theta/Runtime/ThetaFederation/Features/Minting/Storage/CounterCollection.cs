using System.Threading.Tasks;
using Beamable.Microservices.ThetaFederation.Features.Minting.Storage.Models;
using Beamable.Server;
using MongoDB.Driver;

namespace Beamable.Microservices.ThetaFederation.Features.Minting.Storage
{
    public class CounterCollection : IService
    {
        private readonly IStorageObjectConnectionProvider _storageObjectConnectionProvider;
        private IMongoCollection<Counter>? _collection;

        public CounterCollection(IStorageObjectConnectionProvider storageObjectConnectionProvider)
        {
            _storageObjectConnectionProvider = storageObjectConnectionProvider;
        }

        private async ValueTask<IMongoCollection<Counter>> Get()
        {
            if (_collection is null)
            {
                var db = await _storageObjectConnectionProvider.ThetaStorageDatabase();
                _collection = db.GetCollection<Counter>("counter");
            }

            return _collection;
        }

        public async Task<uint> GetNextCounterValue(string counterName)
        {
            var collection = await Get();
            var update = Builders<Counter>.Update.Inc(x => x.State, (uint)1);

            var options = new FindOneAndUpdateOptions<Counter>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true
            };

            var updated = await collection.FindOneAndUpdateAsync<Counter>(x => x.Name == counterName, update, options);

            return updated.State;
        }
    }
}