using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Items.Slots;

[OwlPackable(OwlPackableMode.Generate)]
public class EquipmentSlot<TBlueprintItem> : ItemSlot, IHashable, IOwlPackable<EquipmentSlot<TBlueprintItem>> where TBlueprintItem : BlueprintItemEquipment
{
	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "EquipmentSlot",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_ItemRef", typeof(EntityRef<ItemEntity>)),
			new FieldInfo("m_Active", typeof(bool))
		}
	};

	public EquipmentSlot(BaseUnitEntity owner)
		: base(owner)
	{
	}

	public EquipmentSlot(JsonConstructorMark _)
		: base(_)
	{
	}

	protected EquipmentSlot()
	{
	}

	public override bool IsItemSupported(ItemEntity item)
	{
		MechanicEntity owner = base.Owner;
		if (owner != null && owner.IsInCombat && !Game.Instance.Controllers.TurnController.IsPreparationTurn)
		{
			return false;
		}
		return item?.Blueprint is TBlueprintItem;
	}

	public override bool CanRemoveItem()
	{
		MechanicEntity owner = base.Owner;
		if (owner != null && owner.IsInCombat && !Game.Instance.Controllers.TurnController.IsPreparationTurn)
		{
			return false;
		}
		return base.CanRemoveItem();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		EquipmentSlot<TBlueprintItem> source = new EquipmentSlot<TBlueprintItem>();
		result = Unsafe.As<EquipmentSlot<TBlueprintItem>, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<EquipmentSlot<TBlueprintItem>>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_ItemRef", ref m_ItemRef, state);
		formatter.UnmanagedField(1, "m_Active", ref m_Active, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EquipmentSlot<TBlueprintItem>>();
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
				m_ItemRef = formatter.ReadPackable<EntityRef<ItemEntity>>(state);
				break;
			case 1:
				m_Active = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
