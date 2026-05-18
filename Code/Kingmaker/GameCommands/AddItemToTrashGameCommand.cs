using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class AddItemToTrashGameCommand : GameCommand, IMemoryPackable<AddItemToTrashGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<AddItemToTrashGameCommand>
{
	[Preserve]
	private sealed class AddItemToTrashGameCommandFormatter : MemoryPackFormatter<AddItemToTrashGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref AddItemToTrashGameCommand value)
		{
			AddItemToTrashGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref AddItemToTrashGameCommand value)
		{
			AddItemToTrashGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref AddItemToTrashGameCommand value)
		{
			AddItemToTrashGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref AddItemToTrashGameCommand value)
		{
			AddItemToTrashGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private EntityRef<ItemEntity> m_Item;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	public AddItemToTrashGameCommand()
	{
	}

	[JsonConstructor]
	public AddItemToTrashGameCommand(ItemEntity item)
	{
		m_Item = new EntityRef<ItemEntity>(item);
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.AddItemToTrash(m_Item);
	}

	static AddItemToTrashGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "AddItemToTrashGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_Item", typeof(EntityRef<ItemEntity>))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<AddItemToTrashGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new AddItemToTrashGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AddItemToTrashGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<AddItemToTrashGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref AddItemToTrashGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_Item);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref AddItemToTrashGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<ItemEntity> value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_Item;
				reader.ReadPackable(ref value2);
				goto IL_0070;
			}
			value2 = reader.ReadPackable<EntityRef<ItemEntity>>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(AddItemToTrashGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Item : default(EntityRef<ItemEntity>));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_0070;
			}
		}
		value = new AddItemToTrashGameCommand
		{
			m_Item = value2
		};
		return;
		IL_0070:
		value.m_Item = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref AddItemToTrashGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Item");
		writer.WritePackable(value.m_Item);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref AddItemToTrashGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityRef<ItemEntity> val = ((value != null) ? value.m_Item : default(EntityRef<ItemEntity>));
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_Item")
				{
					val = reader.ReadPackable<EntityRef<ItemEntity>>();
					array[0] = true;
				}
			}
			else if (text == "m_Item")
			{
				reader.ReadPackable(ref val);
			}
		}
		if (value != null)
		{
			value.m_Item = val;
		}
		else
		{
			value = new AddItemToTrashGameCommand
			{
				m_Item = val
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
		AddItemToTrashGameCommand source = new AddItemToTrashGameCommand();
		result = Unsafe.As<AddItemToTrashGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AddItemToTrashGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Item", ref m_Item, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AddItemToTrashGameCommand>();
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
			}
		}
		formatter.LeaveObject();
	}
}
