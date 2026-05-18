using System;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework.PubSubSystem;
using Kingmaker.Code.View.Bridge.Facades;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Units.CameraFollow;

public class CombatFollowTasksProvider : IDisposable, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IInterruptTurnStartHandler, IWarhammerAttackHandler, IUnitCommandStartHandler, IDamageHandler, IUnitDeathHandler, IUnitCommandActHandler, IUnitMissedTurnHandler, ISubscriber<IEntity>, ICameraFocusEventHandler
{
	public static readonly int BaseCombatCameraPriority = 100;

	private static readonly int UnitTurnFollowCameraPriority = BaseCombatCameraPriority - 1;

	private static readonly int FocusAttackTargetCameraPriority = BaseCombatCameraPriority;

	private static readonly int ReturnToUnitCameraPriority = BaseCombatCameraPriority;

	private static readonly int UnitDeathCameraPriority = BaseCombatCameraPriority + 1;

	private static readonly int AttackOfOpportunityCameraPriority = BaseCombatCameraPriority + 1;

	private static readonly int DamageDealtCameraPriority = BaseCombatCameraPriority + 2;

	private static readonly int UnitStartTurnFocusCameraPriority = BaseCombatCameraPriority + 5;

	private static readonly int MissedTurnCameraPriority = BaseCombatCameraPriority + 10;

	private static readonly int CustomActionCameraPriority = BaseCombatCameraPriority + 20;

	private static readonly int LastShootCameraPriority = BaseCombatCameraPriority + 25;

	private readonly Action<ICameraFollowTask, bool, float> m_AddTask;

	private uint m_AttackCooldownHandle;

	private int m_AoeDeathCount;

	private bool m_AoeDeathFocused;

	private static BlueprintCameraFollowSettings CameraFollowSettings => ConfigRoot.Instance.CameraRoot.CameraFollowSettings;

	private static bool CameraFollowUnit => SettingsRoot.Game.TurnBased.CameraFollowUnit.GetValue();

	private static bool CameraScrollToCurrentUnit => SettingsRoot.Game.TurnBased.CameraScrollToCurrentUnit.GetValue();

	private static bool IsTurnBased => Game.Instance.Controllers.TurnController.TurnBasedModeActive;

	private static Rect SafeRect => CameraFollowTasksSceneHelper.Instance.SafeRect;

	private bool IsAttackCooldown => m_AttackCooldownHandle != 0;

	public CombatFollowTasksProvider(Action<ICameraFollowTask, bool, float> addTaskAction)
	{
		m_AddTask = addTaskAction;
		EventBus.Subscribe(this);
	}

	public void Dispose()
	{
		Game.Instance.Controllers.CustomCallbackController.Cancel(m_AttackCooldownHandle);
		m_AttackCooldownHandle = 0u;
		EventBus.Unsubscribe(this);
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		HandleUnitStartTurnInternal();
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		HandleUnitStartTurnInternal();
	}

	private void HandleUnitStartTurnInternal()
	{
		if (!IsTurnBased)
		{
			return;
		}
		Game.Instance.Controllers.CameraController?.Follower?.Release();
		m_AoeDeathCount = 0;
		m_AoeDeathFocused = false;
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		MechanicEntity mechanicEntity2 = mechanicEntity;
		if (mechanicEntity is UnitSquad unitSquad)
		{
			if (unitSquad.Leader != null && IsValidCameraFocusTarget(unitSquad.Leader))
			{
				mechanicEntity2 = unitSquad.Leader;
			}
			else
			{
				foreach (UnitReference unit in unitSquad.Units)
				{
					BaseUnitEntity baseUnitEntity = unit.ToBaseUnitEntity();
					if (IsValidCameraFocusTarget(baseUnitEntity) && baseUnitEntity.CanActInTurnBased)
					{
						mechanicEntity2 = baseUnitEntity;
						break;
					}
				}
			}
		}
		if (mechanicEntity is BaseUnitEntity baseUnitEntity2 && mechanicEntity.IsInPlayerParty)
		{
			SelectionManagerFacade.SelectUnit(baseUnitEntity2.View);
		}
		CameraFollowTaskParams toUnitOnStartTurn = CameraFollowSettings.ToUnitOnStartTurn;
		if (IsValidCameraFocusTarget(mechanicEntity2) && GetCameraFocusSettings(mechanicEntity).FocusOnStartTurn && TargetPointRequiresCameraMovement(toUnitOnStartTurn, mechanicEntity2.View.ViewTransform.position) && CameraScrollToCurrentUnit && CameraFollowUnit)
		{
			CameraFollowTask task = new CameraFollowTask(toUnitOnStartTurn, mechanicEntity2.View.ViewTransform, UnitStartTurnFocusCameraPriority, canStartBrain: true, $"CombatVCam {UnitStartTurnFocusCameraPriority}: StartTurn {mechanicEntity2}");
			AddTask(task);
			if (!mechanicEntity2.IsDirectlyControllable && mechanicEntity2.CanActInTurnBased && GetCameraFocusSettings(mechanicEntity).FollowMovement)
			{
				CameraFollowTurnTask task2 = new CameraFollowTurnTask(new CameraFollowTaskParams
				{
					TimeScale = 1f
				}, mechanicEntity, mechanicEntity2.View.ViewTransform, UnitTurnFollowCameraPriority, canStartBrain: true, $"CombatVCam {UnitTurnFollowCameraPriority}: WatchTurn {mechanicEntity2}");
				AddTask(task2);
			}
		}
	}

	public void HandleAttack(RulePerformAttack attackRule)
	{
		if (IsAttackCooldown)
		{
			return;
		}
		if (ActiveEncounter.Current != null && ActiveEncounter.Current.IsMoraleVictoryConditionMet())
		{
			HandleLastShootInCombat(attackRule);
			return;
		}
		CameraFollowTaskParams taskParams = (attackRule.Ability.IsBurst ? CameraFollowSettings.ToTargetOnBurstAttack : CameraFollowSettings.ToTargetOnAttack);
		if (IsValidCameraFocusTarget(attackRule.Target) && GetCameraFocusSettings(attackRule.Initiator).FocusTargetOnAttack && TargetPointRequiresCameraMovement(taskParams, attackRule.Target.Position) && CameraScrollToCurrentUnit)
		{
			CameraFollowTask task = new CameraFollowTask(taskParams, attackRule.Target.Position, FocusAttackTargetCameraPriority, canStartBrain: true, $"CombatVCam {FocusAttackTargetCameraPriority}: Attack Target {attackRule.Target}");
			AddTask(task);
			StartAttackCooldownTimer(attackRule);
		}
	}

	private void HandleLastShootInCombat(RulePerformAttack attackRule)
	{
		CameraFollowTask task = new CameraFollowTask(CameraFollowSettings.ToTargetOnMoraleVictory, attackRule.Target.Position, LastShootCameraPriority, canStartBrain: true, $"CombatVCam {LastShootCameraPriority}: Attack Target {attackRule.Target}");
		AddTask(task);
	}

	private void StartAttackCooldownTimer(RulePerformAttack attackRule)
	{
		CameraFollowTaskParams toTargetOnAttack = CameraFollowSettings.ToTargetOnAttack;
		Game.Instance.Controllers.CustomCallbackController.Cancel(m_AttackCooldownHandle);
		m_AttackCooldownHandle = Game.Instance.Controllers.CustomCallbackController.InvokeInTime(delegate
		{
			TryAddReturnToUnitTask(attackRule.Initiator);
			m_AttackCooldownHandle = 0u;
		}, toTargetOnAttack.CameraObserveTime);
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (!ShouldFocusOnCommand(command) || !IsValidCameraFocusTarget(command.Executor) || !GetCameraFocusSettings(command.Executor).FocusCasterBeforeAction || !CameraScrollToCurrentUnit)
		{
			return;
		}
		int num = BaseCombatCameraPriority;
		CameraFollowTaskParams taskParams;
		if (!(command is UnitUseAbility))
		{
			if (!(command is UnitAttackOfOpportunity))
			{
				return;
			}
			taskParams = CameraFollowSettings.ToCasterOnAttackOfOpportunity;
			num = AttackOfOpportunityCameraPriority;
		}
		else
		{
			taskParams = CameraFollowSettings.ToTargetOnAttack;
		}
		if (TargetPointRequiresCameraMovement(taskParams, command.Executor.ViewPosition))
		{
			CameraFollowTask task = new CameraFollowTask(taskParams, command.Executor.ViewPosition, num, canStartBrain: false, $"CombatVCam {num}: Used Ability By {command.Executor}");
			AddTask(task);
		}
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		if (command is UnitUseAbility { Target: var target } unitUseAbility)
		{
			bool flag = target != null && ((target.HasEntity && IsValidCameraFocusTarget(target.Entity)) || target.IsPoint);
			CameraFollowTaskParams toTargetOnAttack = CameraFollowSettings.ToTargetOnAttack;
			if (ShouldFocusOnCommand(command) && flag && GetCameraFocusSettings(command.Executor).FocusTargetOnAttack && TargetPointRequiresCameraMovement(toTargetOnAttack, target.Point) && CameraFollowUnit)
			{
				CameraFollowTask task = new CameraFollowTask(toTargetOnAttack, target.Point, FocusAttackTargetCameraPriority, canStartBrain: true, $"CombatVCam {FocusAttackTargetCameraPriority}: Used Command On {unitUseAbility.Target}");
				AddTask(task);
			}
		}
	}

	public void HandleDamageDealt(RuleDealDamage dealDamage)
	{
		AbilityData maybeAbility = dealDamage.MaybeAbility;
		if ((object)maybeAbility != null && maybeAbility.IsAoe && dealDamage.ResultUnitDied)
		{
			m_AoeDeathCount++;
		}
		if (m_AoeDeathCount > 1 && !m_AoeDeathFocused)
		{
			TargetWrapper clickedTarget = EvalContext.Current.ClickedTarget;
			if (clickedTarget != null)
			{
				m_AoeDeathFocused = true;
				CameraFollowTask task = new CameraFollowTask(CameraFollowSettings.ToTargetOnDeath, clickedTarget.Point, DamageDealtCameraPriority, canStartBrain: true, $"CombatVCam {DamageDealtCameraPriority}: AoE Death");
				AddTask(task);
			}
		}
	}

	public void HandleUnitDeath(AbstractUnitEntity unitEntity)
	{
		m_AoeDeathCount = 0;
		m_AoeDeathFocused = false;
		if (unitEntity is BaseUnitEntity baseUnitEntity)
		{
			CameraFollowTaskParams toTargetOnDeath = CameraFollowSettings.ToTargetOnDeath;
			MechanicEntity currentUnit = Game.Instance.Controllers.TurnController.CurrentUnit;
			if (currentUnit != null && GetCameraFocusSettings(currentUnit).FocusTargetOnDeath && TargetPointRequiresCameraMovement(toTargetOnDeath, baseUnitEntity.ViewPosition) && CameraScrollToCurrentUnit)
			{
				CameraFollowTask task = new CameraFollowTask(toTargetOnDeath, baseUnitEntity.ViewPosition, UnitDeathCameraPriority, canStartBrain: false, $"CombatVCam {UnitDeathCameraPriority}: Unit Death {unitEntity}");
				AddTask(task);
			}
		}
	}

	private void TryAddReturnToUnitTask(MechanicEntity unitEntity)
	{
		if (unitEntity == Game.Instance.Controllers.TurnController.CurrentUnit)
		{
			CameraFollowTaskParams toCasterAfterAction = CameraFollowSettings.ToCasterAfterAction;
			if (IsValidCameraFocusTarget(unitEntity) && GetCameraFocusSettings(unitEntity).FocusCasterAfterAction && TargetPointRequiresCameraMovement(toCasterAfterAction, unitEntity.View.ViewTransform.position) && CameraScrollToCurrentUnit)
			{
				CameraFollowTask task = new CameraFollowTask(toCasterAfterAction, unitEntity.View.ViewTransform, ReturnToUnitCameraPriority, canStartBrain: false, $"CombatVCam {ReturnToUnitCameraPriority}: Return To {unitEntity}");
				AddTask(task);
			}
		}
	}

	private static bool IsPointOnScreen(Vector3 point)
	{
		Vector3 point2 = CameraRig.Instance.Camera.Or(null)?.WorldToViewportPoint(point) ?? Vector3.positiveInfinity;
		return SafeRect.Contains(point2);
	}

	public static CombatCameraFocusSettings GetCameraFocusSettings(MechanicEntity entity)
	{
		if (entity.IsDirectlyControllable)
		{
			return CameraFollowSettings.PlayerTurnCameraFocusSettings;
		}
		PartFaction factionOptional = entity.GetFactionOptional();
		if ((object)factionOptional != null && factionOptional.IsAlly(ConfigRoot.Instance.SystemMechanics.PlayerFaction))
		{
			return CameraFollowSettings.AllyTurnCameraFocusSettings;
		}
		return CameraFollowSettings.EnemyTurnCameraFocusSettings;
	}

	private static bool IsValidCameraFocusTarget(MechanicEntity entity)
	{
		if (entity != null && entity.IsInCombat && entity.IsRevealed && !entity.IsDeadOrUnconscious)
		{
			return !entity.IsInvisible();
		}
		return false;
	}

	private static bool ShouldFocusOnCommand(AbstractUnitCommand command)
	{
		if (command.Params.FromCutscene)
		{
			return false;
		}
		if (!(command is UnitUseAbility unitUseAbility) || unitUseAbility.Params.DisableCameraFollow)
		{
			return command is UnitAttackOfOpportunity;
		}
		return true;
	}

	private static bool TargetPointRequiresCameraMovement(CameraFollowTaskParams taskParams, Vector3 position)
	{
		bool num = taskParams?.SkipIfOnScreen ?? false;
		bool flag = taskParams?.ForceTimescale ?? false;
		if (num && IsPointOnScreen(position) && !flag)
		{
			return false;
		}
		return true;
	}

	private void AddTask(ICameraFollowTask task)
	{
		m_AddTask?.Invoke(task, arg2: false, 0f);
	}

	public void HandleOnMissedTurn()
	{
		if (EventInvokerExtensions.Entity is AbstractUnitEntity abstractUnitEntity && abstractUnitEntity == Game.Instance.Controllers.TurnController.CurrentUnit)
		{
			CameraFollowTaskParams toCasterOnMissedTurn = CameraFollowSettings.ToCasterOnMissedTurn;
			if (GetCameraFocusSettings(abstractUnitEntity).FocusOnMissedTurn && TargetPointRequiresCameraMovement(toCasterOnMissedTurn, abstractUnitEntity.View.ViewTransform.position) && CameraScrollToCurrentUnit)
			{
				CameraFollowTask task = new CameraFollowTask(toCasterOnMissedTurn, abstractUnitEntity.View.ViewTransform, MissedTurnCameraPriority, canStartBrain: true, $"CombatVCam {MissedTurnCameraPriority}: Missed turn {abstractUnitEntity}");
				AddTask(task);
			}
		}
	}

	public void HandleCameraFocusEvent(Transform target, CameraFollowTaskParams taskParams, bool pauseCombatTurnOrder)
	{
		CameraFollowTask task = new CameraFollowTask(taskParams, target, CustomActionCameraPriority, canStartBrain: true, $"CombatVCam {CustomActionCameraPriority}: Focus on Event with target: {target.gameObject.name}");
		AddTask(task);
	}
}
