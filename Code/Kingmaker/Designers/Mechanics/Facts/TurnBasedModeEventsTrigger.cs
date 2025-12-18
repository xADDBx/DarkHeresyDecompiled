using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("0cdbc172cfe945e3818c0d49fbd7d65f")]
public class TurnBasedModeEventsTrigger : UnitFactComponentDelegate, ITurnBasedModeHandler, ISubscriber, IRoundStartHandler, IRoundEndHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, ITurnEndHandler, IInterruptTurnStartHandler, IInterruptTurnEndHandler
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ActionList CombatStartActions;

	public ActionList CombatEndActions;

	public ActionList RoundStartActions;

	public ActionList RoundEndActions;

	public bool OnlyFirstRound;

	public bool AnyUnitTurns;

	[ShowIf("AnyUnitTurns")]
	public bool OnlyEnemyTurns;

	public bool ActionsOnTheTurnOwner;

	[Space(4f)]
	public ActionList UnitTurnStartActions;

	public ActionList UnitTurnEndActions;

	[Space(4f)]
	[InspectorName("AdditionalTurnStart actions")]
	public ActionList UnitInterruptTurnStartActions;

	[InspectorName("AdditionalTurnEnd actions")]
	public ActionList UnitInterruptTurnEndActions;

	[Space(4f)]
	public bool DoNotApplyOnInterrupts;

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Context, base.Owner))
			{
				return;
			}
		}
		if (isTurnBased)
		{
			using (base.Fact.MaybeContext?.SetScope(base.OwnerTargetWrapper))
			{
				base.Fact.RunActionInContext(CombatStartActions, base.OwnerTargetWrapper);
				return;
			}
		}
		using (base.Fact.MaybeContext?.SetScope(base.OwnerTargetWrapper))
		{
			base.Fact.RunActionInContext(CombatEndActions, base.OwnerTargetWrapper);
		}
	}

	protected override void OnFactAttached()
	{
		if (!Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			return;
		}
		using (base.Fact.MaybeContext?.SetScope(base.Owner.ToITargetWrapper()))
		{
			base.Fact.RunActionInContext(CombatStartActions, base.Owner.ToITargetWrapper());
		}
	}

	void IRoundStartHandler.HandleRoundStart(bool isTurnBased)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Context, base.Owner))
			{
				return;
			}
		}
		if ((!OnlyFirstRound || Game.Instance.Controllers.TurnController.CombatRound == 1) && isTurnBased)
		{
			using (base.Fact.MaybeContext?.SetScope(base.OwnerTargetWrapper))
			{
				base.Fact.RunActionInContext(RoundStartActions, base.OwnerTargetWrapper);
			}
		}
	}

	void IRoundEndHandler.HandleRoundEnd(bool isTurnBased)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Context, base.Owner))
			{
				return;
			}
		}
		if ((!OnlyFirstRound || Game.Instance.Controllers.TurnController.CombatRound == 1) && isTurnBased)
		{
			using (base.Fact.MaybeContext?.SetScope(base.OwnerTargetWrapper))
			{
				base.Fact.RunActionInContext(RoundEndActions, base.OwnerTargetWrapper);
			}
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (!isTurnBased || (mechanicEntity != base.Owner && !AnyUnitTurns) || (base.Owner.IsAlly(mechanicEntity) && OnlyEnemyTurns))
		{
			return;
		}
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Context, base.Owner))
			{
				return;
			}
		}
		ITargetWrapper targetWrapper;
		if (!ActionsOnTheTurnOwner || !(mechanicEntity is UnitEntity entity))
		{
			targetWrapper = base.OwnerTargetWrapper;
		}
		else
		{
			ITargetWrapper targetWrapper2 = entity.ToTargetWrapper();
			targetWrapper = targetWrapper2;
		}
		ITargetWrapper targetWrapper3 = targetWrapper;
		using (base.Fact.MaybeContext?.SetScope(targetWrapper3))
		{
			base.Fact.RunActionInContext(UnitTurnStartActions, targetWrapper3);
		}
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (!isTurnBased || (EventInvokerExtensions.MechanicEntity != base.Owner && !AnyUnitTurns) || (base.Owner.IsAlly(EventInvokerExtensions.MechanicEntity) && OnlyEnemyTurns))
		{
			return;
		}
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Context, base.Owner))
			{
				return;
			}
		}
		ITargetWrapper targetWrapper;
		if (!ActionsOnTheTurnOwner || !(mechanicEntity is UnitEntity entity))
		{
			targetWrapper = base.OwnerTargetWrapper;
		}
		else
		{
			ITargetWrapper targetWrapper2 = entity.ToTargetWrapper();
			targetWrapper = targetWrapper2;
		}
		ITargetWrapper targetWrapper3 = targetWrapper;
		using (base.Fact.MaybeContext?.SetScope(targetWrapper3))
		{
			base.Fact.RunActionInContext(UnitTurnEndActions, targetWrapper3);
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		if (DoNotApplyOnInterrupts && !interruptionData.AsExtraTurn)
		{
			return;
		}
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if ((mechanicEntity != base.Owner && !AnyUnitTurns) || (base.Owner.IsAlly(mechanicEntity) && OnlyEnemyTurns))
		{
			return;
		}
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Context, base.Owner))
			{
				return;
			}
		}
		ITargetWrapper targetWrapper;
		if (!ActionsOnTheTurnOwner || !(mechanicEntity is UnitEntity entity))
		{
			targetWrapper = base.OwnerTargetWrapper;
		}
		else
		{
			ITargetWrapper targetWrapper2 = entity.ToTargetWrapper();
			targetWrapper = targetWrapper2;
		}
		ITargetWrapper targetWrapper3 = targetWrapper;
		using (base.Fact.MaybeContext?.SetScope(targetWrapper3))
		{
			base.Fact.RunActionInContext(UnitInterruptTurnStartActions, targetWrapper3);
		}
	}

	public void HandleUnitEndInterruptTurn()
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (mechanicEntity != base.Owner && !AnyUnitTurns)
		{
			return;
		}
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Context, base.Owner))
			{
				return;
			}
		}
		ITargetWrapper targetWrapper;
		if (!ActionsOnTheTurnOwner || !(mechanicEntity is UnitEntity entity))
		{
			targetWrapper = base.OwnerTargetWrapper;
		}
		else
		{
			ITargetWrapper targetWrapper2 = entity.ToTargetWrapper();
			targetWrapper = targetWrapper2;
		}
		ITargetWrapper targetWrapper3 = targetWrapper;
		using (base.Fact.MaybeContext?.SetScope(targetWrapper3))
		{
			base.Fact.RunActionInContext(UnitInterruptTurnEndActions, targetWrapper3);
		}
	}
}
