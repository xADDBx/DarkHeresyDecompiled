using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
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
public class ItemSlotRef : IMemoryPackable<ItemSlotRef>, IMemoryPackFormatterRegister, IOwlPackable, IOwlPackable<ItemSlotRef>
{
	[Preserve]
	private sealed class ItemSlotRefFormatter : MemoryPackFormatter<ItemSlotRef>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref ItemSlotRef value)
		{
			ItemSlotRef.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref ItemSlotRef value)
		{
			ItemSlotRef.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ItemSlotRef value)
		{
			ItemSlotRef.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref ItemSlotRef value)
		{
			ItemSlotRef.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private EquipSlotType m_SlotType;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private int m_SetIndex;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private int m_SlotIndex;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private EntityRef<ItemEntity> m_ItemRef;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private ItemsCollectionRef m_CollectionRef;

	public static readonly TypeInfo OwlPackTypeInfo;

	[MemoryPackIgnore]
	public EquipSlotType EquipSlotType => m_SlotType;

	[MemoryPackIgnore]
	public int SetIndex => m_SetIndex;

	[MemoryPackIgnore]
	public int SlotIndex => m_SlotIndex;

	[MemoryPackIgnore]
	public ItemEntity Item => m_ItemRef;

	[MemoryPackIgnore]
	public bool IsEquipment => m_SlotIndex == -1;

	[MemoryPackIgnore]
	public ItemsCollectionRef ItemsCollectionRef => m_CollectionRef;

	[MemoryPackIgnore]
	public ItemsCollection ItemsCollection => m_CollectionRef.ItemsCollection;

	[MemoryPackConstructor]
	private ItemSlotRef()
	{
	}

	[JsonConstructor]
	public ItemSlotRef(EquipSlotType slotType, int setIndex, int slotIndex, ItemEntity item, ItemsCollection collection)
	{
		m_SlotType = slotType;
		m_SetIndex = setIndex;
		m_SlotIndex = slotIndex;
		m_ItemRef = item;
		m_CollectionRef = collection.ToCollectionRef();
	}

	static ItemSlotRef()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "ItemSlotRef",
			OldNames = null,
			Fields = new FieldInfo[5]
			{
				new FieldInfo("m_SlotType", typeof(EquipSlotType)),
				new FieldInfo("m_SetIndex", typeof(int)),
				new FieldInfo("m_SlotIndex", typeof(int)),
				new FieldInfo("m_ItemRef", typeof(EntityRef<ItemEntity>)),
				new FieldInfo("m_CollectionRef", typeof(ItemsCollectionRef))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ItemSlotRef>())
		{
			MemoryPackFormatterProvider.Register(new ItemSlotRefFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ItemSlotRef[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ItemSlotRef>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EquipSlotType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<EquipSlotType>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref ItemSlotRef? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(5, in value.m_SlotType, in value.m_SetIndex, in value.m_SlotIndex);
		writer.WritePackable(in value.m_ItemRef);
		writer.WritePackable(in value.m_CollectionRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ItemSlotRef? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EquipSlotType value2;
		int value3;
		int value4;
		EntityRef<ItemEntity> value5;
		ItemsCollectionRef value6;
		if (memberCount == 5)
		{
			if (value != null)
			{
				value2 = value.m_SlotType;
				value3 = value.m_SetIndex;
				value4 = value.m_SlotIndex;
				value5 = value.m_ItemRef;
				value6 = value.m_CollectionRef;
				reader.ReadUnmanaged<EquipSlotType>(out value2);
				reader.ReadUnmanaged<int>(out value3);
				reader.ReadUnmanaged<int>(out value4);
				reader.ReadPackable(ref value5);
				reader.ReadPackable(ref value6);
				goto IL_0128;
			}
			reader.ReadUnmanaged<EquipSlotType, int, int>(out value2, out value3, out value4);
			value5 = reader.ReadPackable<EntityRef<ItemEntity>>();
			value6 = reader.ReadPackable<ItemsCollectionRef>();
		}
		else
		{
			if (memberCount > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ItemSlotRef), 5, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = EquipSlotType.PrimaryHand;
				value3 = 0;
				value4 = 0;
				value5 = default(EntityRef<ItemEntity>);
				value6 = null;
			}
			else
			{
				value2 = value.m_SlotType;
				value3 = value.m_SetIndex;
				value4 = value.m_SlotIndex;
				value5 = value.m_ItemRef;
				value6 = value.m_CollectionRef;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<EquipSlotType>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<int>(out value4);
						if (memberCount != 3)
						{
							reader.ReadPackable(ref value5);
							if (memberCount != 4)
							{
								reader.ReadPackable(ref value6);
								_ = 5;
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_0128;
			}
		}
		value = new ItemSlotRef
		{
			m_SlotType = value2,
			m_SetIndex = value3,
			m_SlotIndex = value4,
			m_ItemRef = value5,
			m_CollectionRef = value6
		};
		return;
		IL_0128:
		value.m_SlotType = value2;
		value.m_SetIndex = value3;
		value.m_SlotIndex = value4;
		value.m_ItemRef = value5;
		value.m_CollectionRef = value6;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref ItemSlotRef? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_SlotType");
		writer.WriteUnmanaged(value.m_SlotType);
		writer.WriteProperty("m_SetIndex");
		writer.WriteUnmanaged(value.m_SetIndex);
		writer.WriteProperty("m_SlotIndex");
		writer.WriteUnmanaged(value.m_SlotIndex);
		writer.WriteProperty("m_ItemRef");
		writer.WritePackable(value.m_ItemRef);
		writer.WriteProperty("m_CollectionRef");
		writer.WritePackable(value.m_CollectionRef);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref ItemSlotRef? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EquipSlotType v;
		int v2;
		int v3;
		EntityRef<ItemEntity> val;
		ItemsCollectionRef val2;
		if (value == null)
		{
			v = EquipSlotType.PrimaryHand;
			v2 = 0;
			v3 = 0;
			val = default(EntityRef<ItemEntity>);
			val2 = null;
		}
		else
		{
			v = value.m_SlotType;
			v2 = value.m_SetIndex;
			v3 = value.m_SlotIndex;
			val = value.m_ItemRef;
			val2 = value.m_CollectionRef;
		}
		bool[] array = new bool[5];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "m_SlotType":
					reader.ReadUnmanaged<EquipSlotType>(out v);
					array[0] = true;
					break;
				case "m_SetIndex":
					reader.ReadUnmanaged<int>(out v2);
					array[1] = true;
					break;
				case "m_SlotIndex":
					reader.ReadUnmanaged<int>(out v3);
					array[2] = true;
					break;
				case "m_ItemRef":
					val = reader.ReadPackable<EntityRef<ItemEntity>>();
					array[3] = true;
					break;
				case "m_CollectionRef":
					val2 = reader.ReadPackable<ItemsCollectionRef>();
					array[4] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "m_SlotType":
					reader.ReadUnmanaged<EquipSlotType>(out v);
					break;
				case "m_SetIndex":
					reader.ReadUnmanaged<int>(out v2);
					break;
				case "m_SlotIndex":
					reader.ReadUnmanaged<int>(out v3);
					break;
				case "m_ItemRef":
					reader.ReadPackable(ref val);
					break;
				case "m_CollectionRef":
					reader.ReadPackable(ref val2);
					break;
				}
			}
		}
		if (value != null)
		{
			value.m_SlotType = v;
			value.m_SetIndex = v2;
			value.m_SlotIndex = v3;
			value.m_ItemRef = val;
			value.m_CollectionRef = val2;
		}
		else
		{
			value = new ItemSlotRef
			{
				m_SlotType = v,
				m_SetIndex = v2,
				m_SlotIndex = v3,
				m_ItemRef = val,
				m_CollectionRef = val2
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
		ItemSlotRef source = new ItemSlotRef();
		result = Unsafe.As<ItemSlotRef, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<ItemSlotRef>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EnumField(0, "m_SlotType", ref m_SlotType, state);
		formatter.UnmanagedField(1, "m_SetIndex", ref m_SetIndex, state);
		formatter.UnmanagedField(2, "m_SlotIndex", ref m_SlotIndex, state);
		formatter.Field(3, "m_ItemRef", ref m_ItemRef, state);
		formatter.Field(4, "m_CollectionRef", ref m_CollectionRef, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ItemSlotRef>();
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
				m_SlotType = formatter.ReadEnum<EquipSlotType>(state);
				break;
			case 1:
				m_SetIndex = formatter.ReadUnmanaged<int>(state);
				break;
			case 2:
				m_SlotIndex = formatter.ReadUnmanaged<int>(state);
				break;
			case 3:
				m_ItemRef = formatter.ReadPackable<EntityRef<ItemEntity>>(state);
				break;
			case 4:
				m_CollectionRef = formatter.ReadPackable<ItemsCollectionRef>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
