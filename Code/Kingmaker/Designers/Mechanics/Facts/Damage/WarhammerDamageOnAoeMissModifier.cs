using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[Obsolete]
[TypeId("6170069e54404f30bd5e0fbb19127e2a")]
public abstract class WarhammerDamageOnAoeMissModifier : UnitFactComponentDelegate
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ContextValueModifierWithType AoeMissDamageModifier;

	public ModifierDescriptor ModifierDescriptor;

	protected void TryApply(RuleCalculateDamage rule)
	{
	}
}
