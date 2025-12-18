using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[ComponentName("Combat/CombatEventsTrigger")]
[TypeId("312600058795414c823f29d5bcec8849")]
public sealed class CombatEventsTrigger : MechanicEntityFactComponentDelegate, ITurnBasedModeHandler, ISubscriber, IPreparationTurnBeginHandler, IPreparationTurnEndHandler, IRoundStartHandler, IRoundEndHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, ITurnEndHandler, IInterruptTurnStartHandler, IInterruptTurnEndHandler
{
	[Tooltip("CurrentEntity - fact Owner; CurrentTarget - unit who start/end turn/interrupt or fact Owner")]
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[Tooltip("If Owner is unit then by default trigger will work only if Owner in combat")]
	[ShowIf("OwnerIsUnit")]
	public bool InvokeActionsIfOwnerNotInCombat;

	[Header("Deployment")]
	public bool InvokeDeploymentPhaseStartWhenAmbushed;

	public ActionList OnDeploymentPhaseStart;

	public ActionList OnDeploymentPhaseEnd;

	[Header("Combat")]
	public bool InvokeCombatStartWhenFactAttachedInCombat;

	public ActionList OnCombatStart;

	public ActionList OnCombatEnd;

	[Header("Round")]
	public ActionList OnRoundStart;

	public ActionList OnRoundEnd;

	[Header("Turn")]
	public bool InvokeTurnActionsForAnyUnit;

	public ActionList OnTurnStart;

	public ActionList OnTurnEnd;

	[Header("Interrupt")]
	public bool InvokeInterruptActionsForAnyUnit;

	public ActionList OnInterruptStart;

	public ActionList OnInterruptEnd;

	private static TurnController TurnController => Game.Instance.Controllers.TurnController;

	private static bool IsFirstRound
	{
		get
		{
			if (TurnController.InCombat)
			{
				return TurnController.CombatRound == 1;
			}
			return false;
		}
	}

	private static bool IsZeroRound
	{
		get
		{
			if (TurnController.InCombat)
			{
				return TurnController.CombatRound < 1;
			}
			return false;
		}
	}

	private bool OwnerIsUnit => base.OwnerBlueprint is BlueprintUnitFact;

	protected override void OnFactAttached()
	{
		if (InvokeCombatStartWhenFactAttachedInCombat && TurnController.InCombat && TurnController.TurnOrder.CurrentTurnType == CombatTurnType.Default && TurnController.CurrentUnit != null)
		{
			InvokeActionsIfRestrictionsPassed(OnCombatStart);
		}
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			InvokeActionsIfRestrictionsPassed(OnCombatEnd);
		}
	}

	void IPreparationTurnBeginHandler.HandleBeginPreparationTurn(bool canDeploy)
	{
		if (canDeploy || InvokeDeploymentPhaseStartWhenAmbushed)
		{
			InvokeActionsIfRestrictionsPassed(OnDeploymentPhaseStart);
		}
	}

	void IPreparationTurnEndHandler.HandleEndPreparationTurn()
	{
		if ((!OnDeploymentPhaseEnd.HasActions && !OnCombatStart.HasActions && !OnRoundStart.HasActions) || !IsRestrictionPassed())
		{
			return;
		}
		using (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Set(base.Owner))
		{
			OnDeploymentPhaseEnd.Run();
			OnCombatStart.Run();
			OnRoundStart.Run();
		}
	}

	void IRoundStartHandler.HandleRoundStart(bool isTurnBased)
	{
		if (TurnController.InCombat && !IsFirstRound)
		{
			InvokeActionsIfRestrictionsPassed(OnRoundStart);
		}
	}

	void IRoundEndHandler.HandleRoundEnd(bool isTurnBased)
	{
		if (TurnController.InCombat && !IsZeroRound)
		{
			InvokeActionsIfRestrictionsPassed(OnRoundEnd);
		}
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		if (TurnController.InCombat && !ContextData<TurnController.InterruptTurnEndMark>.Current)
		{
			MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
			if (mechanicEntity == base.Owner || InvokeTurnActionsForAnyUnit)
			{
				InvokeActionsIfRestrictionsPassed(OnTurnStart, mechanicEntity);
			}
		}
	}

	void ITurnEndHandler.HandleUnitEndTurn(bool isTurnBased)
	{
		if (TurnController.InCombat)
		{
			MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
			if (mechanicEntity == base.Owner || InvokeTurnActionsForAnyUnit)
			{
				InvokeActionsIfRestrictionsPassed(OnTurnEnd, mechanicEntity);
			}
		}
	}

	void IInterruptTurnStartHandler.HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (mechanicEntity == base.Owner || InvokeInterruptActionsForAnyUnit)
		{
			InvokeActionsIfRestrictionsPassed(OnInterruptStart, mechanicEntity);
		}
	}

	void IInterruptTurnEndHandler.HandleUnitEndInterruptTurn()
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (mechanicEntity == base.Owner || InvokeInterruptActionsForAnyUnit)
		{
			InvokeActionsIfRestrictionsPassed(OnInterruptEnd, mechanicEntity);
		}
	}

	private bool IsRestrictionPassed(MechanicEntity currentTarget = null)
	{
		if (!InvokeActionsIfOwnerNotInCombat)
		{
			MechanicEntity owner = base.Owner;
			if (owner is BaseUnitEntity && !owner.IsInCombat)
			{
				return false;
			}
		}
		return Restrictions.IsPassed(base.Context, null, currentTarget ?? base.Owner);
	}

	private void InvokeActionsIfRestrictionsPassed(ActionList actions, MechanicEntity currentTarget = null)
	{
		if (!actions.HasActions || !IsRestrictionPassed(currentTarget))
		{
			return;
		}
		using (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Set(currentTarget ?? base.Owner))
		{
			actions.Run();
		}
	}
}
