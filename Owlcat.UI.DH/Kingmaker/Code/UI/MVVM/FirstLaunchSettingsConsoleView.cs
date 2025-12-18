using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FirstLaunchSettingsConsoleView : FirstLaunchSettingsBaseView
{
	[Header("Main")]
	[SerializeField]
	private SettingsMenuSelectorBaseView m_MenuSelector;

	[SerializeField]
	private FirstLaunchLanguagePageConsoleView m_LanguagePageConsoleView;

	[SerializeField]
	private FirstLaunchSafeZonePageConsoleView m_SafeZonePageConsoleView;

	[SerializeField]
	private FirstLaunchDisplayPageConsoleView m_DisplayPageConsoleView;

	[SerializeField]
	private FirstLaunchAccessibilityPageConsoleView m_AccessibilityPageConsoleView;

	[Header("Input")]
	[SerializeField]
	private ConsoleHint m_PrevHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	[SerializeField]
	private ConsoleHint m_HorizontalDPadHint;

	[SerializeField]
	private ConsoleHint m_VerticalDPadHint;

	[SerializeField]
	protected ConsoleHintsWidget m_ConsoleHintsWidget;

	private IConsoleHint m_BackHint;

	private IConsoleHint m_ContinueHint;

	private IConsoleHint m_FinishHint;

	private IConsoleHint m_ResetToDefaultHint;

	protected override void InitializeImpl()
	{
		m_MenuSelector.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_MenuSelector.Bind(base.ViewModel.SelectionGroup);
		base.ViewModel.LanguagePageVM.Subscribe(m_LanguagePageConsoleView.Bind).AddTo(this);
		base.ViewModel.SafeZonePageVM.Subscribe(m_SafeZonePageConsoleView.Bind).AddTo(this);
		base.ViewModel.DisplayPageVM.Subscribe(m_DisplayPageConsoleView.Bind).AddTo(this);
		base.ViewModel.AccessiabilityPageVM.Subscribe(m_AccessibilityPageConsoleView.Bind).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_ConsoleHintsWidget.Dispose();
	}

	protected override void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		m_LanguagePageConsoleView.SetNavigationBehaviour(navigationBehaviour);
		m_SafeZonePageConsoleView.SetNavigationBehaviour(navigationBehaviour);
		m_DisplayPageConsoleView.SetNavigationBehaviour(navigationBehaviour);
		m_AccessibilityPageConsoleView.SetNavigationBehaviour(navigationBehaviour);
	}

	protected override void CreateInputImpl(InputLayer inputLayer)
	{
		m_PrevHint.Bind(inputLayer.AddButton(delegate
		{
			m_MenuSelector.OnPrev();
		}, 14, base.ViewModel.BlockHints.Not().ToReadOnlyReactiveProperty(initialValue: false))).AddTo(this);
		m_NextHint.Bind(inputLayer.AddButton(delegate
		{
			m_MenuSelector.OnNext();
		}, 15, base.ViewModel.BlockHints.Not().ToReadOnlyReactiveProperty(initialValue: false))).AddTo(this);
		m_HorizontalDPadHint.BindCustomAction(21, inputLayer, base.ViewModel.IsVisibleHorizontalDPad).AddTo(this);
		m_VerticalDPadHint.BindCustomAction(22, inputLayer, base.ViewModel.IsVisibleVerticalDPad).AddTo(this);
		m_BackHint = m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			DeclineAction();
		}, 9, IsNotOnLanguagePage.And(base.ViewModel.BlockHints.Not()).ToReadOnlyReactiveProperty(initialValue: false))).AddTo(this);
		m_BackHint.SetLabel(UIStrings.Instance.ContextMenu.Back);
		m_ContinueHint = m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			ConfirmAction();
		}, 8, IsVisibleContinueButton.And(base.ViewModel.BlockHints.Not()).ToReadOnlyReactiveProperty(initialValue: false))).AddTo(this);
		m_ContinueHint.SetLabel(UIStrings.Instance.MainMenu.Continue);
		m_FinishHint = m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.NextPage();
		}, 8, IsVisibleFinishButton.And(base.ViewModel.BlockHints.Not()).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustLongPressed)).AddTo(this);
		m_FinishHint.SetLabel(UIStrings.Instance.SettingsUI.FinishSetupHold);
		m_ResetToDefaultHint = m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.RevertSettings();
		}, 11, IsNotOnLanguagePage.And(base.ViewModel.BlockHints.Not()).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustLongPressed)).AddTo(this);
		m_ResetToDefaultHint.SetLabel(UIStrings.Instance.SettingsUI.ResetToDefaultHold);
		inputLayer.AddAxis(ScrollInfoView, 3, repeat: true).AddTo(this);
	}

	private void ScrollInfoView(InputActionEventData arg1, float x)
	{
		if (m_AccessibilityPageConsoleView.gameObject.activeInHierarchy && m_AccessibilityPageConsoleView.ViewModel != null)
		{
			m_AccessibilityPageConsoleView.ScrollDescription(arg1, x);
		}
		else if (m_DisplayPageConsoleView.gameObject.activeInHierarchy && m_DisplayPageConsoleView.ViewModel != null)
		{
			m_DisplayPageConsoleView.ScrollDescription(arg1, x);
		}
	}

	protected override void SetupTexts()
	{
		m_BackHint.SetLabel(UIStrings.Instance.ContextMenu.Back);
		m_ContinueHint.SetLabel(UIStrings.Instance.MainMenu.Continue);
		m_FinishHint.SetLabel(UIStrings.Instance.SettingsUI.FinishSetupHold);
		m_ResetToDefaultHint.SetLabel(UIStrings.Instance.SettingsUI.ResetToDefaultHold);
		m_VerticalDPadHint.SetLabel(UIStrings.Instance.SettingsUI.Navigation);
		m_HorizontalDPadHint.SetLabel(UIStrings.Instance.SettingsUI.Value);
	}

	protected override void ShowPhotoSensitivityScreen()
	{
		m_AccessibilityPageConsoleView.ClearNavigationBehaviour();
		base.ShowPhotoSensitivityScreen();
	}
}
