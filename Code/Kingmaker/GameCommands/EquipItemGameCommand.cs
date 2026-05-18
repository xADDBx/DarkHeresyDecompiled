using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class EquipItemGameCommand : GameCommand, IMemoryPackable<EquipItemGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<EquipItemGameCommand>
{
	[Preserve]
	private sealed class EquipItemGameCommandFormatter : MemoryPackFormatter<EquipItemGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref EquipItemGameCommand value)
		{
			EquipItemGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref EquipItemGameCommand value)
		{
			EquipItemGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref EquipItemGameCommand value)
		{
			EquipItemGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref EquipItemGameCommand value)
		{
			EquipItemGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private EntityRef<ItemEntity> m_Item;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private EntityRef<MechanicEntity> m_Entity;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private ItemSlotRef m_To;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private EquipItemGameCommand()
	{
	}

	[JsonConstructor]
	public EquipItemGameCommand(ItemEntity item, MechanicEntity entity, ItemSlotRef to)
	{
		m_Item = new EntityRef<ItemEntity>(item);
		m_Entity = entity;
		m_To = to;
	}

	protected override void ExecuteInternal()
	{
		if (m_Item.Entity != null && m_Entity.Entity != null)
		{
			ItemSlot itemSlot;
			if (m_To == null)
			{
				GameCommandHelper.EquipItemAutomatically(m_Item, m_Entity.Entity as BaseUnitEntity);
			}
			else if (GameCommandHelper.TryGetEquipSlot(m_Entity.Entity, m_To, out itemSlot))
			{
				GameCommandHelper.TryInsertItem(itemSlot, m_Item.Entity);
			}
		}
	}

	static EquipItemGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "EquipItemGameCommand",
			OldNames = null,
			Fields = new FieldInfo[3]
			{
				new FieldInfo("m_Item", typeof(EntityRef<ItemEntity>)),
				new FieldInfo("m_Entity", typeof(EntityRef<MechanicEntity>)),
				new FieldInfo("m_To", typeof(ItemSlotRef))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<EquipItemGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new EquipItemGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EquipItemGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EquipItemGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref EquipItemGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_Item);
		writer.WritePackable(in value.m_Entity);
		writer.WritePackable(in value.m_To);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref EquipItemGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<ItemEntity> value2;
		EntityRef<MechanicEntity> value3;
		ItemSlotRef value4;
		if (memberCount == 3)
		{
			if (value != null)
			{
				value2 = value.m_Item;
				value3 = value.m_Entity;
				value4 = value.m_To;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				reader.ReadPackable(ref value4);
				goto IL_00d3;
			}
			value2 = reader.ReadPackable<EntityRef<ItemEntity>>();
			value3 = reader.ReadPackable<EntityRef<MechanicEntity>>();
			value4 = reader.ReadPackable<ItemSlotRef>();
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(EquipItemGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<ItemEntity>);
				value3 = default(EntityRef<MechanicEntity>);
				value4 = null;
			}
			else
			{
				value2 = value.m_Item;
				value3 = value.m_Entity;
				value4 = value.m_To;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadPackable(ref value4);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00d3;
			}
		}
		value = new EquipItemGameCommand
		{
			m_Item = value2,
			m_Entity = value3,
			m_To = value4
		};
		return;
		IL_00d3:
		value.m_Item = value2;
		value.m_Entity = value3;
		value.m_To = value4;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref EquipItemGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Item");
		writer.WritePackable(value.m_Item);
		writer.WriteProperty("m_Entity");
		writer.WritePackable(value.m_Entity);
		writer.WriteProperty("m_To");
		writer.WritePackable(value.m_To);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref EquipItemGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityRef<ItemEntity> val;
		EntityRef<MechanicEntity> val2;
		ItemSlotRef val3;
		if (value == null)
		{
			val = default(EntityRef<ItemEntity>);
			val2 = default(EntityRef<MechanicEntity>);
			val3 = null;
		}
		else
		{
			val = value.m_Item;
			val2 = value.m_Entity;
			val3 = value.m_To;
		}
		bool[] array = new bool[3];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "m_Item":
					val = reader.ReadPackable<EntityRef<ItemEntity>>();
					array[0] = true;
					break;
				case "m_Entity":
					val2 = reader.ReadPackable<EntityRef<MechanicEntity>>();
					array[1] = true;
					break;
				case "m_To":
					val3 = reader.ReadPackable<ItemSlotRef>();
					array[2] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "m_Item":
					reader.ReadPackable(ref val);
					break;
				case "m_Entity":
					reader.ReadPackable(ref val2);
					break;
				case "m_To":
					reader.ReadPackable(ref val3);
					break;
				}
			}
		}
		if (value != null)
		{
			value.m_Item = val;
			value.m_Entity = val2;
			value.m_To = val3;
		}
		else
		{
			value = new EquipItemGameCommand
			{
				m_Item = val,
				m_Entity = val2,
				m_To = val3
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		EquipItemGameCommand source = new EquipItemGameCommand();
		result = Unsafe.As<EquipItemGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<EquipItemGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Item", ref m_Item, state);
		formatter.Field(1, "m_Entity", ref m_Entity, state);
		formatter.Field(2, "m_To", ref m_To, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EquipItemGameCommand>();
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
				m_Item = formatter.ReadPackable<EntityRef<ItemEntity>>(state);
				break;
			case 1:
				m_Entity = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			case 2:
				m_To = formatter.ReadPackable<ItemSlotRef>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
