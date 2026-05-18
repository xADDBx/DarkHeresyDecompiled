using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FirstLaunchAccessibilityPagePCView : FirstLaunchSettingsPageBaseView<FirstLaunchAccessiabilityPageVM>
{
	[SerializeField]
	private InfoSectionView m_InfoView;

	[SerializeField]
	private SettingsEntitySliderFontSizePCView m_SettingsEntitySliderFontSizePCView;

	[SerializeField]
	private SettingsEntitySliderPCView m_SettingsEntitySliderProtanopiaPCView;

	[SerializeField]
	private SettingsEntitySliderPCView m_SettingsEntitySliderDeuteranopiaPCView;

	[SerializeField]
	private SettingsEntitySliderPCView m_SettingsEntitySliderTritanopiaPCView;

	protected override void OnBind()
	{
		m_SettingsEntitySliderFontSizePCView.Bind(base.ViewModel.FontSize);
		m_SettingsEntitySliderProtanopiaPCView.Bind(base.ViewModel.Protanopia);
		m_SettingsEntitySliderDeuteranopiaPCView.Bind(base.ViewModel.Deuteranopia);
		m_SettingsEntitySliderTritanopiaPCView.Bind(base.ViewModel.Tritanopia);
		m_InfoView.Bind(base.ViewModel.InfoVM);
		base.OnBind();
	}
}
