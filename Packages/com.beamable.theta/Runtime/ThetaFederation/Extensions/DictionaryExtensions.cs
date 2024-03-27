using System.Collections.Generic;

namespace Beamable.Microservices.ThetaFederation.Extensions
{
    public static class DictionaryExtensions
{
    public static Dictionary<string, string> Flatten(this IDictionary<string, object> nestedData, string levelSeparator)
    {
        void FlattenRecursive(IDictionary<string, object> nestedProperties, string prefix, IDictionary<string, string> flatData)
        {
            foreach (var key in nestedProperties.Keys)
            {
                var value = nestedProperties[key];

                if (value is Dictionary<string, object> objects)
                {
                    var nestedPrefix = string.IsNullOrEmpty(prefix) ? key : $"{prefix}{levelSeparator}{key}";
                    FlattenRecursive(objects, nestedPrefix, flatData);
                }
                else
                {
                    var flatKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}{levelSeparator}{key}";
                    flatData[flatKey] = value?.ToString() ?? "";
                }
            }
        }

        var result = new Dictionary<string, string>();
        FlattenRecursive(nestedData, "", result);
        return result;
    }

    public static Dictionary<string, object> Nest(this IDictionary<string, object> flatData, string levelSeparator)
    {
        void AddNested(Dictionary<string, object> transformedData, string key, object value)
        {
            var parts = key.Split(levelSeparator);
            var propertyName = parts[0];

            if (parts.Length > 1)
            {
                var subPropertyName = string.Join(levelSeparator, parts, 1, parts.Length - 1);

                if (!transformedData.ContainsKey(propertyName)) transformedData[propertyName] = new Dictionary<string, object>();

                var subPropertyDict = (Dictionary<string, object>)transformedData[propertyName];
                AddNested(subPropertyDict, subPropertyName, value);
            }
            else
            {
                transformedData[propertyName] = value;
            }
        }

        var nested = new Dictionary<string, object>();

        foreach (var key in flatData.Keys) AddNested(nested, key, flatData[key]);

        return nested;
    }
}
}