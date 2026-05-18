using System.Collections.Generic;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitFollow : AbstractUnitCommand<UnitFollowParams>
{
	private readonly struct TargetEntityMovementData
	{
		private readonly UnitMovementAgent m_MovementAgent;

		private readonly UnitMoveContinuously m_MoveCommand;

		public bool IsMoving => m_MovementAgent?.IsReallyMoving ?? false;

		public bool IsPositionChanged => m_MovementAgent.IsPositionChanged;

		public float? MaxSpeedOverride
		{
			get
			{
				if (!IsMoving)
				{
					return null;
				}
				return m_MovementAgent.MaxSpeed;
			}
		}

		public WalkSpeedType? MovementType => m_MoveCommand?.MovementType;

		public Vector2? Direction => m_MoveCommand?.Params.Direction;

		public float? Multiplier => m_MoveCommand?.Params.Multiplier;

		public TargetEntityMovementData(MechanicEntity entity)
		{
			m_MovementAgent = entity.MaybeMovementAgent;
			m_MoveCommand = entity.GetOptional<PartUnitCommands>()?.CurrentMoveContinuously;
		}
	}

	private const float NearDestinationRadiusSquared = 3f;

	private const float FarDestinationRadiusSquared = 10f;

	private const float StopMovementRadius = 0.2f;

	private const float RepathTimeThreshold = 0.3f;

	private const float RepathDistanceThreshold = 1f;

	private float m_RepathTimer = float.MaxValue;

	private Vector3 m_RememberedDestination;

	public override bool IsMoveUnit => true;

	private Vector3 Destination => base.Params.Destination;

	private bool IsTooFarFromDestination => (base.Executor.Position - Destination).sqrMagnitude > 10f;

	private bool IsCloseEnoughToDestination => (base.Executor.Position - Destination).sqrMagnitude < 3f;

	private bool HasReachedDestination => (base.Executor.Position - Destination).sqrMagnitude < 0.02f;

	public UnitFollow([NotNull] UnitFollowParams @params)
		: base(@params)
	{
	}

	protected override void OnTick()
	{
		MechanicEntity mechanicEntity = base.Target?.Entity;
		if (mechanicEntity == null)
		{
			ForceFinish(ResultType.Fail);
			return;
		}
		if (HasReachedDestination)
		{
			ResetMovement();
			return;
		}
		TargetEntityMovementData targetData = new TargetEntityMovementData(mechanicEntity);
		base.Params.MovementType = GetMovementType(targetData);
		if (ShouldMatchTargetMovement(targetData))
		{
			if (targetData.IsPositionChanged)
			{
				m_RepathTimer = float.MaxValue;
				MatchTargetMovement(targetData);
			}
		}
		else if (IsCloseEnoughToDestination)
		{
			MoveStraightToDestination();
		}
		else if (base.Executor.MovementAgent.AlwaysUseDirectionalMovement)
		{
			Vector2 vector = (Destination - base.Executor.Position).To2D();
			base.Executor.MovementAgent.SetUpDirectionalMovementParams(vector.normalized, 1f);
			base.Executor.MovementAgent.MaxSpeedOverride = null;
		}
		else
		{
			m_RepathTimer += Game.Instance.RealTimeController.SystemDeltaTime;
			if (ShouldRepath(targetData))
			{
				Repath();
			}
		}
	}

	private WalkSpeedType GetMovementType(TargetEntityMovementData targetData)
	{
		WalkSpeedType? movementType = targetData.MovementType;
		if (!movementType.HasValue)
		{
			if (!IsTooFarFromDestination)
			{
				return base.Params.MovementType;
			}
			return WalkSpeedType.Run;
		}
		return movementType.GetValueOrDefault();
	}

	private bool ShouldMatchTargetMovement(TargetEntityMovementData targetData)
	{
		if (targetData.IsMoving && targetData.Direction.HasValue)
		{
			return IsCloseEnoughToDestination;
		}
		return false;
	}

	private void MatchTargetMovement(TargetEntityMovementData targetData)
	{
		base.Executor.MovementAgent.SetUpDirectionalMovementParams(targetData.Direction.Value, targetData.Multiplier ?? 1f);
		if (base.Params.UseOwnSpeed)
		{
			base.Executor.MovementAgent.MaxSpeedOverride = null;
		}
		else
		{
			base.Executor.MovementAgent.MaxSpeedOverride = targetData.MaxSpeedOverride;
		}
		base.Params.MovementType = targetData.MovementType ?? WalkSpeedType.Walk;
	}

	private void MoveStraightToDestination()
	{
		Vector2 vector = (Destination - base.Executor.Position).To2D();
		base.Executor.MovementAgent.SetUpDirectionalMovementParams(vector.normalized, vector.sqrMagnitude / 3f);
		base.Executor.MovementAgent.MaxSpeedOverride = null;
		base.Params.MovementType = WalkSpeedType.Walk;
	}

	private bool ShouldRepath(TargetEntityMovementData targetData)
	{
		if (!base.Executor.MovementAgent.IsTraverseInProgress)
		{
			if ((!targetData.IsMoving || base.Executor.IsReallyMoving) && !(base.Executor.MovementAgent.EstimatedTimeLeft < 0.3f))
			{
				if ((m_RememberedDestination - Destination).sqrMagnitude > 1f)
				{
					return m_RepathTimer > 0.3f;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private void Repath()
	{
		m_RepathTimer = 0f;
		m_RememberedDestination = Destination;
		AbstractUnitEntity executor = base.Executor;
		if (base.Params.UseStraightPath)
		{
			ForcedPath path2 = ForcedPath.Construct(new List<Vector3>
			{
				base.Executor.Position,
				Destination
			});
			if (!base.Executor.MovementAgent.SmoothRepath)
			{
				executor.StopMoving();
			}
			executor.MoveTo(path2, Destination, 0.2f);
			return;
		}
		PathfindingService.Instance.FindPathRT_Delayed(base.Executor.MovementAgent, Destination, 0.2f, 1, delegate(ForcedPath path)
		{
			if (!base.Executor.MovementAgent.IsTraverseInProgress)
			{
				if (path.error)
				{
					PFLog.Pathfinding.Error("An error path was returned. Ignoring");
				}
				else if (path.path == null || path.path.Count == 0)
				{
					PFLog.Pathfinding.Error(((path.path == null) ? "Path is null" : "Path is empty") + ". Ignoring");
				}
				else
				{
					Vector3 destination = ((ObstacleAnalyzer.GetArea(Destination) == path.path[0].Area) ? Destination : TrimPathToCurrentArea(path));
					if (!base.Executor.MovementAgent.SmoothRepath)
					{
						executor.StopMoving();
					}
					executor.MoveTo(path, destination, 0.2f);
				}
			}
		});
	}

	private static Vector3 TrimPathToCurrentArea(ForcedPath path)
	{
		uint area = path.path[0].Area;
		int num = path.vectorPath.Count;
		while (num > 0 && ObstacleAnalyzer.GetArea(path.vectorPath[num - 1]) != area)
		{
			num--;
		}
		path.vectorPath.RemoveRange(num, path.vectorPath.Count - num);
		if (path.vectorPath == null || path.vectorPath.Count == 0)
		{
			return path.path[0].Vector3Position();
		}
		List<Vector3> vectorPath = path.vectorPath;
		return vectorPath[vectorPath.Count - 1];
	}

	protected override ResultType OnAction()
	{
		return ResultType.None;
	}

	protected override void OnEnded()
	{
		base.OnEnded();
		ResetMovement();
	}

	private void ResetMovement()
	{
		base.Executor.MovementAgent.Stop();
		base.Executor.MovementAgent.MaxSpeedOverride = null;
		if (base.Target != null)
		{
			base.Executor.DesiredOrientation = base.Target.Orientation;
		}
	}
}
