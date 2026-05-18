using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
public class SwapSlotsGameCommand : GameCommand, IMemoryPackable<SwapSlotsGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<SwapSlotsGameCommand>
{
	[Preserve]
	private sealed class SwapSlotsGameCommandFormatter : MemoryPackFormatter<SwapSlotsGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SwapSlotsGameCommand value)
		{
			SwapSlotsGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SwapSlotsGameCommand value)
		{
			SwapSlotsGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SwapSlotsGameCommand value)
		{
			SwapSlotsGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SwapSlotsGameCommand value)
		{
			SwapSlotsGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private ItemSlotRef m_From;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private ItemSlotRef m_To;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private EntityRef<MechanicEntity> m_Owner;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private bool m_IsLoot;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized
	{
		get
		{
			if (!m_From.IsEquipment && !m_To.IsEquipment)
			{
				return !m_From.ItemsCollectionRef.Equals(m_To.ItemsCollectionRef);
			}
			return true;
		}
	}

	[MemoryPackConstructor]
	private SwapSlotsGameCommand()
	{
	}

	[JsonConstructor]
	public SwapSlotsGameCommand(MechanicEntity entity, ItemSlotRef from, ItemSlotRef to, bool isLoot)
	{
		m_From = from;
		m_To = to;
		m_Owner = new EntityRef<MechanicEntity>(entity);
		m_IsLoot = isLoot;
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.TrySwapSlots(m_From, m_To, m_Owner.Entity, m_IsLoot);
	}

	static SwapSlotsGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SwapSlotsGameCommand",
			OldNames = null,
			Fields = new FieldInfo[4]
			{
				new FieldInfo("m_From", typeof(ItemSlotRef)),
				new FieldInfo("m_To", typeof(ItemSlotRef)),
				new FieldInfo("m_Owner", typeof(EntityRef<MechanicEntity>)),
				new FieldInfo("m_IsLoot", typeof(bool))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SwapSlotsGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SwapSlotsGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SwapSlotsGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SwapSlotsGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SwapSlotsGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(4);
		writer.WritePackable(in value.m_From);
		writer.WritePackable(in value.m_To);
		writer.WritePackable(in value.m_Owner);
		writer.WriteUnmanaged(in value.m_IsLoot);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SwapSlotsGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		ItemSlotRef value2;
		ItemSlotRef value3;
		EntityRef<MechanicEntity> value4;
		bool value5;
		if (memberCount == 4)
		{
			if (value != null)
			{
				value2 = value.m_From;
				value3 = value.m_To;
				value4 = value.m_Owner;
				value5 = value.m_IsLoot;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				reader.ReadPackable(ref value4);
				reader.ReadUnmanaged<bool>(out value5);
				goto IL_0101;
			}
			value2 = reader.ReadPackable<ItemSlotRef>();
			value3 = reader.ReadPackable<ItemSlotRef>();
			value4 = reader.ReadPackable<EntityRef<MechanicEntity>>();
			reader.ReadUnmanaged<bool>(out value5);
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SwapSlotsGameCommand), 4, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = null;
				value4 = default(EntityRef<MechanicEntity>);
				value5 = false;
			}
			else
			{
				value2 = value.m_From;
				value3 = value.m_To;
				value4 = value.m_Owner;
				value5 = value.m_IsLoot;
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
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<bool>(out value5);
							_ = 4;
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_0101;
			}
		}
		value = new SwapSlotsGameCommand
		{
			m_From = value2,
			m_To = value3,
			m_Owner = value4,
			m_IsLoot = value5
		};
		return;
		IL_0101:
		value.m_From = value2;
		value.m_To = value3;
		value.m_Owner = value4;
		value.m_IsLoot = value5;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SwapSlotsGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_From");
		writer.WritePackable(value.m_From);
		writer.WriteProperty("m_To");
		writer.WritePackable(value.m_To);
		writer.WriteProperty("m_Owner");
		writer.WritePackable(value.m_Owner);
		writer.WriteProperty("m_IsLoot");
		writer.WriteUnmanaged(value.m_IsLoot);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SwapSlotsGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		ItemSlotRef val;
		ItemSlotRef val2;
		EntityRef<MechanicEntity> val3;
		bool v;
		if (value == null)
		{
			val = null;
			val2 = null;
			val3 = default(EntityRef<MechanicEntity>);
			v = false;
		}
		else
		{
			val = value.m_From;
			val2 = value.m_To;
			val3 = value.m_Owner;
			v = value.m_IsLoot;
		}
		bool[] array = new bool[4];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "m_From":
					val = reader.ReadPackable<ItemSlotRef>();
					array[0] = true;
					break;
				case "m_To":
					val2 = reader.ReadPackable<ItemSlotRef>();
					array[1] = true;
					break;
				case "m_Owner":
					val3 = reader.ReadPackable<EntityRef<MechanicEntity>>();
					array[2] = true;
					break;
				case "m_IsLoot":
					reader.ReadUnmanaged<bool>(out v);
					array[3] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "m_From":
					reader.ReadPackable(ref val);
					break;
				case "m_To":
					reader.ReadPackable(ref val2);
					break;
				case "m_Owner":
					reader.ReadPackable(ref val3);
					break;
				case "m_IsLoot":
					reader.ReadUnmanaged<bool>(out v);
					break;
				}
			}
		}
		if (value != null)
		{
			value.m_From = val;
			value.m_To = val2;
			value.m_Owner = val3;
			value.m_IsLoot = v;
		}
		else
		{
			value = new SwapSlotsGameCommand
			{
				m_From = val,
				m_To = val2,
				m_Owner = val3,
				m_IsLoot = v
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
		SwapSlotsGameCommand source = new SwapSlotsGameCommand();
		result = Unsafe.As<SwapSlotsGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SwapSlotsGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_From", ref m_From, state);
		formatter.Field(1, "m_To", ref m_To, state);
		formatter.Field(2, "m_Owner", ref m_Owner, state);
		formatter.UnmanagedField(3, "m_IsLoot", ref m_IsLoot, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SwapSlotsGameCommand>();
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
				m_From = formatter.ReadPackable<ItemSlotRef>(state);
				break;
			case 1:
				m_To = formatter.ReadPackable<ItemSlotRef>(state);
				break;
			case 2:
				m_Owner = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			case 3:
				m_IsLoot = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
