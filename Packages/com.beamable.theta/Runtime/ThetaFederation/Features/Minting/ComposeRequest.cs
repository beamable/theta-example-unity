using System.Collections.Generic;
using Newtonsoft.Json;

namespace Beamable.Microservices.ThetaFederation.Features.Minting
{
    public record ComposeRequest
    {
        public uint TokenId { get; set; }
        public string ContentId { get; }
        public List<uint> AttachIds { get; }
        public List<uint> DetachIds { get; }
        public HashSet<string> CustomizationOptions { get; }

        [JsonIgnore]
        public HashSet<uint> ResolvedAttachments { get; }
        [JsonIgnore]
        public List<OptionMetadata> ResolvedOptions { get; }

        public string Serialize() => JsonConvert.SerializeObject(this);

        public ComposeRequest(uint tokenId, List<uint> attachIds, List<uint> detachIds, HashSet<string> customizationOptions, List<OptionMetadata> resolvedOptions, string contentId, HashSet<uint> resolvedAttachments)
        {
            TokenId = tokenId;
            AttachIds = attachIds;
            DetachIds = detachIds;
            CustomizationOptions = customizationOptions;
            ResolvedOptions = resolvedOptions;
            ContentId = contentId;
            ResolvedAttachments = resolvedAttachments;
        }
    }
}