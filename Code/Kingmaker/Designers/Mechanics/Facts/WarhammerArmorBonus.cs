using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[Obsolete("VS")]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("186465aada0f422b966541bbf050c271")]
public class WarhammerArmorBonus : MechanicEntityFactComponentDelegate
{
	public ContextValueModifierWithType BonusDeflectionValue;

	public ContextValueModifierWithType BonusAbsorptionValue;

	public bool ForceDeflectionMinimum;

	[ShowIf("ForceDeflectionMinimum")]
	public int PctDeflectionMinimum;

	[ShowIf("ForceDeflectionMinimum")]
	public int DeflectionMinimumValue;

	public bool ForceAbsorptionMinimum;

	[ShowIf("ForceAbsorptionMinimum")]
	public int PctAbsorptionMinimum;

	[ShowIf("ForceAbsorptionMinimum")]
	public int AbsorptionMinimumValue;

	public ModifierDescriptor ModifierDescriptor;
}
