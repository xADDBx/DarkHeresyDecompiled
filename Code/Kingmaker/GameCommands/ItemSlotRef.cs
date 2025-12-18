using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class ItemSlotRef : IOwlPackable, IOwlPackable<ItemSlotRef>
{
	[JsonProperty]
	[OwlPackInclude]
	private EquipSlotType m_SlotType;

	[JsonProperty]
	[OwlPackInclude]
	private int m_SetIndex;

	[JsonProperty]
	[OwlPackInclude]
	private int m_SlotIndex;

	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<ItemEntity> m_ItemRef;

	[JsonProperty]
	[OwlPackInclude]
	private ItemsCollectionRef m_CollectionRef;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
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

	public EquipSlotType EquipSlotType => m_SlotType;

	public int SetIndex => m_SetIndex;

	public int SlotIndex => m_SlotIndex;

	public ItemEntity Item => m_ItemRef;

	public bool IsEquipment => m_SlotIndex == -1;

	public ItemsCollectionRef ItemsCollectionRef => m_CollectionRef;

	public ItemsCollection ItemsCollection => m_CollectionRef.ItemsCollection;

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
