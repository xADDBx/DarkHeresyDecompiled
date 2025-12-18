using System;
using System.Collections.Generic;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InventoryEquipSlotConsoleView : InventoryEquipSlotView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IHasTooltipTemplates
{
	[SerializeField]
	private ItemSlotConsoleView m_ItemSlotConsoleView;

	public void SetAvailable(bool value)
	{
		m_ItemSlotConsoleView.SetAvailable(value);
	}

	public void SetSelected(bool value)
	{
		m_ItemSlotConsoleView.SetSelected(value);
	}

	public void SetFocus(bool value)
	{
		m_ItemSlotConsoleView.SetSelected(value);
	}

	public bool IsValid()
	{
		return m_ItemSlotConsoleView.IsValid();
	}

	public bool CanConfirmClick()
	{
		return m_ItemSlotConsoleView.IsValid();
	}

	public void OnConfirmClick()
	{
		switch (UsableSource)
		{
		case UsableSourceType.Inventory:
			EventBus.RaiseEvent(delegate(IInventoryItemHandler h)
			{
				h.HandleChangeItem(base.ViewModel);
			});
			break;
		case UsableSourceType.Vendor:
			base.ViewModel.VendorTryMove(split: true);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return base.ViewModel?.Tooltip?.CurrentValue ?? new List<TooltipBaseTemplate>();
	}

	public InventoryEquipSlotConsoleView GetSlot()
	{
		return this;
	}
}
