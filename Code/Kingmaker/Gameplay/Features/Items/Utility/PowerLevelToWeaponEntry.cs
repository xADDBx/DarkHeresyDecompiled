using System;
using Kingmaker.Blueprints.Items.Weapons;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;

namespace Kingmaker.Gameplay.Features.Items.Utility;

[Serializable]
public struct PowerLevelToWeaponEntry
{
	public ItemPowerLevel PowerLevel;

	[ValidateNotNull]
	public BpRef<BlueprintItemWeapon> Item;
}
