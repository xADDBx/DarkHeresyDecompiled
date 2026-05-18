using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSetTattooColorGameCommand : GameCommand, IMemoryPackable<CharGenSetTattooColorGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenSetTattooColorGameCommand>
{
	[Preserve]
	private sealed class CharGenSetTattooColorGameCommandFormatter : MemoryPackFormatter<CharGenSetTattooColorGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetTattooColorGameCommand value)
		{
			CharGenSetTattooColorGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetTattooColorGameCommand value)
		{
			CharGenSetTattooColorGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetTattooColorGameCommand value)
		{
			CharGenSetTattooColorGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetTattooColorGameCommand value)
		{
			CharGenSetTattooColorGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly int m_RampIndex;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly int m_Index;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenSetTattooColorGameCommand(int m_rampIndex, int m_index)
	{
		m_RampIndex = m_rampIndex;
		m_Index = m_index;
	}

	private CharGenSetTattooColorGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetTattooColor(m_RampIndex, m_Index);
		});
	}

	static CharGenSetTattooColorGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenSetTattooColorGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_RampIndex", typeof(int)),
				new FieldInfo("m_Index", typeof(int))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetTattooColorGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetTattooColorGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetTattooColorGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetTattooColorGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetTattooColorGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(2, in value.m_RampIndex, in value.m_Index);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetTattooColorGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		int value2;
		int value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<int, int>(out value2, out value3);
			}
			else
			{
				value2 = value.m_RampIndex;
				value3 = value.m_Index;
				reader.ReadUnmanaged<int>(out value2);
				reader.ReadUnmanaged<int>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetTattooColorGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				value3 = 0;
			}
			else
			{
				value2 = value.m_RampIndex;
				value3 = value.m_Index;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new CharGenSetTattooColorGameCommand(value2, value3);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetTattooColorGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_RampIndex");
		writer.WriteUnmanaged(value.m_RampIndex);
		writer.WriteProperty("m_Index");
		writer.WriteUnmanaged(value.m_Index);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetTattooColorGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		int v;
		int v2;
		if (value == null)
		{
			v = 0;
			v2 = 0;
		}
		else
		{
			v = value.m_RampIndex;
			v2 = value.m_Index;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_RampIndex"))
				{
					if (text == "m_Index")
					{
						reader.ReadUnmanaged<int>(out v2);
						array[1] = true;
					}
				}
				else
				{
					reader.ReadUnmanaged<int>(out v);
					array[0] = true;
				}
			}
			else if (!(text == "m_RampIndex"))
			{
				if (text == "m_Index")
				{
					reader.ReadUnmanaged<int>(out v2);
				}
			}
			else
			{
				reader.ReadUnmanaged<int>(out v);
			}
		}
		_ = value;
		value = new CharGenSetTattooColorGameCommand(v, v2);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSetTattooColorGameCommand source = new CharGenSetTattooColorGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSetTattooColorGameCommand, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<CharGenSetTattooColorGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		int value = m_RampIndex;
		formatter.UnmanagedField(0, "m_RampIndex", ref value, state);
		int value2 = m_Index;
		formatter.UnmanagedField(1, "m_Index", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSetTattooColorGameCommand>();
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
				Unsafe.AsRef(in m_RampIndex) = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_Index) = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
