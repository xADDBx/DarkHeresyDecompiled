using System;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Facades;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Units.CameraFollow;

public class CombatFollowTasksProvider : IDisposable, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IInterruptTurnStartHandler, IWarhammerAttackHandler, IUnitCommandStartHandler, IUnitDeathHandler, IUnitCommandActHandler, IUnitMissedTurnHandler, ISubscriber<IEntity>
{
	public static readonly int BaseCombatCameraPriority = 100;

	private readonly Action<ICameraFollowTask, bool, float> m_AddTask;

	private uint m_AttackCooldownHandle;

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
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		MechanicEntity mechanicEntity2 = ((mechanicEntity is UnitSquad unitSquad) ? unitSquad.Leader : mechanicEntity);
		if (mechanicEntity is BaseUnitEntity baseUnitEntity && mechanicEntity.IsInPlayerParty)
		{
			SelectionManagerFacade.SelectUnit(baseUnitEntity.View);
		}
		CameraFollowTaskParams toUnitOnStartTurn = CameraFollowSettings.ToUnitOnStartTurn;
		if (IsValidCameraFocusTarget(mechanicEntity2) && GetCameraFocusSettings(mechanicEntity).FocusOnStartTurn && TargetPointRequiresCameraMovement(toUnitOnStartTurn, mechanicEntity2.View.ViewTransform.position) && CameraScrollToCurrentUnit && CameraFollowUnit)
		{
			CameraFollowTask task = new CameraFollowTask(toUnitOnStartTurn, mechanicEntity2.View.ViewTransform, BaseCombatCameraPriority + 5, canStartBrain: true, $"CombatVCam {BaseCombatCameraPriority + 5}: StartTurn {mechanicEntity2}");
			AddTask(task);
			if (!mechanicEntity2.IsDirectlyControllable && mechanicEntity2.CanActInTurnBased && GetCameraFocusSettings(mechanicEntity).FollowMovement)
			{
				CameraFollowTurnTask task2 = new CameraFollowTurnTask(new CameraFollowTaskParams
				{
					TimeScale = 1f
				}, mechanicEntity, mechanicEntity2.View.ViewTransform, BaseCombatCameraPriority - 1, canStartBrain: true, $"CombatVCam {BaseCombatCameraPriority - 1}: WatchTurn {mechanicEntity2}");
				AddTask(task2);
			}
		}
	}

	public void HandleAttack(RulePerformAttack attackRule)
	{
		if (!IsAttackCooldown)
		{
			CameraFollowTaskParams taskParams = (attackRule.Ability.IsBurst ? CameraFollowSettings.ToTargetOnBurstAttack : CameraFollowSettings.ToTargetOnAttack);
			if (IsValidCameraFocusTarget(attackRule.Target) && GetCameraFocusSettings(attackRule.Initiator).FocusTargetOnAttack && TargetPointRequiresCameraMovement(taskParams, attackRule.Target.Position) && CameraScrollToCurrentUnit)
			{
				CameraFollowTask task = new CameraFollowTask(taskParams, attackRule.Target.Position, BaseCombatCameraPriority, $"CombatVCam {BaseCombatCameraPriority}: Attack Target {attackRule.Target}");
				AddTask(task);
				StartAttackCooldownTimer(attackRule);
			}
		}
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
			num = BaseCombatCameraPriority + 1;
		}
		else
		{
			taskParams = CameraFollowSettings.ToTargetOnAttack;
		}
		if (TargetPointRequiresCameraMovement(taskParams, command.Executor.ViewTransform.position))
		{
			CameraFollowTask task = new CameraFollowTask(taskParams, command.Executor.ViewTransform, num, canStartBrain: false, $"CombatVCam {num}: Used Ability By {command.Executor}");
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
				CameraFollowTask task = new CameraFollowTask(toTargetOnAttack, target.Point, BaseCombatCameraPriority, $"CombatVCam {BaseCombatCameraPriority}: Used Command On {unitUseAbility.Target}");
				AddTask(task);
			}
		}
	}

	public void HandleUnitDeath(AbstractUnitEntity unitEntity)
	{
		if (unitEntity is BaseUnitEntity baseUnitEntity)
		{
			CameraFollowTaskParams toTargetOnDeath = CameraFollowSettings.ToTargetOnDeath;
			MechanicEntity currentUnit = Game.Instance.Controllers.TurnController.CurrentUnit;
			if (currentUnit != null && GetCameraFocusSettings(currentUnit).FocusTargetOnDeath && TargetPointRequiresCameraMovement(toTargetOnDeath, baseUnitEntity.ViewTransform.position) && CameraScrollToCurrentUnit)
			{
				CameraFollowTask task = new CameraFollowTask(toTargetOnDeath, baseUnitEntity.ViewTransform, BaseCombatCameraPriority + 1, canStartBrain: false, $"CombatVCam {BaseCombatCameraPriority + 1}: Unit Death {unitEntity}");
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
				CameraFollowTask task = new CameraFollowTask(toCasterAfterAction, unitEntity.View.ViewTransform, BaseCombatCameraPriority, canStartBrain: false, $"CombatVCam {BaseCombatCameraPriority}: Return To {unitEntity}");
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
			if (IsValidCameraFocusTarget(abstractUnitEntity) && GetCameraFocusSettings(abstractUnitEntity).FocusOnMissedTurn && TargetPointRequiresCameraMovement(toCasterOnMissedTurn, abstractUnitEntity.View.ViewTransform.position) && CameraScrollToCurrentUnit)
			{
				CameraFollowTask task = new CameraFollowTask(toCasterOnMissedTurn, abstractUnitEntity.View.ViewTransform, BaseCombatCameraPriority + 2, canStartBrain: true, $"CombatVCam {BaseCombatCameraPriority + 2}: Missed turn {abstractUnitEntity}");
				AddTask(task);
			}
		}
	}
}
