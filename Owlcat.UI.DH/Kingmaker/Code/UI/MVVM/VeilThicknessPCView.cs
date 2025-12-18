using R3;

namespace Kingmaker.Code.UI.MVVM;

public class VeilThicknessPCView : VeilThicknessView
{
	protected override void OnBind()
	{
		base.OnBind();
		m_TooltipArea.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
	}
}
