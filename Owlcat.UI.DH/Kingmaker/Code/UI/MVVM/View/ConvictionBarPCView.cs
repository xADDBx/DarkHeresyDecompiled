namespace Kingmaker.Code.UI.MVVM.View;

public class ConvictionBarPCView : ConvictionBarBaseView
{
	protected override void OnBind()
	{
		base.OnBind();
		m_RightButtonRadical.SetTooltip(base.ViewModel.RadicalTooltip);
		m_LeftButtonPuritan.SetTooltip(base.ViewModel.PuritanTooltip);
		m_RightLabel.SetTooltip(base.ViewModel.PuritanTooltip);
		m_LeftLabel.SetTooltip(base.ViewModel.RadicalTooltip);
	}
}
