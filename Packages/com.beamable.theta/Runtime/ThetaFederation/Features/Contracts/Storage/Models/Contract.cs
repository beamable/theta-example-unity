using MongoDB.Bson.Serialization.Attributes;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Storage.Models
{
    public record Contract
    {
        [BsonElement("_id")]
        public string Name { get; set; } = null!;
        public string PublicKey { get; set; } = null!;
        public string BaseMetadataUri { get; set; } = null!;
        public string CollectionMetadataUri { get; set; } = null!;
    }
}