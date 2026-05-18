using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VendorConsoleView : VendorView<InventoryStashConsoleView, ItemsFilterConsoleView, VendorLevelItemsConsoleView, VendorTransitionWindowConsoleView>
{
	[Header("TooltipPlaces")]
	[SerializeField]
	private TooltipPlaces m_StashTooltipPlaces;

	[SerializeField]
	private TooltipPlaces m_CenterTooltipPlaces;

	[Header("Console")]
	[SerializeField]
	private HintView m_NextWindowHint;

	[SerializeField]
	private HintView m_PrevWindowHint;

	protected override void OnUnbind()
	{
		TooltipHelper.HideTooltip();
		base.OnUnbind();
	}
}
