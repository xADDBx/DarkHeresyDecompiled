using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FirstLaunchLanguagePagePCView : FirstLaunchSettingsPageBaseView<FirstLaunchLanguagePageVM>
{
	[SerializeField]
	private FirstLaunchEntityLanguagePCView m_FirstLaunchEntityLanguagePCView;

	protected override void OnBind()
	{
		m_FirstLaunchEntityLanguagePCView.Bind(base.ViewModel.Languages);
		base.OnBind();
	}
}
