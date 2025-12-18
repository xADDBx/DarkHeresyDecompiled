using System;
using Kingmaker.Settings;

namespace Kingmaker.Code.Framework.Settings.UISettings;

[Serializable]
public class UIGameTurnBasedSettings : IUISettingsSheet
{
	public UISettingsEntityBool AutoEndTurn;

	public UISettingsEntityBool CameraFollowUnit;

	public UISettingsEntityBool CameraScrollToCurrentUnit;

	public UISettingsEntityDropdownSpeedUpMode SpeedUpMode;

	public UISettingsEntityBool FastMovement;

	public UISettingsEntityBool FastPartyCast;

	public UISettingsEntityBool DisableActionCamera;

	public UISettingsEntitySliderFloat TimeScaleInPlayerTurn;

	public UISettingsEntitySliderFloat TimeScaleInNonPlayerTurn;

	public UISettingsEntityBool AutoSelectWeaponAbility;

	public void LinkToSettings()
	{
		AutoEndTurn.LinkSetting(SettingsRoot.Game.TurnBased.AutoEndTurn);
		CameraFollowUnit.LinkSetting(SettingsRoot.Game.TurnBased.CameraFollowUnit);
		CameraScrollToCurrentUnit.LinkSetting(SettingsRoot.Game.TurnBased.CameraScrollToCurrentUnit);
		SpeedUpMode.LinkSetting(SettingsRoot.Game.TurnBased.SpeedUpMode);
		FastMovement.LinkSetting(SettingsRoot.Game.TurnBased.FastMovement);
		FastPartyCast.LinkSetting(SettingsRoot.Game.TurnBased.FastPartyCast);
		DisableActionCamera.LinkSetting(SettingsRoot.Game.TurnBased.DisableActionCamera);
		TimeScaleInPlayerTurn.LinkSetting(SettingsRoot.Game.TurnBased.TimeScaleInPlayerTurn);
		TimeScaleInNonPlayerTurn.LinkSetting(SettingsRoot.Game.TurnBased.TimeScaleInNonPlayerTurn);
		AutoSelectWeaponAbility.LinkSetting(SettingsRoot.Game.TurnBased.AutoSelectWeaponAbility);
	}

	public void InitializeSettings()
	{
	}
}
