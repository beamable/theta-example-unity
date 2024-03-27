using System.Linq;
using System.Numerics;
using System.Reflection;
using Beamable.Server;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;

namespace Beamable.Microservices.ThetaFederation.Extensions
{
    public static class MongoExtensions
{
	public static void SetupMongoExtensions(this IServiceInitializer initializer)
	{
		BsonSerializer.RegisterSerializer(new HexBigIntegerSerializer());
		BsonSerializer.RegisterSerializer(new BigIntegerNullableSerializer());
		BsonSerializer.RegisterSerializer(new BigIntegerSerializer());

		// Required for correct transaction log serialization of function messages (polymorphic)
		BsonClassMap.RegisterClassMap<FunctionMessage>(cm =>
		{
			cm.AutoMap();
			cm.SetIsRootClass(true);

			Assembly.GetExecutingAssembly()
				.GetDerivedTypes<FunctionMessage>()
				.ToList()
				.ForEach(cm.AddKnownType);
		});
	}
}

public class HexBigIntegerSerializer : SerializerBase<HexBigInteger?>
{
	public override HexBigInteger? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
	{
		if (context.Reader.CurrentBsonType == BsonType.String)
		{
			var bigIntegerString = context.Reader.ReadString();
			return new HexBigInteger(BigInteger.Parse(bigIntegerString));
		}

		if (context.Reader.CurrentBsonType == BsonType.Null)
		{
			context.Reader.ReadNull();
		}
		return null;
	}

	public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, HexBigInteger? value)
	{
		if (value is not null)
			context.Writer.WriteString(value.Value.ToString());
		else
			context.Writer.WriteNull();
	}
}


public class BigIntegerNullableSerializer : SerializerBase<BigInteger?>
{
	public override BigInteger? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
	{
		if (context.Reader.CurrentBsonType == BsonType.String)
		{
			var bigIntegerString = context.Reader.ReadString();
			return BigInteger.Parse(bigIntegerString);
		}

		if (context.Reader.CurrentBsonType == BsonType.Null)
		{
			context.Reader.ReadNull();
		}
		return null;
	}

	public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, BigInteger? value)
	{
		if (value is not null)
			context.Writer.WriteString(value.Value.ToString());
		else
			context.Writer.WriteNull();
	}
}

public class BigIntegerSerializer : SerializerBase<BigInteger>
{
	public override BigInteger Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
	{
		if (context.Reader.CurrentBsonType == BsonType.String)
		{
			var bigIntegerString = context.Reader.ReadString();
			return BigInteger.Parse(bigIntegerString);
		}

		if (context.Reader.CurrentBsonType == BsonType.Null)
		{
			context.Reader.ReadNull();
		}

		return default;
	}

	public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, BigInteger value)
	{
		context.Writer.WriteString(value.ToString());
	}
}
}