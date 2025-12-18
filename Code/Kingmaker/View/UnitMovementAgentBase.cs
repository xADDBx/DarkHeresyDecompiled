using System;
using System.Collections.Generic;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Optimization;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.CountingGuard;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View;

public class UnitMovementAgentBase : MonoBehaviour, IEntitySubscriber, IUnitLifeStateChanged<EntitySubscriber>, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, ISubscriber, IEventTag<IUnitLifeStateChanged, EntitySubscriber>
{
	public const float DefaultAcceleration = 10f;

	public const float DefaultAngularSpeedInCombat = 360f;

	public const float DefaultAngularSpeedInNonCombat = 180f;

	public const float DefaultAngularSpeedWhenMove = 220f;

	private const float DefaultStoppingDistance = 1.35f;

	private const float DefaultSlowDownCoefficient = 0.7f;

	private const float DefaultMinSpeed = 1.5f;

	private const float WalkingSpeed = 1.5f;

	private GameObject m_Owner;

	public float Acceleration = 10f;

	[SerializeField]
	public bool DecelerateBeforeStop;

	[ShowIf("DecelerateBeforeStop")]
	public float StoppingDistance = 1.35f;

	[SerializeField]
	protected float m_MinSpeed = 1.5f;

	[SerializeField]
	private float m_AngularSpeedInCombat = 360f;

	[SerializeField]
	private float m_AngularSpeedInNonCombat = 180f;

	[SerializeField]
	private float m_AngularSpeedWhenMove = 220f;

	[SerializeField]
	protected float m_SlowDownCoefficient = 0.7f;

	[SerializeField]
	private bool m_AvoidanceDisabled;

	public bool ShowCurrentPath;

	private readonly CountingGuard m_AvoidanceDisabledCounter = new CountingGuard();

	protected PathfindingService.Options m_TurnBasedOptions;

	protected PathfindingService.Options m_RealTimeOptions;

	protected WarhammerSingleNodeBlocker m_Blocker;

	protected WarhammerTraversalProvider m_TraversalProvider;

	protected WarhammerNodeLinkTraverser m_NodeLinkTraverser;

	private bool m_WasTraverseInProgress;

	private PathCursor m_PathCursor = new PathCursor();

	private bool m_Roaming;

	internal float EstimatedTimeLeft;

	protected float m_Speed;

	private float m_RemainingPathDistance;

	protected Vector3? m_Destination;

	private Vector3? m_NoPathDestination;

	protected float m_LastTurnAngle;

	internal static readonly List<UnitMovementAgentBase> AllAgents = new List<UnitMovementAgentBase>();

	[NonSerialized]
	[CanBeNull]
	public List<UnitMovementAgentBase> ObstaclesGroup;

	[NonSerialized]
	[CanBeNull]
	public List<UnitMovementAgentBase> UnitContacts;

	protected const float StuckTimeStop = 1f;

	protected const float StuckTimeResetDirection = 0.15f;

	protected float m_StuckTimeStop;

	protected float m_StuckTimeDirection;

	protected float m_MinWaypointDistance;

	protected Vector2 m_PreviousPosition;

	protected const float InitialSlowDownTimeMin = 0.2f;

	protected const float InitialSlowDownTimeMax = 0.3f;

	protected const float FadeSlowDownTime = 0.1f;

	private static readonly Vector3 s_DebugShift = Vector3.up * 0.05f;

	private static readonly float s_IgnoreAngleCos = Mathf.Cos(MathF.PI / 6f);

	protected float m_SlowDownTime;

	protected bool m_FirstTick;

	protected float m_CurrentSlowDownCoefficient = 1f;

	private int m_ChargingCounter;

	private TimeSpan m_ChargeAvoidanceFinishTime;

	public const float DefaultCorpulenceInPlayerParty = 2f;

	private int m_FirstTickCounter;

	protected Vector3 m_Velocity;

	protected Vector3 m_NextVelocity;

	[Cheat(Name = "movement_use_raycast")]
	public static bool FallbackToRayCast { get; set; }

	public float CurrentAngularSpeed
	{
		get
		{
			if (!IsReallyMoving)
			{
				return AngularSpeedWhenStand;
			}
			return AngularSpeedWhenMove;
		}
	}

	public float AngularSpeedWhenStand
	{
		get
		{
			if (!(Unit != null))
			{
				return 180f;
			}
			if (!Unit.Data.IsInCombat)
			{
				return m_AngularSpeedInNonCombat;
			}
			return m_AngularSpeedInCombat;
		}
	}

	public float AngularSpeedWhenMove => m_AngularSpeedWhenMove;

	private float m_WalkingSpeed
	{
		get
		{
			if (Unit == null || Unit.AnimationManager == null)
			{
				return 1.5f;
			}
			if (Unit.AnimationManager.AnimationSet.GetAction(UnitAnimationType.LocoMotion) is UnitAnimationActionLocomotion unitAnimationActionLocomotion)
			{
				return unitAnimationActionLocomotion.WalkParameters.Speed;
			}
			return 1.5f;
		}
	}

	public PathfindingService.Options RealTimeOptions => m_RealTimeOptions;

	public PathfindingService.Options TurnBasedOptions => m_TurnBasedOptions;

	public WarhammerSingleNodeBlocker Blocker => m_Blocker;

	public ITraversalProvider TraversalProvider
	{
		get
		{
			m_TraversalProvider.SetIsPlayerEnemy((Unit?.Data?.IsPlayerEnemy).GetValueOrDefault());
			return m_TraversalProvider;
		}
	}

	public WarhammerNodeLinkTraverser NodeLinkTraverser => m_NodeLinkTraverser;

	public bool IsInNodeLinkQueue => m_NodeLinkTraverser?.IsInQueue ?? false;

	public bool IsTraverseInProgress => m_NodeLinkTraverser.LastState != WarhammerNodeLinkTraverser.State.None;

	public PathCursor PathCursor => m_PathCursor;

	public bool ConnectedToObstacles { get; set; }

	public bool FirstTick => m_FirstTick;

	public Vector2 MoveDirection { get; protected set; }

	public Vector2 FaceDirection { get; protected set; }

	public virtual Vector3 FinalDirection { get; protected set; }

	public bool IsStopping { get; protected set; }

	public float? MaxSpeedOverride { get; set; }

	public bool ForceRoaming { get; set; }

	public float SpeedIndicator => m_Speed / MaxSpeed;

	public virtual bool AvoidanceDisabled
	{
		get
		{
			if (!m_AvoidanceDisabled && !m_AvoidanceDisabledCounter)
			{
				if (Unit != null)
				{
					if (Unit.EntityData.LifeState.IsConscious)
					{
						return Unit.EntityData.IsInCombat;
					}
					return true;
				}
				return false;
			}
			return true;
		}
		set
		{
			m_AvoidanceDisabledCounter.Value = value;
		}
	}

	public bool IsCharging
	{
		get
		{
			return m_ChargingCounter > 0;
		}
		set
		{
			if (value)
			{
				m_ChargingCounter++;
				return;
			}
			m_ChargingCounter--;
			if (m_ChargingCounter <= 0)
			{
				m_ChargeAvoidanceFinishTime = Game.Instance.Controllers.TimeController.GameTime + 1.Seconds();
			}
		}
	}

	private bool ChargingAvoidance
	{
		get
		{
			if (IsCharging)
			{
				return true;
			}
			if (Game.Instance.Controllers.TimeController.GameTime < m_ChargeAvoidanceFinishTime)
			{
				return true;
			}
			return false;
		}
	}

	public float Corpulence { get; private set; }

	public float ApproachRadiusMeters { get; private set; }

	protected bool CombatMode
	{
		get
		{
			if (Unit != null && Unit.EntityData != null)
			{
				return Unit.EntityData.IsInCombat;
			}
			return false;
		}
	}

	private bool OnFirstSegment => m_PathCursor.OnFirstSegment;

	private bool OnLastSegment => m_PathCursor.OnLastSegment;

	public float RemainingPathDistance => m_RemainingPathDistance;

	public virtual bool IsReallyMoving => m_PathCursor.HasPath;

	public bool IsPositionChanged { get; protected set; }

	public Vector3 LastVelocity => m_Velocity;

	public float MaxSpeed
	{
		get
		{
			float? maxSpeedOverride = MaxSpeedOverride;
			if (!maxSpeedOverride.HasValue)
			{
				if (!(Unit != null) || Unit.EntityData == null)
				{
					return 30.Feet().Meters / 3f;
				}
				return Unit.EntityData.Movable.CurrentSpeedMps;
			}
			return maxSpeedOverride.GetValueOrDefault();
		}
	}

	public float Speed => m_Speed;

	[CanBeNull]
	public AbstractUnitEntityView Unit { get; private set; }

	public virtual Vector3 Position
	{
		get
		{
			if (Unit != null)
			{
				return Unit.Data.Position;
			}
			return base.transform.position;
		}
		set
		{
			if (Unit != null)
			{
				Unit.Data.Position = value;
			}
			else
			{
				base.transform.position = value;
			}
		}
	}

	public IEntity GetSubscribingEntity()
	{
		return Unit.Data;
	}

	private void InitTurnBasedOptions()
	{
		m_TurnBasedOptions = new PathfindingService.Options
		{
			Modifiers = new IPathModifier[2]
			{
				new StartEndModifier
				{
					exactStartPoint = StartEndModifier.Exactness.SnapToNode,
					exactEndPoint = StartEndModifier.Exactness.SnapToNode
				},
				new ReplaceNodeLinkPositionsModifier
				{
					TurnBased = true
				}
			}
		};
		ResetBlocker();
	}

	public void ResetBlocker()
	{
		if (Unit != null)
		{
			m_Blocker?.Unblock();
			m_Blocker = new WarhammerSingleNodeBlocker(Unit.Data);
			m_TraversalProvider = CreateTraversalProvider(Unit, m_Blocker);
		}
		UpdateBlocker();
	}

	private WarhammerTraversalProvider CreateTraversalProvider([NotNull] AbstractUnitEntityView unit, [NotNull] WarhammerSingleNodeBlocker blocker)
	{
		return new WarhammerTraversalProvider(blocker, unit.Data.SizeRect, unit.Data.IsPlayerEnemy);
	}

	private void InitRealTimeOptions()
	{
		if (Unit != null)
		{
			float num = (Unit.Data.IsPlayerFaction ? 2f : ((float)(Unit.Data.SizeRect.Width + Unit.Data.SizeRect.Height)));
			num = num / 4f * GraphParamsMechanicsCache.GridCellSize * 0.55f;
			Corpulence = num;
		}
		else
		{
			Corpulence = 1f;
		}
		m_RealTimeOptions = new PathfindingService.Options
		{
			Modifiers = new IPathModifier[3]
			{
				new StartEndModifier
				{
					exactStartPoint = StartEndModifier.Exactness.Original,
					exactEndPoint = StartEndModifier.Exactness.ClosestOnNode
				},
				new WarhammerFunnelModifier
				{
					AgentRadius = Corpulence,
					Unit = new EntityRef<AbstractUnitEntity>(Unit.EntityData),
					DebugAmplify = Unit.Data.IsInPlayerParty
				},
				new ReplaceNodeLinkPositionsModifier()
			}
		};
	}

	public void Init([NotNull] GameObject owner)
	{
		if ((bool)m_Owner)
		{
			throw new InvalidOperationException("CharacterAgent already initialized");
		}
		m_Owner = owner;
		Unit = m_Owner.GetComponent<AbstractUnitEntityView>();
		m_NodeLinkTraverser = new WarhammerNodeLinkTraverser(this);
		InitRealTimeOptions();
		InitTurnBasedOptions();
		InitFaceDirection();
	}

	public void Tick()
	{
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		ShowCurrentPath = false;
		AllAgents.Add(this);
		UpdateBlocker();
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		AllAgents.Remove(this);
		ObstaclesHelper.RemoveFromGroup(this);
		m_Blocker?.Unblock();
	}

	public void ForcePath([NotNull] ForcedPath p, bool disableApproachRadius = false)
	{
		ApproachRadiusMeters = (disableApproachRadius ? 0.05f : 0.3f);
		List<Vector3> vectorPath = p.vectorPath;
		m_Destination = vectorPath[vectorPath.Count - 1];
		StartMovingWithPath(p, isForcedPath: true, requestedNewPath: true);
	}

	public void FollowPath(ForcedPath p, Vector3 destination, float approachRadiusMeters)
	{
		bool num = m_Destination.HasValue && destination == m_Destination.Value;
		bool flag = m_NoPathDestination.HasValue && destination == m_NoPathDestination.Value;
		bool requestedNewPath = !num && !flag;
		ApproachRadiusMeters = approachRadiusMeters;
		m_Destination = destination;
		InitFaceDirection();
		if (ForceRoaming)
		{
			m_Roaming = true;
		}
		else
		{
			m_Roaming = (Unit.Or(null)?.EntityData.Commands.CurrentMoveTo)?.Roaming ?? false;
		}
		StartMovingWithPath(p, isForcedPath: false, requestedNewPath);
	}

	public static HashSet<GraphNode> CacheThreateningAreaCells(AbstractUnitEntity entity)
	{
		HashSet<GraphNode> hashSet = new HashSet<GraphNode>();
		if (!(entity is BaseUnitEntity baseUnitEntity))
		{
			return hashSet;
		}
		foreach (UnitGroupMemory.UnitInfo enemy in baseUnitEntity.CombatGroup.Memory.Enemies)
		{
			BaseUnitEntity unit = enemy.Unit;
			if (unit.CanMakeAttackOfOpportunity(baseUnitEntity))
			{
				hashSet.UnionWith(unit.GetThreateningArea());
			}
		}
		return hashSet;
	}

	public Vector3 SimulateSimplifiedMovement(int systemTicksCount)
	{
		if (IsTraverseInProgress)
		{
			return (Vector3)m_NodeLinkTraverser.DestinationNode.position;
		}
		Vector3 vector = Position;
		int num = m_PathCursor.NextWaypointIndex;
		Vector2 vector2 = m_PathCursor.GetWaypoint(num).To2D();
		Vector3 velocity = m_Velocity;
		if (velocity.magnitude < m_MinSpeed)
		{
			velocity = ((vector2 - vector.To2D()).normalized * m_MinSpeed).To3D();
		}
		for (int i = 0; i < systemTicksCount; i++)
		{
			Vector3 shift = m_Velocity * RealTimeController.SystemStepDurationSeconds;
			float magnitude = shift.magnitude;
			float num2 = Vector2.Distance(vector2, vector.To2D());
			if (magnitude >= num2)
			{
				vector = m_PathCursor.GetWaypoint(num);
				num++;
				if (num >= m_PathCursor.Count)
				{
					return vector;
				}
				if (m_PathCursor.IsLinkWaypoint(num) && m_PathCursor.IsLinkWaypoint(num - 1))
				{
					vector = m_PathCursor.GetWaypoint(num);
					num++;
				}
				vector2 = m_PathCursor.GetWaypoint(num).To2D();
			}
			else
			{
				vector = MoveInternal(vector, shift, Corpulence);
			}
		}
		return vector;
	}

	public bool HasPathAfterTraverse(Vector3 traverseEndPosition)
	{
		return (m_PathCursor.LastWaypoint3D_SizeAdjusted.To2D() - traverseEndPosition.To2D()).sqrMagnitude > 1f;
	}

	public virtual void TickMovement(float deltaTime)
	{
		m_NodeLinkTraverser.Tick(deltaTime);
		if (IsTraverseInProgress)
		{
			FaceDirection = Unit.ViewTransform.forward.To2D();
			m_WasTraverseInProgress = true;
			m_Speed = m_NodeLinkTraverser.CurrentSpeed;
			return;
		}
		bool wasTraverseInProgress = m_WasTraverseInProgress;
		m_WasTraverseInProgress = false;
		if (Unit != null && (Unit.IsCommandsPreventMovement || (Unit.AnimationManager != null && (Unit.AnimationManager.IsPreventingMovement || Unit.AnimationManager.IsGoingCover))))
		{
			return;
		}
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
			if (wasTraverseInProgress && OnLastSegment && !m_NodeLinkTraverser.HasPathAfterTraverse)
			{
				CompleteMovement(interrupted: false);
				return;
			}
			Vector2 vector = Position.To2D();
			float num = Vector2.Distance(m_PathCursor.NextWaypoint2D_SizeAdjusted, vector);
			bool flag3 = (IsPositionChanged = (vector - m_PreviousPosition).sqrMagnitude > 1E-08f);
			m_PreviousPosition = vector;
			float num2 = deltaTime;
			while (!OnLastSegment && num < m_Speed * deltaTime && IsDistanceCloseEnough(num))
			{
				Vector2 nextWaypoint2D_SizeAdjusted = m_PathCursor.NextWaypoint2D_SizeAdjusted;
				Unit.Data.Translocate(m_PathCursor.NextWaypoint3D, null);
				deltaTime -= num / m_Speed;
				AdvanceWaypoint();
				num = Vector2.Distance(m_PathCursor.NextWaypoint2D_SizeAdjusted, nextWaypoint2D_SizeAdjusted);
				m_StuckTimeStop = 0f;
				m_NodeLinkTraverser.Tick(0f);
				if (IsTraverseInProgress)
				{
					FaceDirection = Unit.ViewTransform.forward.To2D();
					m_WasTraverseInProgress = true;
					m_Speed = 0f;
					return;
				}
			}
			float num3 = ((Unit != null && Unit.Data.IsInFogOfWar && Game.Instance.Controllers.TurnController.TurnBasedModeActive) ? (MaxSpeed * 8f) : MaxSpeed);
			EstimatedTimeLeft = Mathf.Max(0f, (m_RemainingPathDistance + num) / MaxSpeed);
			Vector2 nextWaypoint2D_SizeAdjusted2 = m_PathCursor.NextWaypoint2D_SizeAdjusted;
			Vector2 vector2 = nextWaypoint2D_SizeAdjusted2 - vector;
			Vector2 faceDirection = FaceDirection;
			Vector2 desiredDir = vector2;
			float desiredSpeed = CalcSpeed(m_Speed, num3, num2);
			UpdateAvoidance(ref desiredDir, ref desiredSpeed, out var intersectsObstacles, out var hasNavmeshObstacles);
			if (Vector2.Angle(faceDirection, desiredDir) > num2 * m_AngularSpeedWhenMove)
			{
				SlowDown(ref m_SlowDownTime);
			}
			faceDirection = (((firstTick && hasNavmeshObstacles) || (Unit != null && Unit.Data.IsInFogOfWar)) ? desiredDir : CalcFaceDirection(faceDirection, desiredDir, num3, num2));
			m_Speed = desiredSpeed;
			m_NextVelocity = vector2.normalized.To3D() * m_Speed;
			if (flag)
			{
				Unit.Data.Translocate(m_PathCursor.NextWaypoint3D, null);
			}
			else
			{
				Position = MoveInternal(Position, m_NextVelocity * deltaTime, Corpulence);
			}
			Vector2 vector3 = Position.To2D();
			MoveDirection = vector2;
			FaceDirection = faceDirection;
			UpdateRemainingDistance();
			Vector2 prevWaypoint2D_SizeAdjusted = m_PathCursor.PrevWaypoint2D_SizeAdjusted;
			Vector2 lhs = nextWaypoint2D_SizeAdjusted2 - prevWaypoint2D_SizeAdjusted;
			Vector2 rhs = nextWaypoint2D_SizeAdjusted2 - vector3;
			bool flag4 = num <= 0.0001f || Vector2.Dot(lhs, rhs) < 0f;
			if (OnLastSegment)
			{
				if (flag)
				{
					Unit.Data.SetOrientation(Quaternion.LookRotation(desiredDir.To3D()).eulerAngles.y);
				}
				float sqrMagnitude = (vector3 - nextWaypoint2D_SizeAdjusted2).sqrMagnitude;
				flag4 = flag4 || sqrMagnitude < 0.0025f;
				flag4 |= m_Roaming && intersectsObstacles && sqrMagnitude <= Corpulence * Corpulence * 4f;
			}
			if (flag4)
			{
				if (OnLastSegment)
				{
					CompleteMovement(interrupted: false);
				}
				else
				{
					AdvanceWaypoint();
					nextWaypoint2D_SizeAdjusted2 = m_PathCursor.NextWaypoint2D_SizeAdjusted;
				}
			}
			if (OnLastSegment)
			{
				float num4 = Mathf.Min(m_Speed, AstarPath.active.data.gridGraph.nodeSize / 5f);
				float sqrMagnitude2 = (vector3 - nextWaypoint2D_SizeAdjusted2).sqrMagnitude;
				float sqrMagnitude3 = (vector - nextWaypoint2D_SizeAdjusted2).sqrMagnitude;
				bool num5 = sqrMagnitude2 < num4 * num4;
				bool flag5 = (vector3 - vector).sqrMagnitude < 1E-08f;
				bool flag6 = sqrMagnitude3 < sqrMagnitude2;
				if (num5 && (flag5 || flag6))
				{
					CompleteMovement(interrupted: false);
				}
			}
			if (num < m_MinWaypointDistance || flag3)
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
			if (m_StuckTimeStop > 1f && m_PathCursor.HasPath)
			{
				if (Unit != null)
				{
					Unit.Data.Translocate(m_PathCursor.NextWaypoint3D, null);
				}
				else
				{
					Position = m_PathCursor.NextWaypoint3D;
				}
				if (OnLastSegment)
				{
					CompleteMovement(interrupted: false);
				}
				else
				{
					AdvanceWaypoint();
				}
				m_StuckTimeStop = 0f;
			}
		}
		finally
		{
		}
	}

	private void UpdateRemainingDistance()
	{
		if (!m_PathCursor.HasPath)
		{
			m_RemainingPathDistance = 0f;
			return;
		}
		m_RemainingPathDistance = Vector2.Distance(m_PathCursor.NextWaypoint2D_SizeAdjusted, Position.To2D());
		m_RemainingPathDistance += m_PathCursor.RemainingLength();
		if (m_Destination.HasValue)
		{
			Vector3 lastWaypoint3D_SizeAdjusted = m_PathCursor.LastWaypoint3D_SizeAdjusted;
			m_RemainingPathDistance += Vector2.Distance(lastWaypoint3D_SizeAdjusted.To2D(), m_Destination.Value.To2D());
		}
	}

	private Vector2 CalcFaceDirection(Vector2 faceDir, Vector2 desiredFaceDir, float maxSpeed, float deltaTime)
	{
		float num = m_AngularSpeedWhenMove;
		float f = Vector2.SignedAngle(faceDir, desiredFaceDir);
		float num2 = Mathf.Abs(f);
		float num3 = Mathf.Sign(f);
		if (OnLastSegment)
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

	private float CalcSpeed(float currentSpeed, float desiredSpeed, float deltaTime)
	{
		if (m_SlowDownTime > 0f)
		{
			float t = ((Acceleration * deltaTime < m_CurrentSlowDownCoefficient - m_SlowDownCoefficient) ? (Acceleration * deltaTime / (m_CurrentSlowDownCoefficient - m_SlowDownCoefficient)) : 1f);
			m_CurrentSlowDownCoefficient = Mathf.Max(Mathf.Lerp(m_CurrentSlowDownCoefficient, m_SlowDownCoefficient, t), m_SlowDownCoefficient);
			if (m_CurrentSlowDownCoefficient < m_SlowDownCoefficient + 0.01f)
			{
				m_SlowDownTime -= deltaTime;
			}
		}
		else if (m_CurrentSlowDownCoefficient < 0.99f)
		{
			float t2 = ((Acceleration * deltaTime < 1f - m_CurrentSlowDownCoefficient) ? (Acceleration * deltaTime / (1f - m_CurrentSlowDownCoefficient)) : 1f);
			m_CurrentSlowDownCoefficient = Mathf.Max(Mathf.Lerp(m_CurrentSlowDownCoefficient, 1f, t2), m_SlowDownCoefficient);
		}
		desiredSpeed *= m_CurrentSlowDownCoefficient;
		if (DecelerateBeforeStop && RemainingPathDistance <= StoppingDistance)
		{
			return Mathf.Max(m_WalkingSpeed, desiredSpeed * RemainingPathDistance / StoppingDistance);
		}
		if (!(currentSpeed < desiredSpeed))
		{
			return Mathf.Max(currentSpeed - Acceleration * deltaTime, desiredSpeed);
		}
		return Mathf.Min(currentSpeed + Acceleration * deltaTime, desiredSpeed);
	}

	protected virtual Vector3 MoveInternal(Vector3 currentPos, Vector3 shift, float movementCorpulence)
	{
		GridNodeBase targetNode;
		return Move(currentPos, shift, movementCorpulence, out targetNode);
	}

	public static Vector3 PushAwayFromBorders(Vector3 targetPos, GridNodeBase targetNode, float movementCorpulence)
	{
		for (int i = 0; i < 8; i++)
		{
			if (targetNode.HasConnectionInDirection(i))
			{
				continue;
			}
			GridNode gridNode = (GridNode)targetNode.GetNeighbourAlongDirection(i, checkConnectivity: false);
			if (gridNode == null)
			{
				continue;
			}
			Vector3 vector = (gridNode.ClosestPointOnNode(targetPos).To2D().To3D() - targetPos).To2D().To3D();
			if (vector.magnitude < movementCorpulence)
			{
				if (vector.magnitude < movementCorpulence * 0.01f)
				{
					targetPos += ((Vector3)targetNode.position - targetPos).normalized * movementCorpulence;
					continue;
				}
				Vector3 v = -1f * (vector.normalized * movementCorpulence - vector);
				targetPos += v.To2D().To3D();
			}
		}
		return targetPos;
	}

	public static Vector3 Move(Vector3 currentPos, Vector3 shift, float movementCorpulence, out GridNodeBase targetNode)
	{
		using (ProfileScope.NewScope("Move"))
		{
			NNInfo nearestNode = ObstacleAnalyzer.GetNearestNode(currentPos);
			GraphNode node = nearestNode.node;
			currentPos = nearestNode.position;
			Vector3 vector = currentPos + shift;
			if (Game.Instance.CurrentModeType == GameModeType.Default && !Game.Instance.Controllers.TurnController.TurnBasedModeActive)
			{
				Linecast.LinecastGrid(node.Graph, currentPos, vector, node, out var hit, ObstacleAnalyzer.DefaultXZConstraint, ref Linecast.HasConnectionTransition.Instance);
				vector = hit.point;
				targetNode = hit.node;
				vector = PushAwayFromBorders(vector, targetNode, movementCorpulence);
			}
			else
			{
				targetNode = nearestNode.node as GridNodeBase;
			}
			return vector;
		}
	}

	public void UpdateVelocity()
	{
		if (IsTraverseInProgress)
		{
			m_Velocity = Vector3.zero;
		}
		else
		{
			m_Velocity = (IsReallyMoving ? m_NextVelocity : Vector3.zero);
		}
	}

	protected void ResetStuck()
	{
		m_StuckTimeStop = 0f;
		m_StuckTimeDirection = 0f;
		m_MinWaypointDistance = 1000000f;
	}

	protected void CompleteMovement(bool interrupted)
	{
		if (interrupted && m_Destination.HasValue && (bool)Unit)
		{
			Unit.OnMovementInterrupted(m_Destination.Value);
		}
		Stop();
		if ((bool)Unit)
		{
			Unit.OnMovementComplete();
		}
	}

	protected void UpdateAvoidance(ref Vector2 desiredDir, ref float desiredSpeed, out bool intersectsObstacles, out bool hasNavmeshObstacles)
	{
		intersectsObstacles = false;
		if (AvoidanceDisabled)
		{
			hasNavmeshObstacles = false;
			return;
		}
		Vector2 vector = Position.To2D();
		Vector2 nextWaypoint2D_SizeAdjusted = m_PathCursor.NextWaypoint2D_SizeAdjusted;
		float b = Vector2.Distance(vector, nextWaypoint2D_SizeAdjusted);
		float num = Mathf.Min(3f, b);
		ObstacleAnalyzer obstacleAnalyzer = new ObstacleAnalyzer(Position, desiredDir, Corpulence, MaxSpeed);
		using (ProfileScope.New("Navmesh Avoidance", this))
		{
			hasNavmeshObstacles = obstacleAnalyzer.AddNavmeshObstacles();
		}
		using (ProfileScope.New("Units Avoidance", this))
		{
			BaseUnitEntity obj = m_Owner.GetComponent<UnitEntityView>()?.Data;
			bool flag = obj?.IsInPlayerParty ?? false;
			IList<BaseUnitEntity> list = obj?.Vision.CanBeInRange.Values ?? EntityBoundsHelper.FindUnitsInRange(Position, num + Corpulence);
			for (int i = 0; i < list.Count; i++)
			{
				BaseUnitEntity baseUnitEntity = list[i];
				if (!flag || !baseUnitEntity.IsInPlayerParty)
				{
					UpdateUnitAvoidance(ref obstacleAnalyzer, desiredDir, ref intersectsObstacles, baseUnitEntity, vector, num, nextWaypoint2D_SizeAdjusted);
				}
			}
		}
		float num2 = 0f;
		using (ProfileScope.New("Calc Direction", this))
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

	private static Vector2 Rotate(Vector2 v, float degrees)
	{
		float num = Mathf.Sin(degrees * (MathF.PI / 180f));
		float num2 = Mathf.Cos(degrees * (MathF.PI / 180f));
		float x = v.x;
		float y = v.y;
		v.x = num2 * x - num * y;
		v.y = num * x + num2 * y;
		return v;
	}

	private void UpdateUnitAvoidance(ref ObstacleAnalyzer obstacleAnalyzer, Vector2 desiredDir, ref bool intersectsObstacles, BaseUnitEntity unit, Vector2 pA, float maxObstacleDistance, Vector2 dest)
	{
		if (unit == null)
		{
			return;
		}
		UnitMovementAgentBase movementAgent = unit.View.MovementAgent;
		if (!movementAgent || movementAgent.AvoidanceDisabled || movementAgent == this)
		{
			return;
		}
		Vector2 vector = movementAgent.Position.To2D();
		Vector2 velB = movementAgent.m_Velocity.To2D();
		float num = Corpulence + movementAgent.Corpulence;
		float num2 = Vector2.Distance(pA, vector);
		float num3 = maxObstacleDistance + num;
		if (!(num2 > num3))
		{
			if (num2 < num)
			{
				intersectsObstacles = true;
			}
			if (movementAgent.ChargingAvoidance == ChargingAvoidance && (!movementAgent.IsReallyMoving || !(num2 > num) || !(movementAgent.SpeedIndicator > 0.9f) || !(Vector2.Dot(movementAgent.MoveDirection, desiredDir) > s_IgnoreAngleCos)) && (movementAgent.IsReallyMoving || !(Vector2.Dot(vector - dest, pA - dest) < 0f) || !(num2 > num)))
			{
				float coreCorpulenceDelta = (IsSoftObstacle(movementAgent) ? 10f : 0.3f);
				obstacleAnalyzer.AddObstacle(vector, velB, num, coreCorpulenceDelta);
			}
		}
	}

	private void InitFaceDirection()
	{
		FaceDirection = ((Unit != null) ? Unit.EntityData.OrientationDirection.To2D() : base.transform.forward.To2D());
	}

	private void SlowDown(ref float slowDownTime)
	{
		if (!IsCharging)
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
	}

	private void ClaimPath([CanBeNull] Path path)
	{
		path?.Claim(this);
		if (path == null || path.path.Count <= 1)
		{
			return;
		}
		for (int i = 0; i < path.path.Count - 1; i++)
		{
			GraphNode from = path.path[i];
			GraphNode to = path.path[i + 1];
			if (NodeLinksExtensions.AreConnected(from, to, out var currentLink))
			{
				currentLink.UnitsWithActivePathTroughLink++;
			}
		}
	}

	private void ReleasePath([CanBeNull] Path path)
	{
		if (path == null)
		{
			return;
		}
		if (path.path.Count > 1)
		{
			for (int i = 0; i < path.path.Count - 1; i++)
			{
				GraphNode from = path.path[i];
				GraphNode to = path.path[i + 1];
				if (NodeLinksExtensions.AreConnected(from, to, out var currentLink))
				{
					currentLink.UnitsWithActivePathTroughLink--;
				}
			}
		}
		path?.Release(this);
	}

	protected virtual void StartMovingWithPath([NotNull] ForcedPath path, bool isForcedPath, bool requestedNewPath)
	{
		if (requestedNewPath)
		{
			m_LastTurnAngle = 0f;
			m_FirstTick = true;
		}
		if (!m_Destination.HasValue)
		{
			return;
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
		ObstaclePathingResult obstaclePathingResult = ((CombatMode && !isForcedPath) ? ObstaclePathfinder.PathAroundStandingObstacles(path, this, null) : ObstaclePathingResult.PathClear);
		bool flag = !path.error && obstaclePathingResult != ObstaclePathingResult.NoPath;
		m_NoPathDestination = (flag ? null : m_Destination);
		if (!flag)
		{
			PartUnitCommands partUnitCommands = ((Unit != null && Unit.EntityData != null) ? Unit.EntityData.Commands : null);
			if (partUnitCommands != null && (partUnitCommands.HasAiCommand || obstaclePathingResult != ObstaclePathingResult.NoPath || path.vectorPath.Count <= 1))
			{
				if (Unit != null)
				{
					Unit.OnPathNotFound();
				}
				return;
			}
			List<Vector3> vectorPath2 = path.vectorPath;
			pathDestination = vectorPath2[vectorPath2.Count - 1];
		}
		if (path.vectorPath.Count == 1)
		{
			FinalDirection = Unit.Data.Forward;
			CompleteMovement(interrupted: false);
			return;
		}
		bool valueOrDefault = (Unit?.Data?.IsInPlayerParty).GetValueOrDefault();
		bool flag2 = Unit?.Data?.CutsceneControlledUnit != null;
		if (valueOrDefault && !flag2 && (double)(path.vectorPath[0] - Unit.Data.Position).magnitude > 0.5)
		{
			path = PathfindingService.Instance.FindPathRT_Blocking(Unit.MovementAgent, m_Destination.Value, 0.3f);
		}
		m_PathCursor.SetPath(Unit.EntityData, path);
		if (Unit != null)
		{
			Unit.OnMovementStarted(pathDestination);
		}
		ObstaclesHelper.RemoveFromGroup(this);
		FinalDirection = GetFinalDirectionNormalized(path.vectorPath, Unit.Data.Forward);
		UpdateRemainingDistance();
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

	public void AdvanceWaypointAfterTraversal()
	{
		if (!m_PathCursor.AdvanceWaypointAfterTraversal())
		{
			CompleteMovement(interrupted: false);
		}
	}

	protected void AdvanceWaypoint()
	{
		if (!m_PathCursor.HasPath)
		{
			PFLog.Default.Warning("No path");
			return;
		}
		m_PathCursor.AdvanceWaypoint();
		Vector3 nextWaypoint3D = m_PathCursor.NextWaypoint3D;
		if (Game.Instance.CurrentlyLoadedArea.IsNavmeshArea)
		{
			GraphNode node = AstarPath.active?.GetNearest(nextWaypoint3D).node;
			if (WarhammerBlockManager.Instance == null || WarhammerBlockManager.Instance.NodeContainsInvisibleAnyExcept(node, m_Blocker))
			{
				CompleteMovement(interrupted: true);
				return;
			}
		}
		if (Unit != null)
		{
			Unit.OnMovementWaypointUpdate();
		}
		ResetStuck();
	}

	private bool IsSoftObstacle(UnitMovementAgentBase obstacle)
	{
		if (!IsReallyMoving)
		{
			return false;
		}
		return ((Unit != null && Unit.EntityData != null) ? Unit.EntityData.GetFactionOptional() : null) != ((obstacle.Unit != null && obstacle.Unit.EntityData != null) ? obstacle.Unit.EntityData.GetFactionOptional() : null);
	}

	public void Stop()
	{
		m_NodeLinkTraverser.Reset();
		m_PathCursor.ClearPath();
		IsStopping = false;
		m_Speed = 0f;
		m_Destination = null;
		m_Velocity = Vector3.zero;
		ObstaclesHelper.ConnectToGroups(this);
	}

	private void OnDrawGizmos()
	{
		if (UnitContacts != null)
		{
			foreach (UnitMovementAgentBase unitContact in UnitContacts)
			{
				if ((bool)unitContact)
				{
					Debug.DrawLine(Position + s_DebugShift, unitContact.transform.position + s_DebugShift, Color.magenta);
				}
			}
		}
		if (ShowCurrentPath)
		{
			m_PathCursor.DrawGizmos();
		}
	}

	public bool IsValid()
	{
		if (Unit != null && Unit.EntityData != null)
		{
			return Unit.Blueprint != null;
		}
		return false;
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		UpdateBlocker();
	}

	public void UpdateBlocker()
	{
		if (Blocker == null)
		{
			return;
		}
		bool flag = Game.Instance.Player.IsInCombat && IsNodeBlockNeeded();
		if (Blocker.IsBlocking != flag)
		{
			if (flag)
			{
				Blocker.BlockAtCurrentPosition();
			}
			else
			{
				Blocker.Unblock();
			}
		}
	}

	public void ReinitBlocker(UnitEntity entity)
	{
		m_Blocker = new WarhammerSingleNodeBlocker(entity);
		m_TraversalProvider = CreateTraversalProvider(Unit, m_Blocker);
		UpdateBlocker();
	}

	private bool IsNodeBlockNeeded()
	{
		if (Unit != null && !Unit.Data.LifeState.IsDead && (!Unit.Data.IsPlayerFaction || !Unit.Data.LifeState.IsUnconscious || Unit.Data.Health.HitPointsLeft != 0))
		{
			return Unit.Data.GetOptional<PartDetectiveServoSkull>() == null;
		}
		return false;
	}

	public static float GetDistanceToSegment(Vector3 start, Vector3 end, Vector3 point)
	{
		Vector3 rhs = end - start;
		float num = 0f - (rhs.x * start.x + rhs.y * start.y + rhs.z * start.z);
		float num2 = 0f - (rhs.x * end.x + rhs.y * end.y + rhs.z * end.z);
		bool num3 = rhs.x * point.x + rhs.y * point.y + rhs.z * point.z + num < 0f;
		bool flag = rhs.x * point.x + rhs.y * point.y + rhs.z * point.z + num2 < 0f;
		if (num3)
		{
			return Vector3.Distance(start, point);
		}
		if (!flag)
		{
			return Vector3.Distance(end, point);
		}
		return Vector3.Cross(start - point, rhs).magnitude / rhs.magnitude;
	}

	protected virtual bool IsDistanceCloseEnough(float distance)
	{
		return true;
	}
}
