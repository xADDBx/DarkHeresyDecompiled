using System;
using System.Linq;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Settings;
using Kingmaker.UI.Canvases;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsConsoleView : SettingsBaseView
{
	[Serializable]
	public class SettingsViews
	{
		[SerializeField]
		private SettingsEntityHeaderConsoleView m_SettingsEntityHeaderViewPrefab;

		[SerializeField]
		private SettingsEntityBoolConsoleView m_SettingsEntityBoolViewPrefab;

		[SerializeField]
		private SettingsEntityDropdownConsoleView m_SettingsEntityDropdownViewPrefab;

		[SerializeField]
		private SettingsEntitySliderConsoleView m_SettingsEntitySliderViewPrefab;

		[SerializeField]
		private SettingsEntityDropdownGameDifficultyConsoleView m_SettingsEntityDropdownGameDifficultyViewPrefab;

		[SerializeField]
		private SettingsEntitySliderGammaCorrectionConsoleView m_SettingsEntitySliderGammaCorrectionViewPrefab;

		[SerializeField]
		private SettingEntityKeyBindingConsoleView m_SettingEntityKeyBindingViewPrefab;

		[SerializeField]
		private SettingsEntityDisplayImagesConsoleView m_SettingEntityDisplayImagesViewPrefab;

		[SerializeField]
		private SettingsEntityAccessibilityImageConsoleView m_SettingEntityAccessibilityImageViewPrefab;

		[SerializeField]
		private SettingsEntitySliderFontSizeConsoleView m_SettingEntityFontSizeViewPrefab;

		[SerializeField]
		private SettingsEntityBoolOnlyOneSaveConsoleView m_SettingsEntityBoolOnlyOneSaveViewPrefab;

		public void InitializeVirtualList(VirtualListComponent virtualListComponent)
		{
			virtualListComponent.Initialize(new VirtualListElementTemplate<SettingsEntityHeaderVM>(m_SettingsEntityHeaderViewPrefab), new VirtualListElementTemplate<SettingsEntityBoolVM>(m_SettingsEntityBoolViewPrefab), new VirtualListElementTemplate<SettingsEntityDropdownVM>(m_SettingsEntityDropdownViewPrefab, 0), new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingsEntitySliderViewPrefab, 0), new VirtualListElementTemplate<SettingEntityKeyBindingVM>(m_SettingEntityKeyBindingViewPrefab), new VirtualListElementTemplate<SettingsEntityDropdownGameDifficultyVM>(m_SettingsEntityDropdownGameDifficultyViewPrefab, 0), new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingsEntitySliderGammaCorrectionViewPrefab, 1), new VirtualListElementTemplate<SettingsEntityDisplayImagesVM>(m_SettingEntityDisplayImagesViewPrefab), new VirtualListElementTemplate<SettingsEntityAccessibilityImageVM>(m_SettingEntityAccessibilityImageViewPrefab), new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingEntityFontSizeViewPrefab, 2), new VirtualListElementTemplate<SettingsEntityBoolOnlyOneSaveVM>(m_SettingsEntityBoolOnlyOneSaveViewPrefab));
		}
	}

	[Header("Console")]
	[SerializeField]
	private SettingsViews m_SettingsViews;

	[SerializeField]
	private RectTransform m_BigGreenScreenView;

	[SerializeField]
	private RectTransform m_LittleGreenScreenView;

	[SerializeField]
	private RectTransform m_PaperGroup;

	[SerializeField]
	private RectTransform m_ScrollBarObject;

	[Header("Input")]
	[SerializeField]
	private HintView m_PrevHint;

	[SerializeField]
	private HintView m_NextHint;

	[Header("Controls")]
	[SerializeField]
	private GameObject m_ControlsConsoleGroup;

	[SerializeField]
	private SettingsControlConsole m_PSConsoleGroup;

	[SerializeField]
	private SettingsControlConsole m_DualSenseConsoleGroup;

	[SerializeField]
	private SettingsControlConsole m_XBoxConsoleGroup;

	[SerializeField]
	private SettingsControlConsole m_SwitchConsoleGroup;

	[SerializeField]
	private SettingsControlConsole m_SteamConsoleGroup;

	[SerializeField]
	private SettingsControlConsole m_SteamDeckConsoleGroup;

	[Header("SafeZone")]
	[SerializeField]
	private RectTransform m_SafeZoneFrame;

	private readonly ReactiveProperty<bool> m_IsVisibleConfirm = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsVisibleResetToDefault = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_GlossaryMode = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasGlossary = new ReactiveProperty<bool>();

	private TooltipConfig m_TooltipConfig;

	public static readonly string SettingsInputLayerName = "SettingsConsoleViewInput";

	public static readonly string GlossarySettingsInputLayerName = "SettingsGlossary";

	protected override void OnBind()
	{
		m_SettingsViews.InitializeVirtualList(m_VirtualList);
		m_TooltipConfig.IsGlossary = true;
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.OnSwitchSettings, delegate
		{
			OnSelectedMenuEntity(base.ViewModel.SelectedMenuEntity.CurrentValue);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.OnApplyWindowClose, delegate
		{
			DelayedWindowClose();
		}).AddTo(this);
		base.ViewModel.IsConsoleControls.Subscribe(delegate(bool value)
		{
			m_ScrollBarObject.gameObject.SetActive(!value);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.LanguageChanged, delegate
		{
			UpdateLocalization();
		}).AddTo(this);
		SettingsRoot.Display.SafeZoneOffset.OnTempValueChanged += OnSafeZoneChanged;
		OnSafeZoneChanged(SettingsRoot.Display.SafeZoneOffset.GetValue());
		OnSelectedMenuEntity(base.ViewModel.SelectedMenuEntity.CurrentValue);
		BindHints();
		base.OnBind();
	}

	protected override void OnUnbind()
	{
		SettingsRoot.Display.SafeZoneOffset.OnTempValueChanged -= OnSafeZoneChanged;
		base.OnUnbind();
	}

	protected override void OnSelectedMenuEntity(SettingsMenuEntityVM entity)
	{
		m_InfoView.gameObject.SetActive(!base.ViewModel.IsConsoleControls.CurrentValue);
		m_PaperGroup.gameObject.SetActive(!base.ViewModel.IsConsoleControls.CurrentValue);
		m_LittleGreenScreenView.gameObject.SetActive(!base.ViewModel.IsConsoleControls.CurrentValue);
		m_VirtualList.gameObject.SetActive(!base.ViewModel.IsConsoleControls.CurrentValue);
		m_BigGreenScreenView.gameObject.SetActive(base.ViewModel.IsConsoleControls.CurrentValue);
		m_ControlsConsoleGroup.SetActive(base.ViewModel.IsConsoleControls.CurrentValue);
		m_IsVisibleResetToDefault.Value = !base.ViewModel.IsConsoleControls.CurrentValue && base.ViewModel.IsDefaultButtonInteractable.CurrentValue;
		m_SafeZoneFrame.gameObject.SetActive(entity.SettingsScreenType == UISettingsManager.SettingsScreen.Display);
		int index = base.ViewModel.MenuEntitiesList.IndexOf(base.ViewModel.MenuEntitiesList.FirstOrDefault((SettingsMenuEntityVM e) => e == base.ViewModel.SelectedMenuEntity.CurrentValue));
		m_SelectorView.ChangeTab(index);
		if (base.ViewModel.IsConsoleControls.CurrentValue)
		{
			SetupConsoleControls();
		}
	}

	private void OnApplyWindowClose()
	{
		SettingsVM viewModel = base.ViewModel;
		if (viewModel != null && viewModel.IsConsoleControls.CurrentValue)
		{
			SetupConsoleControls();
		}
	}

	private void DelayedWindowClose()
	{
		ObservableSubscribeExtensions.Subscribe(Observable.TimerFrame(1), delegate
		{
			OnApplyWindowClose();
		}).AddTo(this);
	}

	private void SetupConsoleControls()
	{
	}

	private void GetInputLayer()
	{
	}

	private void Scroll(float value)
	{
		m_InfoView.Scroll(value);
	}

	private void Update()
	{
		bool flag = SettingsController.Instance.HasUnconfirmedSettings();
		if (m_IsVisibleConfirm.Value != flag)
		{
			m_IsVisibleConfirm.Value = flag;
		}
	}

	private void BindHints()
	{
	}

	private void OnSafeZoneChanged(int value)
	{
		float num = (float)value / 200f;
		Rect rect = MainCanvas.Instance.RectTransform.rect;
		m_SafeZoneFrame.offsetMin = new Vector2(rect.width * num, rect.height * num);
		m_SafeZoneFrame.offsetMax = new Vector2((0f - rect.width) * num, (0f - rect.height) * num);
	}

	public void OnGlossaryFocusedChanged(IConsoleEntity entity)
	{
	}

	protected override void UpdateLocalization()
	{
	}

	private void OnCurrentInputLayerChanged()
	{
	}
}
