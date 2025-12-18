using R3;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenCareerPhaseDetailedPCView : CharGenCareerPhaseDetailedView
{
	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.UnitProgressionVM.State.Subscribe(UpdateInfo).AddTo(this);
	}

	private void UpdateInfo(UnitProgressionWindowState state)
	{
		if (state == UnitProgressionWindowState.CareerPathProgression)
		{
			base.ViewModel.InfoVM.SetTemplate(null);
		}
		else
		{
			base.ViewModel.UpdateTooltipTemplate(base.ViewModel.UnitProgressionVM?.PreselectedCareer.CurrentValue?.CareerPath);
		}
	}
}
