using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.Mechadendrites;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EquipSlotVM : ItemSlotVM, IEquipSlotPossibleTarget, ISubscriber<ItemEntity>, ISubscriber, IInsertItemHandler, IEquipSlotHoverHandler, INetRoleSetHandler
{
	private readonly ReactiveProperty<bool> m_CanBeFakeItem = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_NetLock = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsNotRemovable = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<ItemEntity> m_FakeItem = new ReactiveProperty<ItemEntity>();

	private readonly ItemSlot m_ItemSlot;

	public readonly EquipSlotType SlotType;

	public readonly EquipSlotSubtype SlotSubtype;

	public ReadOnlyReactiveProperty<bool> CanBeFakeItem => m_CanBeFakeItem;

	public ReadOnlyReactiveProperty<bool> NetLock => m_NetLock;

	public ReadOnlyReactiveProperty<bool> IsNotRemovable => m_IsNotRemovable;

	public ItemSlot ItemSlot => m_ItemSlot;

	public int SetIndex { get; }

	public EquipSlotVM(EquipSlotType slotType, ItemSlot itemSlot, int index = -1, EquipSlotVM primaryHand = null, int setIndex = -1)
		: base(GetItemFromSlot(itemSlot), index)
	{
		SlotType = slotType;
		m_ItemSlot = itemSlot;
		SetIndex = setIndex;
		if (itemSlot.Owner.HasMechadendrites() && slotType == EquipSlotType.SecondaryHand)
		{
			SlotSubtype = EquipSlotSubtype.Ranged;
		}
		if (primaryHand != null)
		{
			m_FakeItem = primaryHand.m_Item;
			AddDisposable(m_FakeItem.Subscribe(delegate
			{
				m_Icon.Value = GetIcon();
			}));
			AddDisposable(m_FakeItem.CombineLatest(base.Item, (ItemEntity fake, ItemEntity item) => new { fake, item }).Subscribe(value =>
			{
				m_CanBeFakeItem.Value = value.fake != null && value.item == null;
			}));
		}
		m_NetLock.Value = itemSlot.Owner is BaseUnitEntity entry && !entry.CanBeControlled();
		AddDisposable(base.Item.Subscribe(delegate(ItemEntity item)
		{
			m_IsNotRemovable.Value = item?.IsNonRemovable ?? false;
		}));
	}

	private static ItemEntity GetItemFromSlot(ItemSlot itemSlot)
	{
		object obj = itemSlot?.MaybeItem;
		if (obj == null)
		{
			WeaponSlot obj2 = itemSlot as WeaponSlot;
			if (obj2 == null)
			{
				return null;
			}
			obj = obj2.MaybeWeapon;
		}
		return (ItemEntity)obj;
	}

	protected override Sprite GetIcon()
	{
		ItemEntity value = m_FakeItem.Value;
		if (base.HasItem || !(value is ItemEntityWeapon { HoldInTwoHands: not false } itemEntityWeapon))
		{
			return base.GetIcon();
		}
		if (!itemEntityWeapon.HoldInTwoHands)
		{
			return value.Icon;
		}
		return itemEntityWeapon.Icon;
	}

	protected override void DisposeImplementation()
	{
		m_FakeItem.Value = null;
		base.DisposeImplementation();
	}

	public void HandleHighlightStart(ItemEntity item)
	{
		m_IsPossibleHighlighted = IsPossibleTarget(item);
		UpdatePossibleTarget();
	}

	public override void HandleHighlightStop()
	{
		m_IsPossibleHighlighted = false;
		UpdatePossibleTarget();
	}

	public void HandleHoverStart(ItemEntity item)
	{
		m_IsPossibleHovered = IsPossibleTarget(item);
		UpdatePossibleTarget();
	}

	public override void HandleHoverStop()
	{
		m_IsPossibleHovered = false;
		UpdatePossibleTarget();
	}

	private bool IsPossibleTarget(ItemEntity item)
	{
		if (m_ItemSlot.CanInsertItem(item))
		{
			if (m_ItemSlot.HasItem)
			{
				return m_ItemSlot.CanRemoveItem();
			}
			return true;
		}
		return false;
	}

	private void UpdatePossibleTarget()
	{
		m_PossibleTarget.Value = m_IsPossibleHighlighted || m_IsPossibleHovered;
	}

	void IInsertItemHandler.HandleInsertItem(ItemSlot slot)
	{
		if (m_ItemSlot == slot)
		{
			m_Item.Value = GetItemFromSlot(slot);
		}
	}

	public void HandleRoleSet(string entityId)
	{
		if (!(ItemSlot.Owner.UniqueId != entityId))
		{
			m_NetLock.Value = ItemSlot.Owner is BaseUnitEntity entry && !entry.CanBeControlled();
		}
	}
}
