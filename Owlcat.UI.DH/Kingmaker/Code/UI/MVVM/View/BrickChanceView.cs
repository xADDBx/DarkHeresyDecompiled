using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class BrickChanceView : BrickCombatLogBaseView<BrickChanceVM>
{
	[Header("Slider")]
	[SerializeField]
	private RollSliderWidget RollSliderWidget;

	protected override void OnBind()
	{
		base.OnBind();
		RollSliderWidget.Bind((base.ViewModel.SufficientValue, base.ViewModel.CurrentValue));
	}
}
