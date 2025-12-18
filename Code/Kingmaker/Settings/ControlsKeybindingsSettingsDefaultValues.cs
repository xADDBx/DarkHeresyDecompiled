using System;

namespace Kingmaker.Settings;

[Serializable]
public class ControlsKeybindingsSettingsDefaultValues : IValidatable
{
	public ControlsKeybindingsGeneralSettingsDefaultValues General;

	public ControlsKeybindingsActionBarSettingsDefaultValues ActionBar;

	public ControlsKeybindingsDialogSettingsDefaultValues Dialog;

	public ControlsKeybindingsPreciseAttackSettingsDefaultValues PreciseAttack;

	public ControlsKeybindingsSelectCharacterSettingsDefaultValues SelectCharacter;

	public ControlsKeybindingsTurnBasedSettingsDefaultValues TurnBased;

	public void OnValidate()
	{
		ActionBar.OnValidate();
		Dialog.OnValidate();
		PreciseAttack.OnValidate();
		SelectCharacter.OnValidate();
		TurnBased.OnValidate();
	}
}
