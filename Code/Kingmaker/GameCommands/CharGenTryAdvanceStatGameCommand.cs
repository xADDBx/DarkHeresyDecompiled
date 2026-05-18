using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CharGenTryAdvanceStatGameCommand : GameCommand, IMemoryPackable<CharGenTryAdvanceStatGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenTryAdvanceStatGameCommand>
{
	[Preserve]
	private sealed class CharGenTryAdvanceStatGameCommandFormatter : MemoryPackFormatter<CharGenTryAdvanceStatGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenTryAdvanceStatGameCommand value)
		{
			CharGenTryAdvanceStatGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenTryAdvanceStatGameCommand value)
		{
			CharGenTryAdvanceStatGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenTryAdvanceStatGameCommand value)
		{
			CharGenTryAdvanceStatGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenTryAdvanceStatGameCommand value)
		{
			CharGenTryAdvanceStatGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly StatType m_StatType;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly bool m_Advance;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenTryAdvanceStatGameCommand(StatType m_statType, bool m_advance)
	{
		m_StatType = m_statType;
		m_Advance = m_advance;
	}

	private CharGenTryAdvanceStatGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenAttributesPhaseHandler h)
		{
			h.HandleTryAdvanceStat(m_StatType, m_Advance);
		});
	}

	static CharGenTryAdvanceStatGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenTryAdvanceStatGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_StatType", typeof(StatType)),
				new FieldInfo("m_Advance", typeof(bool))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenTryAdvanceStatGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenTryAdvanceStatGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenTryAdvanceStatGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenTryAdvanceStatGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<StatType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<StatType>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenTryAdvanceStatGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(2, in value.m_StatType, in value.m_Advance);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenTryAdvanceStatGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		StatType value2;
		bool value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<StatType, bool>(out value2, out value3);
			}
			else
			{
				value2 = value.m_StatType;
				value3 = value.m_Advance;
				reader.ReadUnmanaged<StatType>(out value2);
				reader.ReadUnmanaged<bool>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenTryAdvanceStatGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = StatType.Unknown;
				value3 = false;
			}
			else
			{
				value2 = value.m_StatType;
				value3 = value.m_Advance;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<StatType>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new CharGenTryAdvanceStatGameCommand(value2, value3);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenTryAdvanceStatGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_StatType");
		writer.WriteUnmanaged(value.m_StatType);
		writer.WriteProperty("m_Advance");
		writer.WriteUnmanaged(value.m_Advance);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenTryAdvanceStatGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		StatType v;
		bool v2;
		if (value == null)
		{
			v = StatType.Unknown;
			v2 = false;
		}
		else
		{
			v = value.m_StatType;
			v2 = value.m_Advance;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_StatType"))
				{
					if (text == "m_Advance")
					{
						reader.ReadUnmanaged<bool>(out v2);
						array[1] = true;
					}
				}
				else
				{
					reader.ReadUnmanaged<StatType>(out v);
					array[0] = true;
				}
			}
			else if (!(text == "m_StatType"))
			{
				if (text == "m_Advance")
				{
					reader.ReadUnmanaged<bool>(out v2);
				}
			}
			else
			{
				reader.ReadUnmanaged<StatType>(out v);
			}
		}
		_ = value;
		value = new CharGenTryAdvanceStatGameCommand(v, v2);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenTryAdvanceStatGameCommand source = new CharGenTryAdvanceStatGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenTryAdvanceStatGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenTryAdvanceStatGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		StatType value = m_StatType;
		formatter.EnumField(0, "m_StatType", ref value, state);
		bool value2 = m_Advance;
		formatter.UnmanagedField(1, "m_Advance", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenTryAdvanceStatGameCommand>();
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
				Unsafe.AsRef(in m_StatType) = formatter.ReadEnum<StatType>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_Advance) = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
