using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.DialogSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Events;
using Kingmaker.UI.Sound;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class LootVM : ViewModel, INewSlotsHandler, ISubscriber, ICollectLootHandler, IMoveItemHandler, IFullScreenUIHandler, IInventoryHandler, ILootHandler, ITransferItemHandler, ISplitItemHandler
{
	public readonly LootCollectorVM LootCollectorExitLocation;

	public readonly LootCollectorVM LootCollectorOnLocation;

	public readonly InventoryStashVM InventoryStash;

	public readonly InteractionSlotPartVM InteractionSlot;

	public readonly PlayerStashVM PlayerStash;

	private readonly ReactiveProperty<ExitLocationWindowVM> m_ExitLocationWindowVM = new ReactiveProperty<ExitLocationWindowVM>();

	private readonly ReactiveCommand<Unit> m_LootUpdated = new ReactiveCommand<Unit>();

	private readonly ReactiveProperty<bool> m_NoLoot = new ReactiveProperty<bool>();

	private readonly List<ItemsCollection> m_ItemsCollections;

	private readonly ItemEntity m_CatchInteractionSlotItemEntity;

	private readonly Action m_AreaTransitionCallback;

	private Action m_CloseCallback;

	private bool m_AllCollected;

	private bool m_ContextLootIsDirty;

	private readonly ReactiveProperty<bool> m_ExtendedView = new ReactiveProperty<bool>();

	public LootWindowMode Mode { get; }

	public ObservableList<LootObjectVM> ContextLoot { get; } = new ObservableList<LootObjectVM>();


	public SkillCheckResult SkillCheckResult { get; }

	public ReadOnlyReactiveProperty<ExitLocationWindowVM> ExitLocationWindowVM => m_ExitLocationWindowVM;

	public Observable<Unit> LootUpdated => m_LootUpdated;

	public ReadOnlyReactiveProperty<bool> NoLoot => m_NoLoot;

	private IEnumerable<ItemEntity> AllItems => m_ItemsCollections.SelectMany((ItemsCollection collection) => collection.Items);

	private IEnumerable<ItemEntity> LootableItems => AllItems.Where(IsLootable);

	public bool IsOneSlot => InteractionSlot != null;

	public bool IsPlayerStash => PlayerStash != null;

	public ReadOnlyReactiveProperty<bool> ExtendedView => m_ExtendedView;

	private static bool IsLootable(ItemEntity item)
	{
		if (!item.IsAvailable())
		{
			return false;
		}
		if (!item.IsLootable)
		{
			return false;
		}
		ItemSlot holdingSlot = item.HoldingSlot;
		if (holdingSlot != null && !holdingSlot.CanRemoveItem())
		{
			return false;
		}
		return true;
	}

	private LootVM(LootWindowMode mode, IEnumerable<ItemsCollection> lootCollections, Action closeCallback, Action leaveZoneAction = null, Func<ItemEntity, bool> canInsertItem = null, SkillCheckResult skillCheckResult = null)
	{
		LootVM lootVM = this;
		Disposable.Create(DisposeImplementation).AddTo(this);
		m_AllCollected = false;
		Mode = mode;
		SkillCheckResult = skillCheckResult;
		m_CloseCallback = closeCallback;
		m_AreaTransitionCallback = leaveZoneAction;
		m_ExtendedView.Value = Game.Instance.Player.UISettings.LootExtendedView;
		m_ItemsCollections = new List<ItemsCollection>(lootCollections);
		if (Mode != LootWindowMode.OneSlot)
		{
			var (displayName, description) = GetLootObjectStrings(LootObjectType.Normal);
			AddLootObject(new LootObjectVM(LootObjectType.Normal, displayName, description, null, m_ItemsCollections.First(), LootableItems, Mode));
		}
		else
		{
			var (displayName2, description2) = GetLootObjectStrings(LootObjectType.SingleSlot);
			AddLootObject(new LootObjectVM(LootObjectType.Normal, displayName2, description2, null, m_ItemsCollections.First(), null, Mode));
		}
		m_NoLoot.Value = LootableItems.Empty();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, lootVM.GetFullScreenUIType(mode));
		});
		EventBus.Subscribe(this).AddTo(this);
		switch (mode)
		{
		case LootWindowMode.OneSlot:
			InteractionSlot = new InteractionSlotPartVM(ContextLoot.FirstItem(), canInsertItem, Close, CloseOneSlot).AddTo(this);
			m_CatchInteractionSlotItemEntity = InteractionSlot.Group.Items?.FirstOrDefault();
			break;
		case LootWindowMode.PlayerChest:
			PlayerStash = new PlayerStashVM(this).AddTo(this);
			break;
		case LootWindowMode.ZoneExit:
			LootCollectorExitLocation = new LootCollectorVM(this).AddTo(this);
			break;
		default:
			LootCollectorOnLocation = new LootCollectorVM(this).AddTo(this);
			break;
		}
		ItemsSortingContext sortingCtx = new ItemsSortingContext
		{
			SorterType = Game.Instance.Player.UISettings.InventorySorter,
			FilterType = Game.Instance.Player.UISettings.InventoryFilter,
			RemoveEmptySlots = true
		};
		InsertableLootSlotsGroupVM slotsGroup = new InsertableLootSlotsGroupVM(Game.Instance.PartySharedInventory.Collection, canInsertItem, 9, 81, sortingCtx, Game.Instance.Player.UISettings.ShowUnavailableItems).AddTo(this);
		InventoryStash = new InventoryStashVM(slotsGroup).AddTo(this);
		Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate).Subscribe(Update).AddTo(this);
		UIEventType eventType = mode switch
		{
			LootWindowMode.Short => UIEventType.LootShortOpen, 
			LootWindowMode.ShortUnit => UIEventType.LootShortUnitOpen, 
			LootWindowMode.ZoneExit => UIEventType.LootZoneExitOpen, 
			LootWindowMode.PlayerChest => UIEventType.LootPlayerChestOpen, 
			LootWindowMode.StandardChest => UIEventType.LootStandardChestOpen, 
			LootWindowMode.OneSlot => UIEventType.LootOneSlotOpen, 
			_ => UIEventType.LootShortOpen, 
		};
		EventBus.RaiseEvent(delegate(IUIEventHandler h)
		{
			h.HandleUIEvent(eventType);
		});
		void AddLootObject(LootObjectVM lootObject)
		{
			lootVM.ContextLoot.Add(lootObject);
			lootObject.AddTo(lootVM);
			OwlcatR3UnitExtensions.Subscribe(lootObject.SlotsGroup.CollectionChangedCommand, delegate
			{
				lootVM.m_LootUpdated.Execute(Unit.Default);
			}).AddTo(lootVM);
		}
	}

	public LootVM(LootWindowMode mode, EntityViewBase[] objects, Action closeCallback)
		: this(mode, GetLootCollections(objects), closeCallback, null, GetCanInsertItemPred(mode, objects))
	{
	}

	public LootVM(LootWindowMode mode, ILootable[] objects, LootContainerType _, Action closeCallback, SkillCheckResult skillCheckResult)
		: this(mode, GetLootCollections(objects), closeCallback, null, null, skillCheckResult)
	{
		if (NoLoot.CurrentValue)
		{
			LeaveZone();
		}
	}

	public LootVM(LootWindowMode mode, IEnumerable<LootWrapper> zoneLoot, Action leaveZoneAction, Action closeCallback)
		: this(mode, GetLootCollections(zoneLoot), closeCallback, leaveZoneAction)
	{
		if (NoLoot.CurrentValue)
		{
			LeaveZone();
		}
	}

	protected void DisposeImplementation()
	{
		Game.Instance.Player.UISettings.LootExtendedView = ExtendedView.CurrentValue;
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, GetFullScreenUIType(Mode));
		});
	}

	private FullScreenUIType GetFullScreenUIType(LootWindowMode mode)
	{
		switch (mode)
		{
		case LootWindowMode.Short:
		case LootWindowMode.ShortUnit:
		case LootWindowMode.ZoneExit:
			return FullScreenUIType.Loot;
		case LootWindowMode.PlayerChest:
			return FullScreenUIType.PlayerChest;
		case LootWindowMode.StandardChest:
			return FullScreenUIType.Loot;
		case LootWindowMode.OneSlot:
			return FullScreenUIType.OneSlotLoot;
		default:
			return FullScreenUIType.Loot;
		}
	}

	private static Func<ItemEntity, bool> GetCanInsertItemPred(LootWindowMode mode, IEnumerable<EntityViewBase> objects)
	{
		foreach (EntityViewBase @object in objects)
		{
			if (!(@object.GetComponent<InteractionLoot>() == null))
			{
				InteractionLootPart optional = @object.Data.ToEntity().GetOptional<InteractionLootPart>();
				if (optional != null && mode == LootWindowMode.OneSlot)
				{
					return optional.CanInsertItem;
				}
			}
		}
		return null;
	}

	public void ChangeView()
	{
		m_ExtendedView.Value = !ExtendedView.CurrentValue;
	}

	private static IEnumerable<ItemsCollection> GetLootCollections(IEnumerable<EntityViewBase> objects)
	{
		foreach (EntityViewBase @object in objects)
		{
			if (@object is UnitEntityView unitEntityView && unitEntityView.EntityData.Inventory.Collection != Game.Instance.PartySharedInventory.Collection)
			{
				yield return unitEntityView.EntityData.Inventory.Collection;
			}
			else if (!(@object.GetComponent<InteractionLoot>() == null))
			{
				InteractionLootPart optional = @object.Data.ToEntity().GetOptional<InteractionLootPart>();
				if (optional != null)
				{
					yield return optional.Loot;
				}
			}
		}
	}

	private static IEnumerable<ItemsCollection> GetLootCollections(ILootable[] objects)
	{
		for (int i = 0; i < objects.Length; i++)
		{
			ItemsCollection items = objects[i].Items;
			if (items != null && !items.All((ItemEntity item) => !IsLootable(item)))
			{
				yield return items;
			}
		}
	}

	private static IEnumerable<ItemsCollection> GetLootCollections(IEnumerable<LootWrapper> zoneLoot)
	{
		foreach (LootWrapper item in zoneLoot)
		{
			ItemsCollection itemsCollection = item.Unit?.Inventory.Collection ?? item.InteractionLoot?.Loot;
			if (itemsCollection != null && !itemsCollection.All((ItemEntity item) => !IsLootable(item)))
			{
				yield return itemsCollection;
			}
		}
	}

	private void MarkContextLootAsDirty()
	{
		m_ContextLootIsDirty = true;
	}

	private void Update()
	{
		if (m_ContextLootIsDirty)
		{
			m_ContextLootIsDirty = false;
			UpdateContextLoot();
		}
	}

	private void UpdateContextLoot()
	{
		foreach (LootObjectVM item in ContextLoot)
		{
			item.SetNewItems(LootableItems);
		}
		m_NoLoot.Value = m_AllCollected && LootableItems.Empty();
		if (NoLoot.CurrentValue)
		{
			LeaveZone();
		}
	}

	void ILootHandler.HandleChangeLoot(ItemSlotVM slot)
	{
		if (!slot.HasItem)
		{
			return;
		}
		LootCollectorVM lootCollectorExitLocation = LootCollectorExitLocation;
		if (lootCollectorExitLocation == null || !lootCollectorExitLocation.IsTrashMode.CurrentValue)
		{
			LootCollectorVM lootCollectorOnLocation = LootCollectorOnLocation;
			if (lootCollectorOnLocation == null || !lootCollectorOnLocation.IsTrashMode.CurrentValue)
			{
				if (IsPlayerStash || IsOneSlot)
				{
					UIInventoryHelper.TryCollectLootSlot(slot);
					MarkContextLootAsDirty();
				}
				else
				{
					Game.Instance.GameCommandQueue.TransferItem(slot.ItemEntity, Game.Instance.Player.SharedStash.ToCollectionRef(), 1);
					m_AllCollected = true;
					MarkContextLootAsDirty();
				}
				return;
			}
		}
		if (!slot.ItemEntity.Blueprint.IsNotable)
		{
			slot.MarkAsTrashLoot(!slot.MarkedAsTrash.CurrentValue);
			ModalWindowsSounds.Instance.Loot.MarkAsTrash.Play();
			MarkContextLootAsDirty();
		}
	}

	private (string, string) GetLootObjectStrings(LootObjectType lootObjectType)
	{
		if (Mode == LootWindowMode.OneSlot)
		{
			InteractionLootPart interactionLootPart = m_ItemsCollections.First().ConcreteOwner?.GetOptional<InteractionLootPart>();
			if (interactionLootPart != null)
			{
				string name = interactionLootPart.Name;
				string description = interactionLootPart.GetDescription();
				return (name, description);
			}
		}
		return UIStrings.Instance.LootWindow.GetLootObjectStrings(lootObjectType);
	}

	public void TryCollectLoot()
	{
		foreach (LootObjectVM item in ContextLoot)
		{
			if (item.Type == LootObjectType.Normal && item.HasLootableItems)
			{
				Game.Instance.GameCommandQueue.CollectLoot(item.LootableItems.Select((ItemEntity x) => new EntityRef<ItemEntity>(x)).ToList());
			}
		}
		m_AllCollected = true;
		MarkContextLootAsDirty();
		if (!ExtendedView.CurrentValue)
		{
			LeaveZone();
		}
	}

	public void LeaveZone()
	{
		TooltipHelper.HideTooltip();
		Close();
		BaseLeaveZone();
	}

	public void BaseLeaveZone()
	{
		m_AreaTransitionCallback?.Invoke();
	}

	public void Close()
	{
		m_CloseCallback?.Invoke();
		m_CloseCallback = null;
	}

	private void CloseOneSlot()
	{
		if (Mode == LootWindowMode.OneSlot && m_CatchInteractionSlotItemEntity != InteractionSlot.ItemSlot?.CurrentValue?.ItemEntity)
		{
			if (m_CatchInteractionSlotItemEntity == null)
			{
				UIInventoryHelper.TryMoveSlot(InteractionSlot?.ItemSlot?.CurrentValue, InventoryStash.FirstEmptySlot, InteractionSlot);
			}
			else
			{
				InsertableLootSlotVM visibleElementOrDefault = InteractionSlot.SlotsGroup.CurrentValue.GetVisibleElementOrDefault((InsertableLootSlotVM slot) => slot.ItemEntity == m_CatchInteractionSlotItemEntity);
				if (visibleElementOrDefault != null)
				{
					UIInventoryHelper.InsertToInteractionSlot(visibleElementOrDefault, InteractionSlot);
				}
			}
		}
		Close();
	}

	void IInventoryHandler.Refresh()
	{
	}

	void IInventoryHandler.TryEquip(ItemSlotVM slot)
	{
	}

	void IInventoryHandler.TryDrop(ItemSlotVM slot)
	{
	}

	void IInventoryHandler.TryMoveToInventory(ItemSlotVM slot, bool immediately)
	{
		if (slot?.ItemEntity == null)
		{
			return;
		}
		LootCollectorVM lootCollectorExitLocation = LootCollectorExitLocation;
		if (lootCollectorExitLocation != null && lootCollectorExitLocation.IsTrashMode.CurrentValue)
		{
			return;
		}
		LootCollectorVM lootCollectorOnLocation = LootCollectorOnLocation;
		if (lootCollectorOnLocation == null || !lootCollectorOnLocation.IsTrashMode.CurrentValue)
		{
			if (immediately)
			{
				UIInventoryHelper.TryMoveSlot(slot, InventoryStash.FirstEmptySlot, InteractionSlot);
				m_AllCollected = true;
				MarkContextLootAsDirty();
			}
			else
			{
				UIInventoryHelper.TryTransferInventorySlot(slot, ContextLoot[0]);
				MarkContextLootAsDirty();
			}
		}
	}

	void INewSlotsHandler.HandleTryInsertSlot(InsertableLootSlotVM slot)
	{
		UIInventoryHelper.InsertToInteractionSlot(slot, InteractionSlot);
	}

	void INewSlotsHandler.HandleTrySplitSlot(ItemSlotVM slot)
	{
		UIInventoryHelper.TrySplitSlot(slot, isLoot: true);
	}

	void INewSlotsHandler.HandleTryMoveSlot(ItemSlotVM from, ItemSlotVM to)
	{
		if (from?.ItemEntity == null)
		{
			return;
		}
		if (from.SlotsGroupType == ItemSlotsGroupType.Loot && to.SlotsGroupType == ItemSlotsGroupType.Loot)
		{
			if (from.Group != to.Group)
			{
				((ILootHandler)this).HandleChangeLoot(from);
			}
		}
		else
		{
			UIInventoryHelper.TryMoveSlot(from, to, InteractionSlot);
			MarkContextLootAsDirty();
		}
	}

	void ICollectLootHandler.HandleCollectAll(ItemsCollection from, ItemsCollection to)
	{
	}

	void IMoveItemHandler.HandleMoveItem(bool isEquip)
	{
		MarkContextLootAsDirty();
		InventoryStash.CollectionChanged();
	}

	void ITransferItemHandler.HandleTransferItem(ItemsCollection from, ItemsCollection to)
	{
		if (m_ItemsCollections.Contains(from) || m_ItemsCollections.Contains(to))
		{
			MarkContextLootAsDirty();
		}
	}

	void IFullScreenUIHandler.HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		if (state)
		{
			Close();
		}
	}

	void ISplitItemHandler.HandleSplitItem()
	{
	}

	void ISplitItemHandler.HandleAfterSplitItem(ItemEntity item)
	{
		MarkContextLootAsDirty();
	}

	void ISplitItemHandler.HandleBeforeSplitItem(ItemEntity item, ItemsCollection from, ItemsCollection to)
	{
	}
}
