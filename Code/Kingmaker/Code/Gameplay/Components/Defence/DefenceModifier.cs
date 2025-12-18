using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Components.Defence;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("8ace7d5a46f843d197a09a644992baed")]
public abstract class DefenceModifier : MechanicEntityFactComponentDelegate
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ContextValue Defence = new ContextValue();

	protected void TryApply(RuleCalculateDefence rule)
	{
		if (Restrictions.IsPassed(base.Context, null, null, rule))
		{
			rule.DefenceValueModifiers.Add(ModifierType.ValAdd, Defence.Calculate(base.Context), base.Fact);
		}
	}
}
