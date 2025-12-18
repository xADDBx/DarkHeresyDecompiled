using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FirstLaunchLanguagePagePCView : FirstLaunchSettingsPageBaseView<FirstLaunchLanguagePageVM>
{
	[SerializeField]
	private FirstLaunchEntityLanguagePCView m_FirstLaunchEntityLanguagePCView;

	protected override void SetNavigationBehaviourImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		m_FirstLaunchEntityLanguagePCView.SetNavigationBehaviour(navigationBehaviour);
	}

	protected override void OnBind()
	{
		m_FirstLaunchEntityLanguagePCView.Bind(base.ViewModel.Languages);
		base.OnBind();
	}

	protected override void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		navigationBehaviour.AddRow(AdditionalEntities);
	}
}
