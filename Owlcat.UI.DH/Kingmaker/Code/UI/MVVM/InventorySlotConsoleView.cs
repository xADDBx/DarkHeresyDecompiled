using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InventorySlotConsoleView : InventorySlotView, IConfirmClickHandler, IConsoleEntity, IConsoleNavigationEntity, IHasTooltipTemplates, IHasCompareTooltipTemplates
{
	[Header("Console")]
	[SerializeField]
	private ItemSlotConsoleView m_ItemSlotConsoleView;

	public IReadOnlyList<TooltipBaseTemplate> MainTemplates => base.ViewModel.MainTooltips.CurrentValue;

	public IReadOnlyList<TooltipBaseTemplate> CompareTemplates => base.ViewModel.CompareTooltips.CurrentValue;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ItemSlotConsoleView.Bind(base.ViewModel);
	}

	protected override void SetupContextMenu()
	{
		UIContextMenu contextMenu = UIStrings.Instance.ContextMenu;
		List<ContextMenuCollectionEntity> contextMenu2 = new List<ContextMenuCollectionEntity>
		{
			new ContextMenuCollectionEntity(contextMenu.Equip, base.EquipItem, base.ViewModel.IsEquipPossible),
			new ContextMenuCollectionEntity(UIStrings.Instance.LootWindow.SendToInventory, delegate
			{
				MoveToInventory(immediately: true);
			}, condition: true),
			new ContextMenuCollectionEntity(contextMenu.Split, Split, base.ViewModel.IsSplitPossible),
			new ContextMenuCollectionEntity(contextMenu.Drop, base.DropItem, base.ViewModel.CanDropItem),
			new ContextMenuCollectionEntity(contextMenu.Information, base.ViewModel.ShowInfo, base.ViewModel.HasItem)
		};
		base.ViewModel.SetContextMenu(contextMenu2);
	}

	public void SetFocus(bool value)
	{
		if (value)
		{
			OnHoverStart();
		}
		else
		{
			OnHoverEnd();
		}
		m_ItemSlotConsoleView.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_ItemSlotConsoleView.IsValid();
	}

	public bool CanConfirmClick()
	{
		if (RootUIContext.Instance.IsInventoryShow)
		{
			if (base.ViewModel.HasItem)
			{
				return base.ViewModel.IsEquipPossible;
			}
			return false;
		}
		return base.ViewModel.HasItem;
	}

	public void OnConfirmClick()
	{
		if (RootUIContext.Instance.IsInventoryShow)
		{
			m_ItemSlotConsoleView.SetWaitingForSlotState(state: true);
			EventBus.RaiseEvent(delegate(IEquipSlotHandler h)
			{
				h.ChooseSlotToItem(this);
			});
		}
		else
		{
			OnClick();
		}
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return base.ViewModel.Tooltip.CurrentValue;
	}

	public void ReleaseSlot()
	{
		m_ItemSlotConsoleView.SetWaitingForSlotState(state: false);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_ItemSlotConsoleView.SetFocus(value: false);
	}
}
