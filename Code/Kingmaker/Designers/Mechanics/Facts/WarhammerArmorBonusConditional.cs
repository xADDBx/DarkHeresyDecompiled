using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete("VS")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("4eaba0ad9abe479eae3618b5e97d5c71")]
public class WarhammerArmorBonusConditional : BlueprintComponent
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ContextValue BonusDeflectionValue;

	public ContextValue BonusAbsorptionValue;

	public bool SpecificRangeType;

	[ShowIf("SpecificRangeType")]
	public WeaponRangeType WeaponRangeType;

	public bool SpecificDamageType;

	[ShowIf("SpecificDamageType")]
	public DamageType Type;

	[ShowIf("SpecificDamageType")]
	public bool AllDamageExceptThisType;

	public bool OnlyFromAlliedAttacks;

	public bool IgnoreArmour;
}
