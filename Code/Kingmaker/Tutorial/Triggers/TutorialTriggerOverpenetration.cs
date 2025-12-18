using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("43d83d5013c44c4ca7ad55885eae16d1")]
public class TutorialTriggerOverpenetration : TutorialTriggerRulebookEvent<RulePerformAttack>
{
	protected override bool ShouldTrigger(RulePerformAttack rule)
	{
		if (rule.ResultDamageRule != null && rule.Target.IsPlayerFaction)
		{
			return rule.FromOverpenetration;
		}
		return false;
	}
}
