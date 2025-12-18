using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class EquipItemGameCommand : GameCommand, IOwlPackable<EquipItemGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<ItemEntity> m_Item;

	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<MechanicEntity> m_Entity;

	[JsonProperty]
	[OwlPackInclude]
	private ItemSlotRef m_To;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
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

	public override bool IsSynchronized => true;

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
