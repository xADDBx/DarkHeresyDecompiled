using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

[RequireComponent(typeof(InteractionSkillCheck))]
public class WarhammerOneWayNodeLink : GraphModifier, IWarhammerNodeLink, ITurnBasedModeHandler, ISubscriber
{
	public Transform End;

	public InteractionSkillCheck InteractionSkillCheck;

	private Vector3 m_StartPosition;

	private Vector3 m_EndPosition;

	private GridNodeBase m_StartNode;

	private GridNodeBase m_EndNode;

	private OffMeshLinks.OffMeshLinkSource m_LinkSource;

	private IOffMeshLinkHandler m_OffMeshLinkHandler;

	private readonly List<ILinkTraversalProvider> m_CurrentTraverserList = new List<ILinkTraversalProvider>();

	public Transform StartTransform => base.transform;

	public Transform EndTransform => End;

	public GraphNode StartNode => m_StartNode;

	public GraphNode EndNode => m_EndNode;

	public bool IsValid { get; private set; }

	public bool IsConnected
	{
		get
		{
			if (m_StartNode != null && m_EndNode != null && m_StartNode.Walkable)
			{
				return m_EndNode.Walkable;
			}
			return false;
		}
	}

	public bool IsActiveAndEnabled => base.isActiveAndEnabled;

	private bool CanUseTraverserLink => m_CurrentTraverserList.All((ILinkTraversalProvider t) => t.AllowOtherToUseLink);

	public float CostFactor => 1f;

	public int UnitsWithActivePathTroughLink { get; set; }

	public event IWarhammerNodeLink.OnTransitionCompletedDel OnTransitionCompleted;

	private void Start()
	{
		EventBus.Subscribe(this);
	}

	public override void OnGraphsPostUpdate()
	{
		UpdateNodes(StartTransform.position, EndTransform.position);
		ValidateNodeLink();
		if (m_LinkSource != null)
		{
			RemoveOffMeshLink();
			AddOffMeshLink();
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		RemoveOffMeshLink();
		if (isTurnBased)
		{
			AddOffMeshLink();
		}
	}

	public bool IsInTraverse(ILinkTraversalProvider traverser)
	{
		return m_CurrentTraverserList.Contains(traverser);
	}

	public bool CanStartTraverse(ILinkTraversalProvider traverser)
	{
		if (IsValid)
		{
			return CanUseTraverserLink;
		}
		return false;
	}

	public void StartTransition(ILinkTraversalProvider traverser)
	{
		m_CurrentTraverserList.Add(traverser);
	}

	public void CompleteTransition(ILinkTraversalProvider traverser)
	{
		if (m_CurrentTraverserList.Contains(traverser))
		{
			this.OnTransitionCompleted?.Invoke(traverser);
			m_CurrentTraverserList.Remove(traverser);
		}
	}

	public bool ConnectsNodes(GraphNode from, GraphNode to)
	{
		if (from == m_StartNode && to == m_EndNode)
		{
			return IsConnected;
		}
		return false;
	}

	private void AddOffMeshLink()
	{
		if (m_LinkSource != null && (m_LinkSource.status == OffMeshLinks.OffMeshLinkStatus.Inactive || (m_LinkSource.status & OffMeshLinks.OffMeshLinkStatus.PendingRemoval) != 0))
		{
			m_LinkSource = null;
		}
		if (m_LinkSource == null && AstarPath.active != null && End != null)
		{
			StartTransform.hasChanged = false;
			EndTransform.hasChanged = false;
			m_LinkSource = new OffMeshLinks.OffMeshLinkSource
			{
				start = new OffMeshLinks.Anchor
				{
					center = m_StartNode.Vector3Position()
				},
				end = new OffMeshLinks.Anchor
				{
					center = m_EndNode.Vector3Position()
				},
				directionality = OffMeshLinks.Directionality.OneWay,
				costFactor = 1f,
				graphMask = GraphMask.everything,
				maxSnappingDistance = 0.2f,
				component = this,
				handler = m_OffMeshLinkHandler
			};
			AstarPath.active.offMeshLinks.Add(m_LinkSource);
		}
	}

	private void RemoveOffMeshLink()
	{
		if (AstarPath.active != null && m_LinkSource != null)
		{
			AstarPath.active.offMeshLinks.Remove(m_LinkSource);
		}
		m_LinkSource = null;
	}

	public void ApplyChangesIfNeeded(Vector3 startPosition, Vector3 endPosition)
	{
		if (!(StartTransform == null) && !(EndTransform == null) && (!(startPosition == m_StartPosition) || !(endPosition == m_EndPosition) || m_StartNode == null || m_EndNode == null))
		{
			UpdatePositions(startPosition, endPosition);
			ValidateNodeLink();
		}
	}

	private void UpdatePositions(Vector3 startPosition, Vector3 endPosition)
	{
		m_StartPosition = startPosition;
		m_EndPosition = endPosition;
		UpdateNodes(startPosition, endPosition);
	}

	private void UpdateNodes(Vector3 startPosition, Vector3 endPosition)
	{
		NNConstraint.Walkable.distanceMetric = DistanceMetric.Euclidean;
		GridNodeBase nearestNodeXZUnwalkable = ObstacleAnalyzer.GetNearestNodeXZUnwalkable(startPosition);
		GridNodeBase nearestNodeXZUnwalkable2 = ObstacleAnalyzer.GetNearestNodeXZUnwalkable(endPosition);
		m_StartNode = nearestNodeXZUnwalkable;
		m_EndNode = nearestNodeXZUnwalkable2;
	}

	private void ValidateNodeLink()
	{
		bool flag = m_StartNode.XCoordinateInGrid != m_EndNode.XCoordinateInGrid && m_StartNode.ZCoordinateInGrid != m_EndNode.ZCoordinateInGrid;
		IsValid = !flag && m_StartNode != m_EndNode;
	}
}
