using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class TooltipBrickChanceView : TooltipBrickCombatLogBaseView<TooltipBrickChanceVM>
{
	[Header("Slider")]
	[SerializeField]
	private RollSlider m_RollSlider;

	protected override void OnBind()
	{
		base.OnBind();
		m_RollSlider.SetData(base.ViewModel.SufficientValue, base.ViewModel.CurrentValue);
	}
}
