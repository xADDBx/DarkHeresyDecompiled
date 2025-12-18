using System;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.Attributes;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Serializable]
[ComponentName("Command/CommandMoveUnit")]
[TypeId("dfbf27283a9d4924cb6f705e71ffe1a9")]
public class CommandMoveUnit : CommandBase
{
	private class Data
	{
		[CanBeNull]
		public AbstractUnitEntity Unit;

		[CanBeNull]
		public UnitCommandHandle CommandHandle;

		public Path Path;

		public Exception ExceptionToThrow;
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[SerializeReference]
	[WorkspaceSecondTarget]
	public PositionEvaluator Target;

	[SerializeReference]
	public FloatEvaluator Orientation;

	public WalkSpeedType Animation = WalkSpeedType.Walk;

	public bool OverrideSpeed;

	[ShowIf("OverrideSpeed")]
	public float Speed = 5f;

	public bool DisableAvoidance = true;

	[Tooltip("Unit will be hidden when reach target")]
	[JsonProperty(PropertyName = "RunAway")]
	public bool HideOnReachTarget;

	[SerializeField]
	[Tooltip("Timeout in case something breaks, forces this command to stop after this many seconds")]
	private float m_Timeout = 20f;

	[SerializeField]
	[Tooltip("Hotfix WH-118691")]
	private bool m_SnapToGridInCombat;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit = Unit.GetValue();
		Vector3 targetPosition = Target.GetValue();
		float value;
		float? orientation = ((Orientation != null && Orientation.TryGetValue(out value)) ? new float?(value) : null);
		if (commandData.Unit == null)
		{
			throw new Exception($"Unit {commandData.Unit} not found");
		}
		commandData.Path = PathfindingService.Instance.FindPathRT_Delayed(commandData.Unit.MovementAgent, targetPosition, 0.3f, 1, delegate(ForcedPath path)
		{
			commandData.Path = null;
			if (path.error)
			{
				commandData.ExceptionToThrow = new Exception("An error path was returned. Ignoring. Unit [" + commandData.Unit?.UniqueId + ", '" + commandData.Unit?.Blueprint?.Name + "']");
			}
			else if (commandData.Unit == null || commandData.Unit.IsDisposed)
			{
				path.Claim(this);
				path.Release(this);
				commandData.ExceptionToThrow = new Exception("Unit was disposed while searching for path");
			}
			else
			{
				UnitMoveToParams unitMoveToParams = new UnitMoveToParams(path, targetPosition)
				{
					MovementType = ((Animation == WalkSpeedType.Sprint) ? WalkSpeedType.Walk : Animation),
					Orientation = orientation,
					OverrideSpeed = (OverrideSpeed ? new float?(Speed) : null)
				};
				unitMoveToParams.MarkFromCutscene();
				commandData.CommandHandle = commandData.Unit.Commands.Run(unitMoveToParams);
				if (DisableAvoidance)
				{
					commandData.Unit.View.MovementAgent.AvoidanceDisabled = true;
				}
			}
		});
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
		Unit.GetValue().Translocate(Target.GetValue(), null);
		if (HideOnReachTarget)
		{
			DoHideOnReachTarget();
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (IsUnitDisabled(commandData.Unit))
		{
			return true;
		}
		if (commandData.Path != null)
		{
			return false;
		}
		UnitCommandHandle commandHandle = commandData.CommandHandle;
		if (commandHandle != null && !commandHandle.IsFinished)
		{
			return false;
		}
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		Exception exceptionToThrow = commandData.ExceptionToThrow;
		if (exceptionToThrow != null)
		{
			commandData.ExceptionToThrow = null;
			throw new Exception("Re-throwing path exception", exceptionToThrow);
		}
		if (time > (double)m_Timeout)
		{
			Interrupt(player);
		}
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Path = null;
		UnitCommandHandle commandHandle = commandData.CommandHandle;
		if (commandHandle != null && !commandHandle.IsFinished)
		{
			commandHandle.Interrupt();
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		AbstractUnitEntity unit = commandData.Unit;
		if (unit == null)
		{
			return;
		}
		UnitCommandHandle commandHandle = commandData.CommandHandle;
		if (commandHandle == null || commandHandle.Result != AbstractUnitCommand.ResultType.Success)
		{
			unit.Translocate(Target.GetValue(), null);
		}
		if ((bool)unit.View)
		{
			if (DisableAvoidance)
			{
				unit.View.MovementAgent.AvoidanceDisabled = false;
			}
			commandData.Unit?.View.MovementAgent.Blocker.BlockAtCurrentPosition();
		}
		if (m_SnapToGridInCombat && Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			(unit as UnitEntity)?.SnapToGrid();
		}
		if (HideOnReachTarget)
		{
			DoHideOnReachTarget();
		}
	}

	private bool IsUnitDisabled(AbstractUnitEntity unit)
	{
		if (unit != null && !unit.IsDisposed && unit.LifeState.IsConscious && unit.IsInGame)
		{
			return !unit.IsInState;
		}
		return true;
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (!Unit || !Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}

	public override string GetCaption()
	{
		return Unit?.GetCaptionShort() + " <b>move</b> to " + (Target ? Target.GetCaptionShort() : "???");
	}

	public override string GetWarning()
	{
		if ((bool)Unit && Unit.CanEvaluate())
		{
			return null;
		}
		return "No unit";
	}

	private void DoHideOnReachTarget()
	{
		if (Unit != null && Unit.TryGetValue(out var value) && value != null)
		{
			value.IsInGame = false;
		}
	}
}
