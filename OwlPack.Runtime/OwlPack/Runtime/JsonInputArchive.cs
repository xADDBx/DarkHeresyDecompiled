using System;
using System.IO;
using System.Text.Json;

namespace OwlPack.Runtime;

public class JsonInputArchive : IInputArchive, ITestInputArchive
{
	protected byte[] m_Data;

	protected int m_TypesPosition;

	protected int m_DataPosition;

	private byte m_Version;

	public Action<TypeLibrary> OnTypeLibraryDeserialize { get; set; }

	public JsonInputArchive(Stream stream)
	{
		m_Data = new byte[stream.Length];
		stream.Read(m_Data, 0, m_Data.Length);
		Construct();
	}

	private void Construct()
	{
		Utf8JsonReader utf8JsonReader = new Utf8JsonReader(m_Data);
		utf8JsonReader.Read();
		if (utf8JsonReader.TokenType != JsonTokenType.StartArray)
		{
			throw new InvalidDataException($"Expecting JSON data to contain an array at the top level, got {utf8JsonReader.TokenType}");
		}
		utf8JsonReader.Read();
		if (utf8JsonReader.TokenType != JsonTokenType.Number)
		{
			throw new InvalidDataException($"Expecting the first member of JSON array to be serializer version number, got {utf8JsonReader.TokenType}");
		}
		m_Version = utf8JsonReader.GetByte();
		if (m_Version > 3)
		{
			throw new InvalidDataException($"Serialized version {m_Version} is newer than current version {(byte)3}");
		}
		utf8JsonReader.Read();
		if (utf8JsonReader.TokenType != JsonTokenType.StartObject)
		{
			throw new InvalidDataException($"Expecting the second member of JSON array to be an object containing serialized TypeLibrary, got {utf8JsonReader.TokenType}");
		}
		m_TypesPosition = (int)utf8JsonReader.BytesConsumed - 1;
		utf8JsonReader.Skip();
		if (utf8JsonReader.TokenType != JsonTokenType.EndObject)
		{
			throw new InvalidDataException($"TypeLibrary object doesn't end properly, expecting EndObject, got {utf8JsonReader.TokenType}");
		}
		utf8JsonReader.Read();
		if (utf8JsonReader.TokenType != JsonTokenType.StartObject)
		{
			throw new InvalidDataException($"The third member of JSON array should be serialized object, got {utf8JsonReader.TokenType}");
		}
		m_DataPosition = (int)utf8JsonReader.BytesConsumed - 1;
	}

	public JsonInputArchive(byte[] bytes)
	{
		m_Data = bytes;
		Construct();
	}

	public T Deserialize<T>()
	{
		TypeLibrary typeLibrary = Serializer.DeserializeObject<TypeLibrary>(new JsonInputFormatter(m_Data, m_TypesPosition), new DeserializerState(m_Version, new TypeLibrary()));
		typeLibrary.UpdateWithOldNames(TypeLibrary.OldNames);
		if (OnTypeLibraryDeserialize != null)
		{
			OnTypeLibraryDeserialize(typeLibrary);
		}
		return Serializer.DeserializeObject<T>(new JsonInputFormatter(m_Data, m_DataPosition), new DeserializerState(m_Version, typeLibrary));
	}
}
