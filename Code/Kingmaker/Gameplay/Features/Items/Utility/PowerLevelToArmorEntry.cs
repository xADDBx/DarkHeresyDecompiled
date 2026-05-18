using System;
using Kingmaker.Blueprints.Items.Armors;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;

namespace Kingmaker.Gameplay.Features.Items.Utility;

[Serializable]
public struct PowerLevelToArmorEntry
{
	public ItemPowerLevel PowerLevel;

	[ValidateNotNull]
	public BpRef<BlueprintItemArmor> Item;
}
