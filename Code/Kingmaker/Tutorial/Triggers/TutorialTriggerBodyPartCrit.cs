using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("a292a48524c443659adc418610bf63f8")]
public class TutorialTriggerBodyPartCrit : TutorialTrigger, IWarhammerAttackHandler, ISubscriber
{
	public void HandleAttack(RulePerformAttack withWeaponAttackHit)
	{
		if (withWeaponAttackHit.Result != AttackResult.Hit)
		{
			return;
		}
		bool? flag = withWeaponAttackHit.ResultDamageRule?.ResultIsCritical;
		if (!flag.HasValue || !flag.GetValueOrDefault() || withWeaponAttackHit.RollPerformBodyPartHitRule == null)
		{
			return;
		}
		BaseUnitEntity targetUnit = withWeaponAttackHit.TargetUnit;
		if (targetUnit != null && targetUnit.IsConscious)
		{
			TryToTrigger(withWeaponAttackHit, delegate(TutorialContext context)
			{
				context.SolutionAbility = withWeaponAttackHit.Ability;
				context.TargetUnit = withWeaponAttackHit.TargetUnit;
			});
		}
	}
}
