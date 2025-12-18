using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.Armor;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("74f81f7e6fde4fd49cc488b9b07266ce")]
public abstract class ArmorDamageModifier : MechanicEntityFactComponentDelegate
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ContextValueModifierWithType DamageAddValue;

	protected void TryApply(RuleCalculateDamage evt)
	{
		if (Restrictions.IsPassed(base.Context, null, null, evt))
		{
			evt.ArmorDamageModifiers.Add(DamageAddValue.ModifierType, DamageAddValue.Calculate(base.Context), base.Fact);
		}
	}
}
