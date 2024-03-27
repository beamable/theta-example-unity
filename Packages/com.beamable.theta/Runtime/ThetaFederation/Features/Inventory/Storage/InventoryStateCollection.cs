using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Microservices.ThetaFederation.Features.Inventory.Storage.Models;
using Beamable.Server;
using MongoDB.Driver;

namespace Beamable.Microservices.ThetaFederation.Features.Inventory.Storage
{
    public class InventoryStateCollection : IService
    {
        private readonly IStorageObjectConnectionProvider _storageObjectConnectionProvider;
        private IMongoCollection<InventoryState>? _collection;

        public InventoryStateCollection(IStorageObjectConnectionProvider storageObjectConnectionProvider)
        {
            _storageObjectConnectionProvider = storageObjectConnectionProvider;
        }

        private async ValueTask<IMongoCollection<InventoryState>> Get()
        {
            if (_collection is null)
                _collection = (await _storageObjectConnectionProvider.ThetaStorageDatabase()).GetCollection<InventoryState>("inventory-state");

            return _collection;
        }

        public async Task Save(InventoryState state)
        {
            var collection = await Get();
            await collection.ReplaceOneAsync(
                x => x.Id == state.Id,
                state,
                new ReplaceOptions
                {
                    IsUpsert = true
                }
            );
        }

        public async Task<FederatedInventoryProxyState?> Get(string id)
        {
            var collection = await Get();
            return await collection
                .Find(x => x.Id == id)
                .Project(x => x.Inventory)
                .FirstOrDefaultAsync();
        }
    }
}