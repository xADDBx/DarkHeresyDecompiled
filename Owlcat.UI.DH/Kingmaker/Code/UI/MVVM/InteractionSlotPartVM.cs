using System;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class InteractionSlotPartVM : ViewModel
{
	public readonly string Name;

	public readonly string Description;

	private readonly Func<ItemEntity, bool> m_CanInsertItem;

	public readonly SlotsGroupVM<ItemSlotVM> Group;

	private readonly ReactiveProperty<ItemSlotVM> m_ItemSlot = new ReactiveProperty<ItemSlotVM>();

	private readonly ReactiveProperty<InsertableLootSlotsGroupVM> m_SlotsGroup = new ReactiveProperty<InsertableLootSlotsGroupVM>();

	private readonly Action m_Close;

	public ReadOnlyReactiveProperty<ItemSlotVM> ItemSlot => m_ItemSlot;

	public ReadOnlyReactiveProperty<InsertableLootSlotsGroupVM> SlotsGroup => m_SlotsGroup;

	public InteractionSlotPartVM(LootObjectVM lootObject, Func<ItemEntity, bool> canInsertItem, Action close)
	{
		m_Close = close;
		Name = lootObject.DisplayName;
		Description = lootObject.Description;
		m_CanInsertItem = canInsertItem;
		Group = lootObject.SlotsGroup;
		UpdateItem();
		ObservableSubscribeExtensions.Subscribe(Group.CollectionChangedCommand, delegate
		{
			UpdateItem();
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	private void UpdateItem()
	{
		m_ItemSlot.Value = Group.VisibleCollection.FirstItem((ItemSlotVM slot) => slot?.ItemEntity != null && m_CanInsertItem(slot.ItemEntity));
	}

	private void HandleDropItem(ItemSlotVM item)
	{
		if (item == null)
		{
			return;
		}
		InsertableLootSlotVM insertableLootSlotVM = item as InsertableLootSlotVM;
		if (insertableLootSlotVM != null && insertableLootSlotVM.CanInsert.CurrentValue)
		{
			EventBus.RaiseEvent(delegate(INewSlotsHandler h)
			{
				h.HandleTryInsertSlot(insertableLootSlotVM);
			});
		}
	}

	public void TryInsert()
	{
	}

	public void ClearItemSlot()
	{
		EventBus.RaiseEvent(delegate(INewSlotsHandler h)
		{
			h.HandleTryMoveSlot(ItemSlot.CurrentValue, SlotsGroup.CurrentValue.VisibleCollection.FirstItem());
		});
	}

	public void RequestItems()
	{
		ItemsSortingContext itemsSortingContext = default(ItemsSortingContext);
		itemsSortingContext.RemoveEmptySlots = true;
		ItemsSortingContext sortingCtx = itemsSortingContext;
		m_SlotsGroup.Value = new InsertableLootSlotsGroupVM(Game.Instance.PartySharedInventory.Collection, m_CanInsertItem, 3, 12, sortingCtx, showUnavailableItems: true, showSlotHoldItemsInSlots: false, ItemSlotsGroupType.Loot).AddTo(this);
	}

	public void Close()
	{
		m_Close?.Invoke();
	}
}
