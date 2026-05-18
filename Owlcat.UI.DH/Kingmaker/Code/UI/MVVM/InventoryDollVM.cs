using System;
using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Events;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class InventoryDollVM : CharInfoComponentVM, IInventoryItemHandler, ISubscriber, IEquipSlotHandler
{
	public EquipSlotVM Armor;

	public EquipSlotVM Belt;

	public EquipSlotVM Head;

	public EquipSlotVM Feet;

	public EquipSlotVM Gloves;

	public EquipSlotVM Neck;

	public EquipSlotVM Ring1;

	public EquipSlotVM Ring2;

	public EquipSlotVM Wrist;

	public EquipSlotVM Shoulders;

	public EquipSlotVM Glasses;

	public EquipSlotVM Shirt;

	public List<WeaponSetVM> WeaponSets;

	private readonly ReactiveProperty<WeaponSetVM> m_CurrentSet = new ReactiveProperty<WeaponSetVM>();

	private readonly ReactiveProperty<bool> m_CanChangeEquipment = new ReactiveProperty<bool>(value: true);

	private readonly AutoDisposingList<EquipSlotVM> m_AllEquipSlots = new AutoDisposingList<EquipSlotVM>();

	private readonly ReactiveProperty<CharacterVisualSettingsVM> m_VisualSettingsVM = new ReactiveProperty<CharacterVisualSettingsVM>();

	private readonly ReactiveProperty<InventorySelectorWindowVM> m_InventorySelectorWindowVM;

	private readonly ReactiveProperty<bool> m_ChooseSlotMode = new ReactiveProperty<bool>();

	private InventorySlotConsoleView m_ItemToSlotView;

	private readonly Action m_OnWeaponSetChangedAction;

	public readonly EquipSlotVM[] QuickSlots = new EquipSlotVM[2];

	public readonly bool IsPet;

	public SelectionGroupRadioVM<WeaponSetVM> WeaponSetSelector;

	public ReadOnlyReactiveProperty<CharacterVisualSettingsVM> VisualSettingsVM => m_VisualSettingsVM;

	public ReadOnlyReactiveProperty<InventorySelectorWindowVM> InventorySelectorWindowVM => m_InventorySelectorWindowVM;

	public ReadOnlyReactiveProperty<bool> ChooseSlotMode => m_ChooseSlotMode;

	public InventorySlotConsoleView ItemToSlotView => m_ItemToSlotView;

	public InventoryDollVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, Action onWeaponSetChanged)
		: base(unit)
	{
		m_OnWeaponSetChangedAction = onWeaponSetChanged;
		IsPet = unit?.CurrentValue != null && (unit.CurrentValue.Body.IsPolymorphed || unit.CurrentValue.IsPet);
		m_CurrentSet.Subscribe(OnWeaponSetChanged).AddTo(this);
		m_InventorySelectorWindowVM = new ReactiveProperty<InventorySelectorWindowVM>().AddTo(this);
		Disposable.Create(DisposeImplementation).AddTo(this);
	}

	private void DisposeImplementation()
	{
		m_ChooseSlotMode.Value = false;
		HideSelectionWindow();
	}

	public void SetIsChooseSlotMode(bool value)
	{
		m_ChooseSlotMode.Value = value;
	}

	private void OnWeaponSetChanged(WeaponSetVM set)
	{
		if (m_CanChangeEquipment.CurrentValue && Unit?.CurrentValue != null)
		{
			Game.Instance.GameCommandQueue.SwitchHandEquipment(Unit.CurrentValue, set.Index);
			m_OnWeaponSetChangedAction();
		}
	}

	private void HideSelectionWindow()
	{
		InventorySelectorWindowVM.CurrentValue?.Dispose();
		m_InventorySelectorWindowVM.Value = null;
	}

	public override void HandleUICommitChanges()
	{
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		if (Unit.CurrentValue == null)
		{
			return;
		}
		ClearEquipSlots();
		PartUnitBody body = Unit.CurrentValue.Body;
		Armor = new EquipSlotVM(EquipSlotType.Armor, body.Armor).AddTo(this);
		Belt = new EquipSlotVM(EquipSlotType.Belt, body.Belt).AddTo(this);
		Head = new EquipSlotVM(EquipSlotType.Head, body.Head).AddTo(this);
		Feet = new EquipSlotVM(EquipSlotType.Feet, body.Feet).AddTo(this);
		Gloves = new EquipSlotVM(EquipSlotType.Gloves, body.Gloves).AddTo(this);
		Neck = new EquipSlotVM(EquipSlotType.Neck, body.Neck).AddTo(this);
		Ring1 = new EquipSlotVM(EquipSlotType.Ring1, body.Ring1).AddTo(this);
		Ring2 = new EquipSlotVM(EquipSlotType.Ring2, body.Ring2).AddTo(this);
		Wrist = new EquipSlotVM(EquipSlotType.Wrist, body.Wrist).AddTo(this);
		Shoulders = new EquipSlotVM(EquipSlotType.Shoulders, body.Shoulders).AddTo(this);
		Glasses = new EquipSlotVM(EquipSlotType.Glasses, body.Glasses).AddTo(this);
		Shirt = new EquipSlotVM(EquipSlotType.Shirt, body.Shirt).AddTo(this);
		m_AllEquipSlots.Add(Armor);
		m_AllEquipSlots.Add(Belt);
		m_AllEquipSlots.Add(Head);
		m_AllEquipSlots.Add(Feet);
		m_AllEquipSlots.Add(Gloves);
		m_AllEquipSlots.Add(Neck);
		m_AllEquipSlots.Add(Ring1);
		m_AllEquipSlots.Add(Ring2);
		m_AllEquipSlots.Add(Wrist);
		m_AllEquipSlots.Add(Shoulders);
		m_AllEquipSlots.Add(Glasses);
		m_AllEquipSlots.Add(Shirt);
		for (int i = 0; i < 2; i++)
		{
			QuickSlots[i]?.Dispose();
			QuickSlots[i] = new EquipSlotVM(EquipSlotType.QuickSlot1, body.QuickSlots.ElementAt(i), -1, null, i).AddTo(this);
			m_AllEquipSlots.Add(QuickSlots[i]);
		}
		if (WeaponSets == null)
		{
			WeaponSets = new List<WeaponSetVM>();
			for (int j = 0; j < 2; j++)
			{
				int setId = j;
				WeaponSetVM item = new WeaponSetVM(j, delegate
				{
					m_CurrentSet.Value = WeaponSets.ElementAtOrDefault(setId);
				}).AddTo(this);
				WeaponSets.Add(item);
			}
			WeaponSetSelector = new SelectionGroupRadioVM<WeaponSetVM>(WeaponSets, m_CurrentSet).AddTo(this);
		}
		int weaponSetsCount = UIUtilityUnit.GetWeaponSetsCount(Unit.CurrentValue);
		for (int k = 0; k < WeaponSets.Count; k++)
		{
			WeaponSets[k].SetEnabled(k < weaponSetsCount);
			WeaponSets[k].Primary?.Dispose();
			WeaponSets[k].Primary = new EquipSlotVM(EquipSlotType.PrimaryHand, body.HandsEquipmentSets[k].PrimaryHand, -1, null, k).AddTo(this);
			m_AllEquipSlots.Add(WeaponSets[k].Primary);
			WeaponSets[k].Secondary?.Dispose();
			WeaponSets[k].Secondary = new EquipSlotVM(EquipSlotType.SecondaryHand, body.HandsEquipmentSets[k].SecondaryHand, -1, WeaponSets[k].Primary, k).AddTo(this);
			m_AllEquipSlots.Add(WeaponSets[k].Secondary);
		}
		m_CurrentSet.Value = WeaponSets[Unit.CurrentValue.Body.CurrentHandEquipmentSetIndex];
		UpdateVisualSettings();
	}

	private void ClearEquipSlots()
	{
		m_AllEquipSlots.Clear();
	}

	public void SwitchVisualSettings()
	{
		if (VisualSettingsVM.CurrentValue == null)
		{
			ShowVisualSettings();
		}
		else
		{
			HideVisualSettings();
		}
	}

	public void ShowVisualSettings()
	{
		ReactiveProperty<CharacterVisualSettingsVM> visualSettingsVM = m_VisualSettingsVM;
		if (visualSettingsVM.Value == null)
		{
			CharacterVisualSettingsVM characterVisualSettingsVM2 = (visualSettingsVM.Value = new CharacterVisualSettingsVM(Unit.CurrentValue, HideVisualSettings).AddTo(this));
		}
	}

	private void HideVisualSettings()
	{
		VisualSettingsVM.CurrentValue?.Dispose();
		m_VisualSettingsVM.Value = null;
	}

	private void UpdateVisualSettings()
	{
		if (VisualSettingsVM.CurrentValue != null && VisualSettingsVM.CurrentValue.Unit != Unit.CurrentValue)
		{
			HideVisualSettings();
		}
	}

	public void HandleChangeItem(EquipSlotVM slot)
	{
		if (ChooseSlotMode.CurrentValue && ItemToSlotView != null)
		{
			UIInventoryHelper.TryMoveSlotInInventory(ItemToSlotView.SlotVM, slot);
			EventBus.RaiseEvent(delegate(IInventoryHandler h)
			{
				h.Refresh();
			});
			m_ChooseSlotMode.Value = false;
		}
		else
		{
			List<EquipSelectorSlotVM> list = SetupSlots(Game.Instance.PartySharedInventory.Collection, slot);
			if (list.Count > 0)
			{
				ShowSelectorWindow(slot, list);
			}
		}
	}

	private static List<EquipSelectorSlotVM> SetupSlots(ItemsCollection itemsCollection, EquipSlotVM slot)
	{
		List<ItemEntity> list = itemsCollection.ToList();
		Comparison<ItemEntity> itemsDefaultComparison = ItemsFilter.GetItemsDefaultComparison(ItemsSorterType.TypeUp, ItemsFilterType.NoFilter);
		if (itemsDefaultComparison != null)
		{
			list.Sort(itemsDefaultComparison);
		}
		list.RemoveAll((ItemEntity item) => item != null && !ItemsFilter.ShouldShowItem(item, ItemsFilterType.NoFilter));
		list.RemoveAll((ItemEntity item) => item.HoldingSlot != null || !slot.ItemSlot.PossibleEquipItem(item));
		if (slot.HasItem)
		{
			list.Insert(0, slot.ItemEntity);
		}
		return list.Select((ItemEntity item) => new EquipSelectorSlotVM(item)).ToList();
	}

	private void ShowSelectorWindow(EquipSlotVM slot, List<EquipSelectorSlotVM> possibleItems)
	{
		InventorySelectorWindowVM.CurrentValue?.Dispose();
		m_InventorySelectorWindowVM.Value = new InventorySelectorWindowVM(TryInsertItem, HideSelectionWindow, possibleItems, slot);
		void TryInsertItem(EquipSelectorSlotVM selectorSlotVM)
		{
			if (UIUtilityCombat.IsCombatLockActive())
			{
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning(WarningNotificationType.EquipInCombatIsImpossible, null, addToLog: false, WarningNotificationFormat.Warning);
				});
			}
			else
			{
				BaseUnitEntity baseUnitEntity = (BaseUnitEntity)slot.ItemSlot.Owner;
				if (baseUnitEntity != null)
				{
					Game.Instance.GameCommandQueue.EquipItem(selectorSlotVM.Item, baseUnitEntity, slot.ToSlotRef());
					EventBus.RaiseEvent(delegate(IInventoryHandler h)
					{
						h.Refresh();
					});
					HideSelectionWindow();
				}
			}
		}
	}

	public void ChooseSlotToItem(InventorySlotConsoleView item)
	{
		if (UIUtilityCombat.IsCombatLockActive())
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.EquipInCombatIsImpossible, null, addToLog: false, WarningNotificationFormat.Warning);
			});
			item.ReleaseSlot();
		}
		int count = m_AllEquipSlots.Where((EquipSlotVM slotVM) => slotVM.ItemSlot.CanInsertItem(item.SlotVM.ItemEntity)).ToList().Count;
		if (count <= 1)
		{
			if (count == 1)
			{
				EventBus.RaiseEvent(delegate(IInventoryHandler h)
				{
					h.TryEquip(item.SlotVM);
				});
			}
			else
			{
				item.ReleaseSlot();
			}
		}
		else
		{
			m_ItemToSlotView = item;
			m_ChooseSlotMode.Value = true;
		}
	}
}
