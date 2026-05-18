using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters.Checkers;

[Serializable]
[TypeId("bf643770cc8346ac89f822a1f8adb6d1")]
public sealed class CheckAttackHitGetter : BoolPropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Attack hit";
	}

	protected override bool GetBaseValue()
	{
		RulebookEvent rule = EvalContext.Current.Rule;
		if (rule is RulePerformAttack rulePerformAttack)
		{
			return rulePerformAttack.ResultIsHit;
		}
		if (rule is RulePerformAttackRoll rulePerformAttackRoll)
		{
			return rulePerformAttackRoll.ResultIsHit;
		}
		throw new InvalidOperationException("Missing rule in context: RulePerformAttack or RulePerformAttackRoll");
	}
}
