using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace OwlPack.Runtime;

public class BinaryInputFormatter : IInputFormatter
{
	private BinaryReader m_Reader;

	private Stack<int> m_ArrayIterators = new Stack<int>();

	private const ushort NullableBit = 32768;

	private const byte EnumKind_Name = 0;

	private const byte EnumKind_Flags = 1;

	public BinaryInputFormatter(BinaryReader reader)
	{
		m_Reader = reader;
	}

	public IOutputFormatter.ObjectKind ReadObjectHeader(out uint objectId, out ushort type)
	{
		byte b = m_Reader.ReadByte();
		switch (b)
		{
		case 0:
			objectId = 0u;
			type = 0;
			return IOutputFormatter.ObjectKind.Null;
		case 1:
			objectId = m_Reader.ReadUInt32();
			type = 0;
			return IOutputFormatter.ObjectKind.Ref;
		case 2:
			objectId = m_Reader.ReadUInt32();
			type = m_Reader.ReadUInt16();
			return IOutputFormatter.ObjectKind.Object;
		default:
			throw new InputFormatterException($"Object header contains invalid value {b}. Should be IOutputFormatter.ObjectKind.");
		}
	}

	public void EnterObject()
	{
	}

	public void LeaveObject()
	{
	}

	private int Read7BitEncodedInt()
	{
		int num = 0;
		int num2 = 0;
		byte b;
		do
		{
			if (num2 == 35)
			{
				throw new InputFormatterException("Bad format for 7-bit encoded int");
			}
			b = m_Reader.ReadByte();
			num |= (b & 0x7F) << num2;
			num2 += 7;
		}
		while ((b & 0x80u) != 0);
		return num;
	}

	public void ReadFieldHeader(TypeInfo serializedTypeInfo, out byte fieldID, out int size)
	{
		fieldID = m_Reader.ReadByte();
		size = Read7BitEncodedInt();
	}

	public void EnterArray(out int count)
	{
		count = m_Reader.ReadInt32();
		m_ArrayIterators.Push(count);
	}

	public bool NextArrayElement()
	{
		if (m_ArrayIterators.Peek() == 0)
		{
			return false;
		}
		int num = m_ArrayIterators.Pop();
		num--;
		m_ArrayIterators.Push(num);
		return true;
	}

	public void LeaveArray()
	{
		m_ArrayIterators.Pop();
	}

	public void EnterTuple(out byte count)
	{
		count = m_Reader.ReadByte();
		m_ArrayIterators.Push(count);
	}

	public bool NextTupleElement()
	{
		return NextArrayElement();
	}

	public void LeaveTuple()
	{
		LeaveArray();
	}

	private T ReadUnmanagedInternal<T>(ushort currentTypeID)
	{
		switch (currentTypeID)
		{
		case 0:
		{
			byte source12 = m_Reader.ReadByte();
			return Unsafe.As<byte, T>(ref source12);
		}
		case 1:
		{
			sbyte source11 = m_Reader.ReadSByte();
			return Unsafe.As<sbyte, T>(ref source11);
		}
		case 2:
		{
			short source10 = m_Reader.ReadInt16();
			return Unsafe.As<short, T>(ref source10);
		}
		case 3:
		{
			ushort source9 = m_Reader.ReadUInt16();
			return Unsafe.As<ushort, T>(ref source9);
		}
		case 4:
		{
			int source8 = m_Reader.ReadInt32();
			return Unsafe.As<int, T>(ref source8);
		}
		case 5:
		{
			uint source7 = m_Reader.ReadUInt32();
			return Unsafe.As<uint, T>(ref source7);
		}
		case 6:
		{
			long source6 = m_Reader.ReadInt64();
			return Unsafe.As<long, T>(ref source6);
		}
		case 7:
		{
			ulong source5 = m_Reader.ReadUInt64();
			return Unsafe.As<ulong, T>(ref source5);
		}
		case 8:
		{
			float source4 = m_Reader.ReadSingle();
			return Unsafe.As<float, T>(ref source4);
		}
		case 9:
		{
			double source3 = m_Reader.ReadDouble();
			return Unsafe.As<double, T>(ref source3);
		}
		case 10:
		{
			char source2 = (char)m_Reader.ReadInt16();
			return Unsafe.As<char, T>(ref source2);
		}
		case 11:
		{
			bool source = m_Reader.ReadByte() != 0;
			return Unsafe.As<bool, T>(ref source);
		}
		default:
			throw new InputFormatterException($"Unmanaged type {currentTypeID} is not supported by BinaryInputFormatter");
		}
	}

	public void SkipField(int size)
	{
		m_Reader.ReadBytes(size);
	}

	private T TrySafeConvert<T>(ushort serializedTypeID, ushort currentTypeID) where T : unmanaged
	{
		return serializedTypeID switch
		{
			0 => Converters.TrySafeConvert<T, byte>(ReadUnmanagedInternal<byte>(serializedTypeID)), 
			1 => Converters.TrySafeConvert<T, sbyte>(ReadUnmanagedInternal<sbyte>(serializedTypeID)), 
			3 => Converters.TrySafeConvert<T, ushort>(ReadUnmanagedInternal<ushort>(serializedTypeID)), 
			2 => Converters.TrySafeConvert<T, short>(ReadUnmanagedInternal<short>(serializedTypeID)), 
			5 => Converters.TrySafeConvert<T, uint>(ReadUnmanagedInternal<uint>(serializedTypeID)), 
			4 => Converters.TrySafeConvert<T, int>(ReadUnmanagedInternal<int>(serializedTypeID)), 
			8 => Converters.TrySafeConvert<T, float>(ReadUnmanagedInternal<float>(serializedTypeID)), 
			_ => throw new InvalidCastException($"Conversion from {serializedTypeID} to {typeof(T)} not supported"), 
		};
	}

	public T ReadUnmanaged<T>(DeserializerState state) where T : unmanaged
	{
		ushort typeID = state.TypeLibrary.GetTypeID<T>();
		ushort num = m_Reader.ReadUInt16();
		if ((num & 0x8000u) != 0)
		{
			num = (ushort)(num & 0xFFFF7FFFu);
			if (num != typeID)
			{
				return TrySafeConvert<T>(num, typeID);
			}
			throw new InputFormatterException($"Expecting non-nullable unmanaged type {typeID}, serialized type is nullable");
		}
		if (num != typeID)
		{
			return TrySafeConvert<T>(num, typeID);
		}
		return ReadUnmanagedInternal<T>(typeID);
	}

	public T? ReadNullableUnmanaged<T>(DeserializerState state) where T : unmanaged
	{
		if (m_Reader.ReadByte() == 0)
		{
			return null;
		}
		ushort typeID = state.TypeLibrary.GetTypeID<T>();
		ushort num = m_Reader.ReadUInt16();
		if ((num & 0x8000) == 0)
		{
			throw new InputFormatterException($"Expecting nullable unmanaged type {typeID}, serialized type is non-nullable");
		}
		num = (ushort)(num & 0xFFFF7FFFu);
		if (num != typeID)
		{
			throw new InputFormatterException($"Expecting unmanaged type {typeID}, serialized type {num}");
		}
		return ReadUnmanagedInternal<T>(typeID);
	}

	public string ReadString(DeserializerState state)
	{
		ushort num = m_Reader.ReadUInt16();
		if (num != 12)
		{
			throw new InputFormatterException($"Expecting string type {(ushort)12}, serialized type {num}");
		}
		if (m_Reader.ReadByte() == 0)
		{
			return null;
		}
		return m_Reader.ReadString();
	}

	public T ReadEnum<T>(DeserializerState state) where T : Enum
	{
		ushort num = m_Reader.ReadUInt16();
		if ((num & 0x8000u) != 0)
		{
			num = (ushort)(num & 0xFFFF7FFFu);
			if (num != 13)
			{
				throw new InputFormatterException($"Expecting enum type {(ushort)13}, serialized type {num}");
			}
			throw new InputFormatterException("Expecting non-nullable unmanaged enum type, serialized type is nullable");
		}
		if (((state.Version >= 3) ? m_Reader.ReadByte() : 0) == 1)
		{
			return EnumHelper.FromInt64<T>(m_Reader.ReadInt64());
		}
		string text = m_Reader.ReadString();
		if (EnumHelper.TryParse<T>(typeof(T), text, out T result))
		{
			return result;
		}
		throw new InputFormatterException("Cannot parse enum value " + text + " in enum " + typeof(T).FullName);
	}

	public T? ReadNullableEnum<T>(DeserializerState state) where T : struct, Enum
	{
		ushort num = m_Reader.ReadUInt16();
		if ((num & 0x8000) == 0)
		{
			throw new InputFormatterException("Expecting nullable enum type, serialized type is non-nullable");
		}
		num = (ushort)(num & 0xFFFF7FFFu);
		if (num != 13)
		{
			throw new InputFormatterException($"Expecting enum type {(ushort)13}, serialized type {num}");
		}
		byte b = (byte)((state.Version >= 3) ? m_Reader.ReadByte() : 0);
		if (m_Reader.ReadByte() == 0)
		{
			return null;
		}
		if (b == 1)
		{
			return EnumHelper.FromInt64<T>(m_Reader.ReadInt64());
		}
		string text = m_Reader.ReadString();
		if (EnumHelper.TryParse<T>(typeof(T), text, out T result))
		{
			return result;
		}
		throw new InputFormatterException("Cannot parse enum value " + text + " in enum " + typeof(T).FullName);
	}

	public T ReadPackable<T>(DeserializerState state)
	{
		return Serializer.DeserializeObject<T>(this, state);
	}

	public T? ReadNullablePackable<T>(DeserializerState state) where T : struct
	{
		return Serializer.DeserializeNullableStruct<T>(this, state);
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
}
