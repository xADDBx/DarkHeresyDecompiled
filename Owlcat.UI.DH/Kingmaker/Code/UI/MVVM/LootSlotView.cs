using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;

namespace Kingmaker.Code.UI.MVVM;

public abstract class LootSlotView : ItemSlotView<ItemSlotVM>
{
	protected void OnClick()
	{
		EventBus.RaiseEvent(delegate(ILootHandler h)
		{
			h.HandleChangeLoot(base.ViewModel);
		});
	}

	protected void MoveToInventory(bool immediately)
	{
		EventBus.RaiseEvent(delegate(IInventoryHandler h)
		{
			h.TryMoveToInventory(base.ViewModel, immediately);
		});
	}

	protected void OnDoubleClick()
	{
		EventBus.RaiseEvent(delegate(IInventoryHandler h)
		{
			h.TryMoveToInventory(base.ViewModel, immediately: true);
		});
	}

	protected void Split()
	{
		EventBus.RaiseEvent(delegate(INewSlotsHandler h)
		{
			h.HandleTrySplitSlot(base.ViewModel);
		});
	}
}
