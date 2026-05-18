using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Utility.FlagCountable;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
[HashRoot]
public class CountableFlag : IMemoryPackable<CountableFlag>, IMemoryPackFormatterRegister, IHashable, IOwlPackable, IOwlPackable<CountableFlag>
{
	[Preserve]
	private sealed class CountableFlagFormatter : MemoryPackFormatter<CountableFlag>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CountableFlag value)
		{
			CountableFlag.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CountableFlag value)
		{
			CountableFlag.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CountableFlag value)
		{
			CountableFlag.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CountableFlag value)
		{
			CountableFlag.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private int m_Count;

	public static readonly TypeInfo OwlPackTypeInfo;

	[MemoryPackIgnore]
	public int Count => m_Count;

	[MemoryPackIgnore]
	public bool Value => this;

	[MemoryPackConstructor]
	public CountableFlag()
	{
	}

	public void Retain()
	{
		m_Count++;
	}

	public void Release()
	{
		if (Application.isEditor && m_Count < 1)
		{
			PFLog.Default.Error("Can't release countable flag: no one retain it");
		}
		m_Count = Math.Max(0, m_Count - 1);
	}

	public void ReleaseAll()
	{
		m_Count = 0;
	}

	public static implicit operator bool(CountableFlag flag)
	{
		if (flag != null)
		{
			return flag.m_Count > 0;
		}
		return false;
	}

	public override string ToString()
	{
		return $"{(bool)this}({m_Count})";
	}

	static CountableFlag()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CountableFlag",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_Count", typeof(int))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CountableFlag>())
		{
			MemoryPackFormatterProvider.Register(new CountableFlagFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CountableFlag[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CountableFlag>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CountableFlag? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_Count);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CountableFlag? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		int value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_Count;
				reader.ReadUnmanaged<int>(out value2);
				goto IL_006b;
			}
			reader.ReadUnmanaged<int>(out value2);
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CountableFlag), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Count : 0);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006b;
			}
		}
		value = new CountableFlag
		{
			m_Count = value2
		};
		return;
		IL_006b:
		value.m_Count = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CountableFlag? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Count");
		writer.WriteUnmanaged(value.m_Count);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CountableFlag? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		int v = ((value != null) ? value.m_Count : 0);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_Count")
				{
					reader.ReadUnmanaged<int>(out v);
					array[0] = true;
				}
			}
			else if (text == "m_Count")
			{
				reader.ReadUnmanaged<int>(out v);
			}
		}
		if (value != null)
		{
			value.m_Count = v;
		}
		else
		{
			value = new CountableFlag
			{
				m_Count = v
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref m_Count);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CountableFlag source = new CountableFlag();
		result = Unsafe.As<CountableFlag, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<CountableFlag>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "m_Count", ref m_Count, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CountableFlag>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				m_Count = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
