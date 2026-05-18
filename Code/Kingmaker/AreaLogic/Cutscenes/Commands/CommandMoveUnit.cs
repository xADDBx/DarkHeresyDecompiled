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

		public CommandResult? PathCalculationResult;
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

	public override bool ShouldHaveControlledUnit => true;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (!Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Cant find unit");
		}
		if (!Target.TryGetValue(out var targetPosition))
		{
			return CommandResult.Fail("Cant find target");
		}
		commandData.Unit = value;
		float value2;
		float? orientation = ((Orientation != null && Orientation.TryGetValue(out value2)) ? new float?(value2) : null);
		commandData.Path = PathfindingService.Instance.FindPathRT_Delayed(commandData.Unit.MovementAgent, targetPosition, 0.3f, 1, delegate(ForcedPath path)
		{
			commandData.Path = null;
			if (path.error)
			{
				commandData.PathCalculationResult = CommandResult.Fail($"An error path was returned. Ignoring. Unit [{commandData.Unit}']");
			}
			else if (commandData.Unit == null || commandData.Unit.IsDisposed)
			{
				path.Claim(this);
				path.Release(this);
				commandData.PathCalculationResult = CommandResult.Fail("Unit was disposed while searching for path");
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
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		if (!Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Cant find unit");
		}
		if (!Target.TryGetValue(out var value2))
		{
			return CommandResult.Fail("Cant find target");
		}
		value.Translocate(value2, null);
		if (HideOnReachTarget)
		{
			value.IsInGame = false;
		}
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (Game.Instance.Controllers.EntitySpawner.IsEntityInCreationQueue(commandData.Unit))
		{
			return false;
		}
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

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		CommandResult? pathCalculationResult = commandData.PathCalculationResult;
		if (pathCalculationResult.HasValue && !pathCalculationResult.GetValueOrDefault().IsSuccess)
		{
			return commandData.PathCalculationResult.Value;
		}
		if (time > (double)m_Timeout)
		{
			Interrupt(player);
		}
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Path = null;
		UnitCommandHandle commandHandle = commandData.CommandHandle;
		if (commandHandle != null && !commandHandle.IsFinished)
		{
			commandHandle.Interrupt();
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		if (!Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Cant find unit");
		}
		Data commandData = player.GetCommandData<Data>(this);
		switch (commandData.CommandHandle?.Result)
		{
		case AbstractUnitCommand.ResultType.None:
			commandData.CommandHandle.Interrupt();
			break;
		case AbstractUnitCommand.ResultType.Fail:
		case AbstractUnitCommand.ResultType.Interrupt:
			value.Translocate(Target.GetValue(), null);
			break;
		}
		if (value.View != null)
		{
			if (DisableAvoidance)
			{
				value.View.MovementAgent.AvoidanceDisabled = false;
			}
			commandData.Unit?.View.MovementAgent.Blocker.BlockAtCurrentPosition();
		}
		if (m_SnapToGridInCombat && Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			(value as UnitEntity)?.SnapToGrid();
		}
		if (HideOnReachTarget)
		{
			value.IsInGame = false;
		}
		return CommandResult.Success;
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
}
