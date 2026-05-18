using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LootConsoleView : LootView<LootCollectorConsoleView, InteractionSlotPartConsoleView, PlayerStashConsoleView>
{
	[Header("Console")]
	[SerializeField]
	private TooltipPlaces m_StashTooltipPlaces;

	[SerializeField]
	private TooltipPlaces m_CargoTooltipPlaces;

	[SerializeField]
	private TooltipPlaces m_CenterTooltipPlaces;

	[SerializeField]
	private CanvasSortingComponent m_SortingComponent;

	[SerializeField]
	protected RectTransform m_LeftCanvas;

	[SerializeField]
	protected RectTransform m_RightCanvas;

	[SerializeField]
	protected RectTransform m_CenterCanvas;
}
