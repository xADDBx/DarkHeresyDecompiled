using System;
using System.Text.Json;

namespace OwlPack.Runtime;

public class JsonOutputFormatter : IOutputFormatter
{
	private Utf8JsonWriter m_Writer;

	public JsonOutputFormatter(Utf8JsonWriter writer)
	{
		m_Writer = writer;
	}

	public void NullObject()
	{
		m_Writer.WriteNullValue();
	}

	public void StartObject(byte fieldId, ushort type, string name, string typeName, uint objectId)
	{
		m_Writer.WritePropertyName(name);
		StartObject(fieldId, type, name, typeName, objectId);
	}

	public void StartObject(ushort type, string typeName, uint objectId)
	{
		m_Writer.WriteStartObject();
		m_Writer.WriteNumber("$id", objectId);
		m_Writer.WriteNumber("$type", type);
		m_Writer.WriteString("$typeName", typeName);
	}

	public void EndObject()
	{
		m_Writer.WriteEndObject();
	}

	public void ObjectRef(uint objectId)
	{
		m_Writer.WriteStartObject();
		m_Writer.WriteNumber("$ref", objectId);
		m_Writer.WriteEndObject();
	}

	public void StartArray(byte fieldId, ushort type, string name, string collectionTypeName, string elementTypeName, uint objectId, int count)
	{
		m_Writer.WritePropertyName(name);
		StartArray(type, collectionTypeName, elementTypeName, objectId, count);
	}

	public void StartArray(ushort type, string collectionTypeName, string elementTypeName, uint objectId, int count)
	{
		m_Writer.WriteStartObject();
		m_Writer.WriteNumber("$id", objectId);
		m_Writer.WriteNumber("$type", type);
		m_Writer.WriteString("$typeName", collectionTypeName);
		m_Writer.WriteNumber("$count", count);
		m_Writer.WriteString("$elementTypeName", elementTypeName);
		m_Writer.WritePropertyName("$elements");
		m_Writer.WriteStartArray();
	}

	public void EndArray()
	{
		m_Writer.WriteEndArray();
		m_Writer.WriteEndObject();
	}

	public void Field<T>(byte fieldId, string name, ref T? value, SerializerState state)
	{
		if (value == null)
		{
			m_Writer.WritePropertyName(name);
			NullObject();
			return;
		}
		m_Writer.WritePropertyName(name);
		if (value is IOwlPackable owlPackable)
		{
			owlPackable.Serialize(this, state);
			return;
		}
		Type type = value.GetType();
		if (type.IsArray)
		{
			type = typeof(Array);
		}
		state.TypeLibrary.GetExternalTypeSerializer(type).Serialize(this, ref value, state);
	}

	public void NullableField<T>(byte fieldId, string name, ref T? value, SerializerState state) where T : struct
	{
		if (!value.HasValue)
		{
			m_Writer.WriteNull(name);
			return;
		}
		T value2 = value.Value;
		Field(fieldId, name, ref value2, state);
	}

	public void EnumField<T>(byte fieldId, string name, ref T? value, SerializerState state) where T : Enum
	{
		if (FlagsHelper<T>.IsFlags)
		{
			m_Writer.WriteNumber(name, EnumHelper.ToInt64(value));
		}
		else
		{
			m_Writer.WriteString(name, Enum.GetName(typeof(T), value));
		}
	}

	public void EnumNullableField<T>(byte fieldId, string name, ref T? value, SerializerState state) where T : struct, Enum
	{
		if (!value.HasValue)
		{
			m_Writer.WriteNull(name);
		}
		else if (FlagsHelper<T>.IsFlags)
		{
			m_Writer.WriteNumber(name, EnumHelper.ToInt64(value.Value));
		}
		else
		{
			m_Writer.WriteString(name, Enum.GetName(typeof(T), value));
		}
	}

	private void WriteUnmanaged<T>(ref T value) where T : unmanaged
	{
		T val = value;
		if (val is byte value2)
		{
			m_Writer.WriteNumberValue(value2);
			return;
		}
		val = value;
		if (val is sbyte value3)
		{
			m_Writer.WriteNumberValue(value3);
			return;
		}
		val = value;
		if (val is short value4)
		{
			m_Writer.WriteNumberValue(value4);
			return;
		}
		val = value;
		if (val is ushort value5)
		{
			m_Writer.WriteNumberValue(value5);
			return;
		}
		val = value;
		if (val is int value6)
		{
			m_Writer.WriteNumberValue(value6);
			return;
		}
		val = value;
		if (val is uint value7)
		{
			m_Writer.WriteNumberValue(value7);
			return;
		}
		val = value;
		if (val is long value8)
		{
			m_Writer.WriteNumberValue(value8);
			return;
		}
		val = value;
		if (val is ulong value9)
		{
			m_Writer.WriteNumberValue(value9);
			return;
		}
		val = value;
		if (val is float num)
		{
			m_Writer.WriteNumberValue((double)num);
			return;
		}
		val = value;
		if (val is double value10)
		{
			m_Writer.WriteNumberValue(value10);
			return;
		}
		val = value;
		if (val is char value11)
		{
			m_Writer.WriteNumberValue(value11);
			return;
		}
		val = value;
		if (val is bool value12)
		{
			m_Writer.WriteBooleanValue(value12);
			return;
		}
		throw new OutputFormatterException("Type " + typeof(T).FullName + " is not supported by JsonOutputFormatter");
	}

	public void UnmanagedField<T>(byte fieldId, string name, ref T value, SerializerState state) where T : unmanaged
	{
		m_Writer.WritePropertyName(name);
		WriteUnmanaged(ref value);
	}

	public void UnmanagedNullableField<T>(byte fieldId, string name, ref T? value, SerializerState state) where T : unmanaged
	{
		if (!value.HasValue)
		{
			m_Writer.WriteNull(name);
			return;
		}
		T value2 = value.Value;
		UnmanagedField(fieldId, name, ref value2, state);
	}

	public void StringField(byte fieldId, string name, ref string? value, SerializerState state)
	{
		if (value == null)
		{
			m_Writer.WriteNull(name);
		}
		else
		{
			m_Writer.WriteString(name, value);
		}
	}

	public void ArrayElement<T>(ref T? value, SerializerState state)
	{
		if (value == null)
		{
			NullObject();
		}
		else if (value is IOwlPackable owlPackable)
		{
			owlPackable.Serialize(this, state);
		}
		else
		{
			(state.TypeLibrary.GetExternalTypeSerializer(value.GetType()) ?? throw new OutputFormatterException($"External type {value.GetType()} is not registred in TypeLibrary")).Serialize(this, ref value, state);
		}
	}

	public void UnmanagedArrayElement<T>(ref T value, SerializerState state) where T : unmanaged
	{
		WriteUnmanaged(ref value);
	}

	public void UnmanagedNullableArrayElement<T>(ref T? value, SerializerState state) where T : unmanaged
	{
		if (!value.HasValue)
		{
			m_Writer.WriteNullValue();
			return;
		}
		T value2 = value.Value;
		WriteUnmanaged(ref value2);
	}

	public void StringArrayElement(ref string? value, SerializerState state)
	{
		if (value == null)
		{
			m_Writer.WriteNullValue();
		}
		else
		{
			m_Writer.WriteStringValue(value);
		}
	}

	public void EnumArrayElement<T>(ref T? value, SerializerState state) where T : Enum
	{
		if (FlagsHelper<T>.IsFlags)
		{
			m_Writer.WriteNumberValue(EnumHelper.ToInt64(value));
		}
		else
		{
			m_Writer.WriteStringValue(Enum.GetName(typeof(T), value));
		}
	}

	public void EnumNullableArrayElement<T>(ref T? value, SerializerState state) where T : struct, Enum
	{
		if (!value.HasValue)
		{
			m_Writer.WriteNullValue();
		}
		else if (FlagsHelper<T>.IsFlags)
		{
			m_Writer.WriteNumberValue(EnumHelper.ToInt64(value.Value));
		}
		else
		{
			m_Writer.WriteStringValue(Enum.GetName(typeof(T), value));
		}
	}

	public void StartTuple(byte fieldId, string name, byte count)
	{
		m_Writer.WritePropertyName(name);
		StartTuple(count);
	}

	public void StartTuple(byte count)
	{
		m_Writer.WriteStartObject();
		m_Writer.WriteNumber("$count", count);
		m_Writer.WriteStartArray("$elements");
	}

	public void NextTupleElement()
	{
	}

	public void EndTuple()
	{
		m_Writer.WriteEndArray();
		m_Writer.WriteEndObject();
	}
}
