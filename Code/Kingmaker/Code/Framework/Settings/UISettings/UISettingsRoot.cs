using System;
using Kingmaker.Blueprints.Root;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.Framework.Settings.UISettings;

public class UISettingsRoot : ScriptableObject
{
	[Serializable]
	public class UISettingsMechanicLinks
	{
		[Header("Difficulty")]
		public UIDifficultySettings UIDifficultySettings;

		[Header("Graphics")]
		public UIGraphicsSettings UIGraphicsSettings;

		[Header("Display")]
		public UIDisplaySettings UIDisplaySettings;

		[Header("Sound")]
		public UISoundSettings UISoundSettings;

		[Header("Accessiability")]
		public UIAccessiabilitySettings UIAccessiabilitySettings;

		[Header("Game")]
		public UIGameMainSettings UIGameMainSettings;

		public UISwitchJoyConAsMouse UISwitchJoyConAsMouse;

		public UIGameSaveSettings UIGameSaveSettings;

		public UIGameTooltipsSettings UIGameTooltipsSettings;

		public UIGameTutorialSettings UIGameTutorialSettings;

		public UIGameCombatTextsSettings UIGameCombatTextsSettings;

		public UIGameDialogsSettings UIGameDialogsSettings;

		public UIGameAutopauseSettings UIGameAutopauseSettings;

		public UIGameTurnBasedSettings UIGameTurnBasedSettings;

		[Header("Controls")]
		public UIControlsSettings UIControlsSettings;

		[Header("KeyBindings")]
		public UIKeybindGeneralSettings UIKeybindGeneralSettings;

		public UIKeybindActionBarSettings UIKeybindActionBarSettings;

		public UIKeybindDialogSettings UIKeybindDialogSettings;

		public UIKeybindPreciseAttackSettings UIKeybindPreciseAttackSettings;

		public UIKeybindSelectCharacterSettings UIKeybindSelectCharacterSettings;

		[Header("Development")]
		public UIDevelopmentSettings UIDevelopmentSettings;

		public void LinkToSettings()
		{
			UIDifficultySettings.LinkToSettings();
			UIGraphicsSettings.LinkToSettings();
			UIDisplaySettings.LinkToSettings();
			UISoundSettings.LinkToSettings();
			UIAccessiabilitySettings.LinkToSettings();
			UIGameAutopauseSettings.LinkToSettings();
			UIGameCombatTextsSettings.LinkToSettings();
			UIGameDialogsSettings.LinkToSettings();
			UIGameMainSettings.LinkToSettings();
			UIGameSaveSettings.LinkToSettings();
			UIGameTooltipsSettings.LinkToSettings();
			UIGameTutorialSettings.LinkToSettings();
			UIGameTurnBasedSettings.LinkToSettings();
			UIControlsSettings.LinkToSettings();
			UIKeybindGeneralSettings.LinkToSettings();
			UIKeybindActionBarSettings.LinkToSettings();
			UIKeybindDialogSettings.LinkToSettings();
			UIKeybindPreciseAttackSettings.LinkToSettings();
			UIKeybindSelectCharacterSettings.LinkToSettings();
			UIDevelopmentSettings.LinkToSettings();
			UISwitchJoyConAsMouse.LinkToSettings();
		}

		public void InitializeSettings()
		{
			UIDifficultySettings.InitializeSettings();
			UIGraphicsSettings.InitializeSettings();
			UIGameTutorialSettings.InitializeSettings();
			UIDevelopmentSettings.InitializeSettings();
		}
	}

	[Header("UI settings groups")]
	public UISettingsGroup[] GameSettings;

	public UISettingsGroup[] DifficultySettings;

	public UISettingsGroup[] GraphicsSettings;

	public UISettingsGroup[] Controls;

	public UISettingsGroup[] SoundSettings;

	public UISettingsGroup[] StartGame;

	public UISettingsGroup[] DisplaySettings;

	public UISettingsGroup[] AccessiabilitySettings;

	public UISettingsGroup[] SafeZone;

	public UISettingsGroup[] Development;

	[FormerlySerializedAs("Settings")]
	[Header("Mechanic usages")]
	[SerializeField]
	public UISettingsMechanicLinks UISettings;

	public static UISettingsMechanicLinks Instance => ConfigRoot.Instance.UISettingsRoot.UISettings;
}
