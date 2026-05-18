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
public class DropItemGameCommand : GameCommand, IMemoryPackable<DropItemGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<DropItemGameCommand>
{
	[Preserve]
	private sealed class DropItemGameCommandFormatter : MemoryPackFormatter<DropItemGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DropItemGameCommand value)
		{
			DropItemGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref DropItemGameCommand value)
		{
			DropItemGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DropItemGameCommand value)
		{
			DropItemGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref DropItemGameCommand value)
		{
			DropItemGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private EntityRef<ItemEntity> m_Item;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private bool m_Split;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private int m_SplitCount;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private DropItemGameCommand()
	{
	}

	[JsonConstructor]
	public DropItemGameCommand(ItemEntity item, bool split, int splitCount)
	{
		m_Item = new EntityRef<ItemEntity>(item);
		m_Split = split;
		m_SplitCount = splitCount;
	}

	protected override void ExecuteInternal()
	{
		if (m_Item.Entity != null)
		{
			GameCommandHelper.DropItem(m_Item.Entity, m_Split, m_SplitCount);
		}
	}

	static DropItemGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "DropItemGameCommand",
			OldNames = null,
			Fields = new FieldInfo[3]
			{
				new FieldInfo("m_Item", typeof(EntityRef<ItemEntity>)),
				new FieldInfo("m_Split", typeof(bool)),
				new FieldInfo("m_SplitCount", typeof(int))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DropItemGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new DropItemGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DropItemGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<DropItemGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DropItemGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_Item);
		writer.WriteUnmanaged(in value.m_Split, in value.m_SplitCount);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DropItemGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<ItemEntity> value2;
		bool value3;
		int value4;
		if (memberCount == 3)
		{
			if (value != null)
			{
				value2 = value.m_Item;
				value3 = value.m_Split;
				value4 = value.m_SplitCount;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<bool>(out value3);
				reader.ReadUnmanaged<int>(out value4);
				goto IL_00c9;
			}
			value2 = reader.ReadPackable<EntityRef<ItemEntity>>();
			reader.ReadUnmanaged<bool, int>(out value3, out value4);
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DropItemGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<ItemEntity>);
				value3 = false;
				value4 = 0;
			}
			else
			{
				value2 = value.m_Item;
				value3 = value.m_Split;
				value4 = value.m_SplitCount;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<int>(out value4);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00c9;
			}
		}
		value = new DropItemGameCommand
		{
			m_Item = value2,
			m_Split = value3,
			m_SplitCount = value4
		};
		return;
		IL_00c9:
		value.m_Item = value2;
		value.m_Split = value3;
		value.m_SplitCount = value4;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref DropItemGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Item");
		writer.WritePackable(value.m_Item);
		writer.WriteProperty("m_Split");
		writer.WriteUnmanaged(value.m_Split);
		writer.WriteProperty("m_SplitCount");
		writer.WriteUnmanaged(value.m_SplitCount);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref DropItemGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityRef<ItemEntity> val;
		bool v;
		int v2;
		if (value == null)
		{
			val = default(EntityRef<ItemEntity>);
			v = false;
			v2 = 0;
		}
		else
		{
			val = value.m_Item;
			v = value.m_Split;
			v2 = value.m_SplitCount;
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
				case "m_Split":
					reader.ReadUnmanaged<bool>(out v);
					array[1] = true;
					break;
				case "m_SplitCount":
					reader.ReadUnmanaged<int>(out v2);
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
				case "m_Split":
					reader.ReadUnmanaged<bool>(out v);
					break;
				case "m_SplitCount":
					reader.ReadUnmanaged<int>(out v2);
					break;
				}
			}
		}
		if (value != null)
		{
			value.m_Item = val;
			value.m_Split = v;
			value.m_SplitCount = v2;
		}
		else
		{
			value = new DropItemGameCommand
			{
				m_Item = val,
				m_Split = v,
				m_SplitCount = v2
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
		DropItemGameCommand source = new DropItemGameCommand();
		result = Unsafe.As<DropItemGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<DropItemGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Item", ref m_Item, state);
		formatter.UnmanagedField(1, "m_Split", ref m_Split, state);
		formatter.UnmanagedField(2, "m_SplitCount", ref m_SplitCount, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DropItemGameCommand>();
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
				m_Split = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_SplitCount = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
