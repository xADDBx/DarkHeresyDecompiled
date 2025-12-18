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

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

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
		BuildNavigation();
		SetupTexts();
		base.ViewModel.AccessiabilityPageVM.Subscribe(delegate(FirstLaunchAccessiabilityPageVM vm)
		{
			SetContinueButtonVisibility(vm == null);
		}).AddTo(this);
		base.ViewModel.LanguagePageVM.Subscribe(delegate(FirstLaunchLanguagePageVM vm)
		{
			OnLanguagePage(vm != null);
		}).AddTo(this);
		m_NavigationBehaviour.Focus.Subscribe(OnFocusEntity).AddTo(this);
		base.ViewModel.LanguageChanged.Subscribe(SetupTexts).AddTo(this);
		base.ViewModel.ShowPhotosensitivityScreen.Subscribe(ShowPhotoSensitivityScreen).AddTo(this);
		Show();
	}

	protected override void OnUnbind()
	{
		Hide();
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
		m_InputLayer = null;
	}

	protected virtual void InitializeImpl()
	{
	}

	private void BuildNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		BuildNavigationImpl(m_NavigationBehaviour);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "FirstLaunchSettingsBaseViewInput"
		});
		CreateInputImpl(m_InputLayer);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	protected virtual void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer)
	{
	}

	protected void ConfirmAction()
	{
		if (m_IsLanguagePage && IsFocusedOnLanguageItem)
		{
			m_NavigationBehaviour.CurrentEntity.OnConfirmClick();
		}
		base.ViewModel.NextPage();
	}

	protected void DeclineAction()
	{
		if (!m_IsLanguagePage)
		{
			base.ViewModel.PreviousPage();
		}
	}

	private void Show()
	{
		m_Animator.AppearAnimation();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.FirstLaunchSettings);
		});
		UISounds.Instance.Sounds.Settings.SettingsOpen.Play();
	}

	private void Hide()
	{
		m_Animator.DisappearAnimation();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.FirstLaunchSettings);
		});
		UISounds.Instance.Sounds.Settings.SettingsClose.Play();
	}

	private void OnLanguagePage(bool value)
	{
		m_IsLanguagePage = value;
		IsNotOnLanguagePage.Value = !value;
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		IsFocusedOnLanguageItem = entity is FirstLaunchEntityLanguageItemBaseView;
		IsNotOnLanguagePage.Value = !m_IsLanguagePage;
		SetupTexts();
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
