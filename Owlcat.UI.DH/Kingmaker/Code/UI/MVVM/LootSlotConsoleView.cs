using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LootSlotConsoleView : LootSlotView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IHasTooltipTemplates
{
	[Header("Console")]
	[SerializeField]
	private ItemSlotConsoleView m_ItemSlotConsoleView;

	private ContextMenuCollectionEntity m_ToCargoAuto = new ContextMenuCollectionEntity();

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
			new ContextMenuCollectionEntity(UIStrings.Instance.LootWindow.SendToInventory, delegate
			{
				MoveToInventory(immediately: true);
			}, condition: true),
			m_ToCargoAuto,
			new ContextMenuCollectionEntity(contextMenu.Split, base.Split, base.ViewModel.IsSplitPossible),
			new ContextMenuCollectionEntity(contextMenu.Information, base.ViewModel.ShowInfo, base.ViewModel.HasItem)
		};
		base.ViewModel.SetContextMenu(contextMenu2);
	}

	public void SetFocus(bool value)
	{
		m_ItemSlotConsoleView.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_ItemSlotConsoleView.IsValid();
	}

	public bool CanConfirmClick()
	{
		return base.ViewModel?.HasItem ?? false;
	}

	public void OnConfirmClick()
	{
		OnClick();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return m_ItemSlotConsoleView.SlotVM?.Tooltip?.CurrentValue;
	}
}
