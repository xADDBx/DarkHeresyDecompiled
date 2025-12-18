using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Combat/ExtraTurnWithReasonTrigger")]
[TypeId("6a1959011ccce504cb1b80db926ed4cc")]
public class ExtraTurnWithReasonTrigger : UnitFactComponentDelegate, IInterruptCurrentTurnHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	[InfoBox("CurrentEntity - fact owner")]
	[SerializeField]
	protected RestrictionCalculator RestrictionsOwner = new RestrictionCalculator();

	[InfoBox("CurrentEntity - turn source\nCurrentTarget - turn receiver")]
	[SerializeField]
	protected RestrictionCalculator RestrictionsSourceAndTarget = new RestrictionCalculator();

	public bool AnyUnitTurns;

	[ShowIf("AnyUnitTurns")]
	public bool OnlyEnemyTurns;

	[SerializeReference]
	public AbstractUnitEvaluator CasterForActions;

	[SerializeReference]
	public AbstractUnitEvaluator TargetForActions;

	public ActionList UnitInterruptTurnStartActions;

	public void HandleOnInterruptCurrentTurn()
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!RestrictionsOwner.IsPassed(base.Context, base.Owner))
			{
				return;
			}
		}
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!RestrictionsSourceAndTarget.IsPassed(base.Context, ContextData<InterruptTurnData>.Current?.Source, ContextData<InterruptTurnData>.Current?.Unit))
			{
				return;
			}
		}
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (!Game.Instance.Player.IsInCombat || (mechanicEntity != base.Owner && !AnyUnitTurns) || (base.Owner.IsAlly(mechanicEntity) && OnlyEnemyTurns))
		{
			return;
		}
		MechanicEntity caster = base.Context.MaybeCaster;
		MechanicEntity scope = mechanicEntity;
		if (CasterForActions != null)
		{
			caster = CasterForActions.GetValue();
		}
		if (TargetForActions != null)
		{
			scope = TargetForActions.GetValue();
		}
		using MechanicsContext mechanicsContext = MechanicsContext.Claim(base.Context.Blueprint, caster, base.Context.MaybeOwner, base.Context, base.Context.ClickedTarget, base.Fact, base.Context.Ability);
		using (mechanicsContext?.SetScope(scope))
		{
			UnitInterruptTurnStartActions.Run();
		}
	}
}
