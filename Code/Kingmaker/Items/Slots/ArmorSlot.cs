using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Items.Slots;

[OwlPackable(OwlPackableMode.Generate)]
public class ArmorSlot : ItemSlot, IHashable, IOwlPackable<ArmorSlot>
{
	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ArmorSlot",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_ItemRef", typeof(EntityRef<ItemEntity>)),
			new FieldInfo("m_Active", typeof(bool))
		}
	};

	public bool HasArmor => HasItem;

	[NotNull]
	public ItemEntityArmor Armor => (ItemEntityArmor)base.Item;

	[CanBeNull]
	public ItemEntityArmor MaybeArmor => (ItemEntityArmor)base.MaybeItem;

	public override bool IsItemSupported(ItemEntity item)
	{
		if (base.Owner == null || (base.Owner.IsInCombat && !ContextData<IgnoreLock>.Current))
		{
			return false;
		}
		if (item is ItemEntityArmor)
		{
			return item.CanBeEquippedBy(base.Owner);
		}
		return false;
	}

	public override bool CanRemoveItem()
	{
		if (base.Owner == null || (base.Owner.IsInCombat && !ContextData<IgnoreLock>.Current))
		{
			return false;
		}
		return base.CanRemoveItem();
	}

	public ArmorSlot(BaseUnitEntity owner)
		: base(owner)
	{
	}

	public ArmorSlot(JsonConstructorMark _)
		: base(_)
	{
	}

	protected ArmorSlot()
	{
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
		ArmorSlot source = new ArmorSlot();
		result = Unsafe.As<ArmorSlot, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ArmorSlot>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_ItemRef", ref m_ItemRef, state);
		formatter.UnmanagedField(1, "m_Active", ref m_Active, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ArmorSlot>();
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
