using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("c2cf00c429d34991996903faaac054c5")]
public class TutorialTriggerAttackIsDefended : TutorialTrigger, IWarhammerAttackHandler, ISubscriber
{
	public void HandleAttack(RulePerformAttack withWeaponAttackHit)
	{
		if (withWeaponAttackHit.Result == AttackResult.Defended)
		{
			TryToTrigger(withWeaponAttackHit, delegate(TutorialContext context)
			{
				context.SolutionAbility = withWeaponAttackHit.Ability;
				context.TargetUnit = withWeaponAttackHit.TargetUnit;
			});
		}
	}
}
