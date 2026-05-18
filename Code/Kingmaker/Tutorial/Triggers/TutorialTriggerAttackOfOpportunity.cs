using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("8ac221330c6f48dba2d89cced59cd5d9")]
public class TutorialTriggerAttackOfOpportunity : TutorialTrigger, IWarhammerAttackHandler, ISubscriber
{
	public void HandleAttack(RulePerformAttack withWeaponAttackHit)
	{
		if (!withWeaponAttackHit.ResultIsHit || !withWeaponAttackHit.Ability.IsAttackOfOpportunity)
		{
			return;
		}
		MechanicEntity currentUnit = Game.Instance.Controllers.TurnController.CurrentUnit;
		if (currentUnit != null && currentUnit.IsPlayerFaction)
		{
			TryToTrigger(withWeaponAttackHit, delegate(TutorialContext context)
			{
				context.SolutionAbility = withWeaponAttackHit.Ability;
				context.TargetUnit = withWeaponAttackHit.TargetUnit;
			});
		}
	}
}
