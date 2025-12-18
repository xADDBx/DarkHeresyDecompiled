using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Networking;
using Kingmaker.Settings;
using Kingmaker.UI.Canvases;
using Kingmaker.UI.Common;
using Owlcat.UI;
using R3;
using Rewired;
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
		private SettingsEntityStatisticsOptOutConsoleView m_SettingsEntityStatisticsOptOutViewPrefab;

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
			virtualListComponent.Initialize(new VirtualListElementTemplate<SettingsEntityHeaderVM>(m_SettingsEntityHeaderViewPrefab), new VirtualListElementTemplate<SettingsEntityBoolVM>(m_SettingsEntityBoolViewPrefab), new VirtualListElementTemplate<SettingsEntityDropdownVM>(m_SettingsEntityDropdownViewPrefab, 0), new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingsEntitySliderViewPrefab, 0), new VirtualListElementTemplate<SettingEntityKeyBindingVM>(m_SettingEntityKeyBindingViewPrefab), new VirtualListElementTemplate<SettingsEntityDropdownGameDifficultyVM>(m_SettingsEntityDropdownGameDifficultyViewPrefab, 0), new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingsEntitySliderGammaCorrectionViewPrefab, 1), new VirtualListElementTemplate<SettingsEntityStatisticsOptOutVM>(m_SettingsEntityStatisticsOptOutViewPrefab), new VirtualListElementTemplate<SettingsEntityDisplayImagesVM>(m_SettingEntityDisplayImagesViewPrefab), new VirtualListElementTemplate<SettingsEntityAccessibilityImageVM>(m_SettingEntityAccessibilityImageViewPrefab), new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingEntityFontSizeViewPrefab, 2), new VirtualListElementTemplate<SettingsEntityBoolOnlyOneSaveVM>(m_SettingsEntityBoolOnlyOneSaveViewPrefab));
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
	private ConsoleHint m_PrevHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	[SerializeField]
	protected ConsoleHintsWidget m_ConsoleHintsWidget;

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

	private InputBindStruct m_ResetToDefaultStruct;

	private InputBindStruct m_ConfirmStruct;

	private InputBindStruct m_DeclineStruct;

	private readonly ReactiveProperty<bool> m_IsVisibleConfirm = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsVisibleResetToDefault = new ReactiveProperty<bool>();

	private GridConsoleNavigationBehaviour m_NavigationBehavior;

	private InputLayer m_GlossaryInputLayer;

	private GridConsoleNavigationBehaviour m_GlossaryNavigationBehavior;

	private readonly ReactiveProperty<bool> m_GlossaryMode = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasGlossary = new ReactiveProperty<bool>();

	private TooltipConfig m_TooltipConfig;

	private IConsoleHint m_ResetToDefaultHint;

	private IConsoleHint m_ConfirmHint;

	private IConsoleHint m_DeclineHint;

	private InputLayer m_SettingsInputLayer;

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
		m_GlossaryNavigationBehavior = new GridConsoleNavigationBehaviour().AddTo(this);
		m_GlossaryNavigationBehavior.DeepestFocusAsObservable.Subscribe(OnGlossaryFocusedChanged).AddTo(this);
		m_NavigationBehavior = m_VirtualList.GetNavigationBehaviour().AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_VirtualList.AttachedFirstValidView, delegate
		{
			FocusOnValidEntity(m_NavigationBehavior);
		}).AddTo(this);
		base.ViewModel.ReactiveTooltipTemplate.Subscribe(delegate
		{
			CalculateGlossary();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.LanguageChanged, delegate
		{
			UpdateLocalization();
		}).AddTo(this);
		SettingsRoot.Display.SafeZoneOffset.OnTempValueChanged += OnSafeZoneChanged;
		OnSafeZoneChanged(SettingsRoot.Display.SafeZoneOffset.GetValue());
		OnSelectedMenuEntity(base.ViewModel.SelectedMenuEntity.CurrentValue);
		BindHints();
		GamePad.Instance.PushLayer(GetInputLayer(m_NavigationBehavior)).AddTo(this);
		GamePad.Instance.BaseLayer?.Unbind();
		GamePad.Instance.OnLayerPushed.Subscribe(OnCurrentInputLayerChanged).AddTo(this);
		base.OnBind();
	}

	protected override void OnUnbind()
	{
		GamePad.Instance.BaseLayer?.Bind();
		m_ConsoleHintsWidget.Dispose();
		SettingsRoot.Display.SafeZoneOffset.OnTempValueChanged -= OnSafeZoneChanged;
		base.OnUnbind();
	}

	private void FocusOnValidEntity(GridConsoleNavigationBehaviour navigationBehavior)
	{
		foreach (IConsoleEntity entity in navigationBehavior.Entities)
		{
			if (entity.IsValid())
			{
				navigationBehavior.FocusOnEntityManual(entity);
				return;
			}
		}
		navigationBehavior.FocusOnFirstValidEntity();
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
		m_DualSenseConsoleGroup.gameObject.SetActive(value: false);
		m_PSConsoleGroup.gameObject.SetActive(value: false);
		m_XBoxConsoleGroup.gameObject.SetActive(value: false);
		m_SwitchConsoleGroup.gameObject.SetActive(value: false);
		m_SteamConsoleGroup.gameObject.SetActive(value: false);
		m_SteamDeckConsoleGroup.gameObject.SetActive(value: false);
		SettingsControlConsole settingsControlConsole = null;
		switch (GamePad.Instance.Type)
		{
		case ConsoleType.PS4:
			settingsControlConsole = m_PSConsoleGroup;
			m_PSConsoleGroup.gameObject.SetActive(value: true);
			break;
		case ConsoleType.PS5:
			settingsControlConsole = m_DualSenseConsoleGroup;
			m_DualSenseConsoleGroup.gameObject.SetActive(value: true);
			break;
		case ConsoleType.XBox:
			settingsControlConsole = m_XBoxConsoleGroup;
			m_XBoxConsoleGroup.gameObject.SetActive(value: true);
			break;
		case ConsoleType.Switch:
			settingsControlConsole = m_SwitchConsoleGroup;
			m_SwitchConsoleGroup.gameObject.SetActive(value: true);
			break;
		case ConsoleType.SteamController:
			settingsControlConsole = m_SteamConsoleGroup;
			m_SteamConsoleGroup.gameObject.SetActive(value: true);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case ConsoleType.Common:
			break;
		}
		if (!(settingsControlConsole == null))
		{
			InputLayer currentInputLayer = GamePad.Instance.CurrentInputLayer;
			settingsControlConsole.LeftStickButtonHint.BindCustomAction(18, currentInputLayer).AddTo(this);
			settingsControlConsole.LeftStickButtonHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlLeftStickButtonHint);
			settingsControlConsole.LeftStickButtonHint.SetActive(state: true);
			settingsControlConsole.DPadRightHint.BindCustomAction(5, currentInputLayer).AddTo(this);
			settingsControlConsole.DPadRightHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlDPadRightHint);
			settingsControlConsole.DPadDownHint.BindCustomAction(7, currentInputLayer).AddTo(this);
			settingsControlConsole.DPadDownHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlDPadDownHint);
			settingsControlConsole.DPadLeftHint.BindCustomAction(4, currentInputLayer).AddTo(this);
			settingsControlConsole.DPadLeftHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlDPadLeftHint);
			settingsControlConsole.DPadUpHint.BindCustomAction(6, currentInputLayer).AddTo(this);
			settingsControlConsole.DPadUpHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlDPadUpHint);
			settingsControlConsole.LeftUpHint.BindCustomAction(14, currentInputLayer).AddTo(this);
			settingsControlConsole.LeftUpHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlLeftUpHint);
			settingsControlConsole.LeftBottomHint.BindCustomAction(12, currentInputLayer).AddTo(this);
			settingsControlConsole.LeftBottomHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlLeftBottomHint);
			settingsControlConsole.FuncAdditionalHint.BindCustomAction(17, currentInputLayer).AddTo(this);
			settingsControlConsole.FuncAdditionalHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlFuncAdditionalHint);
			settingsControlConsole.RightBottomHint.BindCustomAction(13, currentInputLayer).AddTo(this);
			settingsControlConsole.RightBottomHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlRightBottomHint);
			settingsControlConsole.RightUpHint.BindCustomAction(15, currentInputLayer).AddTo(this);
			settingsControlConsole.RightUpHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlRightUpHint);
			settingsControlConsole.OptionsHint.BindCustomAction(16, currentInputLayer).AddTo(this);
			settingsControlConsole.OptionsHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlOptionsHint);
			settingsControlConsole.Func02Hint.BindCustomAction(11, currentInputLayer).AddTo(this);
			settingsControlConsole.Func02Hint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlFunc02Hint);
			settingsControlConsole.DeclineHint.BindCustomAction(9, currentInputLayer).AddTo(this);
			settingsControlConsole.DeclineHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlDeclineHint);
			settingsControlConsole.ConfirmHint.BindCustomAction(8, currentInputLayer).AddTo(this);
			settingsControlConsole.ConfirmHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlConfirmHint);
			settingsControlConsole.Func01Hint.BindCustomAction(10, currentInputLayer).AddTo(this);
			settingsControlConsole.Func01Hint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlFunc01Hint);
			settingsControlConsole.RightStickButtonHint.BindCustomAction(19, currentInputLayer).AddTo(this);
			settingsControlConsole.RightStickButtonHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlRightStickButtonHint);
			bool isActive = PhotonManager.Lobby.IsActive;
			settingsControlConsole.ConsoleCoopPingHint.gameObject.SetActive(isActive);
			if (isActive)
			{
				settingsControlConsole.ConsoleCoopPingHint.text = UIStrings.Instance.SettingsUI.ConsoleControlPingCoopHint;
			}
		}
	}

	private InputLayer GetInputLayer(ConsoleNavigationBehaviour navigationBehavior)
	{
		m_SettingsInputLayer = navigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = SettingsInputLayerName
		});
		m_GlossaryInputLayer = m_GlossaryNavigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = GlossarySettingsInputLayerName
		});
		m_DeclineStruct = m_SettingsInputLayer.AddButton(delegate
		{
			base.ViewModel.Close();
		}, 9).AddTo(this);
		m_ConfirmStruct = m_SettingsInputLayer.AddButton(delegate
		{
			base.ViewModel.OpenApplySettingsDialog();
		}, 8, m_IsVisibleConfirm).AddTo(this);
		m_ResetToDefaultStruct = m_SettingsInputLayer.AddButton(delegate
		{
			base.ViewModel.OpenDefaultSettingsDialog();
		}, 11, m_IsVisibleResetToDefault, InputActionEventType.ButtonJustLongPressed).AddTo(this);
		m_SettingsInputLayer.AddAxis(Scroll, 3, repeat: true).AddTo(this);
		m_PrevHint.Bind(m_SettingsInputLayer.AddButton(delegate
		{
			m_MenuSelector.OnPrev();
		}, 14)).AddTo(this);
		m_NextHint.Bind(m_SettingsInputLayer.AddButton(delegate
		{
			m_MenuSelector.OnNext();
		}, 15)).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_SettingsInputLayer.AddButton(ShowGlossary, 11, m_HasGlossary, InputActionEventType.ButtonJustReleased), UIStrings.Instance.Dialog.OpenGlossary).AddTo(this);
		m_GlossaryInputLayer.AddAxis(Scroll, 3, repeat: true).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_GlossaryInputLayer.AddButton(delegate
		{
			CloseGlossary();
		}, 9, m_GlossaryMode), UIStrings.Instance.Dialog.CloseGlossary).AddTo(this);
		m_GlossaryInputLayer.AddButton(delegate
		{
			CloseGlossary();
		}, 11, m_GlossaryMode, InputActionEventType.ButtonJustReleased).AddTo(this);
		return m_SettingsInputLayer;
	}

	private void ShowGlossary(InputActionEventData data)
	{
		m_NavigationBehavior.UnFocusCurrentEntity();
		(m_NavigationBehavior.CurrentEntity as ExpandableElement)?.SetCustomLayer("On");
		m_GlossaryMode.Value = true;
		GamePad.Instance.PushLayer(m_GlossaryInputLayer).AddTo(this);
		CalculateGlossary();
		m_GlossaryNavigationBehavior.FocusOnFirstValidEntity();
	}

	private void CloseGlossary()
	{
		TooltipHelper.HideTooltip();
		m_GlossaryNavigationBehavior.UnFocusCurrentEntity();
		GamePad.Instance.PopLayer(m_GlossaryInputLayer);
		m_GlossaryMode.Value = false;
		m_NavigationBehavior.FocusOnCurrentEntity();
	}

	private void CalculateGlossary()
	{
		if (m_GlossaryNavigationBehavior != null)
		{
			m_GlossaryNavigationBehavior.Clear();
			List<IConsoleEntity> entities = m_InfoView.GetNavigationBehaviour().Entities.Where((IConsoleEntity e) => e is FloatConsoleNavigationBehaviour).ToList();
			m_GlossaryNavigationBehavior.AddColumn(entities);
			m_HasGlossary.Value = m_GlossaryNavigationBehavior != null && m_GlossaryNavigationBehavior.Entities.Any();
			if (m_GlossaryMode.Value)
			{
				TooltipHelper.HideTooltip();
			}
		}
	}

	private void ShowTooltip()
	{
		IConsoleEntity value = m_GlossaryNavigationBehavior.DeepestFocusAsObservable.Value;
		MonoBehaviour component = (value as MonoBehaviour) ?? (value as IMonoBehaviour)?.MonoBehaviour;
		TooltipBaseTemplate template = (value as IHasTooltipTemplate)?.TooltipTemplate();
		component.ShowConsoleTooltip(template, m_GlossaryNavigationBehavior, m_TooltipConfig);
	}

	private void Scroll(InputActionEventData obj, float value)
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
		m_ResetToDefaultHint = m_ConsoleHintsWidget.BindHint(m_ResetToDefaultStruct).AddTo(this);
		m_ResetToDefaultHint.SetLabel(UIStrings.Instance.SettingsUI.ResetToDefaultHold);
		m_ConfirmHint = m_ConsoleHintsWidget.BindHint(m_ConfirmStruct, "", ConsoleHintsWidget.HintPosition.Right).AddTo(this);
		m_ConfirmHint.SetLabel(UIStrings.Instance.CommonTexts.Accept);
		m_DeclineHint = m_ConsoleHintsWidget.BindHint(m_DeclineStruct, "", ConsoleHintsWidget.HintPosition.Left).AddTo(this);
		m_DeclineHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
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
		MonoBehaviour monoBehaviour = ((!(entity is TMPLinkNavigationEntity tMPLinkNavigationEntity)) ? null : tMPLinkNavigationEntity.MonoBehaviour);
		MonoBehaviour monoBehaviour2 = monoBehaviour;
		if (monoBehaviour2 != null)
		{
			RectTransform targetRect = monoBehaviour2.transform as RectTransform;
			m_InfoView.ScrollRectExtended.EnsureVisibleVertical(targetRect, 50f, smoothly: false, needPinch: false);
		}
		ShowTooltip();
	}

	protected override void UpdateLocalization()
	{
		m_ResetToDefaultHint.SetLabel(UIStrings.Instance.SettingsUI.ResetToDefaultHold);
		m_ConfirmHint.SetLabel(UIStrings.Instance.CommonTexts.Accept);
		m_DeclineHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
	}

	private void OnCurrentInputLayerChanged()
	{
		GamePad instance = GamePad.Instance;
		if (instance.CurrentInputLayer != m_SettingsInputLayer && instance.CurrentInputLayer != m_GlossaryInputLayer && !(instance.CurrentInputLayer.ContextName == BugReportBaseView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == BugReportDrawingView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == InfoWindowConsoleView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == MessageBoxConsoleView.InputLayerName) && !(instance.CurrentInputLayer.ContextName == OwlcatDropdown.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == OwlcatInputField.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == BugReportBaseView.LabelsDisposableString) && !(instance.CurrentInputLayer.ContextName == "BugReportDuplicatesViewInput") && !(instance.CurrentInputLayer.ContextName == CrossPlatformConsoleVirtualKeyboard.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == NetLobbyConsoleView.InputLayerName))
		{
			instance.PopLayer(m_SettingsInputLayer);
			instance.PushLayer(m_SettingsInputLayer);
			if (m_GlossaryMode.Value)
			{
				instance.PopLayer(m_GlossaryInputLayer);
				instance.PushLayer(m_GlossaryInputLayer);
			}
		}
	}
}
