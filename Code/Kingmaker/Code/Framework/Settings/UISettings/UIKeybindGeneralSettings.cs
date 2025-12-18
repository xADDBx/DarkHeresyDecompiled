using System;
using Kingmaker.Settings;

namespace Kingmaker.Code.Framework.Settings.UISettings;

[Serializable]
public class UIKeybindGeneralSettings : IUISettingsSheet
{
	public UISettingsEntityKeyBinding HighlightObjects;

	public UISettingsEntityKeyBinding Hold;

	public UISettingsEntityKeyBinding OpenCharacterScreen;

	public UISettingsEntityKeyBinding OpenInventory;

	public UISettingsEntityKeyBinding OpenJournal;

	public UISettingsEntityKeyBinding OpenMap;

	public UISettingsEntityKeyBinding OpenEncyclopedia;

	public UISettingsEntityKeyBinding OpenDetectiveJournal;

	public UISettingsEntityKeyBinding OpenColonyManagement;

	public UISettingsEntityKeyBinding OpenShipCustomization;

	public UISettingsEntityKeyBinding OpenCargoManagement;

	public UISettingsEntityKeyBinding OpenFormation;

	public UISettingsEntityKeyBinding Pause;

	public UISettingsEntityKeyBinding QuickLoad;

	public UISettingsEntityKeyBinding QuickSave;

	public UISettingsEntityKeyBinding Screenshot;

	public UISettingsEntityKeyBinding Stop;

	public UISettingsEntityKeyBinding Unpause;

	public UISettingsEntityKeyBinding CameraUp;

	public UISettingsEntityKeyBinding CameraDown;

	public UISettingsEntityKeyBinding CameraLeft;

	public UISettingsEntityKeyBinding CameraRight;

	public UISettingsEntityKeyBinding CameraRotateLeft;

	public UISettingsEntityKeyBinding CameraRotateRight;

	public UISettingsEntityKeyBinding CameraRotateToPointNorth;

	public UISettingsEntityKeyBinding FollowUnit;

	public UISettingsEntityKeyBinding SkipBark;

	public UISettingsEntityKeyBinding SkipCutscene;

	public UISettingsEntityKeyBinding OpenModificationsWindow;

	public UISettingsEntityKeyBinding SpeedUpEnemiesTurn;

	public UISettingsEntityKeyBinding SwitchUIVisibility;

	public UISettingsEntityKeyBinding ShowHideCombatLog;

	public UISettingsEntityKeyBinding EndTurn;

	public UISettingsEntityKeyBinding EndPreparationTurn;

	public UISettingsEntityKeyBinding OpenSearchInventory;

	public UISettingsEntityKeyBinding CollectAllLoot;

	public UISettingsEntityKeyBinding PrevTab;

	public UISettingsEntityKeyBinding NextTab;

	public UISettingsEntityKeyBinding PrevCharacter;

	public UISettingsEntityKeyBinding NextCharacter;

	public UISettingsEntityKeyBinding ToggleFullUnitInfo;

	public UISettingsEntityKeyBinding ScrollUp;

	public UISettingsEntityKeyBinding ScrollDown;

	public UISettingsEntityKeyBinding ScrollLeft;

	public UISettingsEntityKeyBinding ScrollRight;

	public void LinkToSettings()
	{
		HighlightObjects.LinkSetting(SettingsRoot.Controls.Keybindings.General.HighlightObjects);
		Hold.LinkSetting(SettingsRoot.Controls.Keybindings.General.Hold);
		OpenCharacterScreen.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenCharacterScreen);
		OpenInventory.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenInventory);
		OpenJournal.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenJournal);
		OpenMap.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenMap);
		OpenEncyclopedia.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenEncyclopedia);
		OpenDetectiveJournal.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenDetectiveJournal);
		OpenColonyManagement.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenColonyManagement);
		OpenShipCustomization.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenShipCustomization);
		OpenCargoManagement.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenCargoManagement);
		OpenFormation.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenFormation);
		Pause.LinkSetting(SettingsRoot.Controls.Keybindings.General.Pause);
		QuickLoad.LinkSetting(SettingsRoot.Controls.Keybindings.General.QuickLoad);
		QuickSave.LinkSetting(SettingsRoot.Controls.Keybindings.General.QuickSave);
		Screenshot.LinkSetting(SettingsRoot.Controls.Keybindings.General.Screenshot);
		Stop.LinkSetting(SettingsRoot.Controls.Keybindings.General.Stop);
		Unpause.LinkSetting(SettingsRoot.Controls.Keybindings.General.Unpause);
		CameraUp.LinkSetting(SettingsRoot.Controls.Keybindings.General.CameraUp);
		CameraDown.LinkSetting(SettingsRoot.Controls.Keybindings.General.CameraDown);
		CameraLeft.LinkSetting(SettingsRoot.Controls.Keybindings.General.CameraLeft);
		CameraRight.LinkSetting(SettingsRoot.Controls.Keybindings.General.CameraRight);
		CameraRotateLeft.LinkSetting(SettingsRoot.Controls.Keybindings.General.CameraRotateLeft);
		CameraRotateRight.LinkSetting(SettingsRoot.Controls.Keybindings.General.CameraRotateRight);
		CameraRotateToPointNorth.LinkSetting(SettingsRoot.Controls.Keybindings.General.CameraRotateToPointNorth);
		FollowUnit.LinkSetting(SettingsRoot.Controls.Keybindings.General.FollowUnit);
		SkipBark.LinkSetting(SettingsRoot.Controls.Keybindings.General.SkipBark);
		SkipCutscene.LinkSetting(SettingsRoot.Controls.Keybindings.General.SkipCutscene);
		OpenModificationsWindow.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenModificationWindow);
		SpeedUpEnemiesTurn.LinkSetting(SettingsRoot.Controls.Keybindings.General.SpeedUpEnemiesTurn);
		SwitchUIVisibility.LinkSetting(SettingsRoot.Controls.Keybindings.General.SwitchUIVisibility);
		ShowHideCombatLog.LinkSetting(SettingsRoot.Controls.Keybindings.General.ShowHideCombatLog);
		EndTurn.LinkSetting(SettingsRoot.Controls.Keybindings.General.EndTurn);
		EndPreparationTurn.LinkSetting(SettingsRoot.Controls.Keybindings.General.EndPreparationTurn);
		OpenSearchInventory.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenSearchInventory);
		CollectAllLoot.LinkSetting(SettingsRoot.Controls.Keybindings.General.CollectAllLoot);
		PrevTab.LinkSetting(SettingsRoot.Controls.Keybindings.General.PrevTab);
		NextTab.LinkSetting(SettingsRoot.Controls.Keybindings.General.NextTab);
		PrevCharacter.LinkSetting(SettingsRoot.Controls.Keybindings.General.PrevCharacter);
		NextCharacter.LinkSetting(SettingsRoot.Controls.Keybindings.General.NextCharacter);
		ToggleFullUnitInfo.LinkSetting(SettingsRoot.Controls.Keybindings.General.ToggleFullUnitInfo);
		ScrollUp.LinkSetting(SettingsRoot.Controls.Keybindings.General.ScrollUp);
		ScrollDown.LinkSetting(SettingsRoot.Controls.Keybindings.General.ScrollDown);
		ScrollLeft.LinkSetting(SettingsRoot.Controls.Keybindings.General.ScrollLeft);
		ScrollRight.LinkSetting(SettingsRoot.Controls.Keybindings.General.ScrollRight);
	}

	public void InitializeSettings()
	{
	}
}
