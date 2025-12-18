using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;
using Kingmaker.UI.Sound;

namespace Kingmaker.Code.UI.MVVM;

public abstract class PersonalChestSlotView : InventorySlotView
{
	protected override void OnClick()
	{
		UISounds.Instance.PlayItemSound(SlotAction.Put, base.ViewModel.ItemEntity, equipSound: false);
		GameCommandHelper.TransferCount(Game.Instance.PartySharedInventory.Collection, base.ViewModel.ItemEntity, 1);
	}

	protected override void OnDoubleClick()
	{
		UISounds.Instance.PlayItemSound(SlotAction.Put, base.ViewModel.ItemEntity, equipSound: false);
		EventBus.RaiseEvent(delegate(ILootHandler h)
		{
			h.HandleChangeLoot(base.ViewModel);
		});
	}

	protected override void Split()
	{
		EventBus.RaiseEvent(delegate(INewSlotsHandler h)
		{
			h.HandleTrySplitSlot(base.ViewModel);
		});
	}
}
