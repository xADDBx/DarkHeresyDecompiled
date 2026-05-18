using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.View;

namespace Kingmaker.Visual.Sound;

public class HitAsksController : BaseAsksController, IWarhammerAttackHandler, ISubscriber
{
	void IWarhammerAttackHandler.HandleAttack(RulePerformAttack withWeaponAttackHit)
	{
		if (!withWeaponAttackHit.ResultIsHit || withWeaponAttackHit.ResultDamageRule == null || withWeaponAttackHit.TargetUnit == null)
		{
			return;
		}
		if (withWeaponAttackHit.TargetUnit.IsAlly(withWeaponAttackHit.Initiator))
		{
			using (EvalContext.PushAsksContext(withWeaponAttackHit.Initiator, withWeaponAttackHit.TargetUnit))
			{
				withWeaponAttackHit.TargetUnit?.View.Asks?.FriendlyFire?.Schedule();
				return;
			}
		}
		if (!withWeaponAttackHit.TargetUnit.IsEnemy(withWeaponAttackHit.Initiator) || !withWeaponAttackHit.ResultDamageRule.ResultIsCritical || !(withWeaponAttackHit.ConcreteInitiator.View is UnitEntityView unitEntityView))
		{
			return;
		}
		using (EvalContext.PushAsksContext(withWeaponAttackHit.Initiator, withWeaponAttackHit.TargetUnit))
		{
			unitEntityView.Asks?.CriticalHit?.Schedule(is2D: false, delegate(AsksContext context)
			{
				HandleTraumaApplied(context, withWeaponAttackHit.TargetUnit);
			});
		}
	}

	private void HandleTraumaApplied(AsksContext asksContext, BaseUnitEntity target)
	{
		target.View.Asks?.TraumaApplied.Schedule(is2D: false, null, asksContext);
	}
}
