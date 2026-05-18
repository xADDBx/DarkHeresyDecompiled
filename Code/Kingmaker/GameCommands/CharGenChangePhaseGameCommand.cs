using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CharGenChangePhaseGameCommand : GameCommand, IMemoryPackable<CharGenChangePhaseGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenChangePhaseGameCommand>
{
	[Preserve]
	private sealed class CharGenChangePhaseGameCommandFormatter : MemoryPackFormatter<CharGenChangePhaseGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenChangePhaseGameCommand value)
		{
			CharGenChangePhaseGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenChangePhaseGameCommand value)
		{
			CharGenChangePhaseGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenChangePhaseGameCommand value)
		{
			CharGenChangePhaseGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenChangePhaseGameCommand value)
		{
			CharGenChangePhaseGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly CharGenPhaseType m_PhaseType;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenChangePhaseGameCommand(CharGenPhaseType m_phaseType)
	{
		m_PhaseType = m_phaseType;
	}

	private CharGenChangePhaseGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenChangePhaseHandler h)
		{
			h.HandlePhaseChange(m_PhaseType);
		});
	}

	static CharGenChangePhaseGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenChangePhaseGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_PhaseType", typeof(CharGenPhaseType))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenChangePhaseGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenChangePhaseGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenChangePhaseGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenChangePhaseGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenPhaseType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<CharGenPhaseType>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenChangePhaseGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_PhaseType);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenChangePhaseGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		CharGenPhaseType value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<CharGenPhaseType>(out value2);
			}
			else
			{
				value2 = value.m_PhaseType;
				reader.ReadUnmanaged<CharGenPhaseType>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenChangePhaseGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_PhaseType : CharGenPhaseType.Pregen);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<CharGenPhaseType>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharGenChangePhaseGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenChangePhaseGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_PhaseType");
		writer.WriteUnmanaged(value.m_PhaseType);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenChangePhaseGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		CharGenPhaseType v = ((value != null) ? value.m_PhaseType : CharGenPhaseType.Pregen);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_PhaseType")
				{
					reader.ReadUnmanaged<CharGenPhaseType>(out v);
					array[0] = true;
				}
			}
			else if (text == "m_PhaseType")
			{
				reader.ReadUnmanaged<CharGenPhaseType>(out v);
			}
		}
		_ = value;
		value = new CharGenChangePhaseGameCommand(v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenChangePhaseGameCommand source = new CharGenChangePhaseGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenChangePhaseGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenChangePhaseGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		CharGenPhaseType value = m_PhaseType;
		formatter.EnumField(0, "m_PhaseType", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenChangePhaseGameCommand>();
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
				Unsafe.AsRef(in m_PhaseType) = formatter.ReadEnum<CharGenPhaseType>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
