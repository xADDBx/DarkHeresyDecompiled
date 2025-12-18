using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.View;
using Pathfinding;
using Pathfinding.Drawing;
using Pathfinding.Util;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class WarhammerNodeLink : NodeLink2, IWarhammerNodeLink
{
	public const float LowLedgeMaxHeight = 2.3f;

	public const float HighLedgeMaxHeight = 3.2f;

	private const float MaxSnappingDistance = 0.3f;

	private GridNodeBase m_StartNode;

	private GridNodeBase m_EndNode;

	private TraverseType m_TraverseType;

	[SerializeField]
	private Bounds m_Bounds;

	private readonly List<ILinkTraversalProvider> m_CurrentTraverserList = new List<ILinkTraversalProvider>();

	public static IEnumerable<WarhammerNodeLink> All => GraphModifier.usedIDs.Values.OfType<WarhammerNodeLink>();

	public float CostFactor => costFactor;

	public GraphNode StartNode => m_StartNode;

	public GraphNode EndNode => m_EndNode;

	private Vector3 StartNodePosition => m_StartNode.Vector3Position();

	private Vector3 EndNodePosition => m_EndNode.Vector3Position();

	public TraverseType TraverseType => m_TraverseType;

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

	public Vector3 EndToStartDirection { get; private set; }

	public Bounds Bounds => m_Bounds;

	public int UnitsWithActivePathTroughLink { get; set; }

	public event IWarhammerNodeLink.OnTransitionCompletedDel OnTransitionCompleted;

	public static TraverseType GetTraverseType(GraphNode from, GraphNode to)
	{
		float num = Math.Abs(from.Vector3Position().y - to.Vector3Position().y);
		if (!(num < 0.3f))
		{
			if (!(num > 3.2f))
			{
				return TraverseType.Ledge;
			}
			return TraverseType.Ladder;
		}
		return TraverseType.Leap;
	}

	public override void OnPostScan()
	{
		Apply();
	}

	public override void OnGraphsPostUpdateBeforeAreaRecalculation()
	{
		Apply();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying && !BatchedEvents.Has(this))
		{
			BatchedEvents.Remove(this);
		}
	}

	public override void OnPostCacheLoad()
	{
		base.OnPostCacheLoad();
		Apply();
	}

	public override void Apply()
	{
		UpdatePositions();
		CalculateTraverseOffset();
		ValidateNodeLink();
		RemoveLink();
		TryAddLink();
	}

	private void UpdatePositions()
	{
		if ((AstarPath.active?.data?.graphs.Length).GetValueOrDefault() != 0)
		{
			if (base.StartTransform == null || base.EndTransform == null)
			{
				PFLog.Default.Error("WarhammerNodeLink is not setup properly or doesn't belong to mechanic scene");
				return;
			}
			Vector3 position = base.StartTransform.position;
			Vector3 position2 = base.EndTransform.position;
			NNConstraint.Walkable.distanceMetric = DistanceMetric.Euclidean;
			GridNodeBase nearestNodeXZUnwalkable = ObstacleAnalyzer.GetNearestNodeXZUnwalkable(position);
			GridNodeBase nearestNodeXZUnwalkable2 = ObstacleAnalyzer.GetNearestNodeXZUnwalkable(position2);
			m_StartNode = nearestNodeXZUnwalkable;
			m_EndNode = nearestNodeXZUnwalkable2;
			m_TraverseType = GetTraverseType(m_StartNode, m_EndNode);
		}
	}

	private void RemoveLink()
	{
		if (AstarPath.active != null && linkSource != null)
		{
			AstarPath.active.offMeshLinks.Remove(linkSource);
		}
		linkSource = null;
	}

	private void TryAddLink()
	{
		if (linkSource != null && (linkSource.status == OffMeshLinks.OffMeshLinkStatus.Inactive || (linkSource.status & OffMeshLinks.OffMeshLinkStatus.PendingRemoval) != 0))
		{
			linkSource = null;
		}
		if (linkSource == null && AstarPath.active != null && base.EndTransform != null)
		{
			base.StartTransform.hasChanged = false;
			base.EndTransform.hasChanged = false;
			linkSource = new OffMeshLinks.OffMeshLinkSource
			{
				start = new OffMeshLinks.Anchor
				{
					center = StartNodePosition,
					rotation = base.StartTransform.rotation,
					width = 0f
				},
				end = new OffMeshLinks.Anchor
				{
					center = EndNodePosition,
					rotation = base.EndTransform.rotation,
					width = 0f
				},
				directionality = ((!oneWay) ? OffMeshLinks.Directionality.TwoWay : OffMeshLinks.Directionality.OneWay),
				tag = pathfindingTag,
				costFactor = costFactor,
				graphMask = graphMask,
				maxSnappingDistance = ((IsValid && IsConnected) ? 0.3f : 0f),
				component = this,
				handler = base.onTraverseOffMeshLink
			};
			AstarPath.active.offMeshLinks.Add(linkSource);
		}
	}

	private void CalculateTraverseOffset()
	{
		if (m_StartNode != null && m_EndNode != null)
		{
			Vector3 vector = StartNodePosition - EndNodePosition;
			vector.y = 0f;
			EndToStartDirection = vector.normalized;
		}
	}

	public bool CanStartTraverse(ILinkTraversalProvider traverser)
	{
		if (IsValid)
		{
			return CanUseTraverserLink;
		}
		return false;
	}

	public bool IsInTraverse()
	{
		return m_CurrentTraverserList.Count > 0;
	}

	public bool IsInTraverse(ILinkTraversalProvider traverser)
	{
		return m_CurrentTraverserList.Contains(traverser);
	}

	public void StartTransition(ILinkTraversalProvider traverser)
	{
		m_CurrentTraverserList.Add(traverser);
	}

	public bool ConnectsNodes(GraphNode from, GraphNode to)
	{
		if (from == StartNode)
		{
			return to == EndNode;
		}
		if (from == EndNode)
		{
			return to == StartNode;
		}
		return false;
	}

	public void CompleteTransition(ILinkTraversalProvider traverser)
	{
		if (m_CurrentTraverserList.Contains(traverser))
		{
			this.OnTransitionCompleted?.Invoke(traverser);
			m_CurrentTraverserList.Remove(traverser);
		}
	}

	private void ValidateNodeLink()
	{
		if (AstarPath.active?.data?.gridGraph == null)
		{
			return;
		}
		IsValid = false;
		if (m_StartNode.XCoordinateInGrid == m_EndNode.XCoordinateInGrid || m_StartNode.ZCoordinateInGrid == m_EndNode.ZCoordinateInGrid)
		{
			int num = m_StartNode.CellDistanceTo(m_EndNode);
			bool isValid;
			switch (TraverseType)
			{
			case TraverseType.Ledge:
			case TraverseType.Ladder:
				isValid = num == 1;
				break;
			case TraverseType.Leap:
				isValid = num == 2 || num == 3;
				break;
			default:
				isValid = IsValid;
				break;
			}
			IsValid = isValid;
			IsValid = IsValid && m_StartNode != m_EndNode;
		}
	}

	public override void DrawGizmos()
	{
		if (!(base.StartTransform == null) && !(base.EndTransform == null))
		{
			Vector3 position = base.StartTransform.position;
			Vector3 position2 = base.EndTransform.position;
			if (linkSource != null && Time.renderedFrameCount % 16 == 0 && Application.isEditor && (linkSource.start.center != position || linkSource.end.center != position2 || linkSource.directionality != ((!oneWay) ? OffMeshLinks.Directionality.TwoWay : OffMeshLinks.Directionality.OneWay) || linkSource.costFactor != costFactor || (int)linkSource.graphMask != (int)graphMask || (uint)linkSource.tag != (uint)pathfindingTag))
			{
				Apply();
			}
			Color color = ((!IsValid) ? Color.red : (IsInTraverse() ? Color.yellow : Color.green));
			Vector3 vector = (IsValid ? StartNode.Vector3Position() : base.StartTransform.position);
			Vector3 vector2 = (IsValid ? EndNode.Vector3Position() : base.EndTransform.position);
			Draw.xz.Circle(vector, 0.4f, color);
			Draw.xz.Circle(vector2, 0.4f, color);
		}
	}
}
