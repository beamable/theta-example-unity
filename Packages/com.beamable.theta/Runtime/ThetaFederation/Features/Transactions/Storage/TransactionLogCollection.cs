using System;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Microservices.ThetaFederation.Features.Transactions.Storage.Models;
using Beamable.Server;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Beamable.Microservices.ThetaFederation.Features.Transactions.Storage
{
    public class TransactionLogCollection : IService
{
	private readonly IStorageObjectConnectionProvider _storageObjectConnectionProvider;
	private IMongoCollection<TransactionLog>? _collection;

	public TransactionLogCollection(IStorageObjectConnectionProvider storageObjectConnectionProvider)
	{
		_storageObjectConnectionProvider = storageObjectConnectionProvider;
	}

	private async ValueTask<IMongoCollection<TransactionLog>> Get()
	{
		if (_collection is null)
		{
			var db = await _storageObjectConnectionProvider.ThetaStorageDatabase();
			_collection = db.GetCollection<TransactionLog>("transaction-log");

			await _collection.Indexes.CreateOneAsync(new CreateIndexModel<TransactionLog>(Builders<TransactionLog>.IndexKeys
				.Ascending(x => x.Wallet)
				.Ascending(x => x.OperationName)
			));
			await _collection.Indexes.CreateOneAsync(new CreateIndexModel<TransactionLog>(Builders<TransactionLog>.IndexKeys
				.Ascending("ChainTransactions.TransactionHash")
			));
		}

		return _collection;
	}

	public async Task Insert(TransactionLog log)
	{
		var collection = await Get();
		await collection.InsertOneAsync(log);
	}

	public async Task SetDone(ObjectId inventoryTransaction)
	{
		var collection = await Get();
		var update = Builders<TransactionLog>.Update.Set(x => x.EndTimestamp, DateTime.UtcNow);
		await collection.UpdateOneAsync(x => x.Id == inventoryTransaction, update);
	}

	public async Task SetMined(ObjectId transactionId)
	{
		var collection = await Get();
		var update = Builders<TransactionLog>.Update.Set(x => x.MinedTimestamp, DateTime.UtcNow);
		await collection.UpdateOneAsync(x => x.Id == transactionId, update);
	}

	public async Task SetError(ObjectId transactionId, string error)
	{
		var collection = await Get();
		var update = Builders<TransactionLog>.Update.Set(x => x.Error, error);
		await collection.UpdateOneAsync(x => x.Id == transactionId, update);
	}

	public async Task AddChainTransaction(ObjectId transactionId, ChainTransaction chainTransaction)
	{
		var collection = await Get();
		var update = Builders<TransactionLog>.Update.Push(x => x.ChainTransactions, chainTransaction);
		await collection.UpdateOneAsync(x => x.Id == transactionId, update);
	}

	public async Task<TransactionLog?> GetByChainTransactionHash(string hash)
	{
		var collection = await Get();
		return await collection
			.Find(x => x.ChainTransactions.Any(xx => xx.TransactionHash == hash))
			.SortByDescending(x => x.Id)
			.FirstOrDefaultAsync();
	}

	public async Task<TransactionLog?> GetById(ObjectId transactionId)
	{
		var collection = await Get();
		return await collection
			.Find(x => x.Id == transactionId)
			.FirstOrDefaultAsync();
	}

	public async Task<TransactionLog?> GetLastByWalletAndOperation(string wallet, string operationName)
	{
		var collection = await Get();
		return await collection
			.Find(x => x.Wallet == wallet && x.OperationName == operationName)
			.SortByDescending(x => x.Id)
			.FirstOrDefaultAsync();
	}
}
}