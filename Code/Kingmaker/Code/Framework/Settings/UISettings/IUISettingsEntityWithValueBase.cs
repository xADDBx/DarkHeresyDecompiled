using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Code.Framework.Settings.UISettings;

public interface IUISettingsEntityWithValueBase : IUISettingsEntityBase
{
	ISettingsEntity SettingsEntity { get; }

	bool ModificationAllowed => true;

	string ModificationAllowedReason { get; set; }
}
