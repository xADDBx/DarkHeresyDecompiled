using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FirstLaunchLanguagePageConsoleView : FirstLaunchSettingsPageBaseView<FirstLaunchLanguagePageVM>
{
	[SerializeField]
	private FirstLaunchEntityLanguageConsoleView m_FirstLaunchEntityLanguageConsoleView;

	protected override void SetNavigationBehaviourImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		m_FirstLaunchEntityLanguageConsoleView.SetNavigationBehaviour(navigationBehaviour);
	}

	protected override void OnBind()
	{
		m_FirstLaunchEntityLanguageConsoleView.Bind(base.ViewModel.Languages);
		base.OnBind();
	}
}
