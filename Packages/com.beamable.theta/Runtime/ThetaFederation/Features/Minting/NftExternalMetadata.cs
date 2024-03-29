using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common.Api.Inventory;
using Newtonsoft.Json;

namespace Beamable.Microservices.ThetaFederation.Features.Minting
{
    [Serializable]
    public class NftExternalMetadata
    {
        public NftExternalMetadata(Dictionary<string, string> properties)
        {
            foreach (var property in properties)
                if (property.Key.StartsWith("$"))
                    SpecialProperties.Add(property.Key.TrimStart('$'), property.Value);
                else
                    Properties.Add(property.Key, property.Value);
        }

        [JsonExtensionData]
        public Dictionary<string, object> SpecialProperties { get; set; } = new ();

        [JsonProperty("properties")]
        public Dictionary<string, string> Properties { get; set; } = new ();

        public Dictionary<string, string> GetProperties()
        {
            var properties = new Dictionary<string, string>();

            foreach (var data in SpecialProperties) properties.Add($"${data.Key}", data.Value.ToString() ?? "");

            foreach (var property in Properties) properties.Add(property.Key, property.Value);

            return properties;
        }

        public static class SpecialProperty
        {
            public const string Name = "$name";
            public const string Image = "$image";
            public const string Description = "$description";
            public const string Uri = "$uri";
        }

        public void Update(Dictionary<string, string> requestProperties)
        {
            foreach (var property in requestProperties)
            {
                if (property.Key.StartsWith("$"))
                {
                    SpecialProperties[property.Key.TrimStart('$')] = property.Value;
                }
                else
                {
                    Properties[property.Key] = property.Value;
                }

            }
        }

        public IEnumerable<ItemProperty> GetItemProperties()
        {
            return GetProperties().Select(kvp => new ItemProperty { name = kvp.Key.TrimStart('$'), value = kvp.Value });
        }
    }
}