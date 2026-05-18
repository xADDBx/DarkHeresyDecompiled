using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("43d83d5013c44c4ca7ad55885eae16d1")]
public class TutorialTriggerOverpenetration : TutorialTrigger, IWarhammerAttackHandler, ISubscriber
{
	public void HandleAttack(RulePerformAttack rule)
	{
		if (rule.ResultDamageRule != null && rule.Initiator.IsPlayerFaction && rule.FromOverpenetration)
		{
			TryToTrigger(rule, delegate(TutorialContext context)
			{
				context.SolutionAbility = rule.Ability;
				context.TargetUnit = rule.TargetUnit;
			});
		}
	}
}
