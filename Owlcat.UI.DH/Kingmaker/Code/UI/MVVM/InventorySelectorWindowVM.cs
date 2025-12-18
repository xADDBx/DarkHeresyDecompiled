using System;
using System.Collections.Generic;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class InventorySelectorWindowVM : SelectorWindowVM<EquipSelectorSlotVM>
{
	private readonly EquipSlotVM m_EquippedSlot;

	public InventorySelectorWindowVM(Action<EquipSelectorSlotVM> onConfirm, Action onDecline, List<EquipSelectorSlotVM> visibleCollection, EquipSlotVM equippedSlot)
		: base(onConfirm, onDecline, visibleCollection, (ReactiveProperty<EquipSelectorSlotVM>)null, equippedSlot)
	{
		m_EquippedSlot = equippedSlot;
	}

	public void Unequip()
	{
		if (UIInventoryHelper.TryUnequip(m_EquippedSlot))
		{
			EventBus.RaiseEvent(delegate(IInventoryHandler h)
			{
				h.Refresh();
			});
		}
	}
}
