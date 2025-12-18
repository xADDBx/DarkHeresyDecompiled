using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.UI.Common.SafeZone;
using Kingmaker.Code.UI.MVVM.Common;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;
using Kingmaker.Localization.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.Runtime.Core.Logging;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsVM : ViewModel, IKeyBindingSetupDialogHandler, ISubscriber, ISettingsDescriptionUIHandler, IOptionsWindowUIHandler, ISaveSettingsHandler, ISettingsFontSizeUIHandler, ILocalizationHandler
{
	public readonly List<SettingsMenuEntityVM> MenuEntitiesList = new List<SettingsMenuEntityVM>();

	public readonly SelectionGroupRadioVM<SettingsMenuEntityVM> SelectionGroup;

	public readonly LensSelectorVM Selector;

	public readonly InfoSectionVM InfoVM;

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<bool> m_IsDefaultButtonInteractable = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsApplyButtonInteractable = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsCancelButtonInteractable = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsConsoleControls = new ReactiveProperty<bool>();

	private readonly ReactiveCommand<Unit> m_OnApplyWindowClose = new ReactiveCommand<Unit>();

	private readonly ReactiveCommand<Unit> m_OnSwitchSettings = new ReactiveCommand<Unit>();

	private readonly ReactiveCommand<Unit> m_LanguageChanged = new ReactiveCommand<Unit>();

	private readonly ObservableList<VirtualListElementVMBase> m_SettingEntities = new ObservableList<VirtualListElementVMBase>();

	private readonly ReactiveProperty<SettingsMenuEntityVM> m_SelectedMenuEntity = new ReactiveProperty<SettingsMenuEntityVM>();

	private readonly ReactiveProperty<KeyBindingSetupDialogVM> m_CurrentKeyBindDialog = new ReactiveProperty<KeyBindingSetupDialogVM>();

	private readonly Action m_CloseAction;

	private SettingsMenuEntityVM m_PreviousSelectedMenuEntity;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> ReactiveTooltipTemplate => m_ReactiveTooltipTemplate;

	public ReadOnlyReactiveProperty<bool> IsDefaultButtonInteractable => m_IsDefaultButtonInteractable;

	public ReadOnlyReactiveProperty<bool> IsApplyButtonInteractable => m_IsApplyButtonInteractable;

	public ReadOnlyReactiveProperty<bool> IsCancelButtonInteractable => m_IsCancelButtonInteractable;

	public ReadOnlyReactiveProperty<bool> IsConsoleControls => m_IsConsoleControls;

	public Observable<Unit> OnApplyWindowClose => m_OnApplyWindowClose;

	public Observable<Unit> OnSwitchSettings => m_OnSwitchSettings;

	public Observable<Unit> LanguageChanged => m_LanguageChanged;

	public ReadOnlyReactiveProperty<SettingsMenuEntityVM> SelectedMenuEntity => m_SelectedMenuEntity;

	public ObservableList<VirtualListElementVMBase> SettingEntities => m_SettingEntities;

	public ReadOnlyReactiveProperty<KeyBindingSetupDialogVM> CurrentKeyBindDialog => m_CurrentKeyBindDialog;

	public SettingsVM(Action closeAction, bool isMainMenu = false)
	{
		UISettingsRoot.Instance.UIGraphicsSettings.UpdateInteractable(initialize: true);
		UISettingsRoot.Instance.UIGameMainSettings.UpdateInteractable();
		m_CloseAction = closeAction;
		UITextSettingsUI settingsUI = UIStrings.Instance.SettingsUI;
		CreateMenuEntity(settingsUI.SectionNameGame, UISettingsManager.SettingsScreen.Game);
		if (!isMainMenu)
		{
			CreateMenuEntity(settingsUI.SectionNameDifficulty, UISettingsManager.SettingsScreen.Difficulty);
		}
		CreateMenuEntity(settingsUI.SectionNameControls, UISettingsManager.SettingsScreen.Controls);
		CreateMenuEntity(settingsUI.SectionNameGraphics, UISettingsManager.SettingsScreen.Graphics);
		CreateMenuEntity(settingsUI.SectionNameDisplay, UISettingsManager.SettingsScreen.Display);
		CreateMenuEntity(settingsUI.SectionNameSound, UISettingsManager.SettingsScreen.Sound);
		CreateMenuEntity(settingsUI.SectionNameAccessiability, UISettingsManager.SettingsScreen.Accessiability);
		if (BuildModeUtility.IsDevelopment)
		{
			CreateMenuEntity(settingsUI.SectionNameDevelopment, UISettingsManager.SettingsScreen.Development);
		}
		SelectionGroup = new SelectionGroupRadioVM<SettingsMenuEntityVM>(MenuEntitiesList, m_SelectedMenuEntity, cyclical: true).AddTo(this);
		Selector = new LensSelectorVM().AddTo(this);
		InfoVM = new InfoSectionVM().AddTo(this);
		if (SettingsController.Instance.HasUnconfirmedSettings())
		{
			ApplySettings();
		}
		m_SelectedMenuEntity.Value = MenuEntitiesList.FirstOrDefault();
		UISettingsManager.SettingsScreen settingsScreen = m_SelectedMenuEntity.Value?.SettingsScreenType ?? UISettingsManager.SettingsScreen.Game;
		SetSettingsList(settingsScreen);
		if (settingsScreen == UISettingsManager.SettingsScreen.Game)
		{
			UISettingsRoot.Instance.UIGameMainSettings.UpdateInteractable();
		}
		ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(LanguageChanged, delegate
		{
			InfoVM.SetTemplate(ReactiveTooltipTemplate.CurrentValue);
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		Disposable.Create(DisposeImplementation).AddTo(this);
	}

	private void DisposeImplementation()
	{
		SettingsController.Instance.RevertAllTempValues();
		DisposeEntities();
		HandleHideSettingsDescription();
	}

	private void CreateMenuEntity(LocalizedString localizedString, UISettingsManager.SettingsScreen screenType)
	{
		SettingsMenuEntityVM item = new SettingsMenuEntityVM(localizedString, screenType, SetSettingsList).AddTo(this);
		MenuEntitiesList.Add(item);
	}

	private void SetSettingsList(UISettingsManager.SettingsScreen settingsScreen)
	{
		if (m_PreviousSelectedMenuEntity == m_SelectedMenuEntity.Value)
		{
			return;
		}
		m_IsDefaultButtonInteractable.Value = settingsScreen != UISettingsManager.SettingsScreen.Difficulty && settingsScreen != UISettingsManager.SettingsScreen.Graphics && RootUIContext.CanChangeLanguage() && RootUIContext.CanChangeInput();
		if (SettingsController.Instance.HasUnconfirmedSettings())
		{
			OnChangeSettingsList(delegate(DialogMessageBoxButton button)
			{
				OnApplyDialogAnswer(button);
				SwitchSettingsScreen(settingsScreen);
			});
		}
		else
		{
			SwitchSettingsScreen(settingsScreen);
		}
		m_PreviousSelectedMenuEntity = m_SelectedMenuEntity.Value;
	}

	private void SwitchSettingsScreen(UISettingsManager.SettingsScreen settingsScreen)
	{
		DisposeEntities();
		m_IsConsoleControls.Value = Game.Instance.IsControllerGamepad && settingsScreen == UISettingsManager.SettingsScreen.Controls;
		IEnumerable<UISettingsGroup> enumerable = from uiSettingsGroup in Game.Instance.UISettingsManager.GetSettingsList(settingsScreen)
			where uiSettingsGroup.IsVisible
			select uiSettingsGroup;
		foreach (UISettingsGroup item in enumerable)
		{
			m_SettingEntities.Add(new SettingsEntityHeaderVM(item.Title));
			foreach (UISettingsEntityBase visibleSettings in item.VisibleSettingsList)
			{
				m_SettingEntities.Add(GetVMForSettingsItem(visibleSettings));
			}
		}
		using (IEnumerator<UISettingsGroup> enumerator = enumerable.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				using IEnumerator<UISettingsEntityBase> enumerator3 = enumerator.Current.VisibleSettingsList.Where((UISettingsEntityBase uiSettingsEntityBase) => !(uiSettingsEntityBase is UISettingsEntityDisplayImages) && !(uiSettingsEntityBase is UISettingsEntityAccessiabilityImage)).GetEnumerator();
				if (enumerator3.MoveNext())
				{
					UISettingsEntityBase current3 = enumerator3.Current;
					HandleShowSettingsDescription(current3);
				}
			}
		}
		m_OnSwitchSettings.Execute();
		if (settingsScreen == UISettingsManager.SettingsScreen.Game)
		{
			UISettingsRoot.Instance.UIGameMainSettings.UpdateInteractable();
		}
	}

	private void DisposeEntities()
	{
		m_SettingEntities.ForEach(delegate(VirtualListElementVMBase s)
		{
			s.Dispose();
		});
		m_SettingEntities.Clear();
	}

	public static VirtualListElementVMBase GetVMForSettingsItem(UISettingsEntityBase uiSettingsEntity, bool isNewGame = false)
	{
		if (!(uiSettingsEntity is UISettingsEntityGameDifficulty uiSettingsEntity2))
		{
			if (!(uiSettingsEntity is UISettingsEntityGammaCorrection uiSettingsEntity3))
			{
				if (!(uiSettingsEntity is UISettingsEntitySliderFontSize uiSettingsEntity4))
				{
					if (!(uiSettingsEntity is UISettingsEntityBool uiSettingsEntity5))
					{
						if (!(uiSettingsEntity is IUISettingsEntityDropdown uiSettingsEntity6))
						{
							if (!(uiSettingsEntity is IUISettingsEntitySlider uiSettingsEntity7))
							{
								if (!(uiSettingsEntity is UISettingsEntityKeyBinding uiSettingsEntity8))
								{
									if (!(uiSettingsEntity is UISettingsEntityDisplayImages uiSettingsEntity9))
									{
										if (!(uiSettingsEntity is UISettingsEntityAccessiabilityImage uiSettingsEntity10))
										{
											if (uiSettingsEntity is UISettingsEntityBoolOnlyOneSave uiSettingsEntity11)
											{
												return new SettingsEntityBoolOnlyOneSaveVM(uiSettingsEntity11, isNewGame);
											}
											UberDebug.LogError($"Error: SettingsVM: GetVMForSettingsItem: uiSettingsEntity {uiSettingsEntity} is undefined");
											return null;
										}
										return new SettingsEntityAccessibilityImageVM(uiSettingsEntity10);
									}
									return new SettingsEntityDisplayImagesVM(uiSettingsEntity9);
								}
								return new SettingEntityKeyBindingVM(uiSettingsEntity8, isNewGame);
							}
							return new SettingsEntitySliderVM(uiSettingsEntity7, SettingsEntitySliderVM.EntitySliderType.UsualSliderIndex, isNewGame);
						}
						return new SettingsEntityDropdownVM(uiSettingsEntity6, SettingsEntityDropdownVM.DropdownType.Default, isNewGame);
					}
					return new SettingsEntityBoolVM(uiSettingsEntity5, isNewGame);
				}
				return new SettingsEntitySliderVM(uiSettingsEntity4, SettingsEntitySliderVM.EntitySliderType.FontSizeIndex, isNewGame);
			}
			return new SettingsEntitySliderVM(uiSettingsEntity3, SettingsEntitySliderVM.EntitySliderType.GammaCorrectionSliderIndex, isNewGame);
		}
		return new SettingsEntityDropdownGameDifficultyVM(uiSettingsEntity2, forceSetValue: false, isNewGame);
	}

	public void OpenKeyBindingSetupDialog(UISettingsEntityKeyBinding uiSettingsEntity, int bindingIndex)
	{
		m_CurrentKeyBindDialog.Value = new KeyBindingSetupDialogVM(uiSettingsEntity, bindingIndex, CloseKeyBindSetupDialog).AddTo(this);
	}

	private void CloseKeyBindSetupDialog()
	{
		m_CurrentKeyBindDialog.Value.Dispose();
		m_CurrentKeyBindDialog.Value = null;
	}

	public void Close()
	{
		if (SettingsController.Instance.HasUnconfirmedSettings())
		{
			OnChangeSettingsList(OnCloseDialogAnswer);
		}
		else
		{
			m_CloseAction?.Invoke();
		}
	}

	private void OnChangeSettingsList(Action<DialogMessageBoxButton> onApplyDialogAction)
	{
		string questionText = string.Format(ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.SettingsUI.SaveChangesMessage, (m_PreviousSelectedMenuEntity != null) ? ((object)m_PreviousSelectedMenuEntity.Title) : ((object)string.Empty));
		LocalizedString yesText = UIStrings.Instance.SettingsUI.DialogSave;
		LocalizedString noText = UIStrings.Instance.SettingsUI.DialogRevert;
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(questionText, DialogMessageBoxType.Dialog, onApplyDialogAction, null, yesText, noText);
		});
	}

	public void OpenApplySettingsDialog()
	{
		if (SettingsController.Instance.HasUnconfirmedSettings())
		{
			OnChangeSettingsList(OnApplyDialogAnswer);
		}
	}

	private void OnApplyDialogAnswer(DialogMessageBoxButton button)
	{
		if (button == DialogMessageBoxButton.Yes)
		{
			ApplySettings();
		}
		else
		{
			RevertSettings();
		}
	}

	private void OnCloseDialogAnswer(DialogMessageBoxButton button)
	{
		if (button == DialogMessageBoxButton.Yes)
		{
			ApplySettings();
		}
		else
		{
			RevertSettings();
		}
		m_CloseAction?.Invoke();
	}

	private void ApplySettings()
	{
		Locale value = SettingsRoot.Game.Main.Localization.GetValue();
		float fontSizeMultiplier = SettingsRoot.Accessiability.FontSizeMultiplier;
		int value2 = SettingsRoot.Display.SafeZoneOffset.GetValue();
		SettingsController.Instance.Sync();
		SettingsController.Instance.ConfirmAllTempValues();
		SettingsController.Instance.SaveAll();
		if (value != SettingsRoot.Game.Main.Localization.GetValue())
		{
			RootUIContext.ResetUI(delegate
			{
				ObservableSubscribeExtensions.Subscribe(Observable.TimerFrame(5), delegate
				{
					EventBus.RaiseEvent(delegate(ISettingsUIHandler h)
					{
						h.HandleOpenSettings(GameUIState.Instance.IsInMainMenu);
					});
				}).AddTo(this);
			});
		}
		if (Math.Abs(fontSizeMultiplier - SettingsRoot.Accessiability.FontSizeMultiplier) > 0.01f)
		{
			EventBus.RaiseEvent(delegate(ISettingsFontSizeUIHandler h)
			{
				h.HandleChangeFontSizeSettings(SettingsRoot.Accessiability.FontSizeMultiplier);
			});
		}
		if (value2 != SettingsRoot.Display.SafeZoneOffset.GetValue())
		{
			EventBus.RaiseEvent(delegate(ISafeZoneUIHandler h)
			{
				h.OnSafeZoneChanged();
			});
		}
		HandleItemChanged(string.Empty);
		m_OnApplyWindowClose.Execute();
	}

	private void RevertSettings()
	{
		SettingsController.Instance.RevertAllTempValues();
		HandleItemChanged(string.Empty);
		m_OnApplyWindowClose.Execute();
	}

	public void OpenDefaultSettingsDialog()
	{
		string text = string.Format(ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.SettingsUI.RestoreAllDefaultsMessage, m_SelectedMenuEntity.Value.Title);
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(text, DialogMessageBoxType.Dialog, OnDefaultDialogAnswer);
		});
	}

	private void OnDefaultDialogAnswer(DialogMessageBoxButton button)
	{
		if (button == DialogMessageBoxButton.Yes)
		{
			SettingsController.Instance.ResetToDefault(m_SelectedMenuEntity.Value.SettingsScreenType);
			HandleItemChanged(string.Empty);
		}
	}

	public void OpenCancelSettingsDialog()
	{
		string text = string.Format(ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.SettingsUI.CancelChangesMessage, m_SelectedMenuEntity.Value.Title);
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(text, DialogMessageBoxType.Dialog, OnCancelDialogAnswer);
		});
	}

	private void OnCancelDialogAnswer(DialogMessageBoxButton buttonType)
	{
		if (buttonType == DialogMessageBoxButton.Yes)
		{
			RevertSettings();
		}
	}

	public void HandleItemChanged(string key)
	{
		m_IsApplyButtonInteractable.Value = SettingsController.Instance.HasUnconfirmedSettings();
		m_IsCancelButtonInteractable.Value = SettingsController.Instance.HasUnconfirmedSettings();
		UISettingsRoot.Instance.UIDifficultySettings.UpdateInteractable();
		UISettingsRoot.Instance.UIGraphicsSettings.UpdateInteractable(initialize: false);
		UISettingsRoot.Instance.UIGameTutorialSettings.UpdateInteractable(key);
	}

	public void HandleShowSettingsDescription(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		SetupTooltipTemplate(entity, ownTitle, ownDescription);
	}

	public void HandleHideSettingsDescription()
	{
	}

	void ISaveSettingsHandler.HandleSaveSettings()
	{
		HandleItemChanged(string.Empty);
	}

	public void HandleChangeFontSizeSettings(float size)
	{
		SelectionGroup.SelectPrevValidEntity();
		SelectionGroup.SelectNextValidEntity();
	}

	private void SetupTooltipTemplate(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		m_ReactiveTooltipTemplate.Value = (IsConsoleControls.CurrentValue ? null : ((entity != null || ownTitle != null || ownDescription != null) ? TooltipTemplate(entity, ownTitle, ownDescription) : null));
	}

	private TooltipBaseTemplate TooltipTemplate(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		return new TooltipTemplateSettingsEntityDescription(entity, ownTitle, ownDescription);
	}

	public void HandleLanguageChanged()
	{
		m_LanguageChanged.Execute();
		IEnumerable<VirtualListElementVMBase> source = m_SettingEntities.Where((VirtualListElementVMBase e) => e is SettingsEntityHeaderVM);
		if (!source.Any())
		{
			return;
		}
		source.ForEach(delegate(VirtualListElementVMBase h)
		{
			if (h is SettingsEntityHeaderVM settingsEntityHeaderVM)
			{
				settingsEntityHeaderVM.UpdateLocalization();
			}
		});
	}
}
