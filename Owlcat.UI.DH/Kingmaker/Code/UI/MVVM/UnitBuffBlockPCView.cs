using System.Collections.Generic;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class UnitBuffBlockPCView : UnitBuffBlockView
{
	[SerializeField]
	protected RectTransform m_TooltipPlace;

	protected override void OnBind()
	{
		base.OnBind();
		this.SetTooltip(base.ViewModel.BuffsTooltip.CurrentValue, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, m_TooltipPlace, 0, 0, 0, new List<Vector2> { Vector2.zero })).AddTo(this);
	}
}
