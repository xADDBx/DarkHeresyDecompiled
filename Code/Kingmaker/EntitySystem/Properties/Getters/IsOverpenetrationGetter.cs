using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("3a49afcdfc7a4aeea20f98ffcd06bbf2")]
public class IsOverpenetrationGetter : BoolPropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override bool GetBaseValue()
	{
		RulePerformAttackRoll rulePerformAttackRoll = Rulebook.CurrentContext.LastEventOfType<RulePerformAttackRoll>();
		if (rulePerformAttackRoll == null || !rulePerformAttackRoll.IsOverpenetration)
		{
			return Rulebook.CurrentContext.LastEventOfType<RulePerformAttack>()?.FromOverpenetration ?? false;
		}
		return true;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check attack is overpenetration";
	}
}
