using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Content;
using Beamable.Microservices.ThetaFederation.Features.Contracts;
using Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Minting.Models;
using Beamable.Microservices.ThetaFederation.Features.Minting.Exceptions;
using Beamable.Microservices.ThetaFederation.Features.Minting.Storage;
using Beamable.Microservices.ThetaFederation.Features.Minting.Storage.Models;

namespace Beamable.Microservices.ThetaFederation.Features.Minting
{
    public class MintingService : IService
{
	private readonly ContractProxy _contractProxy;
	private readonly ContentService _contentService;
	private readonly CounterCollection _counterCollection;
	private readonly MetadataService _metadataService;
	private readonly MintCollection _mintCollection;

	public MintingService(MetadataService metadataService, MintCollection mintCollection, CounterCollection counterCollection, ContractProxy contractProxy, ContentService contentService)
	{
		_metadataService = metadataService;
		_mintCollection = mintCollection;
		_counterCollection = counterCollection;
		_contractProxy = contractProxy;
		_contentService = contentService;
	}

	public async Task EnsureTokenIsAvatar(uint tokenId)
	{
		var mint = await _mintCollection.GetTokenMint(ContractService.DefaultContractName, tokenId);

		if (mint is null)
		{
			throw new InvalidRequestException("Can't find token");
		}
	}

	public async Task<bool> DidAccountReceiveToken(string accountAddress, string contentId)
	{
		return await _mintCollection.DidAccountReceiveToken(accountAddress, contentId);
	}

	public async Task SaveMetadata(UpdateMetadataRequest request)
	{
		await _mintCollection.SaveMetadata(request);
	}


	public async Task Mint(string toAddress, IList<MintRequest> requests)
	{

		var nonUniqueContentIds = requests
			.Where(x => !x.IsNft)
			.Select(x => x.ContentId)
			.ToHashSet();

		var existingMints = (await _mintCollection.GetTokenMappingsForContent(ContractService.DefaultContractName, nonUniqueContentIds))
			.ToDictionary(x => x.ContentId, x => x);

		var tokenIds = new List<uint>();
		var tokenAmounts = new List<uint>();
		var tokenMetadataHashes = new List<string>();

		var mints = new List<Mint>();
		foreach (var request in requests)
		{
			var maybeExistingMint = existingMints.GetValueOrDefault(request.ContentId);
			var tokenId = maybeExistingMint switch
			{
				{ } m => m.TokenId,
				_ => await _counterCollection.GetNextCounterValue(ContractService.DefaultContractName)
			};

			var metadata = _metadataService.BuildMetadata(tokenId, request.ContentId, request.Properties);
			var metadataHash = request.IsNft ? await _metadataService.SaveMetadata(metadata) : "";

			tokenAmounts.Add(request.Amount);
			tokenMetadataHashes.Add(metadataHash);
			tokenIds.Add(tokenId);

			mints.Add(new Mint
			{
				ContentId = request.ContentId,
				ContractName = ContractService.DefaultContractName,
				TokenId = tokenId,
				Metadata = metadata,
				InitialOwnerAddress = toAddress
			});
			BeamableLogger.Log("Generated mint: {@mint}", new
			{
				ToAddress = toAddress,
				request.ContentId, request.Amount,
				IsUnique = request.IsNft, TokenId = tokenId,
				MetadataHash = metadataHash
			});
		}

		var functionMessage = new BatchMintFunctionMessage
		{
			To = toAddress,
			TokenIds = tokenIds,
			Amounts = tokenAmounts,
			MetadataHashes = tokenMetadataHashes
		};

		var transactionHash = await _contractProxy.BatchMint(functionMessage);
		mints.ForEach(x => x.TransactionHash = transactionHash);
		await _mintCollection.InsertMints(mints);
	}
}

public class MintRequest
{
	public string ContentId { get; set; } = null!;
	public uint Amount { get; set; }
	public Dictionary<string, string> Properties { get; set; } = new();
	public bool IsNft { get; set; }
}
}