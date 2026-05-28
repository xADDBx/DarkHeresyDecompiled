using System;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OwlPack.Runtime;

public class JsonInputFormatter : IInputFormatter
{
	private ref struct MyReader
	{
		private Utf8JsonReader m_Reader;

		public JsonTokenType TokenType => m_Reader.TokenType;

		public JsonReaderState CurrentState => m_Reader.CurrentState;

		public MyReader(byte[] data, int position, JsonReaderState state)
		{
			m_Reader = new Utf8JsonReader(new ReadOnlySpan<byte>(data, position, data.Length - position), isFinalBlock: true, state);
		}

		public void Read(ref int position)
		{
			long bytesConsumed = m_Reader.BytesConsumed;
			if (!m_Reader.Read())
			{
				throw new InputFormatterException("Read past the end");
			}
			position += (int)(m_Reader.BytesConsumed - bytesConsumed);
		}

		public string ReadPropertyName(ref int position)
		{
			long bytesConsumed = m_Reader.BytesConsumed;
			if (!m_Reader.Read())
			{
				throw new InputFormatterException("Read past the end");
			}
			position += (int)(m_Reader.BytesConsumed - bytesConsumed);
			return m_Reader.GetString();
		}

		public string ReadString(ref int position)
		{
			long bytesConsumed = m_Reader.BytesConsumed;
			if (!m_Reader.Read())
			{
				throw new InputFormatterException("Read past the end");
			}
			position += (int)(m_Reader.BytesConsumed - bytesConsumed);
			return m_Reader.GetString();
		}

		public byte ReadByte(ref int position)
		{
			long bytesConsumed = m_Reader.BytesConsumed;
			if (!m_Reader.Read())
			{
				throw new InputFormatterException("Read past the end");
			}
			position += (int)(m_Reader.BytesConsumed - bytesConsumed);
			return m_Reader.GetByte();
		}

		public sbyte ReadSByte(ref int position)
		{
			long bytesConsumed = m_Reader.BytesConsumed;
			if (!m_Reader.Read())
			{
				throw new InputFormatterException("Read past the end");
			}
			position += (int)(m_Reader.BytesConsumed - bytesConsumed);
			return m_Reader.GetSByte();
		}

		public short ReadInt16(ref int position)
		{
			long bytesConsumed = m_Reader.BytesConsumed;
			if (!m_Reader.Read())
			{
				throw new InputFormatterException("Read past the end");
			}
			position += (int)(m_Reader.BytesConsumed - bytesConsumed);
			return m_Reader.GetInt16();
		}

		public ushort ReadUInt16(ref int position)
		{
			long bytesConsumed = m_Reader.BytesConsumed;
			if (!m_Reader.Read())
			{
				throw new InputFormatterException("Read past the end");
			}
			position += (int)(m_Reader.BytesConsumed - bytesConsumed);
			return m_Reader.GetUInt16();
		}

		public int ReadInt32(ref int position)
		{
			long bytesConsumed = m_Reader.BytesConsumed;
			if (!m_Reader.Read())
			{
				throw new InputFormatterException("Read past the end");
			}
			position += (int)(m_Reader.BytesConsumed - bytesConsumed);
			return m_Reader.GetInt32();
		}

		public uint ReadUInt32(ref int position)
		{
			long bytesConsumed = m_Reader.BytesConsumed;
			if (!m_Reader.Read())
			{
				throw new InputFormatterException("Read past the end");
			}
			position += (int)(m_Reader.BytesConsumed - bytesConsumed);
			return m_Reader.GetUInt32();
		}

		public long ReadInt64(ref int position)
		{
			long bytesConsumed = m_Reader.BytesConsumed;
			if (!m_Reader.Read())
			{
				throw new InputFormatterException("Read past the end");
			}
			position += (int)(m_Reader.BytesConsumed - bytesConsumed);
			return m_Reader.GetInt64();
		}

		public ulong ReadUInt64(ref int position)
		{
			long bytesConsumed = m_Reader.BytesConsumed;
			if (!m_Reader.Read())
			{
				throw new InputFormatterException("Read past the end");
			}
			position += (int)(m_Reader.BytesConsumed - bytesConsumed);
			return m_Reader.GetUInt64();
		}

		public float ReadSingle(ref int position)
		{
			long bytesConsumed = m_Reader.BytesConsumed;
			if (!m_Reader.Read())
			{
				throw new InputFormatterException("Read past the end");
			}
			position += (int)(m_Reader.BytesConsumed - bytesConsumed);
			return m_Reader.GetSingle();
		}

		public double ReadDouble(ref int position)
		{
			long bytesConsumed = m_Reader.BytesConsumed;
			if (!m_Reader.Read())
			{
				throw new InputFormatterException("Read past the end");
			}
			position += (int)(m_Reader.BytesConsumed - bytesConsumed);
			return m_Reader.GetDouble();
		}

		public char ReadChar(ref int position)
		{
			long bytesConsumed = m_Reader.BytesConsumed;
			if (!m_Reader.Read())
			{
				throw new InputFormatterException("Read past the end");
			}
			position += (int)(m_Reader.BytesConsumed - bytesConsumed);
			return (char)m_Reader.GetUInt16();
		}

		public bool ReadBool(ref int position)
		{
			long bytesConsumed = m_Reader.BytesConsumed;
			if (!m_Reader.Read())
			{
				throw new InputFormatterException("Read past the end");
			}
			position += (int)(m_Reader.BytesConsumed - bytesConsumed);
			return m_Reader.GetBoolean();
		}

		public void Skip(ref int position)
		{
			long bytesConsumed = m_Reader.BytesConsumed;
			m_Reader.Skip();
			position += (int)(m_Reader.BytesConsumed - bytesConsumed);
		}

		public bool TryReadNull(ref int position)
		{
			Utf8JsonReader reader = m_Reader;
			long bytesConsumed = m_Reader.BytesConsumed;
			if (!m_Reader.Read())
			{
				throw new InputFormatterException("Read past the end");
			}
			if (m_Reader.TokenType == JsonTokenType.Null)
			{
				position += (int)(m_Reader.BytesConsumed - bytesConsumed);
				return true;
			}
			m_Reader = reader;
			return false;
		}
	}

	private byte[] m_Data;

	private int m_Position;

	private JsonReaderState m_State;

	public JsonInputFormatter(byte[] data, int position)
	{
		m_Data = data;
		m_Position = position;
	}

	public IOutputFormatter.ObjectKind ReadObjectHeader(out uint objectId, out ushort type)
	{
		MyReader myReader = new MyReader(m_Data, m_Position, m_State);
		myReader.Read(ref m_Position);
		if (myReader.TokenType == JsonTokenType.Null)
		{
			objectId = 0u;
			type = 0;
			m_State = myReader.CurrentState;
			return IOutputFormatter.ObjectKind.Null;
		}
		if (myReader.TokenType != JsonTokenType.StartObject)
		{
			throw new InputFormatterException($"Excepting object, but current element is {myReader.TokenType}. At: {m_Position}");
		}
		string text = myReader.ReadPropertyName(ref m_Position);
		if (text == "$ref")
		{
			objectId = myReader.ReadUInt32(ref m_Position);
			myReader.Read(ref m_Position);
			if (myReader.TokenType != JsonTokenType.EndObject)
			{
				throw new InputFormatterException($"Reference to object should contain only the \"$ref\" : objectID field, and nothing else. At: {m_Position}");
			}
			type = 0;
			m_State = myReader.CurrentState;
			return IOutputFormatter.ObjectKind.Ref;
		}
		if (text == "$id")
		{
			objectId = myReader.ReadUInt32(ref m_Position);
			text = myReader.ReadPropertyName(ref m_Position);
			if (text != "$type")
			{
				throw new Exception($"First property in an object should be \"$id\", got \"{text}\" instead. At: {m_Position}");
			}
			type = myReader.ReadUInt16(ref m_Position);
			text = myReader.ReadPropertyName(ref m_Position);
			if (text != "$typeName")
			{
				throw new Exception($"Second property in an object should be \"$typeName\", got \"{text}\" instead. At: {m_Position}");
			}
			myReader.ReadString(ref m_Position);
			m_State = myReader.CurrentState;
			return IOutputFormatter.ObjectKind.Object;
		}
		throw new InputFormatterException($"Object doesn't contain either a \"$ref\" property, or a combination of \"$id\" and \"$type\". At {myReader.CurrentState}");
	}

	public void EnterObject()
	{
	}

	public void LeaveObject()
	{
		MyReader myReader = new MyReader(m_Data, m_Position, m_State);
		myReader.Read(ref m_Position);
		if (myReader.TokenType != JsonTokenType.EndObject)
		{
			throw new Exception($"Expecting end of object, got {myReader.TokenType} instead. At: {m_Position}");
		}
		m_State = myReader.CurrentState;
	}

	public void ReadFieldHeader(TypeInfo serializedTypeInfo, out byte fieldID, out int size)
	{
		size = 0;
		MyReader myReader = new MyReader(m_Data, m_Position, m_State);
		string text = myReader.ReadPropertyName(ref m_Position);
		for (byte b = 0; b < (byte)serializedTypeInfo.Fields.Length; b++)
		{
			if (serializedTypeInfo.Fields[b].Name == text)
			{
				fieldID = b;
				m_State = myReader.CurrentState;
				return;
			}
			if (serializedTypeInfo.Fields[b].OldNames != null)
			{
				string[] oldNames = serializedTypeInfo.Fields[b].OldNames;
				for (int i = 0; i < oldNames.Length; i++)
				{
					if (oldNames[i] == text)
					{
						fieldID = b;
						m_State = myReader.CurrentState;
						return;
					}
				}
			}
		}
		throw new InputFormatterException("Field with name " + text + " not found in serialized type " + serializedTypeInfo.Name);
	}

	public void EnterArray(out int count)
	{
		MyReader myReader = new MyReader(m_Data, m_Position, m_State);
		if (myReader.ReadPropertyName(ref m_Position) != "$count")
		{
			throw new InputFormatterException("count");
		}
		count = myReader.ReadInt32(ref m_Position);
		myReader.ReadPropertyName(ref m_Position);
		myReader.ReadString(ref m_Position);
		if (myReader.ReadPropertyName(ref m_Position) != "$elements")
		{
			throw new InputFormatterException("elements");
		}
		myReader.Read(ref m_Position);
		if (myReader.TokenType != JsonTokenType.StartArray)
		{
			throw new InputFormatterException("StartArray");
		}
		m_State = myReader.CurrentState;
	}

	public bool NextArrayElement()
	{
		if (new MyReader(m_Data, m_Position, m_State).TokenType == JsonTokenType.EndArray)
		{
			return false;
		}
		return true;
	}

	public void LeaveArray()
	{
		MyReader myReader = new MyReader(m_Data, m_Position, m_State);
		myReader.Read(ref m_Position);
		if (myReader.TokenType != JsonTokenType.EndArray)
		{
			throw new Exception($"Expecting end of array, got {myReader.TokenType} instead. At: {m_Position}");
		}
		myReader.Read(ref m_Position);
		if (myReader.TokenType != JsonTokenType.EndObject)
		{
			throw new Exception($"Expecting end of object, got {myReader.TokenType} instead. At: {m_Position}");
		}
		m_State = myReader.CurrentState;
	}

	public void EnterTuple(out byte count)
	{
		MyReader myReader = new MyReader(m_Data, m_Position, m_State);
		myReader.Read(ref m_Position);
		if (myReader.TokenType != JsonTokenType.StartObject)
		{
			throw new InputFormatterException("StartObject");
		}
		if (myReader.ReadPropertyName(ref m_Position) != "$count")
		{
			throw new InputFormatterException("count");
		}
		count = myReader.ReadByte(ref m_Position);
		if (myReader.ReadPropertyName(ref m_Position) != "$elements")
		{
			throw new InputFormatterException("elements");
		}
		myReader.Read(ref m_Position);
		if (myReader.TokenType != JsonTokenType.StartArray)
		{
			throw new InputFormatterException("StartArray");
		}
		m_State = myReader.CurrentState;
	}

	public bool NextTupleElement()
	{
		if (new MyReader(m_Data, m_Position, m_State).TokenType == JsonTokenType.EndArray)
		{
			return false;
		}
		return true;
	}

	public void LeaveTuple()
	{
		MyReader myReader = new MyReader(m_Data, m_Position, m_State);
		myReader.Read(ref m_Position);
		if (myReader.TokenType != JsonTokenType.EndArray)
		{
			throw new Exception($"Expecting end of array, got {myReader.TokenType} instead. At: {m_Position}");
		}
		myReader.Read(ref m_Position);
		if (myReader.TokenType != JsonTokenType.EndObject)
		{
			throw new Exception($"Expecting end of object, got {myReader.TokenType} instead. At: {m_Position}");
		}
		m_State = myReader.CurrentState;
	}

	private T ReadUnmanagedInternal<T>(ref MyReader reader, ushort currentTypeID)
	{
		switch (currentTypeID)
		{
		case 0:
		{
			byte source12 = reader.ReadByte(ref m_Position);
			return Unsafe.As<byte, T>(ref source12);
		}
		case 1:
		{
			sbyte source11 = reader.ReadSByte(ref m_Position);
			return Unsafe.As<sbyte, T>(ref source11);
		}
		case 2:
		{
			short source10 = reader.ReadInt16(ref m_Position);
			return Unsafe.As<short, T>(ref source10);
		}
		case 3:
		{
			ushort source9 = reader.ReadUInt16(ref m_Position);
			return Unsafe.As<ushort, T>(ref source9);
		}
		case 4:
		{
			int source8 = reader.ReadInt32(ref m_Position);
			return Unsafe.As<int, T>(ref source8);
		}
		case 5:
		{
			uint source7 = reader.ReadUInt32(ref m_Position);
			return Unsafe.As<uint, T>(ref source7);
		}
		case 6:
		{
			long source6 = reader.ReadInt64(ref m_Position);
			return Unsafe.As<long, T>(ref source6);
		}
		case 7:
		{
			ulong source5 = reader.ReadUInt64(ref m_Position);
			return Unsafe.As<ulong, T>(ref source5);
		}
		case 8:
		{
			float source4 = (float)reader.ReadDouble(ref m_Position);
			return Unsafe.As<float, T>(ref source4);
		}
		case 9:
		{
			double source3 = reader.ReadDouble(ref m_Position);
			return Unsafe.As<double, T>(ref source3);
		}
		case 10:
		{
			char source2 = reader.ReadChar(ref m_Position);
			return Unsafe.As<char, T>(ref source2);
		}
		case 11:
		{
			bool source = reader.ReadBool(ref m_Position);
			return Unsafe.As<bool, T>(ref source);
		}
		default:
			throw new InputFormatterException($"Unmanaged type {currentTypeID} is not supported by BinaryInputFormatter");
		}
	}

	public T ReadUnmanaged<T>(DeserializerState state) where T : unmanaged
	{
		ushort typeID = state.TypeLibrary.GetTypeID<T>();
		MyReader reader = new MyReader(m_Data, m_Position, m_State);
		T result = ReadUnmanagedInternal<T>(ref reader, typeID);
		m_State = reader.CurrentState;
		return result;
	}

	public T? ReadNullableUnmanaged<T>(DeserializerState state) where T : unmanaged
	{
		ushort typeID = state.TypeLibrary.GetTypeID<T>();
		MyReader reader = new MyReader(m_Data, m_Position, m_State);
		if (reader.TryReadNull(ref m_Position))
		{
			m_State = reader.CurrentState;
			return null;
		}
		reader = new MyReader(m_Data, m_Position, m_State);
		T value = ReadUnmanagedInternal<T>(ref reader, typeID);
		m_State = reader.CurrentState;
		return value;
	}

	public string ReadString(DeserializerState state)
	{
		MyReader myReader = new MyReader(m_Data, m_Position, m_State);
		string result = myReader.ReadString(ref m_Position);
		m_State = myReader.CurrentState;
		return result;
	}

	public T ReadEnum<T>(DeserializerState state) where T : Enum
	{
		if (state.Version >= 3 && FlagsHelper<T>.IsFlags)
		{
			MyReader myReader = new MyReader(m_Data, m_Position, m_State);
			long value = myReader.ReadInt64(ref m_Position);
			m_State = myReader.CurrentState;
			return EnumHelper.FromInt64<T>(value);
		}
		string text = ReadString(state);
		if (EnumHelper.TryParse<T>(typeof(T), text, out T result))
		{
			return result;
		}
		throw new InputFormatterException("Cannot parse enum value " + text + " in enum " + typeof(T).FullName);
	}

	public T? ReadNullableEnum<T>(DeserializerState state) where T : struct, Enum
	{
		MyReader myReader = new MyReader(m_Data, m_Position, m_State);
		if (myReader.TryReadNull(ref m_Position))
		{
			m_State = myReader.CurrentState;
			return null;
		}
		return ReadEnum<T>(state);
	}

	public T ReadPackable<T>(DeserializerState state)
	{
		return Serializer.DeserializeObject<T>(this, state);
	}

	public T? ReadNullablePackable<T>(DeserializerState state) where T : struct
	{
		MyReader myReader = new MyReader(m_Data, m_Position, m_State);
		if (myReader.TryReadNull(ref m_Position))
		{
			m_State = myReader.CurrentState;
			return null;
		}
		return Serializer.DeserializeObject<T>(this, state);
	}

	public T ReadUnmanagedArrayElement<T>(DeserializerState state) where T : unmanaged
	{
		return ReadUnmanaged<T>(state);
	}

	public T? ReadNullableUnmanagedArrayElement<T>(DeserializerState state) where T : unmanaged
	{
		return ReadNullableUnmanaged<T>(state);
	}

	public string ReadStringArrayElement(DeserializerState state)
	{
		return ReadString(state);
	}

	public T ReadEnumArrayElement<T>(DeserializerState state) where T : Enum
	{
		return ReadEnum<T>(state);
	}

	public T? ReadNullableEnumArrayElement<T>(DeserializerState state) where T : struct, Enum
	{
		return ReadNullableEnum<T>(state);
	}

	public T ReadPackableArrayElement<T>(DeserializerState state)
	{
		return ReadPackable<T>(state);
	}

	public T? ReadNullablePackableArrayElement<T>(DeserializerState state) where T : struct
	{
		return ReadNullablePackable<T>(state);
	}

	public void SkipField(int size)
	{
		MyReader myReader = new MyReader(m_Data, m_Position, m_State);
		myReader.Skip(ref m_Position);
		m_State = myReader.CurrentState;
	}
}
