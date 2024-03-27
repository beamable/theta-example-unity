using System.Linq;
using System.Threading.Tasks;
using Beamable.Microservices.ThetaFederation.Features.Accounts.Storage.Models;
using Beamable.Server;
using MongoDB.Driver;

namespace Beamable.Microservices.ThetaFederation.Features.Accounts.Storage
{
    public class NonceCollection : IService
{
	private readonly IStorageObjectConnectionProvider _storageObjectConnectionProvider;
	private IMongoCollection<Nonce>? _collection;

	public NonceCollection(IStorageObjectConnectionProvider storageObjectConnectionProvider)
	{
		_storageObjectConnectionProvider = storageObjectConnectionProvider;
	}

	private async ValueTask<IMongoCollection<Nonce>> Get()
	{
		if (_collection is null)
		{
			var db = await _storageObjectConnectionProvider.ThetaStorageDatabase();
			_collection = db.GetCollection<Nonce>("nonce");
		}

		return _collection;
	}

	public async Task<long?> GetNextIfNoErrors(string nonceName)
	{
		var collection = await Get();
		var update = Builders<Nonce>.Update.Inc(x => x.State, (int)1);

		var options = new FindOneAndUpdateOptions<Nonce>
		{
			ReturnDocument = ReturnDocument.After,
			IsUpsert = false
		};

		var updated = await collection.FindOneAndUpdateAsync<Nonce>(x => x.Name == nonceName && x.Errors.Count == 0,
			update,
			options);

		if (updated is not null)
		{
			return updated.State;
		}
		return null;
	}

	public async Task<long?> PopError(string nonceName)
	{
		var collection = await Get();

		var update = Builders<Nonce>.Update.PopFirst(x => x.Errors);
		var options = new FindOneAndUpdateOptions<Nonce>
		{
			ReturnDocument = ReturnDocument.Before,
			IsUpsert = false
		};
		var updated = await collection.FindOneAndUpdateAsync<Nonce>(x => x.Name == nonceName && x.Errors.Count > 0,
			update,
			options);

		if (updated is not null)
		{
			return updated.Errors.First();
		}
		return null;
	}

	public async Task PushError(string nonceName, long value)
	{
		var collection = await Get();
		var update = Builders<Nonce>.Update
			.Push(x => x.Errors, value);

		var options = new UpdateOptions
		{
			IsUpsert = false
		};

		await collection.UpdateOneAsync(x => x.Name == nonceName, update, options);
	}

	public async Task Set(string nonceName, long value)
	{
		var collection = await Get();
		var update = Builders<Nonce>.Update
			.Set(x => x.State, value)
			.Set(x => x.Errors, new());

		var options = new UpdateOptions
		{
			IsUpsert = true
		};

		await collection.UpdateOneAsync(x => x.Name == nonceName, update, options);
	}
}
}