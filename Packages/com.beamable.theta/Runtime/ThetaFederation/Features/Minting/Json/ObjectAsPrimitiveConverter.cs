using System;
using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Beamable.Microservices.ThetaFederation.Features.Minting.Json
{
    public class ObjectAsPrimitiveConverter : JsonConverter
    {
        public ObjectAsPrimitiveConverter() : this(FloatFormat.Double, UnknownNumberFormat.JsonElement, ObjectFormat.Dictionary)
        {
        }

        private ObjectAsPrimitiveConverter(FloatFormat floatFormat, UnknownNumberFormat unknownNumberFormat, ObjectFormat objectFormat)
        {
            FloatFormat = floatFormat;
            UnknownNumberFormat = unknownNumberFormat;
            ObjectFormat = objectFormat;
        }

        private FloatFormat FloatFormat { get; }
        private UnknownNumberFormat UnknownNumberFormat { get; }
        private ObjectFormat ObjectFormat { get; }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() == typeof(object))
            {
                writer.WriteStartObject();
                writer.WriteEndObject();
            }
            else
            {
                var jsonObject = JObject.FromObject(value);
                jsonObject.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return null;
                case JsonToken.Boolean:
                    return reader.ReadAsBoolean();
                case JsonToken.String:
                    return reader.ReadAsString();
                case JsonToken.Integer:
                    return reader.ReadAsInt32();
                case JsonToken.Float:
                    return reader.ReadAsDouble();
                case JsonToken.StartArray:
                {
                    var list = new List<object>();
                    while (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            default:
                                list.Add(ReadJson(reader, typeof(object), existingValue, serializer )!);
                                break;
                            case JsonToken.EndArray:
                                return list;
                        }
                    }
                    throw new JsonException();
                }
                case JsonToken.StartObject:
                {
                    var dict = CreateDictionary();
                    while (reader.Read())
                        switch (reader.TokenType)
                        {
                            case JsonToken.EndObject:
                                return dict;
                            case JsonToken.PropertyName:
                                var key = reader.ReadAsString();
                                reader.Read();
                                dict.Add(key!, ReadJson(reader, typeof(object), existingValue, serializer )!);
                                break;
                            default:
                                throw new JsonException();
                        }
                    throw new JsonException();
                }
                default:
                    throw new JsonException($"Unknown token {reader.TokenType}");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        private IDictionary<string, object> CreateDictionary()
        {
            return ObjectFormat == ObjectFormat.Expando ? new ExpandoObject()! : new Dictionary<string, object>();
        }
    }

    public enum FloatFormat
    {
        Double,
        Decimal
    }

    public enum UnknownNumberFormat
    {
        Error,
        JsonElement
    }

    public enum ObjectFormat
    {
        Expando,
        Dictionary
    }
}