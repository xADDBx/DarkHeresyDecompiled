using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Code.View.UI.MVVM.ServiceWindows;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Events;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class InventoryVM : ViewModel, IInventoryHandler, ISubscriber, INewSlotsHandler, IEquipItemAutomaticallyHandler, IInsertItemHandler, IDropItemHandler, IMoveItemHandler, IUnequipItemHandler, IServiceWindow
{
	private readonly ItemSlotsGroupVM m_SlotsGroup;

	private readonly ReactiveCommand<Unit> m_SuppressHideAnimation;

	private readonly ReactiveProperty<BaseUnitEntity> m_Unit = new ReactiveProperty<BaseUnitEntity>();

	public readonly InventoryDollVM DollVM;

	public readonly InventoryStashVM StashVM;

	public readonly PartyVM PartyVM;

	public ReadOnlyReactiveProperty<BaseUnitEntity> Unit => m_Unit;

	public Observable<Unit> SuppressHideAnimation => m_SuppressHideAnimation;

	public InventoryVM()
	{
		m_SuppressHideAnimation = new ReactiveCommand<Unit>().AddTo(this);
		ReactiveProperty<BaseUnitEntity> selectedUnitInUI = Game.Instance.Controllers.SelectionCharacter.SelectedUnitInUI;
		ReactiveProperty<BaseUnitEntity> reactiveProperty = selectedUnitInUI;
		if (reactiveProperty.Value == null)
		{
			BaseUnitEntity baseUnitEntity = (reactiveProperty.Value = Game.Instance.Player.MainCharacterEntity);
		}
		m_Unit = selectedUnitInUI;
		Unit.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate
		{
			OnUnitChanged();
		}).AddTo(this);
		ItemsSortingContext sortingCtx = new ItemsSortingContext
		{
			SorterType = Game.Instance.Player.UISettings.InventorySorter,
			FilterType = Game.Instance.Player.UISettings.InventoryFilter
		};
		m_SlotsGroup = new ItemSlotsGroupVM(Game.Instance.PartySharedInventory.Collection, 5, 100, sortingCtx, Game.Instance.Player.UISettings.ShowUnavailableItems, showSlotHoldItemsInSlots: false, ItemSlotsGroupType.Inventory).AddTo(this);
		StashVM = new InventoryStashVM(m_SlotsGroup).AddTo(this);
		DollVM = new InventoryDollVM(Unit, OnWeaponSetChanged).AddTo(this);
		PartyVM = new PartyVM().AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		EventBus.RaiseEvent(delegate(IUIEventHandler h)
		{
			h.HandleUIEvent(UIEventType.InventoryOpen);
		});
		Metrics.Interface.InterfaceState(InterfaceMetricsEvent.InterfaceStates.Open).InterfaceType(InterfaceMetricsEvent.InterfaceTypes.Inventory).Send();
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		Metrics.Interface.InterfaceState(InterfaceMetricsEvent.InterfaceStates.Close).InterfaceType(InterfaceMetricsEvent.InterfaceTypes.Inventory).Send();
	}

	private void OnWeaponSetChanged()
	{
		m_SlotsGroup.VisibleCollection.ForEach(delegate(ItemSlotVM slot)
		{
			slot.UpdateTooltips(force: true);
		});
	}

	private void OnUnitChanged()
	{
		if (Unit.CurrentValue != null)
		{
			StashVM?.CollectionChanged();
		}
	}

	public void Refresh()
	{
		m_Unit.ForceNotify();
	}

	void IInventoryHandler.TryEquip(ItemSlotVM slot)
	{
		UIInventoryHelper.TryEquip(slot, Unit?.CurrentValue);
	}

	void IInventoryHandler.TryDrop(ItemSlotVM slot)
	{
		UIInventoryHelper.TryDrop(slot);
	}

	void IInventoryHandler.TryMoveToInventory(ItemSlotVM slot, bool immediately)
	{
		UIInventoryHelper.TryMoveToInventory(slot);
	}

	public void HandleTryInsertSlot(InsertableLootSlotVM slot)
	{
	}

	public void HandleTryMoveSlot(ItemSlotVM from, ItemSlotVM to)
	{
		UIInventoryHelper.TryMoveSlotInInventory(from, to);
	}

	public void HandleTrySplitSlot(ItemSlotVM slot)
	{
		UIInventoryHelper.TrySplitSlot(slot, isLoot: false);
	}

	void IEquipItemAutomaticallyHandler.HandleEquipItemAutomatically(ItemEntity item)
	{
		Refresh();
	}

	void IDropItemHandler.HandleDropItem(ItemEntity item, bool split)
	{
		Refresh();
	}

	void IInsertItemHandler.HandleInsertItem(ItemSlot slot)
	{
		Refresh();
	}

	void IUnequipItemHandler.HandleUnequipItem()
	{
		Refresh();
	}

	void IMoveItemHandler.HandleMoveItem(bool isEquip)
	{
		Refresh();
	}

	void IServiceWindow.HandleOnSwitchedFromWindow()
	{
		m_SuppressHideAnimation.Execute();
	}
}
