using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters.Damage;

[Serializable]
[TypeId("3b4bc8d66f98411787f6ac106e02604e")]
public class CheckDealtDamageGetter : CheckDamageGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override bool Check(out DamageType type, out IntermediateDamage data, out RulebookEvent rule)
	{
		rule = this.GetRule();
		RuleDealDamage ruleDealDamage = (rule as RuleDealDamage) ?? throw new ElementLogicException(this);
		data = ruleDealDamage.RollDamageRule.Damage;
		type = data?.Type ?? DamageType.None;
		return true;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return base.GetInnerCaption(useLineBreaks: false) + " (Dealt Damage)";
	}
}
