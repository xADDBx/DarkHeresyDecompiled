using System;
using System.Collections.Generic;
using Core.Cheats;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FirstLaunchSettingsVM : ViewModel, ILocalizationHandler, ISubscriber
{
	private readonly Action m_CloseAction;

	public readonly SelectionGroupRadioVM<SettingsMenuEntityVM> SelectionGroup;

	private readonly ReactiveProperty<SettingsMenuEntityVM> m_SelectedMenuEntity = new ReactiveProperty<SettingsMenuEntityVM>();

	private readonly List<SettingsMenuEntityVM> m_MenuEntitiesList = new List<SettingsMenuEntityVM>();

	private readonly ReactiveProperty<FirstLaunchLanguagePageVM> m_LanguagePageVM = new ReactiveProperty<FirstLaunchLanguagePageVM>();

	private readonly ReactiveProperty<FirstLaunchSafeZonePageVM> m_SafeZonePageVM = new ReactiveProperty<FirstLaunchSafeZonePageVM>();

	private readonly ReactiveProperty<FirstLaunchDisplayPageVM> m_DisplayPageVM = new ReactiveProperty<FirstLaunchDisplayPageVM>();

	private readonly ReactiveProperty<FirstLaunchAccessiabilityPageVM> m_AccessiabilityPageVM = new ReactiveProperty<FirstLaunchAccessiabilityPageVM>();

	private readonly ReactiveCommand<Unit> m_LanguageChanged = new ReactiveCommand<Unit>();

	private readonly ReactiveCommand<Unit> m_ShowPhotosensitivityScreen = new ReactiveCommand<Unit>();

	private readonly ReactiveProperty<bool> m_BlockHints = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsVisibleHorizontalDPad = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsVisibleVerticalDPad = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<FirstLaunchLanguagePageVM> LanguagePageVM => m_LanguagePageVM;

	public ReadOnlyReactiveProperty<FirstLaunchSafeZonePageVM> SafeZonePageVM => m_SafeZonePageVM;

	public ReadOnlyReactiveProperty<FirstLaunchDisplayPageVM> DisplayPageVM => m_DisplayPageVM;

	public ReadOnlyReactiveProperty<FirstLaunchAccessiabilityPageVM> AccessiabilityPageVM => m_AccessiabilityPageVM;

	public Observable<Unit> LanguageChanged => m_LanguageChanged;

	public Observable<Unit> ShowPhotosensitivityScreen => m_ShowPhotosensitivityScreen;

	public ReadOnlyReactiveProperty<bool> BlockHints => m_BlockHints;

	public ReadOnlyReactiveProperty<bool> IsVisibleHorizontalDPad => m_IsVisibleHorizontalDPad;

	public ReadOnlyReactiveProperty<bool> IsVisibleVerticalDPad => m_IsVisibleVerticalDPad;

	public FirstLaunchSettingsVM(Action closeAction)
	{
		m_CloseAction = closeAction;
		CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameLanguage, UISettingsManager.SettingsScreen.Language);
		if (Game.Instance.IsControllerGamepad)
		{
			CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameSafeZone, UISettingsManager.SettingsScreen.SafeZone);
		}
		CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameDisplay, UISettingsManager.SettingsScreen.Display);
		CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameAccessiability, UISettingsManager.SettingsScreen.Accessiability);
		SelectionGroup = new SelectionGroupRadioVM<SettingsMenuEntityVM>(m_MenuEntitiesList, m_SelectedMenuEntity).AddTo(this);
		SelectionGroup.TrySelectFirstValidEntity();
		m_SelectedMenuEntity.Subscribe(delegate(SettingsMenuEntityVM e)
		{
			if (e != null)
			{
				SwitchSettingsScreen(e.SettingsScreenType);
			}
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		DisposeAll();
		PlayerPrefs.Save();
	}

	private void CreateMenuEntity(LocalizedString localizedString, UISettingsManager.SettingsScreen screenType)
	{
		SettingsMenuEntityVM item = new SettingsMenuEntityVM(localizedString, screenType, null).AddTo(this);
		m_MenuEntitiesList.Add(item);
	}

	private void SwitchSettingsScreen(UISettingsManager.SettingsScreen settingsScreen)
	{
		ApplySettings();
		DisposeAll();
		switch (settingsScreen)
		{
		case UISettingsManager.SettingsScreen.Language:
			m_LanguagePageVM.Value = new FirstLaunchLanguagePageVM();
			break;
		case UISettingsManager.SettingsScreen.SafeZone:
			m_SafeZonePageVM.Value = new FirstLaunchSafeZonePageVM();
			break;
		case UISettingsManager.SettingsScreen.Display:
			m_DisplayPageVM.Value = new FirstLaunchDisplayPageVM();
			break;
		case UISettingsManager.SettingsScreen.Accessiability:
			m_AccessiabilityPageVM.Value = new FirstLaunchAccessiabilityPageVM();
			break;
		}
		SetHorizontalAndVerticalDPadVisible(settingsScreen);
	}

	private void SetHorizontalAndVerticalDPadVisible(UISettingsManager.SettingsScreen settingsScreen)
	{
		m_IsVisibleHorizontalDPad.Value = settingsScreen == UISettingsManager.SettingsScreen.SafeZone || settingsScreen == UISettingsManager.SettingsScreen.Display || settingsScreen == UISettingsManager.SettingsScreen.Accessiability;
		m_IsVisibleVerticalDPad.Value = settingsScreen == UISettingsManager.SettingsScreen.Language || settingsScreen == UISettingsManager.SettingsScreen.Display || settingsScreen == UISettingsManager.SettingsScreen.Accessiability;
	}

	private void DisposeAll()
	{
		LanguagePageVM.CurrentValue?.Dispose();
		m_LanguagePageVM.Value = null;
		SafeZonePageVM.CurrentValue?.Dispose();
		m_SafeZonePageVM.Value = null;
		DisplayPageVM.CurrentValue?.Dispose();
		m_DisplayPageVM.Value = null;
		AccessiabilityPageVM.CurrentValue?.Dispose();
		m_AccessiabilityPageVM.Value = null;
	}

	private void ApplySettings()
	{
		float fontSizeMultiplier = SettingsRoot.Accessiability.FontSizeMultiplier;
		SettingsController.Instance.ConfirmAllTempValues();
		SettingsController.Instance.SaveAll();
		if (Math.Abs(fontSizeMultiplier - SettingsRoot.Accessiability.FontSizeMultiplier) > 0.01f)
		{
			EventBus.RaiseEvent(delegate(ISettingsFontSizeUIHandler h)
			{
				h.HandleChangeFontSizeSettings(SettingsRoot.Accessiability.FontSizeMultiplier);
			});
		}
	}

	public void RevertSettings()
	{
		SettingsController.Instance.ResetToDefault(m_SelectedMenuEntity.Value.SettingsScreenType);
	}

	public void NextPage()
	{
		if (!SelectionGroup.SelectNextValidEntity())
		{
			ApplySettings();
			m_BlockHints.Value = true;
			m_ShowPhotosensitivityScreen.Execute(Unit.Default);
		}
	}

	public void PreviousPage()
	{
		SelectionGroup.SelectPrevValidEntity();
	}

	public void Close()
	{
		m_CloseAction?.Invoke();
		m_BlockHints.Value = false;
	}

	[Cheat(Name = "clear_first_launch")]
	public static void ClearFirstLaunchPrefs()
	{
		FirstLaunchSettings.SetHasShownValue(0);
	}

	[Cheat(Name = "set_first_launch")]
	public static void SetFirstLaunchPrefs()
	{
		FirstLaunchSettings.SetHasShownValue(1);
	}

	public void HandleLanguageChanged()
	{
		SelectionGroup.EntitiesCollection.ForEach(delegate(SettingsMenuEntityVM e)
		{
			e.UpdateTitle();
		});
		m_LanguageChanged.Execute(Unit.Default);
	}
}
