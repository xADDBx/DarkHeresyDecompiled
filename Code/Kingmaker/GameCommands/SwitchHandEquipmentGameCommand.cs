using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class SwitchHandEquipmentGameCommand : GameCommand, IMemoryPackable<SwitchHandEquipmentGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<SwitchHandEquipmentGameCommand>
{
	[Preserve]
	private sealed class SwitchHandEquipmentGameCommandFormatter : MemoryPackFormatter<SwitchHandEquipmentGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SwitchHandEquipmentGameCommand value)
		{
			SwitchHandEquipmentGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SwitchHandEquipmentGameCommand value)
		{
			SwitchHandEquipmentGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SwitchHandEquipmentGameCommand value)
		{
			SwitchHandEquipmentGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SwitchHandEquipmentGameCommand value)
		{
			SwitchHandEquipmentGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_Unit;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly sbyte m_HandEquipmentSetIndex;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private SwitchHandEquipmentGameCommand()
	{
	}

	[MemoryPackConstructor]
	private SwitchHandEquipmentGameCommand(EntityRef<BaseUnitEntity> m_unit, sbyte m_handEquipmentSetIndex)
	{
		m_Unit = m_unit;
		m_HandEquipmentSetIndex = m_handEquipmentSetIndex;
	}

	public SwitchHandEquipmentGameCommand([NotNull] BaseUnitEntity unit, int handEquipmentSetIndex)
		: this((EntityRef<BaseUnitEntity>)unit, (sbyte)handEquipmentSetIndex)
	{
		if (unit == null)
		{
			throw new NullReferenceException("unit");
		}
		if (handEquipmentSetIndex < -128 || 127 < handEquipmentSetIndex)
		{
			throw new ArgumentOutOfRangeException($"handEquipmentSetIndex={handEquipmentSetIndex}");
		}
	}

	protected override void ExecuteInternal()
	{
		BaseUnitEntity entity = m_Unit.Entity;
		if (entity == null)
		{
			PFLog.GameCommands.Error("Unit #" + m_Unit.Id + " not found!");
		}
		else
		{
			entity.Body.CurrentHandEquipmentSetIndex = m_HandEquipmentSetIndex;
		}
	}

	static SwitchHandEquipmentGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SwitchHandEquipmentGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_Unit", typeof(EntityRef<BaseUnitEntity>)),
				new FieldInfo("m_HandEquipmentSetIndex", typeof(sbyte))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SwitchHandEquipmentGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SwitchHandEquipmentGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SwitchHandEquipmentGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SwitchHandEquipmentGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SwitchHandEquipmentGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_Unit);
		writer.WriteUnmanaged(in value.m_HandEquipmentSetIndex);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SwitchHandEquipmentGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<BaseUnitEntity> value2;
		sbyte value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
				reader.ReadUnmanaged<sbyte>(out value3);
			}
			else
			{
				value2 = value.m_Unit;
				value3 = value.m_HandEquipmentSetIndex;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<sbyte>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SwitchHandEquipmentGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<BaseUnitEntity>);
				value3 = 0;
			}
			else
			{
				value2 = value.m_Unit;
				value3 = value.m_HandEquipmentSetIndex;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<sbyte>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new SwitchHandEquipmentGameCommand(value2, value3);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SwitchHandEquipmentGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Unit");
		writer.WritePackable(value.m_Unit);
		writer.WriteProperty("m_HandEquipmentSetIndex");
		writer.WriteUnmanaged(value.m_HandEquipmentSetIndex);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SwitchHandEquipmentGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityRef<BaseUnitEntity> val;
		sbyte v;
		if (value == null)
		{
			val = default(EntityRef<BaseUnitEntity>);
			v = 0;
		}
		else
		{
			val = value.m_Unit;
			v = value.m_HandEquipmentSetIndex;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_Unit"))
				{
					if (text == "m_HandEquipmentSetIndex")
					{
						reader.ReadUnmanaged<sbyte>(out v);
						array[1] = true;
					}
				}
				else
				{
					val = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
					array[0] = true;
				}
			}
			else if (!(text == "m_Unit"))
			{
				if (text == "m_HandEquipmentSetIndex")
				{
					reader.ReadUnmanaged<sbyte>(out v);
				}
			}
			else
			{
				reader.ReadPackable(ref val);
			}
		}
		_ = value;
		value = new SwitchHandEquipmentGameCommand(val, v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SwitchHandEquipmentGameCommand source = new SwitchHandEquipmentGameCommand();
		result = Unsafe.As<SwitchHandEquipmentGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SwitchHandEquipmentGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef<BaseUnitEntity> value = m_Unit;
		formatter.Field(0, "m_Unit", ref value, state);
		sbyte value2 = m_HandEquipmentSetIndex;
		formatter.UnmanagedField(1, "m_HandEquipmentSetIndex", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SwitchHandEquipmentGameCommand>();
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
				Unsafe.AsRef(in m_Unit) = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_HandEquipmentSetIndex) = formatter.ReadUnmanaged<sbyte>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
