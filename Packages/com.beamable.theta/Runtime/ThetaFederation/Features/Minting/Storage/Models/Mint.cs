using MongoDB.Bson.Serialization.Attributes;

namespace Beamable.Microservices.ThetaFederation.Features.Minting.Storage.Models
{
    [BsonIgnoreExtraElements]
    public record Mint : TokenIdMapping
    {
        public string ContractName { get; set; } = null!;
        public string TransactionHash { get; set; } = null!;
        public NftExternalMetadata Metadata { get; set; } = null!;
        public string InitialOwnerAddress { get; set; } = null!;

        [BsonIgnore]
        public bool IsNft { get; set; }
    }
}