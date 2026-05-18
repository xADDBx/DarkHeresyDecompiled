using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Visual.Sound;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CharGenChangeVoiceGameCommand : GameCommand, IMemoryPackable<CharGenChangeVoiceGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenChangeVoiceGameCommand>
{
	[Preserve]
	private sealed class CharGenChangeVoiceGameCommandFormatter : MemoryPackFormatter<CharGenChangeVoiceGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenChangeVoiceGameCommand value)
		{
			CharGenChangeVoiceGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenChangeVoiceGameCommand value)
		{
			CharGenChangeVoiceGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenChangeVoiceGameCommand value)
		{
			CharGenChangeVoiceGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenChangeVoiceGameCommand value)
		{
			CharGenChangeVoiceGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly BlueprintUnitAsksListReference m_Blueprint;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private CharGenChangeVoiceGameCommand([NotNull] BlueprintUnitAsksListReference m_blueprint)
	{
		if (m_blueprint == null)
		{
			throw new ArgumentNullException("m_blueprint");
		}
		m_Blueprint = m_blueprint;
	}

	private CharGenChangeVoiceGameCommand(OwlPackConstructorParameter _)
	{
	}

	public CharGenChangeVoiceGameCommand([NotNull] BlueprintUnitAsksList blueprint)
		: this(blueprint.ToReference<BlueprintUnitAsksListReference>())
	{
		if (blueprint == null)
		{
			throw new ArgumentNullException("blueprint");
		}
	}

	protected override void ExecuteInternal()
	{
		BlueprintUnitAsksList blueprint = m_Blueprint;
		if (blueprint == null)
		{
			PFLog.GameCommands.Log("[CharGenChangeVoiceGameCommand] BlueprintUnitAsksList was not found id=" + m_Blueprint.Guid);
			return;
		}
		EventBus.RaiseEvent(delegate(ICharGenAppearancePhaseVoiceHandler h)
		{
			h.HandleChangeVoice(blueprint);
		});
	}

	static CharGenChangeVoiceGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenChangeVoiceGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_Blueprint", typeof(BlueprintUnitAsksListReference))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenChangeVoiceGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenChangeVoiceGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenChangeVoiceGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenChangeVoiceGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenChangeVoiceGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_Blueprint);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenChangeVoiceGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintUnitAsksListReference value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<BlueprintUnitAsksListReference>();
			}
			else
			{
				value2 = value.m_Blueprint;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenChangeVoiceGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Blueprint : null);
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharGenChangeVoiceGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenChangeVoiceGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Blueprint");
		writer.WritePackable(value.m_Blueprint);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenChangeVoiceGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		BlueprintUnitAsksListReference val = ((value != null) ? value.m_Blueprint : null);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_Blueprint")
				{
					val = reader.ReadPackable<BlueprintUnitAsksListReference>();
					array[0] = true;
				}
			}
			else if (text == "m_Blueprint")
			{
				reader.ReadPackable(ref val);
			}
		}
		_ = value;
		value = new CharGenChangeVoiceGameCommand(val);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenChangeVoiceGameCommand source = new CharGenChangeVoiceGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenChangeVoiceGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenChangeVoiceGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintUnitAsksListReference value = m_Blueprint;
		formatter.Field(0, "m_Blueprint", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenChangeVoiceGameCommand>();
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
				Unsafe.AsRef(in m_Blueprint) = formatter.ReadPackable<BlueprintUnitAsksListReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
