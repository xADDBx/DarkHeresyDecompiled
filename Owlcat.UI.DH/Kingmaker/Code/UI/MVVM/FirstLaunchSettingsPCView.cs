using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Localization;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FirstLaunchSettingsPCView : FirstLaunchSettingsBaseView
{
	[Header("Main")]
	[SerializeField]
	private SettingsMenuSelectorBaseView m_MenuSelector;

	[SerializeField]
	private FirstLaunchLanguagePagePCView m_LanguagePagePCView;

	[SerializeField]
	private FirstLaunchDisplayPagePCView m_DisplayPagePCView;

	[SerializeField]
	private FirstLaunchAccessibilityPagePCView m_AccessibilityPagePCView;

	[Header("Buttons")]
	[SerializeField]
	private OwlcatButton m_BackButton;

	[SerializeField]
	private OwlcatButton m_ResetToDefaultButton;

	[SerializeField]
	private OwlcatButton m_ContinueButton;

	[SerializeField]
	private TextMeshProUGUI m_BackText;

	[SerializeField]
	private TextMeshProUGUI m_ResetToDefaultText;

	[SerializeField]
	private TextMeshProUGUI m_ContinueText;

	protected override void InitializeImpl()
	{
		m_MenuSelector.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_MenuSelector.Bind(base.ViewModel.SelectionGroup);
		base.ViewModel.LanguagePageVM.Subscribe(m_LanguagePagePCView.Bind).AddTo(this);
		base.ViewModel.DisplayPageVM.Subscribe(m_DisplayPagePCView.Bind).AddTo(this);
		base.ViewModel.AccessiabilityPageVM.Subscribe(m_AccessibilityPagePCView.Bind).AddTo(this);
		IsNotOnLanguagePage.Subscribe(m_BackButton.gameObject.SetActive).AddTo(this);
		IsNotOnLanguagePage.Subscribe(m_ResetToDefaultButton.gameObject.SetActive).AddTo(this);
		m_BackButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.PreviousPage).AddTo(this);
		m_ContinueButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.NextPage).AddTo(this);
		m_ResetToDefaultButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.RevertSettings).AddTo(this);
		m_BackButton.OnConfirmClickAsObservable().Subscribe(base.ViewModel.PreviousPage).AddTo(this);
		m_ContinueButton.OnConfirmClickAsObservable().Subscribe(base.ViewModel.NextPage).AddTo(this);
		m_ResetToDefaultButton.OnConfirmClickAsObservable().Subscribe(base.ViewModel.RevertSettings).AddTo(this);
		UIKeybindGeneralSettings uIKeybindGeneralSettings = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.PrevTab.name, base.ViewModel.PreviousPage).AddTo(this);
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.NextTab.name, base.ViewModel.NextPage).AddTo(this);
	}

	protected override void SetupTexts()
	{
		LocalizedString localizedString = (IsFocusedOnLanguageItem ? UIStrings.Instance.SettingsUI.Cancel : UIStrings.Instance.ContextMenu.Back);
		LocalizedString localizedString2 = (IsFocusedOnLanguageItem ? UIStrings.Instance.SettingsUI.Apply : UIStrings.Instance.MainMenu.Continue);
		m_ResetToDefaultText.text = UIStrings.Instance.SettingsUI.Default;
		m_BackText.text = localizedString;
		m_ContinueText.text = localizedString2;
	}
}
