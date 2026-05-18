using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Networking;
using Kingmaker.View.MapObjects;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class TriggerLootGameCommand : GameCommandWithSynchronized, IMemoryPackable<TriggerLootGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<TriggerLootGameCommand>
{
	public enum TriggerType : byte
	{
		None,
		Put,
		Take,
		Close
	}

	[Preserve]
	private sealed class TriggerLootGameCommandFormatter : MemoryPackFormatter<TriggerLootGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref TriggerLootGameCommand value)
		{
			TriggerLootGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref TriggerLootGameCommand value)
		{
			TriggerLootGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref TriggerLootGameCommand value)
		{
			TriggerLootGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref TriggerLootGameCommand value)
		{
			TriggerLootGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly EntityPartRef<Entity, InteractionLootPart> m_InteractionLootPartRef;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly byte m_Type;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly EntityRef<ItemEntity> m_Item;

	public static readonly TypeInfo OwlPackTypeInfo;

	[JsonConstructor]
	private TriggerLootGameCommand()
	{
	}

	[MemoryPackConstructor]
	private TriggerLootGameCommand(EntityPartRef<Entity, InteractionLootPart> m_interactionLootPartRef, byte m_type, EntityRef<ItemEntity> m_item)
	{
		m_InteractionLootPartRef = m_interactionLootPartRef;
		m_Type = m_type;
		m_Item = m_item;
	}

	public TriggerLootGameCommand(InteractionLootPart interactionLootPart, TriggerType type, ItemEntity item)
		: this(interactionLootPart, (byte)type, item)
	{
		m_IsSynchronized = !ContextData<GameCommandContext>.Current && !ContextData<UnitCommandContext>.Current;
	}

	protected override void ExecuteInternal()
	{
		InteractionLootPart entityPart = m_InteractionLootPartRef.EntityPart;
		if (entityPart != null)
		{
			switch ((TriggerType)m_Type)
			{
			case TriggerType.Put:
				entityPart.HandleItemsAddedImplementation(m_Item.Entity);
				break;
			case TriggerType.Take:
				entityPart.HandleItemsRemovedImplementation(m_Item.Entity);
				break;
			case TriggerType.Close:
				entityPart.OnLootClosedImplementation();
				break;
			default:
				throw new ArgumentOutOfRangeException(string.Format("{0}={1}", "m_Type", m_Type));
			}
		}
	}

	static TriggerLootGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "TriggerLootGameCommand",
			OldNames = null,
			Fields = new FieldInfo[3]
			{
				new FieldInfo("m_InteractionLootPartRef", typeof(EntityPartRef<Entity, InteractionLootPart>)),
				new FieldInfo("m_Type", typeof(byte)),
				new FieldInfo("m_Item", typeof(EntityRef<ItemEntity>))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<TriggerLootGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new TriggerLootGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<TriggerLootGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<TriggerLootGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref TriggerLootGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_InteractionLootPartRef);
		writer.WriteUnmanaged(in value.m_Type);
		writer.WritePackable(in value.m_Item);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref TriggerLootGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityPartRef<Entity, InteractionLootPart> value2;
		byte value3;
		EntityRef<ItemEntity> value4;
		if (memberCount == 3)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityPartRef<Entity, InteractionLootPart>>();
				reader.ReadUnmanaged<byte>(out value3);
				value4 = reader.ReadPackable<EntityRef<ItemEntity>>();
			}
			else
			{
				value2 = value.m_InteractionLootPartRef;
				value3 = value.m_Type;
				value4 = value.m_Item;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<byte>(out value3);
				reader.ReadPackable(ref value4);
			}
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(TriggerLootGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityPartRef<Entity, InteractionLootPart>);
				value3 = 0;
				value4 = default(EntityRef<ItemEntity>);
			}
			else
			{
				value2 = value.m_InteractionLootPartRef;
				value3 = value.m_Type;
				value4 = value.m_Item;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<byte>(out value3);
					if (memberCount != 2)
					{
						reader.ReadPackable(ref value4);
						_ = 3;
					}
				}
			}
			_ = value;
		}
		value = new TriggerLootGameCommand(value2, value3, value4);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref TriggerLootGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_InteractionLootPartRef");
		writer.WritePackable(value.m_InteractionLootPartRef);
		writer.WriteProperty("m_Type");
		writer.WriteUnmanaged(value.m_Type);
		writer.WriteProperty("m_Item");
		writer.WritePackable(value.m_Item);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref TriggerLootGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityPartRef<Entity, InteractionLootPart> val;
		byte v;
		EntityRef<ItemEntity> val2;
		if (value == null)
		{
			val = default(EntityPartRef<Entity, InteractionLootPart>);
			v = 0;
			val2 = default(EntityRef<ItemEntity>);
		}
		else
		{
			val = value.m_InteractionLootPartRef;
			v = value.m_Type;
			val2 = value.m_Item;
		}
		bool[] array = new bool[3];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "m_InteractionLootPartRef":
					val = reader.ReadPackable<EntityPartRef<Entity, InteractionLootPart>>();
					array[0] = true;
					break;
				case "m_Type":
					reader.ReadUnmanaged<byte>(out v);
					array[1] = true;
					break;
				case "m_Item":
					val2 = reader.ReadPackable<EntityRef<ItemEntity>>();
					array[2] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "m_InteractionLootPartRef":
					reader.ReadPackable(ref val);
					break;
				case "m_Type":
					reader.ReadUnmanaged<byte>(out v);
					break;
				case "m_Item":
					reader.ReadPackable(ref val2);
					break;
				}
			}
		}
		_ = value;
		value = new TriggerLootGameCommand(val, v, val2);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		TriggerLootGameCommand source = new TriggerLootGameCommand();
		result = Unsafe.As<TriggerLootGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<TriggerLootGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityPartRef<Entity, InteractionLootPart> value = m_InteractionLootPartRef;
		formatter.Field(0, "m_InteractionLootPartRef", ref value, state);
		byte value2 = m_Type;
		formatter.UnmanagedField(1, "m_Type", ref value2, state);
		EntityRef<ItemEntity> value3 = m_Item;
		formatter.Field(2, "m_Item", ref value3, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<TriggerLootGameCommand>();
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
				Unsafe.AsRef(in m_InteractionLootPartRef) = formatter.ReadPackable<EntityPartRef<Entity, InteractionLootPart>>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_Type) = formatter.ReadUnmanaged<byte>(state);
				break;
			case 2:
				Unsafe.AsRef(in m_Item) = formatter.ReadPackable<EntityRef<ItemEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
