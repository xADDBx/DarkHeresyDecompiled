using System;
using Kingmaker.Code.Framework.GameLog;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Visual.Animation.Kingmaker;

namespace Kingmaker.Controllers.Units;

public class UnitLifeController : BaseUnitController
{
	protected override bool ShouldTickOnUnit(AbstractUnitEntity unit)
	{
		PartLifeState lifeStateOptional = unit.GetLifeStateOptional();
		if (lifeStateOptional != null)
		{
			return !lifeStateOptional.IsFinallyDead;
		}
		return false;
	}

	protected override void TickOnUnit(AbstractUnitEntity unit)
	{
		ForceTickOnUnit(unit);
	}

	public static void ForceTickOnUnit(AbstractUnitEntity unit)
	{
		if (unit.LifeState.ScriptedKill || unit.LifeState.MarkedForDeath)
		{
			if (unit.IsMechanism)
			{
				PartArmor required = unit.GetRequired<PartArmor>();
				if (required != null)
				{
					required.SetDurabilityLeft(0);
					goto IL_0041;
				}
			}
			unit.Health.SetHitPointsLeft(0);
		}
		goto IL_0041;
		IL_0041:
		unit.LifeState.MarkedForDeath = false;
		UnitLifeState newLifeState = CalculateLifeState(unit);
		SetLifeState(unit, newLifeState);
		if ((bool)unit.Features.Immortality && !unit.LifeState.ScriptedKill && !Game.Instance.Controllers.TurnController.TurnBasedModeActive && !LoadingProcess.Instance.IsLoadingInProcess)
		{
			UnitAnimationManager maybeAnimationManager = unit.MaybeAnimationManager;
			if ((object)maybeAnimationManager != null && maybeAnimationManager.IsGoingProne)
			{
				unit.LifeState.Resurrect();
			}
		}
	}

	private static UnitLifeState CalculateLifeState(AbstractUnitEntity unit)
	{
		if (unit.LifeState.ScriptedKill)
		{
			return UnitLifeState.Dead;
		}
		if (unit.Health.HitPointsLeft > 0)
		{
			return UnitLifeState.Conscious;
		}
		UnitPartCompanion companionOptional = unit.GetCompanionOptional();
		if (companionOptional == null || companionOptional.State == CompanionState.ExCompanion)
		{
			return UnitLifeState.Dead;
		}
		return UnitLifeState.Unconscious;
	}

	public static void ForceUnitConscious(AbstractUnitEntity unit)
	{
		unit.LifeState.ScriptedKill = false;
		unit.LifeState.MarkedForDeath = false;
		if (unit.Health.HitPointsLeft < 1)
		{
			unit.Health.SetHitPointsLeft(1);
		}
		ForceTickOnUnit(unit);
	}

	private static void SetLifeStateAfterCheck(AbstractUnitEntity unit, UnitLifeState newLifeState, UnitLifeState prevLifeState)
	{
		using (ContextData<GameLogDisabled>.RequestIf(unit.GetOptional<Kill.SilentDeathUnitPart>() != null))
		{
			unit.Remove<Kill.SilentDeathUnitPart>();
			unit.LifeState.Set(newLifeState);
			switch (newLifeState)
			{
			case UnitLifeState.Dead:
				OnUnitDeath(unit);
				break;
			case UnitLifeState.Conscious:
				unit.GetCombatStateOptional()?.ReturnToStartingPositionIfNeeded();
				break;
			}
			if (!unit.LifeState.IsConscious)
			{
				unit.Commands.InterruptAllInterruptible();
			}
			EventBus.RaiseEvent((IAbstractUnitEntity)unit, (Action<IUnitLifeStateChanged>)delegate(IUnitLifeStateChanged h)
			{
				h.HandleUnitLifeStateChanged(prevLifeState);
			}, isCheckRuntime: true);
			if ((unit.IsPlayerFaction && newLifeState == UnitLifeState.Unconscious) || newLifeState == UnitLifeState.Dead)
			{
				EventBus.RaiseEvent(delegate(IUnitDeathHandler h)
				{
					h.HandleUnitDeath(unit);
				});
			}
			if (unit.IsInCombat && newLifeState != 0)
			{
				unit.GetCombatStateOptional()?.LeaveCombat();
			}
		}
	}

	private static void SetLifeState(AbstractUnitEntity unit, UnitLifeState newLifeState)
	{
		UnitLifeState state = unit.LifeState.State;
		if (state != newLifeState)
		{
			SetLifeStateAfterCheck(unit, newLifeState, state);
		}
	}

	private static void OnUnitDeath(AbstractUnitEntity unit)
	{
		if (unit is BaseUnitEntity baseUnitEntity)
		{
			foreach (ItemEntity item in baseUnitEntity.Inventory)
			{
				item.UpdateSlotIndex(force: true);
			}
		}
		unit.View.HandleDeath();
		EventBus.RaiseEvent((IAbstractUnitEntity)unit, (Action<IUnitDieHandler>)delegate(IUnitDieHandler x)
		{
			x.OnUnitDie();
		}, isCheckRuntime: true);
		EventBus.RaiseEvent((IAbstractUnitEntity)unit, (Action<IUnitHandler>)delegate(IUnitHandler h)
		{
			h.HandleUnitDeath();
		}, isCheckRuntime: true);
	}
}
