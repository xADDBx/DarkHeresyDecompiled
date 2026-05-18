using Kingmaker.UI.Common.Animations;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using Owlcat.UI.Navigation;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[ViewFactoryPolicy(ViewFactoryPolicyFlag.DontPool, null)]
[RequireComponent(typeof(FocusLayer))]
public class MainMenuPCView : View<MainMenuVM>
{
	[Header("ShowMenu")]
	[SerializeField]
	private FadeAnimator m_BlackOverlayFadeAnimator;

	[SerializeField]
	protected float m_DelayBeforeShow = 1f;

	[Header("Components")]
	[SerializeField]
	private MainMenuButtonsPCView m_MainMenuButtonsPCView;

	[SerializeField]
	private MainMenuWelcomeWidgetView m_MainMenuWelcomeWidgetView;

	public void Awake()
	{
		m_BlackOverlayFadeAnimator.Initialize();
	}

	protected override void OnBind()
	{
		if (m_BlackOverlayFadeAnimator.CanvasGroup != null)
		{
			m_BlackOverlayFadeAnimator.CanvasGroup.alpha = 1f;
		}
		m_MainMenuButtonsPCView.Bind(base.ViewModel.MainMenuButtonsVm);
		m_MainMenuWelcomeWidgetView.Bind(base.ViewModel.MainMenuWelcomeWidgetVM);
		ObservableSubscribeExtensions.Subscribe(Observable.Timer(m_DelayBeforeShow.Seconds()), delegate
		{
			m_BlackOverlayFadeAnimator.DisappearAnimation();
		}).AddTo(this);
		this.AddNavigation().AddTo(this);
	}
}
