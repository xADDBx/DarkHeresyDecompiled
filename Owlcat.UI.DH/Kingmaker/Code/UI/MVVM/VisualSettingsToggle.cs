using Kingmaker.Localization;

namespace Kingmaker.Code.UI.MVVM;

public struct VisualSettingsToggle
{
	public readonly CharacterVisualSettingsEntityVM EntityVM;

	public readonly LocalizedString ToggleName;

	public VisualSettingsToggle(CharacterVisualSettingsEntityVM entity, LocalizedString name)
	{
		EntityVM = entity;
		ToggleName = name;
	}
}
