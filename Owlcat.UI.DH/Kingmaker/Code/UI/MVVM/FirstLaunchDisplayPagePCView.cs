using Kingmaker.Blueprints.Root.Strings;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FirstLaunchDisplayPagePCView : FirstLaunchSettingsPageBaseView<FirstLaunchDisplayPageVM>
{
	[SerializeField]
	private InfoSectionView m_InfoView;

	[SerializeField]
	private SettingsEntitySliderGammaCorrectionPCView m_SettingsEntitySliderGammaCorrectionPCView;

	[SerializeField]
	private SettingsEntitySliderGammaCorrectionPCView m_SettingsEntitySliderBrightnessPCView;

	[SerializeField]
	private SettingsEntitySliderGammaCorrectionPCView m_SettingsEntitySliderContrastPCView;

	[SerializeField]
	private TextMeshProUGUI m_DisplayImageText_1;

	[SerializeField]
	private TextMeshProUGUI m_DisplayImageText_2;

	[SerializeField]
	private TextMeshProUGUI m_DisplayImageText_3;

	protected override void OnBind()
	{
		m_SettingsEntitySliderGammaCorrectionPCView.Bind(base.ViewModel.GammaCorrection);
		m_SettingsEntitySliderBrightnessPCView.Bind(base.ViewModel.Brightness);
		m_SettingsEntitySliderContrastPCView.Bind(base.ViewModel.Contrast);
		UITextSettingsUI settingsUI = UIStrings.Instance.SettingsUI;
		m_DisplayImageText_1.text = settingsUI.DisplayImageShadows;
		m_DisplayImageText_2.text = settingsUI.DisplayImageMidtones;
		m_DisplayImageText_3.text = settingsUI.DisplayImageBrights;
		m_InfoView.Bind(base.ViewModel.InfoVM);
		base.OnBind();
	}
}
