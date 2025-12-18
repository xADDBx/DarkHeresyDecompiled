using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class DevelopmentSettings
{
	public readonly SettingsEntityBool UseDevSavesFolder;

	public readonly SettingsEntityBool DrawObstaclesGizmo;

	public DevelopmentSettings(ISettingsController settingsController, DevelopmentSettingsDefaultValues defaultValues)
	{
		UseDevSavesFolder = new SettingsEntityBool(settingsController, "use-dev-saves-folder", defaultValues.UseDevSavesFolder);
		DrawObstaclesGizmo = new SettingsEntityBool(settingsController, "draw-obstacles-gizmo", defaultValues.DrawObstaclesGizmo);
	}
}
