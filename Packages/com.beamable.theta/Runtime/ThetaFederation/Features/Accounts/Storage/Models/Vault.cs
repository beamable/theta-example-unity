using System;
using MongoDB.Bson.Serialization.Attributes;
using Nethereum.KeyStore.Model;

namespace Beamable.Microservices.ThetaFederation.Features.Accounts.Storage.Models
{
    public record Vault
    {
        [BsonElement("_id")]
        public string Name { get; set; } = null!;
        public KeyStore<ScryptParams> Value { get; set; } = null!;
        public DateTime Created { get; set; } = DateTime.Now;
    }
}