using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components.Attack;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("d69d33d437cc46cbbc8a98b52b206822")]
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
		using (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Set(evt.Target))
		{
			ActionOnTarget.Run();
		}
		using (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Set(evt.Initiator))
		{
			ActionOnCaster.Run();
		}
	}
}
