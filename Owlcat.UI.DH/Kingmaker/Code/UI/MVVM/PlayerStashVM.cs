using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Events;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class PlayerStashVM : ViewModel, IRefreshVisibleCollectionHandler, ISubscriber, IMoveItemHandler
{
	public readonly string LootDisplayName;

	public readonly ItemsFilterVM ItemsFilter;

	public readonly InventoryStashVM StashVM;

	private readonly LootVM m_Loot;

	public string SkillCheckText { get; }

	public LootWindowMode Mode => m_Loot.Mode;

	public ObservableList<LootObjectVM> ContextLoot => m_Loot?.ContextLoot;

	public PlayerStashVM(LootVM lootVM)
	{
		EventBus.Subscribe(this).AddTo(this);
		m_Loot = lootVM;
		LootDisplayName = UIStrings.Instance.LootWindow.GetLootNameByContext(Mode);
		SkillCheckText = UtilitySkillcheck.GetLootSkillCheck(lootVM.SkillCheckResult);
		ItemsSortingContext sortingCtx = new ItemsSortingContext
		{
			SorterType = Game.Instance.Player.UISettings.InventorySorter,
			FilterType = Game.Instance.Player.UISettings.InventoryFilter
		};
		ItemSlotsGroupVM itemSlotsGroupVM = new ItemSlotsGroupVM(Game.Instance.PartySharedInventory.Collection, 5, 100, sortingCtx, Game.Instance.Player.UISettings.ShowUnavailableItems, showSlotHoldItemsInSlots: false, ItemSlotsGroupType.Inventory);
		itemSlotsGroupVM.AddTo(this);
		StashVM = new InventoryStashVM(itemSlotsGroupVM).AddTo(this);
		ItemsFilter = new ItemsFilterVM(ContextLoot[0].SlotsGroup).AddTo(this);
		ItemsFilter.SetCurrentFilter(ItemsFilterType.NoFilter);
	}

	public void Close()
	{
		m_Loot.Close();
	}

	void IRefreshVisibleCollectionHandler.Refresh()
	{
		StashVM.CollectionChanged();
		ContextLoot[0].SlotsGroup.UpdateVisibleCollection();
	}

	void IMoveItemHandler.HandleMoveItem(bool isEquip)
	{
		StashVM.CollectionChanged();
	}
}
