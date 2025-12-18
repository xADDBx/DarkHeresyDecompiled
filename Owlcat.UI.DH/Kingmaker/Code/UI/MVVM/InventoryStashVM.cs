using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class InventoryStashVM : ViewModel, IPartyEncumbranceHandler, ISubscriber, ISetInventorySorterHandler
{
	public readonly ISlotsGroupVM<ItemSlotVM, ItemEntity> ItemSlotsGroup;

	public readonly ItemsFilterVM ItemsFilter;

	private readonly ReactiveProperty<long> m_Money = new ReactiveProperty<long>();

	public ReadOnlyReactiveProperty<long> Money => m_Money;

	public ReadOnlyReactiveProperty<ItemsSorterType> CurrentSorter => ItemsFilter.CurrentSorter;

	public ReadOnlyReactiveProperty<ItemsFilterType> CurrentFilter => ItemsFilter.CurrentFilter;

	public ItemSlotVM FirstEmptySlot => ItemSlotsGroup.GetFirstEmptySlot();

	public InventoryStashVM(ISlotsGroupVM<ItemSlotVM, ItemEntity> slotsGroup)
	{
		ItemSlotsGroup = slotsGroup;
		ItemsFilter = new ItemsFilterVM(ItemSlotsGroup).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(ItemSlotsGroup.CollectionChangedCommand, delegate
		{
			UpdateValues();
		}).AddTo(this);
		UpdateValues();
		PlayerUISettings uiSettings = Game.Instance.Player.UISettings;
		CurrentSorter.Subscribe(delegate(ItemsSorterType value)
		{
			if (uiSettings.InventorySorter != value)
			{
				Game.Instance.GameCommandQueue.SetInventorySorter(value);
			}
		}).AddTo(this);
		ItemsFilter.ShowUnavailable.Subscribe(delegate(bool value)
		{
			uiSettings.ShowUnavailableItems = value;
		}).AddTo(this);
		CurrentFilter.Subscribe(delegate(ItemsFilterType value)
		{
			uiSettings.InventoryFilter = value;
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	public void CollectionChanged()
	{
		ItemSlotsGroup.UpdateVisibleCollection();
	}

	public void ResetFilter()
	{
		ItemsFilter.ResetFilter();
	}

	private void UpdateValues()
	{
		UpdateGoldCoins();
	}

	private void UpdateGoldCoins()
	{
		m_Money.Value = Game.Instance.Player.Money;
	}

	public void ChangePartyEncumbrance(Encumbrance prevEncumbrance)
	{
	}

	void ISetInventorySorterHandler.HandleSetInventorySorter(ItemsSorterType sorterType)
	{
		if (CurrentSorter.CurrentValue != sorterType)
		{
			ItemsFilter.SetCurrentSorter(sorterType);
		}
	}
}
