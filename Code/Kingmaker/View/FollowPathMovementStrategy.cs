using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Optimization;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View;

public sealed class FollowPathMovementStrategy : IUnitMovementStrategy
{
	private const float InitialSlowDownTimeMin = 0.2f;

	private const float InitialSlowDownTimeMax = 0.3f;

	private const float FadeSlowDownTime = 0.1f;

	private const float StuckTimeStop = 1f;

	private const float StuckTimeResetDirection = 0.15f;

	private static readonly float IgnoreAngleCos = Mathf.Cos(MathF.PI / 6f);

	private readonly UnitMovementAgent m_movementAgent;

	private bool m_FirstTick;

	private int m_FirstTickCounter;

	private Vector3? m_Destination;

	private Vector3? m_NoPathDestination;

	private Vector3 m_Velocity;

	private float m_LastTurnAngle;

	private float m_SlowDownTime;

	private float m_CurrentSlowDownCoefficient = 1f;

	private float m_StuckTimeStop;

	private float m_StuckTimeDirection;

	private float m_MinWaypointDistance;

	private Vector2 m_PreviousPosition;

	private float m_InheritedSpeed;

	public Vector3 FinalDirection { get; private set; }

	public PathCursor PathCursor { get; } = new PathCursor();


	public float Speed { get; private set; }

	public Vector3 Velocity => m_Velocity;

	public Vector3? Destination => m_Destination;

	public bool IsReallyMoving => PathCursor.HasPath;

	public float RemainingPathDistance { get; private set; }

	public FollowPathMovementStrategy(UnitMovementAgent movementAgent)
	{
		m_movementAgent = movementAgent;
	}

	public Vector3 SimulateSimplifiedMovement(int systemTicksCount)
	{
		Vector3 vector = m_movementAgent.Position;
		int num = PathCursor.NextWaypointIndex;
		if (num >= PathCursor.Count)
		{
			return vector;
		}
		Vector2 vector2 = PathCursor.GetWaypoint(num).To2D();
		if (m_Velocity.magnitude < m_movementAgent.MinSpeed)
		{
			m_Velocity = ((vector2 - vector.To2D()).normalized * m_movementAgent.MinSpeed).To3D();
		}
		for (int i = 0; i < systemTicksCount; i++)
		{
			Vector3 shift = m_Velocity * RealTimeController.SystemStepDurationSeconds;
			float magnitude = shift.magnitude;
			float num2 = Vector2.Distance(vector2, vector.To2D());
			if (magnitude >= num2)
			{
				vector = PathCursor.GetWaypoint(num);
				num++;
				if (num >= PathCursor.Count)
				{
					return vector;
				}
				if (PathCursor.IsLinkWaypoint(num) && PathCursor.IsLinkWaypoint(num - 1))
				{
					vector = PathCursor.GetWaypoint(num);
					num++;
				}
				vector2 = PathCursor.GetWaypoint(num).To2D();
			}
			else
			{
				vector = MoveInternal(vector, shift, m_movementAgent.Corpulence);
			}
		}
		return vector;
	}

	public bool TryStartMovingWithPath([NotNull] ForcedPath path, bool isForcedPath, bool requestedNewPath, Vector3 destination, float approachRadiusMeters)
	{
		if (!requestedNewPath)
		{
			bool num = m_Destination.HasValue && destination == m_Destination.Value;
			bool flag = m_NoPathDestination.HasValue && destination == m_NoPathDestination.Value;
			requestedNewPath = !num && !flag;
			m_Destination = destination;
		}
		path.Claim(this);
		try
		{
			if (requestedNewPath)
			{
				m_LastTurnAngle = 0f;
				m_FirstTick = true;
				m_Destination = destination;
			}
			if (!m_Destination.HasValue)
			{
				return false;
			}
			Vector3 vector;
			if (path.vectorPath.Count <= 0)
			{
				vector = Vector3.zero;
			}
			else
			{
				List<Vector3> vectorPath = path.vectorPath;
				vector = vectorPath[vectorPath.Count - 1];
			}
			Vector3 pathDestination = vector;
			AbstractUnitEntity abstractUnitEntity = m_movementAgent.Unit.Or(null)?.EntityData;
			ObstaclePathingResult obstaclePathingResult = ((abstractUnitEntity != null && abstractUnitEntity.IsInCombat && !isForcedPath) ? ObstaclePathfinder.PathAroundStandingObstacles(path, m_movementAgent, null) : ObstaclePathingResult.PathClear);
			bool flag2 = !path.error && obstaclePathingResult != ObstaclePathingResult.NoPath;
			m_NoPathDestination = (flag2 ? null : m_Destination);
			if (!flag2)
			{
				PartUnitCommands partUnitCommands = abstractUnitEntity?.Commands;
				if (partUnitCommands != null && (partUnitCommands.HasAiCommand || obstaclePathingResult != ObstaclePathingResult.NoPath || path.vectorPath.Count <= 1))
				{
					if (m_movementAgent.Unit != null)
					{
						m_movementAgent.Unit.OnPathNotFound();
					}
					return false;
				}
				List<Vector3> vectorPath2 = path.vectorPath;
				pathDestination = vectorPath2[vectorPath2.Count - 1];
			}
			if (path.vectorPath.Count == 1)
			{
				FinalDirection = abstractUnitEntity?.Forward ?? Vector3.forward;
				return false;
			}
			bool num2 = abstractUnitEntity?.IsInPlayerParty ?? false;
			bool flag3 = abstractUnitEntity?.CutsceneControlledUnit != null;
			if (num2 && !flag3 && (double)(path.vectorPath[0] - abstractUnitEntity.Position).magnitude > 0.5)
			{
				path.Release(this);
				path = PathfindingService.Instance.FindPathRT_Blocking(m_movementAgent, m_Destination.Value, approachRadiusMeters);
				path.Claim(this);
			}
			PathCursor.SetPath(abstractUnitEntity, path);
			if (m_movementAgent.Unit != null)
			{
				m_movementAgent.Unit.OnMovementStarted(pathDestination);
			}
			ObstaclesHelper.RemoveFromGroup(m_movementAgent);
			FinalDirection = GetFinalDirectionNormalized(path.vectorPath, abstractUnitEntity?.Forward ?? Vector3.forward);
			UpdateRemainingDistance();
			return true;
		}
		finally
		{
			path.Release(this);
		}
	}

	public void TickMovement(float deltaTime, MovementStrategyInput input, out MovementStrategyProcessingResult result)
	{
		result = new MovementStrategyProcessingResult
		{
			IsPositionChanged = false,
			IsTraverseInProgress = false,
			EstimatedTimeLeft = 0f,
			FaceDirection = input.FaceDirection,
			MoveDirection = input.MoveDirection,
			State = MovementStrategyProcessingResult.MovementState.InProgress
		};
		bool wasTraverseInProgress = input.WasTraverseInProgress;
		try
		{
			if (!IsReallyMoving)
			{
				return;
			}
			bool firstTick = m_FirstTick;
			if (m_FirstTick)
			{
				m_FirstTickCounter = 0;
			}
			else
			{
				m_FirstTickCounter++;
			}
			m_FirstTick = false;
			bool flag = Game.Instance.Player.UISettings.FastMovement && Game.Instance.CurrentModeType != GameModeType.Cutscene && m_FirstTickCounter >= 2 && !CameraRig.Instance.IsScrollingByRoutineSynced && Game.Instance.Player.IsInCombat && Mathf.Abs(Game.Instance.Controllers.TimeController.SlowMoTimeScale - 1f) < 0.01f;
			if (wasTraverseInProgress && PathCursor.OnLastSegment && !m_movementAgent.NodeLinkTraverser.HasPathAfterTraverse)
			{
				result.State = MovementStrategyProcessingResult.MovementState.Completed;
				return;
			}
			Vector2 vector = m_movementAgent.Position.To2D();
			float num = Vector2.Distance(PathCursor.NextWaypoint2D_SizeAdjusted, vector);
			bool flag2 = (result.IsPositionChanged = (vector - m_PreviousPosition).sqrMagnitude > 1E-08f);
			m_PreviousPosition = vector;
			float num2 = deltaTime;
			while (!PathCursor.OnLastSegment && num < Speed * deltaTime && IsDistanceCloseEnough(num))
			{
				if (!TryAdvanceWaypoint(ref result, out var isEnteredTraverse) || isEnteredTraverse)
				{
					return;
				}
				deltaTime -= num / Speed;
				num = Vector2.Distance(PathCursor.NextWaypoint2D_SizeAdjusted, PathCursor.PrevWaypoint2D_SizeAdjusted);
			}
			AbstractUnitEntity abstractUnitEntity = m_movementAgent.Unit.Or(null)?.EntityData;
			float num3 = ((abstractUnitEntity != null && abstractUnitEntity.IsInFogOfWar && Game.Instance.Controllers.TurnController.TurnBasedModeActive) ? (m_movementAgent.MaxSpeed * 8f) : m_movementAgent.MaxSpeed);
			result.EstimatedTimeLeft = Mathf.Max(0f, (RemainingPathDistance + num) / m_movementAgent.MaxSpeed);
			Vector2 nextWaypoint2D_SizeAdjusted = PathCursor.NextWaypoint2D_SizeAdjusted;
			Vector2 vector2 = nextWaypoint2D_SizeAdjusted - vector;
			Vector2 faceDirection = input.FaceDirection;
			Vector2 desiredDir = vector2;
			if (firstTick && m_InheritedSpeed > 0f)
			{
				Speed = Mathf.Min(m_InheritedSpeed, num3);
				m_InheritedSpeed = 0f;
			}
			float desiredSpeed = CalcSpeed(Speed, num3, num2);
			UpdateAvoidance(ref desiredDir, ref desiredSpeed, out var _, out var hasNavmeshObstacles);
			if (Vector2.Angle(faceDirection, desiredDir) > num2 * m_movementAgent.CurrentAngularSpeed)
			{
				SlowDown(ref m_SlowDownTime);
			}
			faceDirection = (((firstTick && hasNavmeshObstacles) || (abstractUnitEntity != null && abstractUnitEntity.IsInFogOfWar)) ? desiredDir : CalcFaceDirection(faceDirection, desiredDir, num3, num2));
			Speed = desiredSpeed;
			m_Velocity = vector2.normalized.To3D() * Speed;
			if (flag)
			{
				abstractUnitEntity?.Translocate(PathCursor.NextWaypoint3D, null);
			}
			else
			{
				m_movementAgent.Position = MoveInternal(m_movementAgent.Position, m_Velocity * deltaTime, m_movementAgent.Corpulence);
			}
			Vector2 vector3 = m_movementAgent.Position.To2D();
			result.MoveDirection = vector2.normalized;
			result.FaceDirection = faceDirection.normalized;
			UpdateRemainingDistance();
			Vector2 prevWaypoint2D_SizeAdjusted = PathCursor.PrevWaypoint2D_SizeAdjusted;
			Vector2 lhs = nextWaypoint2D_SizeAdjusted - prevWaypoint2D_SizeAdjusted;
			Vector2 rhs = nextWaypoint2D_SizeAdjusted - vector3;
			num = Vector2.Distance(PathCursor.NextWaypoint2D_SizeAdjusted, vector3);
			bool flag3 = num <= 0.0001f || Vector2.Dot(lhs, rhs) < 0f;
			if (PathCursor.OnLastSegment)
			{
				if (flag)
				{
					abstractUnitEntity?.SetOrientation(Quaternion.LookRotation(desiredDir.To3D()).eulerAngles.y);
				}
				float sqrMagnitude = (vector3 - nextWaypoint2D_SizeAdjusted).sqrMagnitude;
				flag3 = flag3 || sqrMagnitude < 0.0025f;
			}
			if (flag3)
			{
				if (PathCursor.OnLastSegment)
				{
					result.State = MovementStrategyProcessingResult.MovementState.Completed;
				}
				else
				{
					if (!TryAdvanceWaypoint(ref result, out var isEnteredTraverse2) || isEnteredTraverse2)
					{
						return;
					}
					deltaTime -= num / Speed;
					num = Vector2.Distance(PathCursor.NextWaypoint2D_SizeAdjusted, PathCursor.PrevWaypoint2D_SizeAdjusted);
					nextWaypoint2D_SizeAdjusted = PathCursor.NextWaypoint2D_SizeAdjusted;
				}
			}
			if (PathCursor.OnLastSegment)
			{
				float num4 = Mathf.Min(Speed, AstarPath.active.data.gridGraph.nodeSize / 5f);
				float sqrMagnitude2 = (vector3 - nextWaypoint2D_SizeAdjusted).sqrMagnitude;
				float sqrMagnitude3 = (vector - nextWaypoint2D_SizeAdjusted).sqrMagnitude;
				bool num5 = sqrMagnitude2 < num4 * num4;
				bool flag4 = (vector3 - vector).sqrMagnitude < 1E-08f;
				bool flag5 = sqrMagnitude3 < sqrMagnitude2;
				if (num5 && (flag4 || flag5))
				{
					result.State = MovementStrategyProcessingResult.MovementState.Completed;
				}
			}
			if (num < m_MinWaypointDistance || flag2)
			{
				m_MinWaypointDistance = num;
				m_StuckTimeStop = 0f;
				m_StuckTimeDirection = 0f;
			}
			m_StuckTimeStop += deltaTime / Game.Instance.Controllers.TimeController.GameTimeScale;
			m_StuckTimeDirection += deltaTime / Game.Instance.Controllers.TimeController.GameTimeScale;
			if (m_StuckTimeDirection > 0.15f)
			{
				m_LastTurnAngle = 0f;
				m_StuckTimeDirection = 0f;
			}
			if (m_StuckTimeStop > 1f && PathCursor.HasPath)
			{
				if (m_movementAgent.Unit != null)
				{
					m_movementAgent.Unit.Data.Translocate(PathCursor.NextWaypoint3D, null);
				}
				else
				{
					m_movementAgent.Position = PathCursor.NextWaypoint3D;
				}
				bool isEnteredTraverse3;
				if (PathCursor.OnLastSegment)
				{
					result.State = MovementStrategyProcessingResult.MovementState.Completed;
				}
				else if (!TryAdvanceWaypoint(ref result, out isEnteredTraverse3) || isEnteredTraverse3)
				{
					return;
				}
				m_StuckTimeStop = 0f;
			}
		}
		finally
		{
		}
	}

	public void Stop()
	{
		PathCursor.ClearPath();
		m_Velocity = Vector3.zero;
		m_Destination = null;
		m_FirstTick = true;
		m_InheritedSpeed = 0f;
	}

	public void SetInheritedSpeed(float speed)
	{
		m_InheritedSpeed = speed;
	}

	private static Vector3 GetFinalDirectionNormalized(List<Vector3> path, Vector3 defaultDirection)
	{
		Vector3 vector = (path[path.Count - 1] - path[path.Count - 2]).ToXZ();
		if (vector.sqrMagnitude < 0.01f && path.Count > 2)
		{
			vector = (path[path.Count - 2] - path[path.Count - 3]).ToXZ();
		}
		if (vector.sqrMagnitude < 0.01f)
		{
			vector = defaultDirection;
		}
		if (!(vector.sqrMagnitude >= 0.01f))
		{
			return Vector3.forward;
		}
		return vector.normalized;
	}

	private Vector3 MoveInternal(Vector3 currentPos, Vector3 shift, float movementCorpulence)
	{
		GridNodeBase targetNode;
		return UnitMovementAgent.Move(currentPos, shift, movementCorpulence, out targetNode);
	}

	private static Vector2 Rotate(Vector2 v, float degrees)
	{
		float f = degrees * (MathF.PI / 180f);
		float num = Mathf.Sin(f);
		float num2 = Mathf.Cos(f);
		float x = v.x;
		float y = v.y;
		v.x = num2 * x - num * y;
		v.y = num * x + num2 * y;
		return v;
	}

	private bool TryAdvanceWaypoint(ref MovementStrategyProcessingResult result, out bool isEnteredTraverse)
	{
		isEnteredTraverse = false;
		if (!PathCursor.HasPath)
		{
			PFLog.Pathfinding.Warning("No path");
			return false;
		}
		AbstractUnitEntityView unit = m_movementAgent.Unit;
		(unit.Or(null)?.Data)?.Translocate(PathCursor.NextWaypoint3D, null);
		PathCursor.AdvanceWaypoint();
		ResetStuck();
		Vector3 nextWaypoint3D = PathCursor.NextWaypoint3D;
		if (Game.Instance.CurrentlyLoadedArea.IsNavmeshArea)
		{
			GraphNode node = AstarPath.active?.GetNearest(nextWaypoint3D).node;
			if (WarhammerBlockManager.Instance == null || WarhammerBlockManager.Instance.NodeContainsInvisibleAnyExcept(node, m_movementAgent.Blocker))
			{
				result.State = MovementStrategyProcessingResult.MovementState.Interrupted;
				return false;
			}
		}
		unit.Or(null)?.OnMovementWaypointUpdate();
		m_movementAgent.NodeLinkTraverser.Tick(0f);
		if (m_movementAgent.IsTraverseInProgress)
		{
			isEnteredTraverse = true;
			result.FaceDirection = unit.Or(null)?.ViewTransform.forward.To2D() ?? Vector2.up;
			result.IsTraverseInProgress = true;
			Speed = 0f;
		}
		return true;
	}

	private void ResetStuck()
	{
		m_StuckTimeStop = 0f;
		m_StuckTimeDirection = 0f;
		m_MinWaypointDistance = 1000000f;
	}

	private void UpdateAvoidance(ref Vector2 desiredDir, ref float desiredSpeed, out bool intersectsObstacles, out bool hasNavmeshObstacles)
	{
		intersectsObstacles = false;
		hasNavmeshObstacles = false;
		if (m_movementAgent.AvoidanceDisabled || !(m_movementAgent.Unit.Or(null)?.Data is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		Vector2 vector = m_movementAgent.Position.To2D();
		Vector2 nextWaypoint2D_SizeAdjusted = PathCursor.NextWaypoint2D_SizeAdjusted;
		float b = Vector2.Distance(vector, nextWaypoint2D_SizeAdjusted);
		float num = Mathf.Min(3f, b);
		ObstacleAnalyzer obstacleAnalyzer = new ObstacleAnalyzer(m_movementAgent.Position, desiredDir, m_movementAgent.Corpulence, m_movementAgent.MaxSpeed);
		using (ProfileScope.New("Navmesh Avoidance", m_movementAgent))
		{
			hasNavmeshObstacles = obstacleAnalyzer.AddNavmeshObstacles();
		}
		using (ProfileScope.New("Units Avoidance", m_movementAgent))
		{
			bool flag = baseUnitEntity?.IsInPlayerParty ?? false;
			foreach (BaseUnitEntity item in baseUnitEntity?.Vision.CanBeInRange.Values ?? EntityBoundsHelper.FindUnitsInRange(m_movementAgent.Position, num + m_movementAgent.Corpulence))
			{
				if (!flag || !item.IsInPlayerParty)
				{
					UpdateUnitAvoidance(ref obstacleAnalyzer, desiredDir, ref intersectsObstacles, item, vector, num, nextWaypoint2D_SizeAdjusted);
				}
			}
		}
		float num2;
		using (ProfileScope.New("Calc Direction", m_movementAgent))
		{
			num2 = obstacleAnalyzer.CalcAvoidanceDirection(m_LastTurnAngle);
		}
		if ((double)Mathf.Abs(num2) > 10000.0)
		{
			m_LastTurnAngle = 0f;
			desiredSpeed = 0f;
			return;
		}
		m_LastTurnAngle = num2;
		if (!obstacleAnalyzer.MainDirectionBlockedByStatic && obstacleAnalyzer.ShouldSlowDown)
		{
			bool num3 = m_SlowDownTime > 0f;
			SlowDown(ref m_SlowDownTime);
			if (!num3)
			{
				return;
			}
		}
		desiredDir = Rotate(desiredDir, 0f - num2);
	}

	private void UpdateUnitAvoidance(ref ObstacleAnalyzer obstacleAnalyzer, Vector2 desiredDir, ref bool intersectsObstacles, BaseUnitEntity unit, Vector2 pA, float maxObstacleDistance, Vector2 dest)
	{
		if (unit == null)
		{
			return;
		}
		UnitMovementAgent movementAgent = unit.View.MovementAgent;
		if (!movementAgent || movementAgent.AvoidanceDisabled || movementAgent == m_movementAgent)
		{
			return;
		}
		Vector2 vector = movementAgent.Position.To2D();
		Vector2 velB = movementAgent.LastVelocity.To2D();
		float num = m_movementAgent.Corpulence + movementAgent.Corpulence;
		float num2 = Vector2.Distance(pA, vector);
		float num3 = maxObstacleDistance + num;
		if (!(num2 > num3))
		{
			if (num2 < num)
			{
				intersectsObstacles = true;
			}
			if ((!movementAgent.IsReallyMoving || !(num2 > num) || !(movementAgent.SpeedIndicator > 0.9f) || !(Vector2.Dot(movementAgent.MoveDirection, desiredDir) > IgnoreAngleCos)) && (movementAgent.IsReallyMoving || !(Vector2.Dot(vector - dest, pA - dest) < 0f) || !(num2 > num)))
			{
				float coreCorpulenceDelta = (IsSoftObstacle(movementAgent) ? 10f : 0.3f);
				obstacleAnalyzer.AddObstacle(vector, velB, num, coreCorpulenceDelta);
			}
		}
	}

	private bool IsSoftObstacle(UnitMovementAgent obstacle)
	{
		if (!IsReallyMoving)
		{
			return false;
		}
		AbstractUnitEntity obj = m_movementAgent.Unit.Or(null)?.EntityData;
		AbstractUnitEntity obj2 = obstacle.Unit.Or(null)?.EntityData;
		return obj?.GetFactionOptional() != obj2?.GetFactionOptional();
	}

	private float CalcSpeed(float currentSpeed, float desiredSpeed, float deltaTime)
	{
		float acceleration = m_movementAgent.Acceleration;
		float slowDownCoefficient = m_movementAgent.SlowDownCoefficient;
		if (m_SlowDownTime > 0f)
		{
			float t = ((acceleration * deltaTime < m_CurrentSlowDownCoefficient - slowDownCoefficient) ? (acceleration * deltaTime / (m_CurrentSlowDownCoefficient - slowDownCoefficient)) : 1f);
			m_CurrentSlowDownCoefficient = Mathf.Max(Mathf.Lerp(m_CurrentSlowDownCoefficient, slowDownCoefficient, t), slowDownCoefficient);
			if (m_CurrentSlowDownCoefficient < slowDownCoefficient + 0.01f)
			{
				m_SlowDownTime -= deltaTime;
			}
		}
		else if (m_CurrentSlowDownCoefficient < 0.99f)
		{
			float t2 = ((acceleration * deltaTime < 1f - m_CurrentSlowDownCoefficient) ? (acceleration * deltaTime / (1f - m_CurrentSlowDownCoefficient)) : 1f);
			m_CurrentSlowDownCoefficient = Mathf.Max(Mathf.Lerp(m_CurrentSlowDownCoefficient, 1f, t2), slowDownCoefficient);
		}
		desiredSpeed *= m_CurrentSlowDownCoefficient;
		if (m_movementAgent.DecelerateBeforeStop && RemainingPathDistance <= m_movementAgent.StoppingDistance)
		{
			return Mathf.Max(m_movementAgent.WalkingSpeed, currentSpeed * RemainingPathDistance / m_movementAgent.StoppingDistance);
		}
		if (!(currentSpeed < desiredSpeed))
		{
			return Mathf.Max(currentSpeed - acceleration * deltaTime, desiredSpeed);
		}
		return Mathf.Min(currentSpeed + acceleration * deltaTime, desiredSpeed);
	}

	private Vector2 CalcFaceDirection(Vector2 faceDir, Vector2 desiredFaceDir, float maxSpeed, float deltaTime)
	{
		float num = m_movementAgent.CurrentAngularSpeed;
		float f = Vector2.SignedAngle(faceDir, desiredFaceDir);
		float num2 = Mathf.Abs(f);
		float num3 = Mathf.Sign(f);
		if (PathCursor.OnLastSegment)
		{
			float num4 = RemainingPathDistance / Mathf.Max(maxSpeed, 0.01f);
			float num5 = num2 / num;
			if (num5 * 2f > num4)
			{
				num *= 2f * (num5 / num4);
			}
		}
		return (Quaternion.AngleAxis(Mathf.Min(num * deltaTime, num2), Vector3.down * num3) * faceDir.To3D()).To2D();
	}

	private void UpdateRemainingDistance()
	{
		if (!PathCursor.HasPath)
		{
			RemainingPathDistance = 0f;
			return;
		}
		RemainingPathDistance = Vector2.Distance(PathCursor.NextWaypoint2D_SizeAdjusted, m_movementAgent.Position.To2D());
		RemainingPathDistance += PathCursor.RemainingLength();
		if (m_Destination.HasValue)
		{
			Vector3 lastWaypoint3D_SizeAdjusted = PathCursor.LastWaypoint3D_SizeAdjusted;
			RemainingPathDistance += Vector2.Distance(lastWaypoint3D_SizeAdjusted.To2D(), m_Destination.Value.To2D());
		}
	}

	private static void SlowDown(ref float slowDownTime)
	{
		if (slowDownTime <= 0f)
		{
			slowDownTime = 0.25f;
		}
		else
		{
			slowDownTime = Mathf.Max(slowDownTime, 0.1f);
		}
	}

	private static bool IsDistanceCloseEnough(float distance)
	{
		return distance < 0.3f;
	}
}
