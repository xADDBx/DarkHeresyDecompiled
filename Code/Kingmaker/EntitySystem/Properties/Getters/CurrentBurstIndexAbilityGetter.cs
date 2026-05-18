using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[ClassInfoBox("Номер выстрела в очереди. Выстрелы нумеруются начиная с 0.")]
[TypeId("92afe7375fcc46cea53424f91e0480b5")]
public class CurrentBurstIndexAbilityGetter : IntPropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Current burst index";
	}

	protected override int GetBaseValue()
	{
		RulebookEvent rule = EvalContext.Current.Rule;
		if (rule is RuleCalculateHitChances ruleCalculateHitChances)
		{
			return ruleCalculateHitChances.BurstIndex;
		}
		if (rule is RulePerformAttackRoll rulePerformAttackRoll)
		{
			return rulePerformAttackRoll.BurstIndex;
		}
		return (Rulebook.CurrentContext.LastEventOfType<RulePerformAttackRoll>() ?? Rulebook.CurrentContext.LastEventOfType<RulePerformAttack>()?.RollPerformAttackRule)?.BurstIndex ?? 0;
	}
}
