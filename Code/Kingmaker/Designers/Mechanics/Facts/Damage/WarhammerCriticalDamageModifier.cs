using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[Serializable]
[Obsolete]
[AllowMultipleComponents]
[TypeId("9ea0953a7bcd4081863ab5c9f8d89e99")]
public abstract class WarhammerCriticalDamageModifier : UnitFactComponentDelegate
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ContextValueModifier PercentCriticalDamageModifier = new ContextValueModifier();

	public ContextValueModifier BonusCriticalDamageModifier = new ContextValueModifier();

	public ContextValueModifier BonusCriticalDamageMultipliers = new ContextValueModifier();

	public ContextValueModifier purePercentCriticalDamageModifier = new ContextValueModifier();

	public ModifierDescriptor ModifierDescriptor;

	protected void TryApply(RuleCalculateDamage rule)
	{
	}
}
