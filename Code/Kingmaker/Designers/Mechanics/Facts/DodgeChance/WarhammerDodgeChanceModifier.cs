using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.DodgeChance;

[Serializable]
[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("aa5a2b1c16294395999aa0ac5f146f31")]
public abstract class WarhammerDodgeChanceModifier : MechanicEntityFactComponentDelegate
{
	[Flags]
	public enum PropertyType
	{
		DodgeChance = 1
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[EnumFlagsAsDropdown]
	public PropertyType Properties = PropertyType.DodgeChance;

	[ShowIf("ModifyDodgeChance")]
	public ContextValue DodgeChance;

	public bool PercentDodgeModifier;

	public bool PercentMultiplierModifier;

	public bool SetMinimumDodgeChance;

	[ShowIf("SetMinimumDodgeChance")]
	public ContextValue MinimumDodgeChance;

	private bool ModifyDodgeChance => (Properties & PropertyType.DodgeChance) != 0;
}
