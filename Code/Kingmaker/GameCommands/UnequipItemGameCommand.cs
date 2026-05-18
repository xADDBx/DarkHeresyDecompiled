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
public class UnequipItemGameCommand : GameCommand, IMemoryPackable<UnequipItemGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<UnequipItemGameCommand>
{
	[Preserve]
	private sealed class UnequipItemGameCommandFormatter : MemoryPackFormatter<UnequipItemGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref UnequipItemGameCommand value)
		{
			UnequipItemGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref UnequipItemGameCommand value)
		{
			UnequipItemGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UnequipItemGameCommand value)
		{
			UnequipItemGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref UnequipItemGameCommand value)
		{
			UnequipItemGameCommand.DeserializeJson(ref reader, ref value);
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

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private UnequipItemGameCommand()
	{
	}

	[JsonConstructor]
	public UnequipItemGameCommand(MechanicEntity owner, ItemSlotRef from, ItemSlotRef to)
	{
		m_From = from;
		m_To = to;
		m_Owner = new EntityRef<MechanicEntity>(owner);
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.UnequipItem(m_Owner, m_From, m_To);
	}

	static UnequipItemGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "UnequipItemGameCommand",
			OldNames = null,
			Fields = new FieldInfo[3]
			{
				new FieldInfo("m_From", typeof(ItemSlotRef)),
				new FieldInfo("m_To", typeof(ItemSlotRef)),
				new FieldInfo("m_Owner", typeof(EntityRef<MechanicEntity>))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UnequipItemGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new UnequipItemGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UnequipItemGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UnequipItemGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref UnequipItemGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_From);
		writer.WritePackable(in value.m_To);
		writer.WritePackable(in value.m_Owner);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref UnequipItemGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		ItemSlotRef value2;
		ItemSlotRef value3;
		EntityRef<MechanicEntity> value4;
		if (memberCount == 3)
		{
			if (value != null)
			{
				value2 = value.m_From;
				value3 = value.m_To;
				value4 = value.m_Owner;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				reader.ReadPackable(ref value4);
				goto IL_00cd;
			}
			value2 = reader.ReadPackable<ItemSlotRef>();
			value3 = reader.ReadPackable<ItemSlotRef>();
			value4 = reader.ReadPackable<EntityRef<MechanicEntity>>();
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UnequipItemGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = null;
				value4 = default(EntityRef<MechanicEntity>);
			}
			else
			{
				value2 = value.m_From;
				value3 = value.m_To;
				value4 = value.m_Owner;
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
				goto IL_00cd;
			}
		}
		value = new UnequipItemGameCommand
		{
			m_From = value2,
			m_To = value3,
			m_Owner = value4
		};
		return;
		IL_00cd:
		value.m_From = value2;
		value.m_To = value3;
		value.m_Owner = value4;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref UnequipItemGameCommand? value)
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
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref UnequipItemGameCommand? value)
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
		if (value == null)
		{
			val = null;
			val2 = null;
			val3 = default(EntityRef<MechanicEntity>);
		}
		else
		{
			val = value.m_From;
			val2 = value.m_To;
			val3 = value.m_Owner;
		}
		bool[] array = new bool[3];
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
				}
			}
		}
		if (value != null)
		{
			value.m_From = val;
			value.m_To = val2;
			value.m_Owner = val3;
		}
		else
		{
			value = new UnequipItemGameCommand
			{
				m_From = val,
				m_To = val2,
				m_Owner = val3
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
		UnequipItemGameCommand source = new UnequipItemGameCommand();
		result = Unsafe.As<UnequipItemGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnequipItemGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_From", ref m_From, state);
		formatter.Field(1, "m_To", ref m_To, state);
		formatter.Field(2, "m_Owner", ref m_Owner, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnequipItemGameCommand>();
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
			}
		}
		formatter.LeaveObject();
	}
}
