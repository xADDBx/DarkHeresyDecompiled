using System;
using System.Linq;
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

[ViewFactoryPolicy(ViewFactoryPolicyFlag.DontReparent, null)]
public class SettingsPCView : SettingsBaseView
{
	[Serializable]
	public class SettingsViews
	{
		[SerializeField]
		private SettingsEntityHeaderView m_SettingsEntityHeaderViewPrefab;

		[SerializeField]
		private SettingsEntityBoolPCView m_SettingsEntityBoolViewPrefab;

		[SerializeField]
		private SettingsEntityDropdownPCView m_SettingsEntityDropdownViewPrefab;

		[SerializeField]
		private SettingsEntitySliderPCView m_SettingsEntitySliderViewPrefab;

		[SerializeField]
		private SettingsEntityDropdownGameDifficultyPCView m_SettingsEntityDropdownGameDifficultyViewPrefab;

		[SerializeField]
		private SettingsEntitySliderGammaCorrectionPCView m_SettingsEntitySliderGammaCorrectionViewPrefab;

		[SerializeField]
		private SettingEntityKeyBindingPCView m_SettingEntityKeyBindingViewPrefab;

		[SerializeField]
		private SettingsEntityDisplayImagesPCView m_SettingEntityDisplayImagesViewPrefab;

		[SerializeField]
		private SettingsEntityAccessibilityImagePCView m_SettingEntityAccessibilityImageViewPrefab;

		[SerializeField]
		private SettingsEntitySliderFontSizePCView m_SettingEntityFontSizeViewPrefab;

		[SerializeField]
		private SettingsEntityBoolOnlyOneSavePCView m_SettingsEntityBoolOnlyOneSaveViewPrefab;

		public void InitializeVirtualList(VirtualListComponent virtualListComponent)
		{
			virtualListComponent.Initialize(new VirtualListElementTemplate<SettingsEntityHeaderVM>(m_SettingsEntityHeaderViewPrefab), new VirtualListElementTemplate<SettingsEntityBoolVM>(m_SettingsEntityBoolViewPrefab), new VirtualListElementTemplate<SettingsEntityDropdownVM>(m_SettingsEntityDropdownViewPrefab, 0), new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingsEntitySliderViewPrefab, 0), new VirtualListElementTemplate<SettingEntityKeyBindingVM>(m_SettingEntityKeyBindingViewPrefab), new VirtualListElementTemplate<SettingsEntityDisplayImagesVM>(m_SettingEntityDisplayImagesViewPrefab), new VirtualListElementTemplate<SettingsEntityAccessibilityImageVM>(m_SettingEntityAccessibilityImageViewPrefab), new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingEntityFontSizeViewPrefab, 2), new VirtualListElementTemplate<SettingsEntityDropdownGameDifficultyVM>(m_SettingsEntityDropdownGameDifficultyViewPrefab, 0), new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingsEntitySliderGammaCorrectionViewPrefab, 1), new VirtualListElementTemplate<SettingsEntityBoolOnlyOneSaveVM>(m_SettingsEntityBoolOnlyOneSaveViewPrefab));
		}
	}

	[Header("PC")]
	[SerializeField]
	protected SettingsViews m_SettingsViews;

	[SerializeField]
	private KeyBindingSetupDialogPCView m_KeyBindingSetupDialogView;

	[Header("Buttons")]
	[SerializeField]
	public OwlcatMultiButton m_CloseButton;

	[SerializeField]
	public OwlcatMultiButton m_DefaultButton;

	[SerializeField]
	public TextMeshProUGUI m_DefaultText;

	[SerializeField]
	public OwlcatMultiButton m_CancelButton;

	[SerializeField]
	public TextMeshProUGUI m_CancelText;

	[SerializeField]
	public OwlcatMultiButton m_ApplyButton;

	[SerializeField]
	public TextMeshProUGUI m_ApplyText;

	public override void Awake()
	{
		base.Awake();
		m_KeyBindingSetupDialogView.Initialize();
		m_SettingsViews.InitializeVirtualList(m_VirtualList);
	}

	protected override void OnBind()
	{
		base.ViewModel.CurrentKeyBindDialog.Subscribe(OpenKeyBindSetupDialog).AddTo(this);
		SetButtonsSounds();
		m_CloseButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.Close).AddTo(this);
		m_DefaultButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.OpenDefaultSettingsDialog).AddTo(this);
		m_CancelButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.OpenCancelSettingsDialog).AddTo(this);
		m_ApplyButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.OpenApplySettingsDialog).AddTo(this);
		base.ViewModel.IsApplyButtonInteractable.DelayFrame(1).Subscribe(delegate(bool value)
		{
			m_ApplyButton.Interactable = value;
		}).AddTo(this);
		base.ViewModel.IsCancelButtonInteractable.DelayFrame(1).Subscribe(delegate(bool value)
		{
			m_CancelButton.Interactable = value;
		}).AddTo(this);
		base.ViewModel.IsDefaultButtonInteractable.DelayFrame(1).Subscribe(delegate(bool value)
		{
			m_DefaultButton.Interactable = value;
		}).AddTo(this);
		SetBottomButtonsTexts();
		UIKeybindGeneralSettings uIKeybindGeneralSettings = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.PrevTab.name, delegate
		{
			m_MenuSelector.OnPrev();
		}).AddTo(this);
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.NextTab.name, delegate
		{
			m_MenuSelector.OnNext();
		}).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close).AddTo(this);
		base.OnBind();
	}

	private void SetButtonsSounds()
	{
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_DefaultButton, ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_CancelButton, ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_ApplyButton, ButtonSoundsEnum.PlastickSound);
	}

	private void OpenKeyBindSetupDialog(KeyBindingSetupDialogVM dialogVM)
	{
		if (dialogVM != null)
		{
			m_KeyBindingSetupDialogView.Bind(dialogVM);
		}
	}

	private void SetBottomButtonsTexts()
	{
		m_DefaultText.text = UIStrings.Instance.SettingsUI.Default;
		m_CancelText.text = UIStrings.Instance.SettingsUI.Cancel;
		m_ApplyText.text = UIStrings.Instance.SettingsUI.Apply;
	}

	protected override void UpdateLocalization()
	{
		SetBottomButtonsTexts();
	}

	protected override void OnSelectedMenuEntity(SettingsMenuEntityVM entity)
	{
		int index = base.ViewModel.MenuEntitiesList.IndexOf(base.ViewModel.MenuEntitiesList.FirstOrDefault((SettingsMenuEntityVM e) => e == base.ViewModel.SelectedMenuEntity.CurrentValue));
		m_SelectorView.ChangeTab(index);
	}
}
