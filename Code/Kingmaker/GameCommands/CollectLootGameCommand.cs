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
public class CollectLootGameCommand : GameCommand, IMemoryPackable<CollectLootGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CollectLootGameCommand>
{
	[Preserve]
	private sealed class CollectLootGameCommandFormatter : MemoryPackFormatter<CollectLootGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CollectLootGameCommand value)
		{
			CollectLootGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CollectLootGameCommand value)
		{
			CollectLootGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CollectLootGameCommand value)
		{
			CollectLootGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CollectLootGameCommand value)
		{
			CollectLootGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private List<EntityRef<ItemEntity>> m_Items;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private CollectLootGameCommand()
	{
	}

	[JsonConstructor]
	public CollectLootGameCommand(List<EntityRef<ItemEntity>> items)
	{
		m_Items = items;
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.TryCollect(m_Items);
	}

	static CollectLootGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CollectLootGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_Items", typeof(List<EntityRef<ItemEntity>>))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CollectLootGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CollectLootGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CollectLootGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CollectLootGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<EntityRef<ItemEntity>>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<EntityRef<ItemEntity>>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CollectLootGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		ListFormatter.SerializePackable(ref writer, value.m_Items);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CollectLootGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		List<EntityRef<ItemEntity>> value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_Items;
				ListFormatter.DeserializePackable(ref reader, ref value2);
				goto IL_006a;
			}
			value2 = ListFormatter.DeserializePackable<EntityRef<ItemEntity>>(ref reader);
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CollectLootGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Items : null);
			if (memberCount != 0)
			{
				ListFormatter.DeserializePackable(ref reader, ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006a;
			}
		}
		value = new CollectLootGameCommand
		{
			m_Items = value2
		};
		return;
		IL_006a:
		value.m_Items = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CollectLootGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Items");
		ListFormatter.SerializePackableJson(ref writer, value.m_Items);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CollectLootGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		List<EntityRef<ItemEntity>> value2 = ((value != null) ? value.m_Items : null);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_Items")
				{
					value2 = ListFormatter.DeserializePackableJson<EntityRef<ItemEntity>>(ref reader);
					array[0] = true;
				}
			}
			else if (text == "m_Items")
			{
				ListFormatter.DeserializePackableJson(ref reader, ref value2);
			}
		}
		if (value != null)
		{
			value.m_Items = value2;
		}
		else
		{
			value = new CollectLootGameCommand
			{
				m_Items = value2
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
		CollectLootGameCommand source = new CollectLootGameCommand();
		result = Unsafe.As<CollectLootGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CollectLootGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Items", ref m_Items, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CollectLootGameCommand>();
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
				m_Items = formatter.ReadPackable<List<EntityRef<ItemEntity>>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
