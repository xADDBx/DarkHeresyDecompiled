using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("fe119a0d81774716ae30a24bcb77fdb5")]
public class TutorialTriggerForceMoveCollision : TutorialTriggerRulebookEvent<RulePerformCollision>
{
	protected override bool ShouldTrigger(RulePerformCollision rule)
	{
		if (!rule.Pushed.IsInPlayerParty)
		{
			return rule.Pusher.IsInPlayerParty;
		}
		return true;
	}
}
