using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Beamable.Microservices.ThetaFederation.Features.Accounts.Storage.Models
{
    public class Nonce
    {
        [BsonElement("_id")]
        public string Name { get; set; } = null!;

        public long State { get; set; }

        public HashSet<long> Errors { get; set; } = new();
    }
}