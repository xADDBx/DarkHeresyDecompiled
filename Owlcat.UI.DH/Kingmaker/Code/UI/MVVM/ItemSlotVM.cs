using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Gameplay.Features.Vendor;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Events;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ItemSlotVM : VirtualListElementVMBase, IInventoryChangedHandler, ISubscriber, ISplitItemHandler, IEquipItemHandler, ISubscriber<IItemEntity>, IInventorySlotHoverHandler, ISubscriber<ItemEntity>, IInventorySlotPossibleTarget
{
	private enum VendorItemLayerType
	{
		FearReputation,
		RespectReputation,
		FearReputationLocked,
		RespectReputationLocked,
		ItemNoFactionVendor
	}

	private ItemEntity m_LastItem;

	protected bool m_IsPossibleHighlighted;

	protected bool m_IsPossibleHovered;

	public int Index;

	public bool CompareTooltipEnabled;

	private static readonly List<TooltipBaseTemplate> DefaultTooltipList = new List<TooltipBaseTemplate>();

	private readonly ReactiveProperty<bool> m_CanUse = new ReactiveProperty<bool>();

	private readonly ReactiveCommand<Unit> m_UpdateView = new ReactiveCommand<Unit>();

	private readonly ReactiveProperty<int> m_Count = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_UsableCount = new ReactiveProperty<int>();

	private readonly ReactiveProperty<double> m_ItemPrice = new ReactiveProperty<double>();

	private readonly ReactiveProperty<double> m_ItemPriceNoDiscount = new ReactiveProperty<double>();

	private readonly ReactiveProperty<bool> m_IsNotable = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsTrash = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_MarkedAsTrash = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsUsable = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasInteractions = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<string> m_TypeName = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<string> m_DisplayName = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<ItemGrade> m_ItemGrade = new ReactiveProperty<ItemGrade>(Kingmaker.Code.UI.MVVM.ItemGrade.Common);

	private readonly ReactiveProperty<ItemStatus> m_ItemStatus = new ReactiveProperty<ItemStatus>(Kingmaker.Code.UI.MVVM.ItemStatus.None);

	private readonly ReactiveProperty<ItemTagType> m_ItemTagType = new ReactiveProperty<ItemTagType>(Kingmaker.Code.UI.MVVM.ItemTagType.None);

	private readonly ReactiveProperty<List<TooltipBaseTemplate>> m_Tooltip = new ReactiveProperty<List<TooltipBaseTemplate>>();

	private readonly ReactiveProperty<List<ContextMenuCollectionEntity>> m_ContextMenu = new ReactiveProperty<List<ContextMenuCollectionEntity>>();

	private readonly ReactiveCommand<Unit> m_NeedBlink = new ReactiveCommand<Unit>();

	protected readonly ReactiveProperty<ItemEntity> m_Item = new ReactiveProperty<ItemEntity>(null);

	protected readonly ReactiveProperty<bool> m_PossibleTarget = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<Sprite> m_Icon = new ReactiveProperty<Sprite>(null);

	public ReadOnlyReactiveProperty<ItemEntity> Item => m_Item;

	public ReadOnlyReactiveProperty<Sprite> Icon => m_Icon;

	public ReadOnlyReactiveProperty<int> Count => m_Count;

	public ReadOnlyReactiveProperty<int> UsableCount => m_UsableCount;

	public ReadOnlyReactiveProperty<double> ItemPrice => m_ItemPrice;

	public ReadOnlyReactiveProperty<double> ItemPriceNoDiscount => m_ItemPriceNoDiscount;

	public ReadOnlyReactiveProperty<bool> IsNotable => m_IsNotable;

	public ReadOnlyReactiveProperty<bool> IsTrash => m_IsTrash;

	public ReadOnlyReactiveProperty<bool> MarkedAsTrash => m_MarkedAsTrash;

	public ReadOnlyReactiveProperty<bool> IsUsable => m_IsUsable;

	public ReadOnlyReactiveProperty<bool> HasInteractions => m_HasInteractions;

	public ReadOnlyReactiveProperty<string> TypeName => m_TypeName;

	public ReadOnlyReactiveProperty<string> DisplayName => m_DisplayName;

	public ReadOnlyReactiveProperty<ItemGrade> ItemGrade => m_ItemGrade;

	public ReadOnlyReactiveProperty<ItemStatus> ItemStatus => m_ItemStatus;

	public ReadOnlyReactiveProperty<ItemTagType> ItemTagType => m_ItemTagType;

	public ReadOnlyReactiveProperty<bool> PossibleTarget => m_PossibleTarget;

	public ReadOnlyReactiveProperty<List<TooltipBaseTemplate>> Tooltip => m_Tooltip;

	public ReadOnlyReactiveProperty<List<ContextMenuCollectionEntity>> ContextMenu => m_ContextMenu;

	public Observable<Unit> UpdateView => m_UpdateView;

	public Observable<Unit> NeedBlink => m_NeedBlink;

	public ItemEntity ItemEntity => Item.CurrentValue;

	public ItemEntityWeapon ItemWeapon => ItemEntity as ItemEntityWeapon;

	public ISlotsGroupVM Group { get; }

	public ItemSlotsGroupType SlotsGroupType => Group?.Type ?? ItemSlotsGroupType.Unknown;

	public string ReputationLockedHintText { get; }

	public bool IsReadableItem
	{
		get
		{
			ItemEntity itemEntity = ItemEntity;
			if (itemEntity != null && !(itemEntity.Blueprint is BlueprintItemEquipment))
			{
				return !string.IsNullOrEmpty(ItemEntity.Blueprint.FlavorText);
			}
			return false;
		}
	}

	public ItemsCollection ParentCollection => Item.CurrentValue?.Collection;

	public bool HasItem => Item.CurrentValue != null;

	public bool CanDropItem
	{
		get
		{
			if (HasItem)
			{
				return !ItemEntity.IsNotable;
			}
			return false;
		}
	}

	public bool CanEquip
	{
		get
		{
			if (HasItem && UIUtilityItem.CanEquipItem(Item.CurrentValue))
			{
				return IsInStash;
			}
			return false;
		}
	}

	public bool IsEquipPossible
	{
		get
		{
			if (HasItem && UIUtilityItem.IsEquipPossible(Item.CurrentValue))
			{
				return IsInStash;
			}
			return false;
		}
	}

	public bool IsSplitPossible
	{
		get
		{
			if (HasItem)
			{
				return Item.CurrentValue.Count > 1;
			}
			return false;
		}
	}

	public bool IsInStash => ParentCollection == Game.Instance.PartySharedInventory.Collection;

	public bool IsInVendor => ParentCollection.IsVendorTable;

	public bool IsLockedByRep => !VendorHelper.TradeLogic.IsReputationRestrictionPassed(ItemEntity);

	public bool IsLockedByCost => !VendorHelper.TradeLogic.IsCostRestrictionPassed(ItemEntity);

	public bool IsItemSoldToVendor => VendorHelper.TradeLogic.IsItemSoldToVendor(ItemEntity);

	public bool IsItemUnavailable => !VendorHelper.TradeLogic.IsRestrictionPassed(ItemEntity);

	public int GetVendorItemButtonLayerNumber => GetButtonLayerType(ItemEntity);

	public int ReputationValueToUnlock => (ItemEntity?.GetReputationRestriction()?.Value).GetValueOrDefault();

	public bool HasDiscount => VendorHelper.TradeLogic.HasDiscount;

	public ItemSlotVM(ItemEntity item, int index, ISlotsGroupVM group = null, bool compareTooltipEnabled = true)
	{
		Item.Subscribe(ItemChangedHandler).AddTo(this);
		Game.Instance.Controllers.SelectionCharacter.SelectedUnitInUI.Subscribe(delegate
		{
			Observable.NextFrame().Subscribe(OnUnitChanged);
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		ReputationLockedHintText = UIStrings.Instance.Vendor.ReputationLockHint;
		CompareTooltipEnabled = compareTooltipEnabled;
		m_Item.Value = item;
		Index = index;
		Group = group;
	}

	public void SetContextMenu(List<ContextMenuCollectionEntity> contextMenuEntities)
	{
		m_ContextMenu.Value = contextMenuEntities;
	}

	protected override void DisposeImplementation()
	{
		m_Item.Value = null;
	}

	protected virtual void ItemChangedHandler(ItemEntity item)
	{
		bool flag = item != null && !item.IsDisposed;
		m_Icon.Value = GetIcon();
		m_ItemPrice.Value = (flag ? VendorHelper.TradeLogic.GetItemCost(item) : 0);
		if (VendorHelper.TradeLogic.IsTrading && HasDiscount)
		{
			m_ItemPriceNoDiscount.Value = ((item != null) ? VendorHelper.TradeLogic.GetBaseItemCost(ItemEntity) : 0);
		}
		m_IsNotable.Value = flag && item.Blueprint.IsNotable;
		m_Count.Value = (flag ? item.Count : 0);
		m_UsableCount.Value = (flag ? item.Charges : 0);
		UpdateItemGrade();
		UpdateItemTag();
		UpdateHasInteractions();
		bool canUse = GetCanUse();
		m_ItemStatus.Value = GetItemStatus(canUse, IsTrash.CurrentValue);
		m_IsUsable.Value = GetIsUsable();
		m_IsTrash.Value = false;
		m_TypeName.Value = (flag ? UIUtilityItem.GetItemType(item) : string.Empty);
		ReactiveProperty<string> displayName = m_DisplayName;
		LocalizedString localizedString = item?.Blueprint?.LocalizedName;
		displayName.Value = ((localizedString != null) ? ((string)localizedString) : string.Empty);
		m_Tooltip.Value = GetTooltips();
		m_UpdateView.Execute();
	}

	public void UpdateTooltips(bool force = false)
	{
		m_Tooltip.Value = GetTooltips(force);
	}

	private void OnUnitChanged()
	{
		bool canUse = GetCanUse();
		m_ItemStatus.Value = GetItemStatus(canUse, IsTrash.CurrentValue);
		m_CanUse.Value = GetCanUse();
		m_IsUsable.Value = GetIsUsable();
		UpdateTooltips(force: true);
		m_UpdateView.Execute();
	}

	private List<TooltipBaseTemplate> GetTooltips(bool force = false)
	{
		if (ItemEntity == null)
		{
			return DefaultTooltipList;
		}
		List<TooltipBaseTemplate> list = new List<TooltipBaseTemplate>();
		if (CompareTooltipEnabled && GetType() == typeof(ItemSlotVM) && !IsUsable.CurrentValue)
		{
			List<ItemSlot> list2 = Game.Instance.Controllers.SelectionCharacter.SelectedUnitInUI.Value?.Body.EquipmentSlots.Where(IsSlotAllowedToCompare).EmptyIfNull().ToList();
			if (list2 != null && list2.Count > 2)
			{
				list2 = list2.Where((ItemSlot i) => i.Active).ToList();
			}
			List<TooltipTemplateItem> list3 = new List<TooltipTemplateItem>();
			if (list2 != null && list2.Count >= 4)
			{
				for (int j = 0; j < list2.Count - 1; j += 2)
				{
					List<ItemEntity> fewItems = new List<ItemEntity>
					{
						GetItemToCompare(list2[j]),
						GetItemToCompare(list2[j + 1])
					};
					list3.Add(new TooltipTemplateItem(null, ItemEntity, force, replenishing: false, fewItems));
				}
				if (list2.Count % 2 != 0)
				{
					ItemEntity itemToCompare = GetItemToCompare(list2.Last());
					list3.Add(new TooltipTemplateItem(itemToCompare, ItemEntity, force));
				}
			}
			else
			{
				list3 = list2?.Select((ItemSlot i) => new TooltipTemplateItem(GetItemToCompare(i), ItemEntity, force)).ToList();
			}
			if (list3 != null)
			{
				list.AddRange(list3);
			}
		}
		list.Add(new TooltipTemplateItem(ItemEntity, null, force));
		return list;
	}

	private bool IsSlotAllowedToCompare(ItemSlot slot)
	{
		ItemEntity itemToCompare = GetItemToCompare(slot);
		if (itemToCompare == null || !slot.IsItemSupported(ItemEntity) || ItemEntity is ItemEntityUsable)
		{
			return false;
		}
		if (itemToCompare is ItemEntityWeapon itemEntityWeapon && ItemEntity is ItemEntityWeapon itemEntityWeapon2)
		{
			return itemEntityWeapon.Blueprint.IsRanged == itemEntityWeapon2.Blueprint.IsRanged;
		}
		return true;
	}

	private static ItemEntity GetItemToCompare(ItemSlot slot)
	{
		object obj = slot?.MaybeItem;
		if (obj == null)
		{
			WeaponSlot obj2 = slot as WeaponSlot;
			if (obj2 == null)
			{
				return null;
			}
			obj = obj2.MaybeWeapon;
		}
		return (ItemEntity)obj;
	}

	protected virtual Sprite GetIcon()
	{
		if (!HasItem)
		{
			return null;
		}
		return UIUtilityItem.GetItemIcon(ItemEntity);
	}

	private ItemStatus GetItemStatus(bool canUse, bool isTrash)
	{
		if (!canUse)
		{
			return Kingmaker.Code.UI.MVVM.ItemStatus.Unsuitable;
		}
		if (isTrash)
		{
			return Kingmaker.Code.UI.MVVM.ItemStatus.Uncollectible;
		}
		return Kingmaker.Code.UI.MVVM.ItemStatus.None;
	}

	private bool GetCanUse()
	{
		if (ItemEntity != null)
		{
			if (!UIUtilityItem.GetEquipPosibility(ItemEntity)[0] && ItemEntity.Blueprint.ItemType != ItemsItemType.Other && ItemEntity.Blueprint.ItemType != ItemsItemType.NonUsable && ItemEntity.Blueprint.ItemType != ItemsItemType.ColonyFoundation)
			{
				return ItemEntity.Blueprint.ItemType == ItemsItemType.ResourceMiner;
			}
			return true;
		}
		return false;
	}

	private bool GetIsUsable()
	{
		if (HasItem && Item.CurrentValue.IsUsableFromInventory)
		{
			return Item.CurrentValue.GetBestAvailableUser() != null;
		}
		return false;
	}

	private ItemGrade GetItemGrade()
	{
		if (ItemEntity == null)
		{
			return Kingmaker.Code.UI.MVVM.ItemGrade.Common;
		}
		if (ItemEntity.Blueprint.IsNotable)
		{
			return Kingmaker.Code.UI.MVVM.ItemGrade.Quest;
		}
		return ItemEntity.Blueprint.Rarity switch
		{
			BlueprintItem.ItemRarity.Trash => Kingmaker.Code.UI.MVVM.ItemGrade.Trash, 
			BlueprintItem.ItemRarity.Lore => Kingmaker.Code.UI.MVVM.ItemGrade.Lore, 
			BlueprintItem.ItemRarity.Pattern => Kingmaker.Code.UI.MVVM.ItemGrade.Pattern, 
			BlueprintItem.ItemRarity.Refined => Kingmaker.Code.UI.MVVM.ItemGrade.Refined, 
			BlueprintItem.ItemRarity.Quest => Kingmaker.Code.UI.MVVM.ItemGrade.Quest, 
			BlueprintItem.ItemRarity.Unique => Kingmaker.Code.UI.MVVM.ItemGrade.Unique, 
			_ => Kingmaker.Code.UI.MVVM.ItemGrade.Common, 
		};
	}

	private ItemTagType GetItemTag()
	{
		if (MarkedAsTrash.CurrentValue)
		{
			return Kingmaker.Code.UI.MVVM.ItemTagType.MarkedAsTrash;
		}
		return Kingmaker.Code.UI.MVVM.ItemTagType.None;
	}

	private void RemoveItem()
	{
		m_Item.Value = null;
	}

	private void UpdateItem()
	{
		UpdateItemGrade();
		UpdateItemTag();
		UpdateHasInteractions();
		m_Item.ForceNotify();
	}

	public void SetItem(ItemEntity item)
	{
		m_Item.Value = item;
		item?.SetSlotIndex(Index);
		UpdateItem();
	}

	public void VendorTryMove(bool split)
	{
		if (CanBuy())
		{
			VendorHelper.TryMove(ItemEntity, ParentCollection, split);
		}
	}

	public void VendorTryBuyAll()
	{
		if (CanBuy())
		{
			Game.Instance.GameCommandQueue.AddForBuyVendor(ItemEntity, ItemEntity.Count);
		}
	}

	public void Blink()
	{
		m_NeedBlink?.Execute();
	}

	private bool CanBuy()
	{
		if (!HasItem)
		{
			return false;
		}
		if (IsLockedByRep)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.Vendor.CantBuyItem.Text + ". " + UIStrings.Instance.Vendor.NotEnoughReputation.Text, addToLog: false, WarningNotificationFormat.Attention);
			});
			return false;
		}
		return true;
	}

	public void ShowInfo()
	{
		TooltipHelper.ShowInfo(Tooltip.CurrentValue?.LastItem());
	}

	public void MarkAsTrashLoot(bool isMarkedAsTrash)
	{
		if (isMarkedAsTrash)
		{
			Game.Instance.VendorsManager.AddToTrash(Item.CurrentValue);
		}
		else
		{
			Game.Instance.VendorsManager.RemoveFromTrash(Item.CurrentValue);
		}
		m_MarkedAsTrash.Value = isMarkedAsTrash;
		UpdateItemTag();
		m_UpdateView.Execute();
		EventBus.RaiseEvent(delegate(IRefreshVisibleCollectionHandler h)
		{
			h.Refresh();
		});
	}

	private void UpdateItemGrade()
	{
		m_ItemGrade.Value = GetItemGrade();
	}

	private void UpdateItemTag()
	{
		if (Item.CurrentValue == null)
		{
			m_MarkedAsTrash.Value = false;
			return;
		}
		m_MarkedAsTrash.Value = Game.Instance.VendorsManager.IsMarkedAsTrash(Item.CurrentValue);
		m_ItemTagType.Value = GetItemTag();
	}

	private void UpdateHasInteractions()
	{
		m_HasInteractions.Value = ItemEntity?.UIInteractions.Any() ?? false;
	}

	void IInventoryChangedHandler.HandleSetItem(ItemEntity item, int oldItemIndex)
	{
		if (item.InventorySlotIndex == Index && item.Collection == Group?.MechanicCollection)
		{
			m_Item.Value = item;
			UpdateItem();
		}
		if (item.InventorySlotIndex != oldItemIndex && oldItemIndex == Index && item.Collection == Group?.MechanicCollection)
		{
			m_Item.Value = null;
			UpdateItem();
		}
	}

	void ISplitItemHandler.HandleBeforeSplitItem(ItemEntity item, ItemsCollection from, ItemsCollection to)
	{
		if (ItemEntity == item && from == to && Group != null)
		{
			Group.SetSorterType(ItemsSorterType.NotSorted);
		}
	}

	void ISplitItemHandler.HandleAfterSplitItem(ItemEntity item)
	{
		if (ItemEntity == item)
		{
			Group?.UpdateVisibleCollection();
		}
	}

	void ISplitItemHandler.HandleSplitItem()
	{
	}

	void IInventoryChangedHandler.HandleUpdateItem(ItemEntity item, ItemsCollection collection, int index)
	{
		if (Index == index && Group?.MechanicCollection == collection)
		{
			m_Item.Value = item;
			UpdateItem();
		}
	}

	void IInventoryChangedHandler.HandleRemoveItem(ItemEntity item, ItemsCollection collection, int oldIndex)
	{
		if (Index == oldIndex && Item.CurrentValue == item && Group?.MechanicCollection == collection)
		{
			RemoveItem();
		}
	}

	void IEquipItemHandler.OnDidEquipped()
	{
		HandleItemEquip();
	}

	void IEquipItemHandler.OnWillUnequip()
	{
		HandleItemEquip();
	}

	private void HandleItemEquip()
	{
		if (EventInvokerExtensions.GetEntity<ItemEntity>() == ItemEntity)
		{
			UpdateTooltips(force: true);
		}
	}

	public void HandleHighlightStart(ItemSlot slot)
	{
		m_IsPossibleHighlighted = IsPossibleTarget(slot);
		UpdatePossibleTarget();
	}

	public virtual void HandleHighlightStop()
	{
		m_IsPossibleHighlighted = false;
		UpdatePossibleTarget();
	}

	public void HandleHoverStart(ItemSlot slot)
	{
		m_IsPossibleHovered = IsPossibleTarget(slot);
		UpdatePossibleTarget();
	}

	public virtual void HandleHoverStop()
	{
		m_IsPossibleHovered = false;
		UpdatePossibleTarget();
	}

	private bool IsPossibleTarget(ItemSlot slot)
	{
		if (slot.CanInsertItem(Item.CurrentValue))
		{
			return CanHighLight(Item.CurrentValue);
		}
		return false;
	}

	private void UpdatePossibleTarget()
	{
		m_PossibleTarget.Value = m_IsPossibleHighlighted || m_IsPossibleHovered;
	}

	public void SplitItem(int count)
	{
		Item.CurrentValue?.Split(count);
		Group.TriggerCollectionChanged();
	}

	private bool CanHighLight(ItemEntity _)
	{
		return true;
	}

	private int GetButtonLayerType(ItemEntity itemEntity)
	{
		TradeLogic tradeLogic = Game.Instance.TradeLogic;
		if (tradeLogic.ShowSoldItemsToVendorFilter.Value)
		{
			return 4;
		}
		if (tradeLogic.VendorFactionType == FactionType.None)
		{
			return 4;
		}
		ReputationRestriction reputationRestriction = itemEntity?.GetReputationRestriction();
		if (reputationRestriction != null)
		{
			ReputationType type = reputationRestriction.Type;
			if (reputationRestriction.IsPassed(tradeLogic.VendorFactionType))
			{
				return type switch
				{
					ReputationType.Respect => 1, 
					ReputationType.Fear => 0, 
					_ => 4, 
				};
			}
			return type switch
			{
				ReputationType.Respect => 3, 
				ReputationType.Fear => 2, 
				_ => 4, 
			};
		}
		return 4;
	}

	public void Interact()
	{
		ItemEntity.UIInteractions.ForEach(delegate(ItemUIInteraction i)
		{
			i.Interact(ItemEntity, Game.Instance.Controllers.SelectionCharacter.SelectedUnitInUI.Value);
		});
	}
}
