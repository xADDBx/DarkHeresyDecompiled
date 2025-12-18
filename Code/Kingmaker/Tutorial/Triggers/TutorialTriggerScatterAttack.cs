using System;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("466a4054b343380479e32f289921d4d1")]
public class TutorialTriggerScatterAttack : TutorialTriggerRulebookEvent<RulePerformAttack>
{
	protected override bool ShouldTrigger(RulePerformAttack rule)
	{
		if (rule.Initiator.IsPlayerFaction)
		{
			return rule.Ability.IsBurst;
		}
		return false;
	}
}
