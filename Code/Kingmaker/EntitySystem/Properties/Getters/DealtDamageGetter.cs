using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("71215231c9644775bd7120d21371ff31")]
public sealed class DealtDamageGetter : IntPropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public bool Vital;

	protected override int GetBaseValue()
	{
		RulebookEvent rule = this.GetRule();
		if (Vital)
		{
			if (!(rule is RulePerformAttack rulePerformAttack))
			{
				if (!(rule is RuleDealDamage ruleDealDamage))
				{
					if (rule is RulePerformCriticalEffects rulePerformCriticalEffects)
					{
						return rulePerformCriticalEffects.Damage?.ResultVitalDamageValue ?? 0;
					}
					throw new ElementLogicException(this);
				}
				return ruleDealDamage.ResultDamage.ResultVitalDamageValue;
			}
			return rulePerformAttack.ResultDamageRule?.ResultDamage.ResultVitalDamageValue ?? 0;
		}
		if (!(rule is RulePerformAttack { ResultDamageValue: var resultDamageValue }))
		{
			if (!(rule is RuleDealDamage { ResultValue: var resultValue }))
			{
				if (rule is RulePerformCriticalEffects rulePerformCriticalEffects2)
				{
					return rulePerformCriticalEffects2.Damage?.ResultDamageValue ?? 0;
				}
				throw new ElementLogicException(this);
			}
			return resultValue;
		}
		return resultDamageValue;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Damage";
	}
}
