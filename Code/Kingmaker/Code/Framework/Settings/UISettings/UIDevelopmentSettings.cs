using System;
using Kingmaker.Settings;

namespace Kingmaker.Code.Framework.Settings.UISettings;

[Serializable]
public class UIDevelopmentSettings : IUISettingsSheet
{
	public UISettingsEntityBool UseDevSavesFolder;

	public UISettingsEntityBool DrawObstaclesGizmo;

	public void LinkToSettings()
	{
		UseDevSavesFolder.LinkSetting(SettingsRoot.Development.UseDevSavesFolder);
		DrawObstaclesGizmo.LinkSetting(SettingsRoot.Development.DrawObstaclesGizmo);
	}

	public void InitializeSettings()
	{
	}
}
