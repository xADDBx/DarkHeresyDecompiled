using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public abstract class WarhammerPath<TIntermediateMetric, TFinalMetric> : Path, ILinkTraversePath, IPathBlockModeOwner
{
	private readonly struct PathNode
	{
		public readonly GraphNode Node;

		public readonly GraphNode Parent;

		public readonly TIntermediateMetric Length;

		public PathNode(GraphNode node, GraphNode parent, TIntermediateMetric length)
		{
			Node = node;
			Parent = parent;
			Length = length;
		}

		public override string ToString()
		{
			string arg = null;
			string arg2 = null;
			if (Parent == null)
			{
				arg = "(Start)";
			}
			else if (Parent is GridNodeBase gridNodeBase)
			{
				arg = $"({gridNodeBase.XCoordinateInGrid},{gridNodeBase.ZCoordinateInGrid})";
			}
			else if (Parent is LinkNode)
			{
				arg = "(LinkNode)";
			}
			if (Node is GridNodeBase gridNodeBase2)
			{
				arg2 = $"({gridNodeBase2.XCoordinateInGrid},{gridNodeBase2.ZCoordinateInGrid})";
			}
			else if (Node is LinkNode)
			{
				arg2 = "(LinkNode)";
			}
			return $"{arg} ---{Length}---> {arg2}";
		}
	}

	private Dictionary<GraphNode, TFinalMetric> m_AllNodes;

	private AbstractUnitEntity m_Unit;

	private Vector3 m_StartPoint;

	private float m_MaxLength;

	[CanBeNull]
	private GridNodeBase m_TargetNode;

	[CanBeNull]
	private MechanicEntity m_TargetEntity;

	private ITraversalCostProvider<TIntermediateMetric> m_TraversalCostProvider;

	private TFinalMetric[] m_Path;

	private BlockMode m_BlockMode;

	private bool m_PassThroughSmallUnits;

	private bool m_OneWayLinksAreForbidden;

	private int m_SearchedNodes;

	private PathNode m_Initial;

	private PathNode m_Current;

	private PriorityQueue<PathNode> m_OpenNodes;

	private readonly Dictionary<(uint graphIndex, uint nodeIndex), PathNode> m_ClosedNodes = new Dictionary<(uint, uint), PathNode>();

	private bool m_IsGatheringDeviationNodes;

	private HashSet<GridNodeBase> m_NodesInDeviationRange = new HashSet<GridNodeBase>();

	private PathNode m_PlacedNodeClosestToTarget;

	public ILinkTraversalProvider LinkTraversalProvider { get; set; }

	public BlockMode PathBlockMode => m_BlockMode;

	public bool PassThroughSmallUnits => m_PassThroughSmallUnits;

	public float MaxLength => m_MaxLength;

	[CanBeNull]
	public GridNodeBase TargetNode => m_TargetNode;

	[CanBeNull]
	public MechanicEntity TargetEntity => m_TargetEntity;

	public AbstractUnitEntity Unit => m_Unit;

	public TFinalMetric[] CalculatedPath => m_Path;

	public Dictionary<GraphNode, TFinalMetric> GetAllNodesAndReset()
	{
		Dictionary<GraphNode, TFinalMetric> allNodes = m_AllNodes;
		m_AllNodes = null;
		return allNodes;
	}

	public void OverrideBlockMode(BlockMode unitBlockMode)
	{
		m_BlockMode = unitBlockMode;
	}

	private new void FailWithError(string msg)
	{
		Error();
		PFLog.Default.Error("WarhammerPath: " + msg);
	}

	protected void Setup(AbstractUnitEntity unit, Vector3 start, float maxLength, [CanBeNull] GridNodeBase targetNode, [CanBeNull] MechanicEntity targetEntity, BlockMode blockMode, bool passThroughSmallUnits, TIntermediateMetric initialLength, ITraversalCostProvider<TIntermediateMetric> traversalCostProvider, bool oneWayLinksAreForbidden, OnPathDelegate pathComplete, bool isGatheringDeviationNodes = false)
	{
		callback = pathComplete;
		m_Unit = unit;
		m_StartPoint = start;
		m_MaxLength = maxLength;
		m_TargetNode = targetNode ?? targetEntity?.CurrentUnwalkableNode;
		m_TargetEntity = targetEntity;
		m_BlockMode = blockMode;
		m_PassThroughSmallUnits = passThroughSmallUnits;
		m_Initial = new PathNode(null, null, initialLength);
		m_TraversalCostProvider = traversalCostProvider;
		m_OneWayLinksAreForbidden = oneWayLinksAreForbidden;
		Comparer<PathNode> comparer = Comparer<PathNode>.Create((PathNode a, PathNode b) => Compare(in a.Length, in a.Node, in b.Length, in b.Node));
		m_OpenNodes = new PriorityQueue<PathNode>(comparer, EqualityComparer<PathNode>.Default);
		m_IsGatheringDeviationNodes = isGatheringDeviationNodes;
	}

	protected override void Reset()
	{
		base.Reset();
		m_AllNodes?.Clear();
		m_StartPoint = Vector3.zero;
		m_MaxLength = 0f;
		m_TargetNode = null;
		m_TargetEntity = null;
		heuristic = Heuristic.None;
		m_Initial = default(PathNode);
		m_OpenNodes = null;
		m_TraversalCostProvider = null;
		m_SearchedNodes = 0;
	}

	protected override void Prepare()
	{
		if (traversalProvider != null && !(traversalProvider is IWarhammerTraversalProvider))
		{
			FailWithError("Traversal Provider does not match the type WarhammerTraversalProvider");
			return;
		}
		nnConstraint.tags = enabledTags;
		GridNode gridNode = (GridNode)AstarPath.active.GetNearest(m_StartPoint).node;
		if (gridNode == null)
		{
			FailWithError("Could not find close node to the start point");
		}
		m_Initial = new PathNode(gridNode, null, m_Initial.Length);
		m_OpenNodes.Enqueue(m_Initial);
	}

	private TFinalMetric Convert(PathNode main)
	{
		ref readonly TIntermediateMetric length = ref main.Length;
		ref readonly GraphNode node = ref main.Node;
		ref readonly GraphNode parent = ref main.Parent;
		IWarhammerTraversalProvider warhammerTraversalProvider = traversalProvider as IWarhammerTraversalProvider;
		return Convert(in length, in node, in parent, in warhammerTraversalProvider);
	}

	protected override void Cleanup()
	{
		m_AllNodes = (from v in m_ClosedNodes.Values
			where !m_IsGatheringDeviationNodes || m_NodesInDeviationRange.Contains(v.Node)
			select (Node: v.Node, Metric: Convert(v))).ToDictionary(((GraphNode Node, TFinalMetric Metric) v) => v.Node, ((GraphNode Node, TFinalMetric Metric) v) => v.Metric);
		m_OpenNodes.Clear();
		m_ClosedNodes.Clear();
	}

	protected override void CalculateStep(long targetTick)
	{
		using (ProfileScope.NewScope("CalculateStep"))
		{
			int num = 0;
			do
			{
				if (m_OpenNodes.Count == 0)
				{
					PathNode node = ((m_PlacedNodeClosestToTarget.Node == null) ? m_Current : m_PlacedNodeClosestToTarget);
					TraceBack(in node);
					base.CompleteState = PathCompleteState.Partial;
					return;
				}
				m_Current = m_OpenNodes.Dequeue();
				if (PlaceNode(m_Current))
				{
					if (m_Current.Node is GridNodeBase && IsTargetNode(in m_Current.Length, in m_Current.Node))
					{
						if (!m_IsGatheringDeviationNodes)
						{
							TraceBack(in m_Current);
							base.CompleteState = PathCompleteState.Complete;
							return;
						}
						GatherDeviationNodes(m_Current);
					}
					Open(in m_Current);
				}
				if (num++ > 500)
				{
					if (DateTime.UtcNow.Ticks >= targetTick)
					{
						return;
					}
					num = 0;
				}
			}
			while (m_SearchedNodes++ <= 1000000);
			throw new Exception("Probable infinite loop. Over 1,000,000 nodes searched");
		}
	}

	private void TraceBack(in PathNode node)
	{
		List<PathNode> list = new List<PathNode>();
		PathNode item = node;
		while (item.Node != null)
		{
			list.Add(item);
			item = ((item.Parent != null) ? m_ClosedNodes[(item.Parent.GraphIndex, item.Parent.NodeIndex)] : default(PathNode));
		}
		list.Reverse();
		path = list.Select((PathNode v) => v.Node).ToList();
		vectorPath = list.Select((PathNode v) => v.Node.Vector3Position()).ToList();
		m_Path = list.Select(delegate(PathNode v)
		{
			ref readonly TIntermediateMetric length = ref v.Length;
			ref readonly GraphNode node2 = ref v.Node;
			ref readonly GraphNode parent = ref v.Parent;
			IWarhammerTraversalProvider warhammerTraversalProvider = (IWarhammerTraversalProvider)traversalProvider;
			return Convert(in length, in node2, in parent, in warhammerTraversalProvider);
		}).ToArray();
	}

	private void GatherDeviationNodes(PathNode node)
	{
		PathNode pathNode = node;
		while (pathNode.Node != null)
		{
			if (pathNode.Node is GridNodeBase item)
			{
				m_NodesInDeviationRange.Add(item);
			}
			pathNode = ((pathNode.Parent != null) ? m_ClosedNodes[(pathNode.Parent.GraphIndex, pathNode.Parent.NodeIndex)] : default(PathNode));
		}
	}

	private bool PlaceNode(PathNode newNode)
	{
		if (!m_ClosedNodes.TryGetValue((newNode.Node.GraphIndex, newNode.Node.NodeIndex), out var value))
		{
			m_ClosedNodes[(newNode.Node.GraphIndex, newNode.Node.NodeIndex)] = newNode;
			UpdateClosestToTargetNode(newNode);
			return true;
		}
		if (Compare(in newNode.Length, in newNode.Node, in value.Length, in value.Node) >= 0)
		{
			return false;
		}
		if (newNode.Parent == value.Parent)
		{
			throw new Exception("infinite loop in pathfinding");
		}
		using (ProfileScope.New("InvalidateRecursive"))
		{
			InvalidateRecursive(newNode.Node);
		}
		m_ClosedNodes[(newNode.Node.GraphIndex, newNode.Node.NodeIndex)] = newNode;
		UpdateClosestToTargetNode(newNode);
		return true;
	}

	private void Open(in PathNode pathNode)
	{
		using (ProfileScope.NewScope("Open"))
		{
			GraphNode node = pathNode.Node;
			if (node is LinkNode linkNode)
			{
				if (LinkTraversalProvider == null || linkNode.connections == null || (m_OneWayLinksAreForbidden && linkNode.linkSource.component is WarhammerOneWayNodeLink))
				{
					return;
				}
				bool flag = pathNode.Parent != null && pathNode.Parent.GraphIndex == linkNode.GraphIndex && linkNode.linkSource.component == (pathNode.Parent as LinkNode).linkSource.component;
				for (int i = 0; i < linkNode.connections.Length; i++)
				{
					GraphNode node2 = linkNode.connections[i].node;
					if (flag != (node2.GraphIndex != linkNode.GraphIndex))
					{
						continue;
					}
					if (node2 is LinkNode newNode)
					{
						ProcessOtherNode(newNode, in pathNode);
					}
					else
					{
						if (!(node2 is GridNodeBase linkEndNode))
						{
							continue;
						}
						foreach (GridNodeBase item in GraphHelper.TryGetAllSuitableNodesForUnitAfterLinkTraversal(linkEndNode, pathNode.Parent, Unit))
						{
							if (item != null && CanTraverse(item))
							{
								ProcessOtherNode(item, in pathNode);
							}
						}
					}
				}
				return;
			}
			GridNodeBase gridNodeBase = node as GridNodeBase;
			for (int j = 0; j < 8; j++)
			{
				GridNodeBase neighbourAlongDirection = gridNodeBase.GetNeighbourAlongDirection(j);
				if (neighbourAlongDirection != null && ((IWarhammerTraversalProvider)traversalProvider).CanTraverseAlongDirection(this, gridNodeBase, j))
				{
					ProcessOtherNode(neighbourAlongDirection, in pathNode);
				}
			}
			if (LinkTraversalProvider == null)
			{
				return;
			}
			IntRect sizeRect = Unit.SizeRect;
			foreach (GridNodeBase node3 in GridAreaHelper.GetNodes(gridNodeBase, sizeRect))
			{
				Connection[] connections = node3.connections;
				if (connections == null || connections.Length <= 0)
				{
					continue;
				}
				connections = node3.connections;
				for (int k = 0; k < connections.Length; k++)
				{
					Connection connection = connections[k];
					bool flag2 = false;
					LinkNode linkNode2 = connection.node as LinkNode;
					GridNodeBase to = ((linkNode2.linkConcrete.startNodes[0] == node3) ? linkNode2.linkConcrete.endNodes[0] : linkNode2.linkConcrete.startNodes[0]) as GridNodeBase;
					if ((linkNode2.linkConcrete.startNodes[0] == node3 || linkNode2.linkConcrete.endNodes[0] == node3) && ((node3.XCoordinateInGrid != gridNodeBase.XCoordinateInGrid && node3.ZCoordinateInGrid != gridNodeBase.ZCoordinateInGrid) || !m_ClosedNodes.ContainsKey((linkNode2.GraphIndex, linkNode2.NodeIndex))))
					{
						IWarhammerNodeLink link = linkNode2.linkSource.component as IWarhammerNodeLink;
						if (LinkTraversalProvider.CanBuildPathThroughLink(node3, to, link))
						{
							flag2 = true;
						}
					}
					if (flag2)
					{
						ProcessOtherNode(linkNode2, in pathNode);
					}
				}
			}
		}
	}

	private void ProcessOtherNode(GraphNode newNode, in PathNode parentNode)
	{
		if (!m_ClosedNodes.ContainsKey((newNode.GraphIndex, newNode.NodeIndex)))
		{
			TIntermediateMetric node = m_TraversalCostProvider.Calc(in parentNode.Length, in parentNode.Node, in newNode);
			if (IsWithinRange(in node))
			{
				m_OpenNodes.Enqueue(new PathNode(newNode, parentNode.Node, node));
			}
		}
	}

	private void ProcessOtherNode(LinkNode newNode, in PathNode parentNode)
	{
		if (!m_ClosedNodes.ContainsKey((newNode.GraphIndex, newNode.NodeIndex)))
		{
			ITraversalCostProvider<TIntermediateMetric> traversalCostProvider = m_TraversalCostProvider;
			ref readonly TIntermediateMetric length = ref parentNode.Length;
			ref readonly GraphNode node = ref parentNode.Node;
			GraphNode to = newNode;
			TIntermediateMetric node2 = traversalCostProvider.Calc(in length, in node, in to);
			if (IsWithinRange(in node2))
			{
				m_OpenNodes.Enqueue(new PathNode(newNode, parentNode.Node, node2));
			}
		}
	}

	private void InvalidateRecursive(GraphNode someNode)
	{
		m_ClosedNodes.Remove((someNode.GraphIndex, someNode.NodeIndex));
		while (true)
		{
			int num = m_OpenNodes.FindIndex((PathNode v) => v.Parent == someNode);
			if (num == -1)
			{
				break;
			}
			m_OpenNodes.RemoveAt(num);
		}
		if (someNode is GridNodeBase gridNodeBase)
		{
			for (int i = 0; i < 8; i++)
			{
				GridNodeBase neighbourAlongDirection = gridNodeBase.GetNeighbourAlongDirection(i);
				if (neighbourAlongDirection != null && m_ClosedNodes.TryGetValue((neighbourAlongDirection.GraphIndex, neighbourAlongDirection.NodeIndex), out var value) && value.Parent == gridNodeBase)
				{
					InvalidateRecursive(neighbourAlongDirection);
				}
			}
		}
		if (LinkTraversalProvider == null)
		{
			return;
		}
		Connection[] array = ((someNode is LinkNode linkNode) ? linkNode.connections : (someNode as GridNodeBase).connections);
		if (array == null || array.Length <= 0)
		{
			return;
		}
		Connection[] array2 = array;
		for (int j = 0; j < array2.Length; j++)
		{
			if (array2[j].node is GridNodeBase gridNodeBase2 && m_ClosedNodes.TryGetValue((gridNodeBase2.GraphIndex, gridNodeBase2.NodeIndex), out var value2) && value2.Parent == someNode)
			{
				InvalidateRecursive(gridNodeBase2);
			}
		}
	}

	private void UpdateClosestToTargetNode(PathNode newPathNode)
	{
		GraphNode node = m_PlacedNodeClosestToTarget.Node;
		if (node == null || !m_ClosedNodes.ContainsKey((node.GraphIndex, node.NodeIndex)))
		{
			m_PlacedNodeClosestToTarget = newPathNode;
		}
		else if (ClosestToTarget(node, newPathNode.Node) == newPathNode.Node)
		{
			m_PlacedNodeClosestToTarget = newPathNode;
		}
	}

	protected abstract bool IsWithinRange(in TIntermediateMetric node);

	protected abstract bool IsTargetNode(in TIntermediateMetric distance, in GraphNode node);

	protected abstract GraphNode ClosestToTarget(GraphNode node1, GraphNode node2);

	protected abstract int Compare(in TIntermediateMetric lengthA, in GraphNode nodeA, in TIntermediateMetric lengthB, in GraphNode nodeB);

	protected abstract TFinalMetric Convert(in TIntermediateMetric distance, in GraphNode node, in GraphNode parentNode, in IWarhammerTraversalProvider traversalProvider);
}
