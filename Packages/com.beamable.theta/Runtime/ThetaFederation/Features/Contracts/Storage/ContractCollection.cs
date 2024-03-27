using System.Threading.Tasks;
using Beamable.Server;
using Beamable.Microservices.ThetaFederation.Features.Contracts.Storage.Models;
using MongoDB.Driver;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Storage
{
    public class ContractCollection : IService
{
	private readonly IStorageObjectConnectionProvider _storageObjectConnectionProvider;
	private IMongoCollection<Contract>? _collection;

	public ContractCollection(IStorageObjectConnectionProvider storageObjectConnectionProvider)
	{
		_storageObjectConnectionProvider = storageObjectConnectionProvider;
	}

	private async ValueTask<IMongoCollection<Contract>> Get()
	{
		if (_collection is null)
		{
			var db = await _storageObjectConnectionProvider.ThetaStorageDatabase();
			_collection = db.GetCollection<Contract>("contract");
			await _collection.Indexes.CreateManyAsync(new[]
			{
				new CreateIndexModel<Contract>(Builders<Contract>.IndexKeys.Ascending(x => x.Name).Ascending(x => x.PublicKey), new CreateIndexOptions { Unique = true })
			});
		}

		return _collection;
	}

	public async Task<Contract?> GetContract(string name)
	{
		var collection = await Get();
		return await collection.Find(x => x.Name == name).FirstOrDefaultAsync();
	}

	public async Task SaveContract(Contract contract)
	{
		var collection = await Get();
		await collection.ReplaceOneAsync(
			x => x.Name == contract.Name,
			contract,
			new ReplaceOptions { IsUpsert = true});
	}

	public async Task<bool> TryInsertContract(Contract contract)
	{
		var collection = await Get();
		try
		{
			await collection.InsertOneAsync(contract);
			return true;
		}
		catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
		{
			// Ignore duplicate key errors
			return false;
		}
	}

	public async Task UpdateBaseMetadataUri(Contract contract)
	{
		var collection = await Get();
		var update = Builders<Contract>.Update.Set(x => x.BaseMetadataUri, contract.BaseMetadataUri);
		await collection.UpdateOneAsync(x => x.Name == contract.Name, update);
	}

	public async Task UpdateCollectionMetadataUri(Contract contract)
	{
		var collection = await Get();
		var update = Builders<Contract>.Update.Set(x => x.CollectionMetadataUri, contract.CollectionMetadataUri);
		await collection.UpdateOneAsync(x => x.Name == contract.Name, update);
	}
}
}