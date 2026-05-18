using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.UI;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM;

public class MainMenuConsoleView : View<MainMenuVM>, IInitializable
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	protected float m_DelayBeforeShow = 1f;

	[FormerlySerializedAs("m_MenuSideBarConsoleView")]
	[SerializeField]
	private MainMenuButtonsConsoleView menuButtonsConsoleView;

	[SerializeField]
	private TermsOfUseConsoleView m_TermsOfUseConsoleView;

	[SerializeField]
	private CreditsBaseView m_CreditsView;

	[SerializeField]
	private NewGameConsoleView m_NewGameConsoleView;

	[SerializeField]
	private FirstLaunchSettingsConsoleView m_FirstLaunchSettingsConsoleView;

	[Header("First Time Launch FX")]
	[SerializeField]
	private UIFirstLaunchFX m_FirstLaunchFX;

	public void Initialize()
	{
		m_TermsOfUseConsoleView.Initialize();
		m_NewGameConsoleView.Initialize();
		m_FirstLaunchSettingsConsoleView.Initialize();
		m_FadeAnimator.Initialize();
	}

	protected override void OnBind()
	{
		if (m_FadeAnimator.CanvasGroup != null)
		{
			m_FadeAnimator.CanvasGroup.alpha = 1f;
		}
		menuButtonsConsoleView.Bind(base.ViewModel.MainMenuButtonsVm);
		ObservableSubscribeExtensions.Subscribe(Observable.Timer(m_DelayBeforeShow.Seconds(), UnityTimeProvider.UpdateIgnoreTimeScale), delegate
		{
			m_FadeAnimator.DisappearAnimation();
		}).AddTo(this);
	}

	private void PlayFirstLaunchFX()
	{
		m_FirstLaunchFX.PlayEffect();
	}
}
