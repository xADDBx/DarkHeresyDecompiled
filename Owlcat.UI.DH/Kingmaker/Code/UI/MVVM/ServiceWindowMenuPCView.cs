using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ServiceWindowMenuPCView : View<ServiceWindowsMenuVM>
{
	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	private ServiceWindowMenuSelectorPCView m_MenuSelector;

	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private GameObject m_AdditionalBackground;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_Animator.Initialize();
		m_MenuSelector.Initialize();
	}

	protected override void OnBind()
	{
		m_Animator.AppearAnimation();
		m_MenuSelector.Bind(base.ViewModel.SelectionGroup);
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, ButtonSoundsEnum.PlastickSound);
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close).AddTo(this);
		if (m_AdditionalBackground != null)
		{
			base.ViewModel.IsAdditionalBackgroundNeeded.Subscribe(m_AdditionalBackground.SetActive).AddTo(this);
		}
		m_CloseButton.gameObject.SetActive(Game.Instance.IsControllerMouse);
	}

	protected override void OnUnbind()
	{
		m_Animator.DisappearAnimation();
	}
}
