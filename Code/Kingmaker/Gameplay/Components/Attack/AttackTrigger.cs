using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.Framework;
using Kingmaker.Framework.ContextContract;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components.Attack;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("d69d33d437cc46cbbc8a98b52b206822")]
[SetsContext(ContextField.Target, Availability.Definitely)]
[ContextRoleForField("ActionOnTarget", ContextField.Target, "attack victim", FallsBackTo = "rule.Target")]
[ContextRoleForField("ActionOnCaster", ContextField.Target, "who performed attack", FallsBackTo = "rule.Initiator")]
public abstract class AttackTrigger : MechanicEntityFactComponentDelegate
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public bool TriggerBefore;

	public ActionList ActionOnTarget;

	public ActionList ActionOnCaster;

	protected void TryTrigger(RulePerformAttack evt, bool before)
	{
		if (!Restrictions.IsPassed(base.Context, null, null, evt) || TriggerBefore != before)
		{
			return;
		}
		using (EvalContext.Current.PushTarget(evt.Target))
		{
			ActionOnTarget.Run();
		}
		using (EvalContext.Current.PushTarget(evt.Initiator))
		{
			ActionOnCaster.Run();
		}
	}

	protected void TryTrigger(RulePerformAttackRoll evt, bool before)
	{
		if (!Restrictions.IsPassed(base.Context, null, null, evt) || TriggerBefore != before)
		{
			return;
		}
		using (EvalContext.Current.PushTarget(evt.Target))
		{
			ActionOnTarget.Run();
		}
		using (EvalContext.Current.PushTarget(evt.Initiator))
		{
			ActionOnCaster.Run();
		}
	}
}
