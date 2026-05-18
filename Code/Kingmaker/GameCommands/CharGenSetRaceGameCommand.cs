using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Levelup.CharGen;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSetRaceGameCommand : GameCommand, IMemoryPackable<CharGenSetRaceGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenSetRaceGameCommand>
{
	[Preserve]
	private sealed class CharGenSetRaceGameCommandFormatter : MemoryPackFormatter<CharGenSetRaceGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetRaceGameCommand value)
		{
			CharGenSetRaceGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetRaceGameCommand value)
		{
			CharGenSetRaceGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetRaceGameCommand value)
		{
			CharGenSetRaceGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetRaceGameCommand value)
		{
			CharGenSetRaceGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly BlueprintRaceVisualPresetReference m_Blueprint;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly int m_Index;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private CharGenSetRaceGameCommand([NotNull] BlueprintRaceVisualPresetReference m_blueprint, int m_index)
	{
		if (m_blueprint == null)
		{
			throw new ArgumentNullException("m_blueprint");
		}
		m_Blueprint = m_blueprint;
		m_Index = m_index;
	}

	private CharGenSetRaceGameCommand(OwlPackConstructorParameter _)
	{
	}

	public CharGenSetRaceGameCommand([NotNull] BlueprintRaceVisualPreset blueprint, int m_index)
		: this(blueprint.ToReference<BlueprintRaceVisualPresetReference>(), m_index)
	{
		if (blueprint == null)
		{
			throw new ArgumentNullException("blueprint");
		}
	}

	protected override void ExecuteInternal()
	{
		if ((BlueprintRaceVisualPreset)m_Blueprint == null)
		{
			PFLog.GameCommands.Log("[CharGenSetRaceGameCommand] BlueprintRaceVisualPreset was not found id=" + m_Blueprint.Guid);
			return;
		}
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetRace(m_Blueprint, m_Index);
		});
	}

	static CharGenSetRaceGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenSetRaceGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_Blueprint", typeof(BlueprintRaceVisualPresetReference)),
				new FieldInfo("m_Index", typeof(int))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetRaceGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetRaceGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetRaceGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetRaceGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetRaceGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_Blueprint);
		writer.WriteUnmanaged(in value.m_Index);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetRaceGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintRaceVisualPresetReference value2;
		int value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<BlueprintRaceVisualPresetReference>();
				reader.ReadUnmanaged<int>(out value3);
			}
			else
			{
				value2 = value.m_Blueprint;
				value3 = value.m_Index;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetRaceGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = 0;
			}
			else
			{
				value2 = value.m_Blueprint;
				value3 = value.m_Index;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new CharGenSetRaceGameCommand(value2, value3);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetRaceGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Blueprint");
		writer.WritePackable(value.m_Blueprint);
		writer.WriteProperty("m_Index");
		writer.WriteUnmanaged(value.m_Index);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetRaceGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		BlueprintRaceVisualPresetReference val;
		int v;
		if (value == null)
		{
			val = null;
			v = 0;
		}
		else
		{
			val = value.m_Blueprint;
			v = value.m_Index;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_Blueprint"))
				{
					if (text == "m_Index")
					{
						reader.ReadUnmanaged<int>(out v);
						array[1] = true;
					}
				}
				else
				{
					val = reader.ReadPackable<BlueprintRaceVisualPresetReference>();
					array[0] = true;
				}
			}
			else if (!(text == "m_Blueprint"))
			{
				if (text == "m_Index")
				{
					reader.ReadUnmanaged<int>(out v);
				}
			}
			else
			{
				reader.ReadPackable(ref val);
			}
		}
		_ = value;
		value = new CharGenSetRaceGameCommand(val, v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSetRaceGameCommand source = new CharGenSetRaceGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSetRaceGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSetRaceGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintRaceVisualPresetReference value = m_Blueprint;
		formatter.Field(0, "m_Blueprint", ref value, state);
		int value2 = m_Index;
		formatter.UnmanagedField(1, "m_Index", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSetRaceGameCommand>();
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
				Unsafe.AsRef(in m_Blueprint) = formatter.ReadPackable<BlueprintRaceVisualPresetReference>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_Index) = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
