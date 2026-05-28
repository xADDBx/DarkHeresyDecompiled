using System;

namespace OwlPack.Runtime;

[Obsolete]
public static class StandardSerializer
{
	public static BinaryOutputArchive<ArrayMemoryBufferWriter> SerializeToBinary<T>(ref T? value) where T : IOwlPackable, IOwlPackable<T>
	{
		return Serializer.Serialize<BinaryOutputArchive<ArrayMemoryBufferWriter>, T>(ref value);
	}

	public static BinaryOutputArchive<ArrayMemoryBufferWriter> SerializeAnyToBinary<T>(ref T? value)
	{
		return Serializer.SerializeAny<BinaryOutputArchive<ArrayMemoryBufferWriter>, T>(ref value);
	}

	public static T DeserializeFromBinary<T>(BinaryInputArchive archive)
	{
		return Serializer.Deserialize<BinaryInputArchive, T>(archive);
	}

	public static JsonOutputArchive<ArrayMemoryBufferWriter> SerializeToJson<T>(ref T? value) where T : IOwlPackable, IOwlPackable<T>
	{
		return Serializer.Serialize<JsonOutputArchive<ArrayMemoryBufferWriter>, T>(ref value);
	}

	public static JsonOutputArchive<ArrayMemoryBufferWriter> SerializeAnyToJson<T>(ref T? value)
	{
		return Serializer.SerializeAny<JsonOutputArchive<ArrayMemoryBufferWriter>, T>(ref value);
	}

	public static T DeserializeFromJson<T>(JsonInputArchive archive)
	{
		return Serializer.Deserialize<JsonInputArchive, T>(archive);
	}
}
