using System;
using System.Collections.Generic;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.CountingGuard;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View;

[DisallowMultipleComponent]
public class UnitMovementAgent : MonoBehaviour, IEntitySubscriber, IUnitLifeStateChanged<EntitySubscriber>, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, ISubscriber, IEventTag<IUnitLifeStateChanged, EntitySubscriber>
{
	public const float DefaultCorpulenceInPlayerParty = 2f;

	internal static readonly List<UnitMovementAgent> AllAgents = new List<UnitMovementAgent>();

	private static readonly CountingGuard m_AvoidanceDisabledCounter = new CountingGuard();

	private static readonly PathCursor m_NoPathCursor = new PathCursor();

	internal float EstimatedTimeLeft;

	private bool m_IsInitialized;

	private IUnitMovementStrategy m_ActiveMovementStrategy;

	private FollowPathMovementStrategy m_FollowPathMovementStrategy;

	private DirectionalMovementStrategy m_DirectionalMovementStrategy;

	[SerializeField]
	private bool m_AvoidanceDisabled;

	[SerializeField]
	private bool m_InheritSpeedOnStrategySwitch;

	[SerializeField]
	private bool m_AlwaysUseDirectionalMovement;

	[SerializeField]
	private bool m_SmoothRepath;

	private PathfindingService.Options m_TurnBasedOptions;

	private PathfindingService.Options m_RealTimeOptions;

	private WarhammerSingleNodeBlocker m_Blocker;

	private WarhammerTraversalProvider m_TraversalProvider;

	private WarhammerNodeLinkTraverser m_NodeLinkTraverser;

	private bool m_WasTraverseInProgress;

	[Cheat(Name = "movement_use_raycast")]
	public static bool FallbackToRayCast { get; set; }

	[CanBeNull]
	public AbstractUnitEntityView Unit { get; private set; }

	[CanBeNull]
	public IMovementSettingsProvider MovementSettings { get; set; }

	public virtual Vector3 Position
	{
		get
		{
			return Unit.Or(null)?.GetViewPosition() ?? base.transform.position;
		}
		set
		{
			if (Unit != null)
			{
				Unit.Data.Position = SizePathfindingHelper.FromViewToMechanicsPosition(Unit.Data, value);
			}
			else
			{
				base.transform.position = value;
			}
		}
	}

	public virtual bool IsReallyMoving => m_ActiveMovementStrategy?.IsReallyMoving ?? false;

	public bool IsDirectionalMovementActive => m_ActiveMovementStrategy == m_DirectionalMovementStrategy;

	public bool IsPositionChanged { get; private set; }

	public Vector3 LastVelocity
	{
		get
		{
			if (IsTraverseInProgress || !IsReallyMoving)
			{
				return Vector3.zero;
			}
			return m_ActiveMovementStrategy.Velocity;
		}
	}

	public float Acceleration => MovementSettings?.Acceleration ?? 10f;

	public bool DecelerateBeforeStop => MovementSettings?.DecelerateBeforeStop ?? true;

	public float StoppingDistance => MovementSettings?.StoppingDistance ?? 1.35f;

	public float MinSpeed => MovementSettings?.MinSpeed ?? 0.2f;

	public float Speed
	{
		get
		{
			if (!IsTraverseInProgress)
			{
				return m_ActiveMovementStrategy.Speed;
			}
			return m_NodeLinkTraverser.CurrentSpeed;
		}
	}

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

	public float? MaxSpeedOverride { get; set; }

	public float SpeedIndicator => Speed / MaxSpeed;

	public float WalkingSpeed
	{
		get
		{
			if (Unit == null || Unit.AnimationManager == null)
			{
				return 1.5f;
			}
			if (Unit.AnimationManager.AnimationSet.GetAction(UnitAnimationType.LocoMotion) is UnitAnimationActionLocomotion unitAnimationActionLocomotion)
			{
				return unitAnimationActionLocomotion.WalkSpeed;
			}
			return 1.5f;
		}
	}

	public float? OverridenAngularSpeed { get; set; }

	public float CurrentAngularSpeed
	{
		get
		{
			float? overridenAngularSpeed = OverridenAngularSpeed;
			if (!overridenAngularSpeed.HasValue)
			{
				if (!IsReallyMoving)
				{
					if (!(Unit != null))
					{
						return 180f;
					}
					if (!Unit.Data.IsInCombat)
					{
						return AngularSpeedInNonCombat;
					}
					return AngularSpeedInCombat;
				}
				return AngularSpeedWhenMove;
			}
			return overridenAngularSpeed.GetValueOrDefault();
		}
	}

	public float SlowDownCoefficient => MovementSettings?.SlowDownCoefficient ?? 0.7f;

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

	public PathCursor PathCursor
	{
		get
		{
			if (m_ActiveMovementStrategy != m_FollowPathMovementStrategy)
			{
				return m_NoPathCursor;
			}
			return m_FollowPathMovementStrategy.PathCursor;
		}
	}

	public Vector2 MoveDirection { get; private set; }

	public Vector2 FaceDirection { get; private set; }

	public Vector3 FinalDirection => m_FollowPathMovementStrategy.FinalDirection;

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

	public float Corpulence { get; private set; }

	public bool EnableSlidingAssist
	{
		get
		{
			if (m_ActiveMovementStrategy == m_DirectionalMovementStrategy)
			{
				return m_DirectionalMovementStrategy.EnableSlidingAssist;
			}
			return false;
		}
	}

	public float CurrentSlidingAngle
	{
		get
		{
			if (m_ActiveMovementStrategy != m_DirectionalMovementStrategy)
			{
				return 0f;
			}
			return m_DirectionalMovementStrategy.CurrentSlidingAngle;
		}
	}

	public int SlidingAssistDirection
	{
		get
		{
			if (m_ActiveMovementStrategy != m_DirectionalMovementStrategy)
			{
				return 0;
			}
			return m_DirectionalMovementStrategy.SlidingAssistDirection;
		}
	}

	public GridNodeBase CurrentNode
	{
		get
		{
			if (m_ActiveMovementStrategy != m_DirectionalMovementStrategy)
			{
				return null;
			}
			return m_DirectionalMovementStrategy.CurrentNode;
		}
	}

	private float AngularSpeedInCombat => MovementSettings?.AngularSpeedInCombat ?? 360f;

	private float AngularSpeedInNonCombat => MovementSettings?.AngularSpeedInNonCombat ?? 180f;

	private float AngularSpeedWhenMove => MovementSettings?.AngularSpeedWhenMove ?? 220f;

	public bool AlwaysUseDirectionalMovement => m_AlwaysUseDirectionalMovement;

	public bool SmoothRepath => m_SmoothRepath;

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

	public IEntity GetSubscribingEntity()
	{
		return Unit.Or(null)?.Data;
	}

	public void Init([NotNull] GameObject owner)
	{
		if (m_IsInitialized)
		{
			throw new InvalidOperationException("CharacterAgent already initialized");
		}
		Unit = owner.GetComponent<AbstractUnitEntityView>();
		m_NodeLinkTraverser = new WarhammerNodeLinkTraverser(this);
		m_FollowPathMovementStrategy = new FollowPathMovementStrategy(this);
		m_DirectionalMovementStrategy = new DirectionalMovementStrategy(this);
		m_ActiveMovementStrategy = m_FollowPathMovementStrategy;
		InitRealTimeOptions();
		InitTurnBasedOptions();
		InitFaceDirection();
		m_IsInitialized = true;
	}

	public void ForcePath([NotNull] ForcedPath p, bool disableApproachRadius = false)
	{
		InitFaceDirection();
		float approachRadiusMeters = (disableApproachRadius ? 0.05f : 0.3f);
		InheritSpeedIfEnabled();
		m_ActiveMovementStrategy = m_FollowPathMovementStrategy;
		FollowPathMovementStrategy followPathMovementStrategy = m_FollowPathMovementStrategy;
		List<Vector3> vectorPath = p.vectorPath;
		if (!followPathMovementStrategy.TryStartMovingWithPath(p, isForcedPath: true, requestedNewPath: true, vectorPath[vectorPath.Count - 1], approachRadiusMeters))
		{
			CompleteMovement();
		}
	}

	public void FollowPath(ForcedPath p, Vector3 destination, float approachRadiusMeters)
	{
		InitFaceDirection();
		InheritSpeedIfEnabled();
		m_ActiveMovementStrategy = m_FollowPathMovementStrategy;
		if (!m_FollowPathMovementStrategy.TryStartMovingWithPath(p, isForcedPath: false, requestedNewPath: false, destination, approachRadiusMeters))
		{
			CompleteMovement();
		}
	}

	private void InheritSpeedIfEnabled()
	{
		if (m_InheritSpeedOnStrategySwitch && Speed > 0f)
		{
			m_FollowPathMovementStrategy.SetInheritedSpeed(Speed);
		}
	}

	public Vector3 SimulateSimplifiedMovement(int systemTicksCount)
	{
		if (IsTraverseInProgress)
		{
			return m_NodeLinkTraverser.DestinationNode.Vector3Position();
		}
		m_ActiveMovementStrategy = m_FollowPathMovementStrategy;
		return m_FollowPathMovementStrategy.SimulateSimplifiedMovement(systemTicksCount);
	}

	public bool HasPathAfterTraverse(Vector3 traverseEndPosition)
	{
		return (PathCursor.LastWaypoint3D_SizeAdjusted.To2D() - traverseEndPosition.To2D()).sqrMagnitude > 1f;
	}

	public virtual void TickMovement(float deltaTime)
	{
		if (IsMovementPrevented())
		{
			return;
		}
		m_NodeLinkTraverser.Tick(deltaTime);
		if (IsTraverseInProgress)
		{
			FaceDirection = Unit.Or(null)?.ViewTransform.forward.To2D() ?? Vector2.up;
			m_WasTraverseInProgress = true;
			return;
		}
		m_ActiveMovementStrategy.TickMovement(deltaTime, new MovementStrategyInput(m_WasTraverseInProgress, FaceDirection, MoveDirection), out var result);
		EstimatedTimeLeft = result.EstimatedTimeLeft;
		IsPositionChanged = result.IsPositionChanged;
		FaceDirection = result.FaceDirection;
		MoveDirection = result.MoveDirection;
		m_WasTraverseInProgress = result.IsTraverseInProgress;
		if (result.State == MovementStrategyProcessingResult.MovementState.Interrupted)
		{
			InterruptMovement();
		}
		else if (result.State == MovementStrategyProcessingResult.MovementState.Completed)
		{
			CompleteMovement();
		}
	}

	public void AdvanceWaypointAfterTraversal()
	{
		if (!PathCursor.AdvanceWaypointAfterTraversal())
		{
			CompleteMovement();
		}
	}

	public void Stop()
	{
		m_NodeLinkTraverser.Reset();
		m_ActiveMovementStrategy?.Stop();
		ObstaclesHelper.ConnectToGroups(this);
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

	public void SetUpDirectionalMovementParams(Vector2 direction, float multiplier)
	{
		m_ActiveMovementStrategy = m_DirectionalMovementStrategy;
		m_DirectionalMovementStrategy.DirectionFromController = direction;
		m_DirectionalMovementStrategy.DirectionFromControllerMagnitude = multiplier;
	}

	[UsedImplicitly]
	private void OnEnable()
	{
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
		AbstractUnitEntity abstractUnitEntity = Unit.Or(null)?.EntityData;
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
					Unit = new EntityRef<AbstractUnitEntity>(abstractUnitEntity),
					DebugAmplify = (abstractUnitEntity?.IsInPlayerParty ?? false)
				},
				new ReplaceNodeLinkPositionsModifier()
			}
		};
	}

	private WarhammerTraversalProvider CreateTraversalProvider([NotNull] AbstractUnitEntityView unit, [NotNull] WarhammerSingleNodeBlocker blocker)
	{
		return new WarhammerTraversalProvider(blocker, unit.Data.SizeRect, unit.Data.IsPlayerEnemy);
	}

	private void InterruptMovement()
	{
		if ((bool)Unit && m_ActiveMovementStrategy.Destination.HasValue)
		{
			Unit.OnMovementInterrupted(m_ActiveMovementStrategy.Destination.Value);
		}
		CompleteMovement();
	}

	private void CompleteMovement()
	{
		Stop();
		Unit.Or(null)?.OnMovementComplete();
	}

	private void InitFaceDirection()
	{
		FaceDirection = ((Unit != null) ? Unit.EntityData.OrientationDirection.To2D() : base.transform.forward.To2D());
	}

	private bool IsNodeBlockNeeded()
	{
		if ((bool)Unit && !Unit.Data.LifeState.IsDead && (!Unit.Data.IsPlayerFaction || !Unit.Data.LifeState.IsUnconscious || Unit.Data.Health.HitPointsLeft != 0))
		{
			return Unit.Data.GetOptional<PartDetectiveServoSkull>() == null;
		}
		return false;
	}

	private bool IsMovementPrevented()
	{
		if (Unit == null)
		{
			return false;
		}
		if (Unit.IsCommandsPreventMovement)
		{
			return true;
		}
		UnitAnimationManager animationManager = Unit.AnimationManager;
		if (animationManager == null)
		{
			return false;
		}
		if (!animationManager.IsPreventingMovement)
		{
			return animationManager.IsGoingCover;
		}
		return true;
	}
}
