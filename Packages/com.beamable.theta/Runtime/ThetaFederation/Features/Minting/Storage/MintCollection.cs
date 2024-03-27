using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Microservices.ThetaFederation.Features.Minting.Storage.Models;
using Beamable.Server;
using MongoDB.Driver;

namespace Beamable.Microservices.ThetaFederation.Features.Minting.Storage
{
    public class MintCollection : IService
{
	private readonly IStorageObjectConnectionProvider _storageObjectConnectionProvider;
	private IMongoCollection<Mint>? _collection;

	public MintCollection(IStorageObjectConnectionProvider storageObjectConnectionProvider)
	{
		_storageObjectConnectionProvider = storageObjectConnectionProvider;
	}

	private async ValueTask<IMongoCollection<Mint>> Get()
	{
		if (_collection is null)
		{
			var db = await _storageObjectConnectionProvider.ThetaStorageDatabase();
			_collection = db.GetCollection<Mint>("mint");
			await _collection.Indexes.CreateManyAsync(new[]
			{
				new CreateIndexModel<Mint>(Builders<Mint>.IndexKeys.Ascending(x => x.ContractName).Ascending(x => x.ContentId).Ascending(x => x.TokenId).Ascending(x => x.TransactionHash), new CreateIndexOptions { Unique = true }),
				new CreateIndexModel<Mint>(Builders<Mint>.IndexKeys.Ascending(x => x.ContractName).Ascending(x => x.TokenId).Ascending(x => x.ContentId)),
				new CreateIndexModel<Mint>(Builders<Mint>.IndexKeys.Ascending(x => x.ContractName).Ascending(x => x.InitialOwnerAddress).Ascending(x => x.ContentId)),
				new CreateIndexModel<Mint>(Builders<Mint>.IndexKeys.Ascending(x => x.TransactionHash))
			});
		}

		return _collection;
	}

	public async Task<List<TokenIdMapping>> GetTokenMappingsForContent(string contractName, IEnumerable<string> contentIds)
	{
		var collection = await Get();
		var mints = await collection.Find(x => x.ContractName == contractName && contentIds.Contains(x.ContentId)).Project(x => new TokenIdMapping
		{
			ContentId = x.ContentId,
			TokenId = x.TokenId
		}).ToListAsync();

		return mints;
	}

	public async Task<bool> DidAccountReceiveToken(string accountAddress, string contentId)
	{
		var collection = await Get();
		return await collection
			.Find(x => x.ContentId == contentId && x.InitialOwnerAddress == accountAddress)
			.AnyAsync();
	}

	public async Task<List<Mint>> GetTokenMints(string contractName, IEnumerable<uint> tokenIds)
	{
		var collection = await Get();
		var mints = await collection
			.Find(x => x.ContractName == contractName && tokenIds.Contains(x.TokenId))
			.ToListAsync();

		return mints;
	}

	public async Task<Mint?> GetTokenMint(string contractName, uint tokenId)
	{
		var collection = await Get();
		return await collection.Find(x => x.ContractName == contractName && tokenId == x.TokenId)
			.FirstOrDefaultAsync();
	}

	public async Task InsertMints(IEnumerable<Mint> mints)
	{
		var collection = await Get();
		var options = new InsertManyOptions
		{
			IsOrdered = false
		};
		await collection.InsertManyAsync(mints, options);
	}

	public async Task SaveMetadata(UpdateMetadataRequest request)
	{
		var collection = await Get();
		await collection.UpdateManyAsync(
			x => x.TokenId == request.TokenId,
			Builders<Mint>.Update.Set(x => x.Metadata, request.Metadata));
	}

	public async Task BulkSaveMetadata(IList<UpdateMetadataRequest> requests)
	{
		var collection = await Get();

		var updates = requests.Select(request => new UpdateManyModel<Mint>(Builders<Mint>.Filter.Eq(x => x.TokenId, request.TokenId), Builders<Mint>.Update.Set(x => x.Metadata, request.Metadata))).ToList();

		await collection.BulkWriteAsync(updates, new BulkWriteOptions
		{
			IsOrdered = false
		});
	}

	public async Task DeleteMintByTransactionHash(string transactionHash)
	{
		if (!string.IsNullOrEmpty(transactionHash))
		{
			var collection = await Get();
			await collection.DeleteManyAsync(x => x.TransactionHash == transactionHash);
		}
	}

	public async Task<long> CountMints(string contractName, string contentId)
	{
		var collection = await Get();
		return await collection.Find(x => x.ContractName == contractName && x.ContentId == contentId)
			.CountDocumentsAsync();
	}
}
}