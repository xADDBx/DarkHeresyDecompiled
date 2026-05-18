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
[TypeId("ddf77d0160594fd47aebecbae32de710")]
[SetsContext(ContextField.Target, Availability.Definitely)]
[ContextRoleForField("ActionOnTarget", ContextField.Target, "attack victim", FallsBackTo = "rule.Target")]
[ContextRoleForField("ActionOnCaster", ContextField.Target, "who performed attack", FallsBackTo = "rule.Initiator")]
public abstract class AttackRollTrigger : MechanicEntityFactComponentDelegate
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public bool TriggerBefore;

	public ActionList ActionOnTarget;

	public ActionList ActionOnCaster;

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
