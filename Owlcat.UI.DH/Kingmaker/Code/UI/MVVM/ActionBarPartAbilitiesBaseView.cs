using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class ActionBarPartAbilitiesBaseView : View<ActionBarPartAbilitiesVM>
{
	[SerializeField]
	protected RectTransform m_TooltipPlace;

	protected int SlotsInRow => 10;

	public abstract void Initialize();
}
