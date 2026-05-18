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
public class TransferItemGameCommand : GameCommand, IMemoryPackable<TransferItemGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<TransferItemGameCommand>
{
	[Preserve]
	private sealed class TransferItemGameCommandFormatter : MemoryPackFormatter<TransferItemGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref TransferItemGameCommand value)
		{
			TransferItemGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref TransferItemGameCommand value)
		{
			TransferItemGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref TransferItemGameCommand value)
		{
			TransferItemGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref TransferItemGameCommand value)
		{
			TransferItemGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private ItemsCollectionRef m_To;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private EntityRef<ItemEntity> m_Item;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private int m_Count;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private TransferItemGameCommand()
	{
	}

	[JsonConstructor]
	public TransferItemGameCommand(ItemsCollectionRef to, ItemEntity item, int count)
	{
		m_To = to;
		m_Item = item;
		m_Count = count;
	}

	protected override void ExecuteInternal()
	{
		if (m_Item.Entity != null)
		{
			GameCommandHelper.TransferCount(m_To.ItemsCollection, m_Item, m_Count);
		}
	}

	static TransferItemGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "TransferItemGameCommand",
			OldNames = null,
			Fields = new FieldInfo[3]
			{
				new FieldInfo("m_To", typeof(ItemsCollectionRef)),
				new FieldInfo("m_Item", typeof(EntityRef<ItemEntity>)),
				new FieldInfo("m_Count", typeof(int))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<TransferItemGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new TransferItemGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<TransferItemGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<TransferItemGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref TransferItemGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_To);
		writer.WritePackable(in value.m_Item);
		writer.WriteUnmanaged(in value.m_Count);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref TransferItemGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		ItemsCollectionRef value2;
		EntityRef<ItemEntity> value3;
		int value4;
		if (memberCount == 3)
		{
			if (value != null)
			{
				value2 = value.m_To;
				value3 = value.m_Item;
				value4 = value.m_Count;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				reader.ReadUnmanaged<int>(out value4);
				goto IL_00ce;
			}
			value2 = reader.ReadPackable<ItemsCollectionRef>();
			value3 = reader.ReadPackable<EntityRef<ItemEntity>>();
			reader.ReadUnmanaged<int>(out value4);
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(TransferItemGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = default(EntityRef<ItemEntity>);
				value4 = 0;
			}
			else
			{
				value2 = value.m_To;
				value3 = value.m_Item;
				value4 = value.m_Count;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<int>(out value4);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00ce;
			}
		}
		value = new TransferItemGameCommand
		{
			m_To = value2,
			m_Item = value3,
			m_Count = value4
		};
		return;
		IL_00ce:
		value.m_To = value2;
		value.m_Item = value3;
		value.m_Count = value4;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref TransferItemGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_To");
		writer.WritePackable(value.m_To);
		writer.WriteProperty("m_Item");
		writer.WritePackable(value.m_Item);
		writer.WriteProperty("m_Count");
		writer.WriteUnmanaged(value.m_Count);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref TransferItemGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		ItemsCollectionRef val;
		EntityRef<ItemEntity> val2;
		int v;
		if (value == null)
		{
			val = null;
			val2 = default(EntityRef<ItemEntity>);
			v = 0;
		}
		else
		{
			val = value.m_To;
			val2 = value.m_Item;
			v = value.m_Count;
		}
		bool[] array = new bool[3];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "m_To":
					val = reader.ReadPackable<ItemsCollectionRef>();
					array[0] = true;
					break;
				case "m_Item":
					val2 = reader.ReadPackable<EntityRef<ItemEntity>>();
					array[1] = true;
					break;
				case "m_Count":
					reader.ReadUnmanaged<int>(out v);
					array[2] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "m_To":
					reader.ReadPackable(ref val);
					break;
				case "m_Item":
					reader.ReadPackable(ref val2);
					break;
				case "m_Count":
					reader.ReadUnmanaged<int>(out v);
					break;
				}
			}
		}
		if (value != null)
		{
			value.m_To = val;
			value.m_Item = val2;
			value.m_Count = v;
		}
		else
		{
			value = new TransferItemGameCommand
			{
				m_To = val,
				m_Item = val2,
				m_Count = v
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
		TransferItemGameCommand source = new TransferItemGameCommand();
		result = Unsafe.As<TransferItemGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<TransferItemGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_To", ref m_To, state);
		formatter.Field(1, "m_Item", ref m_Item, state);
		formatter.UnmanagedField(2, "m_Count", ref m_Count, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<TransferItemGameCommand>();
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
				m_To = formatter.ReadPackable<ItemsCollectionRef>(state);
				break;
			case 1:
				m_Item = formatter.ReadPackable<EntityRef<ItemEntity>>(state);
				break;
			case 2:
				m_Count = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
