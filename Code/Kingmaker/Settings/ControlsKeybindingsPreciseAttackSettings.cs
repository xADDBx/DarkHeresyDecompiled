using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class ControlsKeybindingsPreciseAttackSettings
{
	public readonly SettingsEntityKeyBindingPair[] PreciseAttackBodyParts;

	public readonly SettingsEntityKeyBindingPair PreciseAttackConfirm;

	public readonly SettingsEntityKeyBindingPair PreciseAttackPrevTarget;

	public readonly SettingsEntityKeyBindingPair PreciseAttackNextTarget;

	public ControlsKeybindingsPreciseAttackSettings(ISettingsController settingsController, ControlsKeybindingsPreciseAttackSettingsDefaultValues defaultValues)
	{
		PreciseAttackBodyParts = new SettingsEntityKeyBindingPair[7];
		for (int i = 0; i < PreciseAttackBodyParts.Length; i++)
		{
			PreciseAttackBodyParts[i] = new SettingsEntityKeyBindingPair(settingsController, "precise-attack-body-part-" + i, defaultValues.PreciseAttackBodyParts[i]);
		}
		PreciseAttackConfirm = new SettingsEntityKeyBindingPair(settingsController, "precise-attack-confirm", defaultValues.PreciseAttackConfirm);
		PreciseAttackPrevTarget = new SettingsEntityKeyBindingPair(settingsController, "precise-attack-prev-target", defaultValues.PreciseAttackPrevTarget);
		PreciseAttackNextTarget = new SettingsEntityKeyBindingPair(settingsController, "precise-attack-next-target", defaultValues.PreciseAttackNextTarget);
	}
}
