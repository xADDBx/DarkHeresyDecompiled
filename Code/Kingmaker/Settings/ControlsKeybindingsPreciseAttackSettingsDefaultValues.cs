using System;
using Kingmaker.Settings.Entities;

namespace Kingmaker.Settings;

[Serializable]
public class ControlsKeybindingsPreciseAttackSettingsDefaultValues : IValidatable
{
	public KeyBindingPair[] PreciseAttackBodyParts;

	public KeyBindingPair PreciseAttackConfirm;

	public KeyBindingPair PreciseAttackPrevTarget;

	public KeyBindingPair PreciseAttackNextTarget;

	public void OnValidate()
	{
		if (PreciseAttackBodyParts.Length != 7)
		{
			PreciseAttackBodyParts = new KeyBindingPair[7];
		}
	}
}
