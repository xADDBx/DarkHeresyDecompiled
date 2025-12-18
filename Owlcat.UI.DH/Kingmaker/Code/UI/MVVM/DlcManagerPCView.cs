using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerPCView : DlcManagerBaseView
{
	[Header("Views")]
	[SerializeField]
	private DlcManagerTabDlcsPCView m_DlcManagerTabDlcsPCView;

	[SerializeField]
	private DlcManagerTabModsPCView m_DlcManagerTabModsPCView;

	[SerializeField]
	private DlcManagerTabSwitchOnDlcsPCView m_DlcManagerTabSwitchOnDlcsPCView;

	[SerializeField]
	private RectTransform m_BottomButtonsContainer;

	[Header("Buttons")]
	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private OwlcatButton m_ApplyBottomButton;

	[SerializeField]
	private TextMeshProUGUI m_ApplyBottomButtonLabel;

	[SerializeField]
	private OwlcatButton m_DefaultBottomButton;

	[SerializeField]
	private TextMeshProUGUI m_DefaultBottomButtonLabel;

	protected override void InitializeImpl()
	{
		if (!base.ViewModel.InGame)
		{
			m_DlcManagerTabDlcsPCView.Initialize();
		}
		else
		{
			m_DlcManagerTabSwitchOnDlcsPCView.Initialize();
		}
		if (!base.ViewModel.IsConsole)
		{
			m_DlcManagerTabModsPCView.Initialize();
		}
		m_ApplyBottomButton.SetInteractable(state: false);
		m_DefaultBottomButton.SetInteractable(state: false);
	}

	protected override void OnBind()
	{
		base.OnBind();
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.OnClose).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClose();
		}).AddTo(this);
		base.ViewModel.IsModsWindow.CombineLatest(base.ViewModel.IsSwitchOnDlcsWindow, (bool isModsWindow, bool isSwitchOnDlcsWindow) => new { isModsWindow, isSwitchOnDlcsWindow }).Subscribe(value =>
		{
			m_BottomButtonsContainer.gameObject.SetActive(value.isModsWindow || value.isSwitchOnDlcsWindow);
		}).AddTo(this);
		SetButtonsSounds();
		if (!base.ViewModel.InGame)
		{
			m_DlcManagerTabDlcsPCView.Bind(base.ViewModel.DlcsVM);
		}
		else
		{
			m_DlcManagerTabSwitchOnDlcsPCView.Bind(base.ViewModel.SwitchOnDlcsVM);
		}
		if (!base.ViewModel.IsConsole)
		{
			m_DlcManagerTabModsPCView.Bind(base.ViewModel.ModsVM);
		}
		m_ApplyBottomButtonLabel.text = UIStrings.Instance.SettingsUI.Apply;
		m_DefaultBottomButtonLabel.text = UIStrings.Instance.SettingsUI.Default;
		ObservableSubscribeExtensions.Subscribe(m_ApplyBottomButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.CheckToReloadGame(null);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_DefaultBottomButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.RestoreAllToPreviousState();
		}).AddTo(this);
		if (base.ViewModel.InGame)
		{
			base.ViewModel.ModsVM.NeedReload.CombineLatest(base.ViewModel.SwitchOnDlcsVM.NeedResave, (bool needReload, bool needResave) => new { needReload, needResave }).Subscribe(value =>
			{
				m_ApplyBottomButton.SetInteractable(value.needReload || value.needResave);
				m_DefaultBottomButton.SetInteractable(value.needReload || value.needResave);
			}).AddTo(this);
		}
		else
		{
			base.ViewModel.ModsVM.NeedReload.Subscribe(delegate(bool value)
			{
				m_ApplyBottomButton.SetInteractable(value);
				m_DefaultBottomButton.SetInteractable(value);
			}).AddTo(this);
		}
		UIKeybindGeneralSettings uIKeybindGeneralSettings = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.PrevTab.name, m_Selector.OnPrev).AddTo(this);
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.NextTab.name, m_Selector.OnNext).AddTo(this);
	}

	private void SetButtonsSounds()
	{
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_ApplyBottomButton, ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_DefaultBottomButton, ButtonSoundsEnum.PlastickSound);
	}
}
