using DG.Tweening;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Sound;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.GameConst;
using Kingmaker.Visual.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class FirstLaunchSettingsBaseView : View<FirstLaunchSettingsVM>
{
	[Header("Common")]
	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	private CanvasGroup m_AdditionalCanvasGroup;

	protected readonly ReactiveProperty<bool> IsNotOnLanguagePage = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> IsVisibleContinueButton = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> IsVisibleFinishButton = new ReactiveProperty<bool>();

	private bool m_IsLanguagePage;

	protected bool IsFocusedOnLanguageItem;

	[SerializeField]
	private SplashScreenController.ScreenUnit m_PhotosensitivityUnit;

	private Sequence m_TweenSequence;

	public void Initialize()
	{
		m_Animator.Initialize();
		InitializeImpl();
	}

	protected override void OnBind()
	{
		SetupTexts();
		base.ViewModel.AccessiabilityPageVM.Subscribe(delegate(FirstLaunchAccessiabilityPageVM vm)
		{
			SetContinueButtonVisibility(vm == null);
		}).AddTo(this);
		base.ViewModel.LanguagePageVM.Subscribe(delegate(FirstLaunchLanguagePageVM vm)
		{
			OnLanguagePage(vm != null);
		}).AddTo(this);
		base.ViewModel.LanguageChanged.Subscribe(SetupTexts).AddTo(this);
		base.ViewModel.ShowPhotosensitivityScreen.Subscribe(ShowPhotoSensitivityScreen).AddTo(this);
		Show();
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	protected virtual void InitializeImpl()
	{
	}

	private void Show()
	{
		m_Animator.AppearAnimation();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.FirstLaunchSettings);
		});
		FullScreenUniqueSounds.Instance.Settings.Open.Play();
	}

	private void Hide()
	{
		m_Animator.DisappearAnimation();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.FirstLaunchSettings);
		});
		FullScreenUniqueSounds.Instance.Settings.Close.Play();
	}

	private void OnLanguagePage(bool value)
	{
		m_IsLanguagePage = value;
		IsNotOnLanguagePage.Value = !value;
	}

	private void SetContinueButtonVisibility(bool isVisible)
	{
		IsVisibleContinueButton.Value = isVisible;
		IsVisibleFinishButton.Value = !isVisible;
	}

	protected virtual void SetupTexts()
	{
	}

	protected virtual void ShowPhotoSensitivityScreen()
	{
		if (m_PhotosensitivityUnit.CanvasGroup == null)
		{
			OnComplete();
			return;
		}
		m_TweenSequence = DOTween.Sequence();
		m_TweenSequence.AppendInterval(0.1f);
		UIUtilityShowSplashScreen.ShowSplashScreen(m_PhotosensitivityUnit, m_TweenSequence, base.gameObject, m_AdditionalCanvasGroup);
		m_TweenSequence.AppendCallback(OnComplete);
	}

	private void OnComplete()
	{
		SoundBanksManager.UnloadBank(UIConsts.SplashScreens);
		SoundState.Instance.ResetState(SoundStateType.MainMenu);
		base.ViewModel.Close();
	}
}
