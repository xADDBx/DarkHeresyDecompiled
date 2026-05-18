using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class WarhammerNodeLinkTraverser : ILinkTraversalProvider
{
	public enum State
	{
		None,
		WaitForTraverse,
		TraverseHorizontalIn,
		TraverseIn,
		Traverse,
		TraverseOut,
		TraverseHorizontalOut,
		Traversed
	}

	private const int MaxTraverseProgress = 1;

	private static readonly float MinSpeed = 30.Feet().Meters / 2.5f;

	private readonly UnitMovementAgent m_MovementAgent;

	[CanBeNull]
	private IWarhammerNodeLink m_LastTraversedPathLink;

	private float m_TraverseTimer;

	private bool m_IsNextStateRequested;

	public TraverseData TraverseData;

	private static bool IsCorrectGameMode
	{
		get
		{
			if (!(Game.Instance.CurrentModeType == GameModeType.Default) && !(Game.Instance.CurrentModeType == GameModeType.Cutscene))
			{
				return Game.Instance.CurrentModeType == GameModeType.Dialog;
			}
			return true;
		}
	}

	public State LastState { get; private set; }

	public bool IsUpTraverse => TraverseData?.IsUpTraverse ?? false;

	public float TraverseHeight => TraverseData?.TraverseHeight ?? 0f;

	public float TraverseDistance => TraverseData?.TraverseDistance ?? 0f;

	public bool HasPathAfterTraverse => TraverseData?.HasPathAfterTraverse ?? false;

	public float SpeedBeforeTraverse => TraverseData?.SpeedBeforeTraverse ?? 0f;

	public float InUpHorizontalClipDuration { get; set; }

	public float InUpVerticalClipDuration { get; set; }

	public float OutUpVerticalClipDuration { get; set; }

	public float OutUpHorizontalClipDuration { get; set; }

	public float InDownHorizontalClipDuration { get; set; }

	public float InDownVerticalClipDuration { get; set; }

	public float OutDownVerticalClipDuration { get; set; }

	public float OutDownHorizontalClipDuration { get; set; }

	public float InUpVerticalDistance { get; set; }

	public float OutUpVerticalDistance { get; set; }

	public float InDownVerticalDistance { get; set; }

	public float OutDownVerticalDistance { get; set; }

	public float OutUpHorizontalMoveDuration { get; set; }

	public float VerticalSpeed { get; set; }

	public float LeapInTime { get; set; }

	public float LeapOutTime { get; set; }

	public float LeapTraverseTime { get; set; }

	public float CurrentSpeed { get; set; }

	public bool IsInQueue => LastState == State.WaitForTraverse;

	public bool IsTraverseNow => m_LastTraversedPathLink?.IsInTraverse(this) ?? false;

	[CanBeNull]
	public GridNodeBase DestinationNode
	{
		get
		{
			if (LastState == State.None)
			{
				return null;
			}
			return TraverseData.GraphNodeTo as GridNodeBase;
		}
	}

	public MechanicEntity Traverser => Entity;

	public bool AllowOtherToUseLink
	{
		get
		{
			if (IsUpTraverse)
			{
				if ((LastState != State.Traverse || !(StateProgress > 0.5f)) && LastState != State.TraverseOut)
				{
					return LastState == State.TraverseHorizontalOut;
				}
				return true;
			}
			if ((LastState != State.TraverseHorizontalIn || !(StateProgress > 0.5f)) && LastState != State.Traverse)
			{
				return LastState == State.TraverseOut;
			}
			return true;
		}
	}

	private AbstractUnitEntity Entity
	{
		get
		{
			if (!(View != null))
			{
				return null;
			}
			return View.Data;
		}
	}

	public bool IsLargeEntity { get; set; }

	private AbstractUnitEntityView View => m_MovementAgent.Unit;

	private float StateProgress
	{
		get
		{
			if (IsTraverseNow && GetStateDuration(LastState) > 0f)
			{
				return m_TraverseTimer / GetStateDuration(LastState);
			}
			return 1f;
		}
	}

	public WarhammerNodeLinkTraverser(UnitMovementAgent movementAgent)
	{
		m_MovementAgent = movementAgent;
	}

	public void Reset()
	{
		if (IsTraverseNow)
		{
			CompleteTraverse();
			m_LastTraversedPathLink = null;
			LastState = State.Traversed;
		}
		else
		{
			m_IsNextStateRequested = false;
			m_LastTraversedPathLink = null;
			LastState = State.None;
		}
	}

	public void ForceNextState()
	{
		m_IsNextStateRequested = true;
	}

	public bool CanBuildPathThroughLink(GraphNode from, GraphNode to, IWarhammerNodeLink link)
	{
		if (!IsCorrectGameMode)
		{
			return false;
		}
		if (!link.ConnectsNodes(from, to))
		{
			return false;
		}
		if (!Entity.AllowToBuildPathThroughCustomLinks)
		{
			return false;
		}
		return true;
	}

	public void Tick(float deltaTime)
	{
		if (IsCorrectGameMode)
		{
			switch (LastState)
			{
			case State.None:
				TryFindNode();
				break;
			case State.WaitForTraverse:
				TryStartTraverse();
				break;
			case State.TraverseHorizontalIn:
				TraverseHorizontalIn(deltaTime);
				break;
			case State.TraverseIn:
				TraverseIn(deltaTime);
				break;
			case State.Traverse:
				Traverse(deltaTime);
				break;
			case State.TraverseOut:
				TraverseOut(deltaTime);
				break;
			case State.TraverseHorizontalOut:
				TraverseHorizontalOut(deltaTime);
				break;
			case State.Traversed:
				Reset();
				break;
			}
		}
	}

	private void TryFindNode()
	{
		if (!m_MovementAgent.IsReallyMoving)
		{
			return;
		}
		LinkNode nextWaypointLinkNode = m_MovementAgent.PathCursor.NextWaypointLinkNode;
		LinkNode prevWaypointLinkNode = m_MovementAgent.PathCursor.PrevWaypointLinkNode;
		if (prevWaypointLinkNode != null && nextWaypointLinkNode != null)
		{
			IWarhammerNodeLink warhammerNodeLink = prevWaypointLinkNode.linkSource.component as IWarhammerNodeLink;
			if (!(nextWaypointLinkNode.linkSource.component != (Component)warhammerNodeLink) && !(prevWaypointLinkNode.linkSource.component != (Component)warhammerNodeLink) && m_LastTraversedPathLink != warhammerNodeLink)
			{
				GraphNode graphNodeFrom = ((nextWaypointLinkNode.linkConcrete.startLinkNode == nextWaypointLinkNode) ? nextWaypointLinkNode.linkConcrete.endNodes[0] : nextWaypointLinkNode.linkConcrete.startNodes[0]);
				GraphNode graphNodeTo = ((nextWaypointLinkNode.linkConcrete.startLinkNode == nextWaypointLinkNode) ? nextWaypointLinkNode.linkConcrete.startNodes[0] : nextWaypointLinkNode.linkConcrete.endNodes[0]);
				Vector3 prevWaypoint3D_SizeAdjusted = m_MovementAgent.PathCursor.PrevWaypoint3D_SizeAdjusted;
				Vector3 nextWaypoint3D_SizeAdjusted = m_MovementAgent.PathCursor.NextWaypoint3D_SizeAdjusted;
				TraverseData = new TraverseData(graphNodeFrom, graphNodeTo, prevWaypoint3D_SizeAdjusted, nextWaypoint3D_SizeAdjusted)
				{
					HasPathAfterTraverse = m_MovementAgent.HasPathAfterTraverse(nextWaypoint3D_SizeAdjusted),
					SpeedBeforeTraverse = m_MovementAgent.Speed
				};
				m_LastTraversedPathLink = warhammerNodeLink;
				ResetTraversalParameters();
				EnterNextState();
			}
		}
	}

	private void ResetTraversalParameters()
	{
		InUpHorizontalClipDuration = 0f;
		InUpVerticalClipDuration = 0f;
		OutUpVerticalClipDuration = 0f;
		OutUpHorizontalClipDuration = 0f;
		InDownHorizontalClipDuration = 0f;
		InDownVerticalClipDuration = 0f;
		OutDownVerticalClipDuration = 0f;
		OutDownHorizontalClipDuration = 0f;
		InUpVerticalDistance = 0f;
		OutUpVerticalDistance = 0f;
		InDownVerticalDistance = 0f;
		OutDownVerticalDistance = 0f;
		OutUpHorizontalMoveDuration = 0f;
		VerticalSpeed = 0f;
		LeapInTime = 0f;
		LeapOutTime = 0f;
		LeapTraverseTime = 0f;
		CurrentSpeed = 0f;
	}

	private void TryStartTraverse()
	{
		if (!m_LastTraversedPathLink.CanStartTraverse(this))
		{
			MechanicEntity traverser = Traverser;
			if (traverser != null && traverser.IsInCombat)
			{
				PFLog.Pathfinding.Error("Can't start traverse, force complete");
				CompleteTraverse();
			}
			return;
		}
		if (View.AnimationManager != null)
		{
			if (TraverseData.IsLeapTraverse)
			{
				View.AnimationManager.StartLeap();
			}
			else
			{
				View.AnimationManager.StartClimb();
			}
		}
		m_LastTraversedPathLink.StartTransition(this);
		EnterNextState();
	}

	private State GetNextState(State currentState)
	{
		State state;
		while (true)
		{
			switch (currentState)
			{
			case State.None:
				return State.WaitForTraverse;
			case State.WaitForTraverse:
				state = State.TraverseHorizontalIn;
				break;
			case State.TraverseHorizontalIn:
				state = State.TraverseIn;
				break;
			case State.TraverseIn:
				state = State.Traverse;
				break;
			case State.Traverse:
				state = State.TraverseOut;
				break;
			case State.TraverseOut:
				state = State.TraverseHorizontalOut;
				break;
			case State.TraverseHorizontalOut:
				return State.Traversed;
			case State.Traversed:
				return State.Traversed;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (GetStateDuration(state) > 0f)
			{
				break;
			}
			currentState = state;
		}
		return state;
	}

	private void TraverseHorizontalIn(float deltaTime)
	{
		ProcessTraverse(deltaTime);
		if (!(StateProgress < 1f) || m_IsNextStateRequested)
		{
			EnterNextState();
		}
	}

	private void TraverseIn(float deltaTime)
	{
		ProcessTraverse(deltaTime);
		if (!(StateProgress < 1f) || m_IsNextStateRequested)
		{
			EnterNextState();
		}
	}

	private void Traverse(float deltaTime)
	{
		ProcessTraverse(deltaTime);
		if (!(StateProgress < 1f) || m_IsNextStateRequested)
		{
			EnterNextState();
		}
	}

	private void TraverseOut(float deltaTime)
	{
		ProcessTraverse(deltaTime);
		if (!(StateProgress < 1f) || m_IsNextStateRequested)
		{
			EnterNextState();
		}
	}

	private void TraverseHorizontalOut(float deltaTime)
	{
		ProcessTraverse(deltaTime);
		if (!(StateProgress < 1f) || m_IsNextStateRequested)
		{
			CompleteTraverse();
		}
	}

	private void ProcessTraverse(float deltaTime)
	{
		AbstractUnitEntityView unit = m_MovementAgent.Unit;
		if ((object)unit != null)
		{
			AbstractUnitEntity entityData = unit.EntityData;
			if (entityData != null && entityData.TraverseThroughCustomLinksImmediately)
			{
				Reset();
				return;
			}
		}
		m_TraverseTimer += deltaTime;
		Vector3 position = CalculateTraverseViewPosition2D().To3D(GetTraverseY());
		m_MovementAgent.Position = position;
		Vector3 point = CalculateLookPosition();
		Entity.ForceTurnTo(point);
		if (m_MovementAgent.Unit != null)
		{
			IKController ikController = m_MovementAgent.Unit.IkController;
			if ((object)ikController != null && (object)ikController.GrounderIk != null)
			{
				ikController.enabled = false;
				ikController.GrounderIk.enabled = false;
			}
		}
	}

	public float GetTraverseSpeedMps()
	{
		if (VerticalSpeed != 0f)
		{
			return VerticalSpeed;
		}
		if (MinSpeed > Entity.MovementAgent.MaxSpeed)
		{
			return Math.Max(MinSpeed, Entity.Movable.DefaultSpeedMps);
		}
		return Math.Clamp(Entity.Movable.DefaultSpeedMps, MinSpeed, Entity.MovementAgent.MaxSpeed);
	}

	public float GetStateDuration(State state)
	{
		switch (state)
		{
		case State.TraverseHorizontalIn:
			if (!TraverseData.IsLeapTraverse)
			{
				if (IsUpTraverse)
				{
					return InUpHorizontalClipDuration;
				}
				return InDownHorizontalClipDuration;
			}
			return LeapInTime;
		case State.TraverseIn:
			if (IsUpTraverse)
			{
				return InUpVerticalClipDuration;
			}
			return InDownVerticalClipDuration;
		case State.Traverse:
			if (!TraverseData.IsLeapTraverse)
			{
				if (!IsUpTraverse)
				{
					return (TraverseHeight - InDownVerticalDistance - OutDownVerticalDistance) / GetTraverseSpeedMps();
				}
				return (TraverseHeight - InUpVerticalDistance - OutUpVerticalDistance) / GetTraverseSpeedMps();
			}
			return LeapTraverseTime;
		case State.TraverseOut:
			if (!IsUpTraverse)
			{
				return OutDownVerticalClipDuration;
			}
			return OutUpVerticalClipDuration;
		case State.TraverseHorizontalOut:
			if (!TraverseData.IsLeapTraverse)
			{
				if (!IsUpTraverse)
				{
					return OutDownHorizontalClipDuration;
				}
				return OutUpHorizontalClipDuration;
			}
			return LeapOutTime;
		default:
			return 0f;
		}
	}

	private float GetTraverseY()
	{
		switch (LastState)
		{
		case State.TraverseHorizontalIn:
			return TraverseData.GraphNodeFromY;
		case State.TraverseIn:
			if (!IsUpTraverse)
			{
				return Mathf.Lerp(TraverseData.GraphNodeFromY, TraverseData.GraphNodeFromY - InDownVerticalDistance, StateProgress);
			}
			return Mathf.Lerp(TraverseData.GraphNodeFromY, TraverseData.GraphNodeFromY + InUpVerticalDistance, StateProgress);
		case State.Traverse:
			if (!TraverseData.IsLeapTraverse)
			{
				if (!IsUpTraverse)
				{
					return Mathf.Lerp(TraverseData.GraphNodeFromY - InDownVerticalDistance, TraverseData.GraphNodeToY + OutDownVerticalDistance, StateProgress);
				}
				return Mathf.Lerp(TraverseData.GraphNodeFromY + InUpVerticalDistance, TraverseData.GraphNodeToY - OutUpVerticalDistance, StateProgress);
			}
			return TraverseData.GraphNodeToY;
		case State.TraverseOut:
			if (!IsUpTraverse)
			{
				return Mathf.Lerp(TraverseData.GraphNodeToY + OutDownVerticalDistance, TraverseData.GraphNodeToY, StateProgress);
			}
			return Mathf.Lerp(TraverseData.GraphNodeToY - OutUpVerticalDistance, TraverseData.GraphNodeToY, StateProgress);
		case State.TraverseHorizontalOut:
			return TraverseData.GraphNodeToY;
		default:
			throw new NotImplementedException("Only traverse states has traverse position");
		}
	}

	private Vector2 CalculateTraverseViewPosition2D()
	{
		if (TraverseData.IsLeapTraverse)
		{
			return CalculateLeapTraverseViewPosition2D();
		}
		if (TraverseData.IsLedgeTraverse)
		{
			if (!IsUpTraverse)
			{
				if (LastState != State.TraverseHorizontalIn)
				{
					return TraverseData.WaypointTo2D;
				}
				return Vector2.Lerp(TraverseData.WaypointFrom2D, TraverseData.WaypointTo2D, StateProgress);
			}
			if (LastState != State.TraverseHorizontalOut)
			{
				return TraverseData.WaypointFrom2D;
			}
			return Vector2.Lerp(TraverseData.WaypointFrom2D, TraverseData.WaypointTo2D, Mathf.Clamp(m_TraverseTimer / OutUpHorizontalMoveDuration, 0f, 1f));
		}
		Vector2 vector = (IsLargeEntity ? (0.675f * (TraverseData.GraphNodeTo2D - TraverseData.GraphNodeFrom2D).normalized) : Vector2.zero);
		Vector2 vector2 = (IsUpTraverse ? (TraverseData.GraphNodeFrom2D - vector) : (TraverseData.GraphNodeTo2D + vector));
		if (LastState == State.TraverseHorizontalIn)
		{
			return Vector2.Lerp(TraverseData.WaypointFrom2D, vector2, StateProgress);
		}
		if (LastState == State.TraverseHorizontalOut)
		{
			return Vector2.Lerp(vector2, TraverseData.WaypointTo2D, IsUpTraverse ? Mathf.Clamp(m_TraverseTimer / OutUpHorizontalMoveDuration, 0f, 1f) : StateProgress);
		}
		return vector2;
	}

	private Vector2 CalculateLeapTraverseViewPosition2D()
	{
		switch (LastState)
		{
		case State.WaitForTraverse:
		case State.TraverseHorizontalIn:
		case State.TraverseIn:
			return TraverseData.WaypointFrom2D;
		case State.Traverse:
			return Vector2.Lerp(TraverseData.WaypointFrom2D, TraverseData.WaypointTo2D, StateProgress);
		case State.TraverseOut:
		case State.TraverseHorizontalOut:
		case State.Traversed:
			return TraverseData.WaypointTo2D;
		default:
			throw new ArgumentOutOfRangeException();
		case State.None:
			throw new Exception($"Can't calculate position while in {LastState} state");
		}
	}

	private Vector3 CalculateLookPosition()
	{
		if (TraverseData.IsLadderTraverse)
		{
			Vector3 vector = (TraverseData.GraphNodeTo2D - TraverseData.GraphNodeFrom2D).To3D();
			if (LastState == State.TraverseHorizontalOut)
			{
				Vector3 vector2 = (TraverseData.WaypointTo - TraverseData.WaypointFrom).ToXZ();
				Vector3 vector3 = Vector3.Lerp(vector.normalized, vector2.normalized, StateProgress);
				return Entity.Position + vector3;
			}
			return Entity.Position + vector;
		}
		return Entity.Position + TraverseData.WaypointTo - TraverseData.WaypointFrom;
	}

	private void EnterNextState()
	{
		State nextState = GetNextState(LastState);
		m_IsNextStateRequested = false;
		m_TraverseTimer = Mathf.Max(m_TraverseTimer - GetStateDuration(LastState), 0f);
		LastState = nextState;
	}

	private void CompleteTraverse()
	{
		if (m_MovementAgent.Unit != null)
		{
			IKController ikController = m_MovementAgent.Unit.IkController;
			if ((object)ikController != null && (object)ikController.GrounderIk != null)
			{
				ikController.enabled = true;
				ikController.GrounderIk.enabled = true;
			}
		}
		if (View.AnimationManager != null)
		{
			View.AnimationManager.StopClimb();
			View.AnimationManager.StopLeap();
		}
		m_IsNextStateRequested = false;
		m_TraverseTimer = 0f;
		m_MovementAgent.Position = TraverseData.WaypointTo;
		m_LastTraversedPathLink?.CompleteTransition(this);
		LastState = State.Traversed;
		m_MovementAgent.AdvanceWaypointAfterTraversal();
	}
}
