using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Gameplay.Features.Items.Parts;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.Animation;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Items.Slots;

[OwlPackable(OwlPackableMode.Generate)]
public class HandSlot : WeaponSlot, IHashable, IOwlPackable<HandSlot>
{
	private class SuppressNotifyEquipmentScope : ContextFlag<SuppressNotifyEquipmentScope>
	{
	}

	[OwlPackInclude]
	[CanBeNull]
	private ItemEntityWeapon _emptyHandWeapon;

	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "HandSlot",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("m_ItemRef", typeof(EntityRef<ItemEntity>)),
			new FieldInfo("m_Active", typeof(bool)),
			new FieldInfo("_emptyHandWeapon", typeof(ItemEntityWeapon))
		}
	};

	public bool IsDirty { get; set; }

	public override ItemEntityWeapon MaybeWeapon
	{
		get
		{
			ItemEntityWeapon itemEntityWeapon = base.MaybeWeapon;
			if (itemEntityWeapon == null)
			{
				if (_emptyHandWeapon?.Wielder == null)
				{
					return null;
				}
				itemEntityWeapon = _emptyHandWeapon;
			}
			return itemEntityWeapon;
		}
	}

	public bool HasShield => MaybeShield != null;

	[NotNull]
	public ItemEntityShield Shield
	{
		get
		{
			if (MaybeShield == null)
			{
				throw new Exception("Has no shield in slot");
			}
			return MaybeShield;
		}
	}

	[CanBeNull]
	public ItemEntityShield MaybeShield => base.MaybeItem as ItemEntityShield;

	public HandsEquipmentSet HandsEquipmentSet => base.Owner.GetBodyOptional()?.GetHandsEquipmentSet(this) ?? throw new Exception($"Can't find HandsEquipmentSet for HandSlot ({base.Owner})");

	public int HandsEquipmentSetIndex => base.Owner.GetBodyOptional()?.GetHandsEquipmentSetIndex(this) ?? throw new Exception($"Can't find HandsEquipmentSetIndex for HandSlot ({base.Owner})");

	public HandSlot PairSlot
	{
		get
		{
			HandsEquipmentSet handsEquipmentSet = HandsEquipmentSet;
			if (handsEquipmentSet.PrimaryHand != this)
			{
				return handsEquipmentSet.PrimaryHand;
			}
			return handsEquipmentSet.SecondaryHand;
		}
	}

	public bool IsPrimaryHand => HandsEquipmentSet.PrimaryHand == this;

	public override void UpdateActive()
	{
		PartUnitBody bodyOptional = base.Owner.GetBodyOptional();
		base.Active = !base.Disabled && (bodyOptional == null || HandsEquipmentSet == bodyOptional.CurrentHandsEquipmentSet);
	}

	public static IDisposable SuppressNotifyEquipment()
	{
		return ContextData<SuppressNotifyEquipmentScope>.Request();
	}

	protected override void OnActiveChanged()
	{
		base.OnActiveChanged();
		if (!ContextData<SuppressNotifyEquipmentScope>.Current && base.Owner?.View is UnitEntityView unitEntityView)
		{
			unitEntityView.HandsEquipment?.HandleEquipmentSlotUpdated(this, null);
		}
	}

	public override bool IsItemSupported(ItemEntity item)
	{
		if (!base.IsItemSupported(item) && (!(item is ItemEntityShield) || IsPrimaryHand))
		{
			return false;
		}
		ItemEntityWeapon itemEntityWeapon = item as ItemEntityWeapon;
		if (itemEntityWeapon != null && base.Owner.HasMechadendrites())
		{
			if (IsPrimaryHand && itemEntityWeapon.Blueprint.IsRanged)
			{
				return false;
			}
			if (!IsPrimaryHand && itemEntityWeapon.Blueprint.IsMelee)
			{
				return false;
			}
		}
		if (itemEntityWeapon != null && base.Owner.Facts.Contains(ConfigRoot.Instance.SystemMechanics.CommonSpaceMarineFact))
		{
			if (IsPrimaryHand && itemEntityWeapon.Blueprint.IsMelee)
			{
				return false;
			}
			if (!IsPrimaryHand && itemEntityWeapon.Blueprint.IsRanged)
			{
				return false;
			}
		}
		if (itemEntityWeapon == null || !itemEntityWeapon.HoldInTwoHands)
		{
			ItemEntityWeapon maybeWeapon = PairSlot.MaybeWeapon;
			if (maybeWeapon == null || !maybeWeapon.HoldInTwoHands)
			{
				goto IL_00fb;
			}
		}
		if (PairSlot.HasItem && !PairSlot.CanRemoveItem())
		{
			return false;
		}
		goto IL_00fb;
		IL_00fb:
		return true;
	}

	public HandSlot(BaseUnitEntity owner)
		: base(owner)
	{
	}

	public HandSlot(JsonConstructorMark _)
		: base(_)
	{
	}

	protected HandSlot()
	{
	}

	protected override void OnItemInserted()
	{
		if (!IsPrimaryHand)
		{
			HandSlot primaryHand = HandsEquipmentSet.PrimaryHand;
			if (primaryHand.MaybeItem is ItemEntityWeapon { HoldInTwoHands: not false })
			{
				primaryHand.RemoveItem();
			}
		}
		if (base.MaybeItem is ItemEntityWeapon { HoldInTwoHands: not false } itemEntityWeapon2)
		{
			if (IsPrimaryHand)
			{
				PairSlot.RemoveItem();
			}
			else
			{
				RemoveItem();
				PairSlot.InsertItem(itemEntityWeapon2);
			}
		}
		UpdateEmptyHandWeapon(raiseEvent: false);
		PairSlot.UpdateEmptyHandWeapon(raiseEvent: true);
		IsDirty = true;
	}

	protected override void OnItemRemoved()
	{
		UpdateEmptyHandWeapon(raiseEvent: false);
		PairSlot.UpdateEmptyHandWeapon(raiseEvent: true);
	}

	public override bool RemoveItem(bool autoMerge = true, bool force = false)
	{
		if (base.RemoveItem(autoMerge, force))
		{
			return IsDirty = true;
		}
		return false;
	}

	public override WeaponType GetWeaponType(bool isDollRoom = false)
	{
		if (!HasShield)
		{
			return base.GetWeaponType(isDollRoom);
		}
		return WeaponType.Shield;
	}

	protected override void OnPostLoad()
	{
		if (_emptyHandWeapon != null)
		{
			_emptyHandWeapon.HoldingSlot = this;
			_emptyHandWeapon.PostLoad();
		}
	}

	private void UpdateEmptyHandWeapon(bool raiseEvent, bool updateHands = true)
	{
		if (updateHands)
		{
			base.Owner.GetOrCreate<PartEmptyHandWeapons>().UpdateHands();
		}
		if (_emptyHandWeapon == null)
		{
			return;
		}
		bool flag = false;
		bool flag2 = base.MaybeItem == null && (!(PairSlot.MaybeItem is ItemEntityWeapon itemEntityWeapon) || !itemEntityWeapon.HoldInTwoHands);
		if (flag2 && _emptyHandWeapon.Wielder == null)
		{
			_emptyHandWeapon.OnDidEquipped(base.Owner);
			flag = true;
		}
		else if (!flag2 && _emptyHandWeapon.Wielder != null)
		{
			_emptyHandWeapon.OnWillUnequip();
			flag = true;
		}
		if (flag && raiseEvent)
		{
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitEquipmentHandler>)delegate(IUnitEquipmentHandler h)
			{
				h.HandleEquipmentSlotUpdated(this, null);
			}, isCheckRuntime: true);
		}
	}

	public void SetEmptyHandWeapon([CanBeNull] BlueprintItemWeapon blueprint)
	{
		if (_emptyHandWeapon?.Blueprint == blueprint)
		{
			return;
		}
		if (_emptyHandWeapon != null)
		{
			if (_emptyHandWeapon?.Wielder != null)
			{
				_emptyHandWeapon.OnWillUnequip();
			}
			_emptyHandWeapon?.Dispose();
			_emptyHandWeapon = null;
		}
		if (blueprint != null)
		{
			string emptyHandWeaponPersistentId = GetEmptyHandWeaponPersistentId(blueprint);
			_emptyHandWeapon = Entity.Initialize(new ItemEntityWeapon(blueprint, emptyHandWeaponPersistentId, fake: true));
			_emptyHandWeapon.HoldingSlot = this;
			UpdateEmptyHandWeapon(raiseEvent: true, updateHands: false);
		}
	}

	private string GetEmptyHandWeaponPersistentId(BlueprintItemWeapon blueprint)
	{
		PartUnitBody required = base.Owner.GetRequired<PartUnitBody>();
		int num = required.GetHandsEquipmentSetIndex(this) ?? throw new NullReferenceException();
		int num2 = ((required.GetHandsEquipmentSet(this)?.PrimaryHand != this) ? 1 : 0);
		return $"empty_hand_weapon_{num}_{num2}#{blueprint.AssetGuid}#{base.Owner.UniqueId}";
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
		HandSlot source = new HandSlot();
		result = Unsafe.As<HandSlot, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<HandSlot>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_ItemRef", ref m_ItemRef, state);
		formatter.UnmanagedField(1, "m_Active", ref m_Active, state);
		formatter.Field(2, "_emptyHandWeapon", ref _emptyHandWeapon, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<HandSlot>();
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
			case 2:
				_emptyHandWeapon = formatter.ReadPackable<ItemEntityWeapon>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
