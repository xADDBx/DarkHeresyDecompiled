using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs.Components;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("11c26869806dd4345b36424af209e8ce")]
public class CheckDamageFromDOTGetter : BoolPropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override bool GetBaseValue()
	{
		RulebookEvent rule = this.GetRule();
		if (rule is RulePerformAttack)
		{
			return false;
		}
		if (rule is RuleDealDamage ruleDealDamage && rule.Reason.Fact != null && ruleDealDamage.Reason.Fact.Blueprint.HasLogic<DOTLogic>())
		{
			return true;
		}
		if (rule is RuleCalculateDamage ruleCalculateDamage && rule.Reason.Fact != null && ruleCalculateDamage.Reason.Fact.Blueprint.HasLogic<DOTLogic>())
		{
			return true;
		}
		return false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if damage comes from a Damage over Time";
	}
}
