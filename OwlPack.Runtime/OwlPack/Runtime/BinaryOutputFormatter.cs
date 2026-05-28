using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace OwlPack.Runtime;

public class BinaryOutputFormatter<TBufferWriter> : IOutputFormatter where TBufferWriter : IMemoryBufferWriter, new()
{
	private WriterPool<TBufferWriter> m_WriterPool = new WriterPool<TBufferWriter>();

	private Stack<TBufferWriter> m_Writers = new Stack<TBufferWriter>();

	private const ushort NullableBit = 32768;

	private const byte EnumKind_Name = 0;

	private const byte EnumKind_Flags = 1;

	private TBufferWriter Writer => m_Writers.Peek();

	public ReadOnlyArraySequence WrittenMemory => Writer.WrittenMemory;

	private void WriteFieldID(byte fieldID)
	{
		Writer.Write(fieldID);
	}

	private void WriteTypeID(ushort typeID)
	{
		Writer.Write(typeID);
	}

	private void WriteArrayCount(int count)
	{
		Writer.Write(count);
	}

	private void WriteObjectID(uint objectID)
	{
		Writer.Write(objectID);
	}

	private void WriteObjectKind(IOutputFormatter.ObjectKind kind)
	{
		Writer.Write((byte)kind);
	}

	private unsafe void WriteUnmanaged<T>(T value, bool isField, SerializerState state) where T : unmanaged
	{
		if (isField)
		{
			Write7BitEncodedInt(2 + sizeof(T));
		}
		Writer.Write(state.TypeLibrary.GetTypeID<T>());
		Writer.Write(value);
	}

	private unsafe void WriteUnmanagedNullable<T>(T? value, bool isField, SerializerState state) where T : unmanaged
	{
		if (isField)
		{
			Write7BitEncodedInt(1 + (value.HasValue ? (2 + sizeof(T)) : 0));
		}
		if (!value.HasValue)
		{
			((IBufferWriter<byte>)Writer).Write((byte)0);
			return;
		}
		((IBufferWriter<byte>)Writer).Write((byte)1);
		Writer.Write((ushort)(state.TypeLibrary.GetTypeID<T>() | 0x8000u));
		Writer.Write(value.Value);
	}

	public BinaryOutputFormatter()
	{
		m_Writers.Push(m_WriterPool.Get());
	}

	public void NullObject()
	{
		WriteObjectHeader(IOutputFormatter.ObjectKind.Null);
	}

	private void WriteObjectHeader(IOutputFormatter.ObjectKind kind)
	{
		WriteObjectKind(kind);
	}

	public void StartObject(byte fieldId, ushort type, string name, string typeName, uint objectId)
	{
		WriteFieldID(fieldId);
		StartObject(type, typeName, objectId);
	}

	public void StartObject(ushort type, string typeName, uint objectId)
	{
		WriteObjectHeader(IOutputFormatter.ObjectKind.Object);
		WriteObjectID(objectId);
		WriteTypeID(type);
	}

	public void EndObject()
	{
	}

	public void ObjectRef(uint objectId)
	{
		WriteObjectHeader(IOutputFormatter.ObjectKind.Ref);
		WriteObjectID(objectId);
	}

	public void StartArray(byte fieldId, ushort type, string name, string collectionTypeName, string elementTypeName, uint objectId, int count)
	{
		WriteFieldID(fieldId);
		StartArray(type, collectionTypeName, elementTypeName, objectId, count);
	}

	public void StartArray(ushort type, string collectionTypeName, string elementTypeName, uint objectId, int count)
	{
		WriteObjectHeader(IOutputFormatter.ObjectKind.Object);
		WriteObjectID(objectId);
		WriteTypeID(type);
		WriteArrayCount(count);
	}

	public void EndArray()
	{
	}

	private void FieldInternal<T>(string name, ref T value, SerializerState state)
	{
		if (value == null)
		{
			Write7BitEncodedInt(1);
			NullObject();
			return;
		}
		m_Writers.Push(m_WriterPool.Get());
		if (value is IOwlPackable owlPackable)
		{
			owlPackable.Serialize(this, state);
		}
		else
		{
			Type type = value.GetType();
			if (type.IsArray)
			{
				type = typeof(Array);
			}
			state.TypeLibrary.GetExternalTypeSerializer(type).Serialize(this, ref value, state);
		}
		TBufferWriter t = m_Writers.Pop();
		Write7BitEncodedInt(t.WrittenMemory.TotalLength);
		foreach (ReadOnlyArraySequenceSegment item in t.WrittenMemory)
		{
			Writer.Write(item.Span);
		}
		m_WriterPool.Return(t);
	}

	public void Field<T>(byte fieldId, string name, ref T value, SerializerState state)
	{
		WriteFieldID(fieldId);
		FieldInternal(name, ref value, state);
	}

	public void NullableField<T>(byte fieldId, string name, ref T? value, SerializerState state) where T : struct
	{
		WriteFieldID(fieldId);
		if (!value.HasValue)
		{
			Write7BitEncodedInt(1);
			NullObject();
		}
		else
		{
			T value2 = value.Value;
			FieldInternal(name, ref value2, state);
		}
	}

	private void WriteEnum<T>(ref T value, bool isField) where T : Enum
	{
		if (FlagsHelper<T>.IsFlags)
		{
			if (isField)
			{
				Write7BitEncodedInt(11);
			}
			WriteTypeID(13);
			((IBufferWriter<byte>)Writer).Write((byte)1);
			Writer.Write(EnumHelper.ToInt64(value));
			return;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(Enum.GetName(typeof(T), value));
		if (isField)
		{
			Write7BitEncodedInt(3 + Calculate7BitEncodedIntSize(bytes.Length) + bytes.Length);
		}
		WriteTypeID(13);
		((IBufferWriter<byte>)Writer).Write((byte)0);
		Write7BitEncodedInt(bytes.Length);
		Writer.Write(new ReadOnlySpan<byte>(bytes));
	}

	private void WriteNullableEnum<T>(ref T? value, bool isField) where T : struct, Enum
	{
		if (FlagsHelper<T>.IsFlags)
		{
			if (isField)
			{
				Write7BitEncodedInt(4 + (value.HasValue ? 8 : 0));
			}
			WriteTypeID(32781);
			((IBufferWriter<byte>)Writer).Write((byte)1);
			if (!value.HasValue)
			{
				((IBufferWriter<byte>)Writer).Write((byte)0);
				return;
			}
			((IBufferWriter<byte>)Writer).Write((byte)1);
			Writer.Write(EnumHelper.ToInt64(value.Value));
			return;
		}
		byte[] array = ((!value.HasValue) ? null : Encoding.UTF8.GetBytes(Enum.GetName(typeof(T), value)));
		if (isField)
		{
			int num = (value.HasValue ? (Calculate7BitEncodedIntSize(array.Length) + array.Length) : 0);
			Write7BitEncodedInt(4 + num);
		}
		WriteTypeID(32781);
		((IBufferWriter<byte>)Writer).Write((byte)0);
		if (!value.HasValue)
		{
			((IBufferWriter<byte>)Writer).Write((byte)0);
			return;
		}
		((IBufferWriter<byte>)Writer).Write((byte)1);
		Write7BitEncodedInt(array.Length);
		Writer.Write(new ReadOnlySpan<byte>(array));
	}

	public void EnumField<T>(byte fieldId, string name, ref T value, SerializerState state) where T : Enum
	{
		WriteFieldID(fieldId);
		WriteEnum(ref value, isField: true);
	}

	public void EnumNullableField<T>(byte fieldId, string name, ref T? value, SerializerState state) where T : struct, Enum
	{
		WriteFieldID(fieldId);
		WriteNullableEnum(ref value, isField: true);
	}

	public void UnmanagedField<T>(byte fieldId, string name, ref T value, SerializerState state) where T : unmanaged
	{
		WriteFieldID(fieldId);
		WriteUnmanaged(value, isField: true, state);
	}

	public void UnmanagedNullableField<T>(byte fieldId, string name, ref T? value, SerializerState state) where T : unmanaged
	{
		WriteFieldID(fieldId);
		WriteUnmanagedNullable(value, isField: true, state);
	}

	private int Calculate7BitEncodedIntSize(int value)
	{
		int num = 0;
		for (uint num2 = (uint)value; num2 >= 128; num2 >>= 7)
		{
			num++;
		}
		return num + 1;
	}

	private void Write7BitEncodedInt(int value)
	{
		uint num;
		for (num = (uint)value; num >= 128; num >>= 7)
		{
			Writer.Write((byte)(num | 0x80u));
		}
		Writer.Write((byte)num);
	}

	private void WriteStringWithType(ushort typeId, ref string? value, bool isNullable, bool isField)
	{
		byte[] array = ((value != null) ? Encoding.UTF8.GetBytes(value) : null);
		int num = (isNullable ? 1 : 0);
		if (value == null)
		{
			if (!isNullable)
			{
				throw new OutputFormatterException("Trying to write null string value while isNullable=false");
			}
			if (isField)
			{
				Write7BitEncodedInt(2 + num);
			}
		}
		else if (isField)
		{
			Write7BitEncodedInt(2 + num + Calculate7BitEncodedIntSize(array.Length) + array.Length);
		}
		WriteTypeID(typeId);
		if (isNullable)
		{
			if (value == null)
			{
				((IBufferWriter<byte>)Writer).Write((byte)0);
				return;
			}
			((IBufferWriter<byte>)Writer).Write((byte)1);
		}
		Write7BitEncodedInt(array.Length);
		Writer.Write(new ReadOnlySpan<byte>(array));
	}

	public void StringField(byte fieldId, string name, ref string? value, SerializerState state)
	{
		WriteFieldID(fieldId);
		WriteStringWithType(12, ref value, isNullable: true, isField: true);
	}

	public void ArrayElement<T>(ref T? value, SerializerState state)
	{
		if (value == null)
		{
			NullObject();
			return;
		}
		if (value is IOwlPackable owlPackable)
		{
			owlPackable.Serialize(this, state);
			return;
		}
		_ = value.GetType().IsArray;
		state.TypeLibrary.GetExternalTypeSerializer(value.GetType()).Serialize(this, ref value, state);
	}

	public void UnmanagedArrayElement<T>(ref T value, SerializerState state) where T : unmanaged
	{
		WriteUnmanaged(value, isField: false, state);
	}

	public void UnmanagedNullableArrayElement<T>(ref T? value, SerializerState state) where T : unmanaged
	{
		WriteUnmanaged(value.Value, isField: false, state);
	}

	public void EnumArrayElement<T>(ref T? value, SerializerState state) where T : Enum
	{
		WriteEnum(ref value, isField: false);
	}

	public void EnumNullableArrayElement<T>(ref T? value, SerializerState state) where T : struct, Enum
	{
		WriteNullableEnum(ref value, isField: false);
	}

	public void StringArrayElement(ref string? value, SerializerState state)
	{
		WriteStringWithType(12, ref value, isNullable: true, isField: false);
	}

	public void StartTuple(byte fieldId, string name, byte count)
	{
		WriteFieldID(fieldId);
		StartTuple(count);
	}

	public void StartTuple(byte count)
	{
		Writer.Write(count);
	}

	public void NextTupleElement()
	{
	}

	public void EndTuple()
	{
	}
}
