using R3;
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
	private HintView m_PrevHint;

	[SerializeField]
	private HintView m_NextHint;

	[SerializeField]
	private HintView m_HorizontalDPadHint;

	[SerializeField]
	private HintView m_VerticalDPadHint;

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

	protected void CreateInputImpl()
	{
	}

	private void ScrollInfoView(float x)
	{
		if (m_AccessibilityPageConsoleView.gameObject.activeInHierarchy && m_AccessibilityPageConsoleView.ViewModel != null)
		{
			m_AccessibilityPageConsoleView.ScrollDescription(x);
		}
		else if (m_DisplayPageConsoleView.gameObject.activeInHierarchy && m_DisplayPageConsoleView.ViewModel != null)
		{
			m_DisplayPageConsoleView.ScrollDescription(x);
		}
	}

	protected override void SetupTexts()
	{
	}
}
