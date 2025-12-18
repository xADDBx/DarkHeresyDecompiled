using System.Collections.Generic;
using System.Linq;
using Kingmaker.View;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class ForcedPath : Path
{
	public static readonly ForcedPath ErrorPath = new ForcedPath
	{
		CompleteState = PathCompleteState.Error
	};

	public string UserTag { get; set; }

	[JsonConstructor]
	public ForcedPath()
	{
	}

	public static ForcedPath Construct(List<Vector3> points, bool createNodePath = false)
	{
		ForcedPath forcedPath = PathPool.GetPath<ForcedPath>();
		if (!createNodePath && forcedPath.path != null)
		{
			ListPool<GraphNode>.Release(forcedPath.path);
			forcedPath.path = null;
		}
		else if (createNodePath && forcedPath.path == null)
		{
			forcedPath.path = ListPool<GraphNode>.Claim();
		}
		if (forcedPath.vectorPath == null)
		{
			forcedPath.vectorPath = ListPool<Vector3>.Claim();
		}
		if (createNodePath)
		{
			for (int i = 0; i < points.Count; i++)
			{
				Vector3 vector = points[i];
				GridNode node = ObstacleAnalyzer.GetNearestNodeXZUnwalkable(vector);
				if (forcedPath.path.Count > 0)
				{
					List<GraphNode> list = forcedPath.path;
					GridNode gridNode = list[list.Count - 1] as GridNode;
					if (gridNode?.connections != null && gridNode.connections.FirstOrDefault((Connection l) => (l.node as LinkNode).linkConcrete.endNodes.Contains(node)).node is LinkNode linkNode)
					{
						List<Vector3> list2 = forcedPath.vectorPath;
						List<Vector3> list3 = forcedPath.vectorPath;
						list2.Add(list3[list3.Count - 1]);
						forcedPath.path.Add(linkNode);
						forcedPath.vectorPath.Add(vector);
						forcedPath.path.Add(linkNode.linkConcrete.endLinkNode);
					}
				}
				forcedPath.vectorPath.Add(vector);
				forcedPath.path.Add(node);
			}
		}
		else
		{
			forcedPath.vectorPath.AddRange(points);
		}
		forcedPath.pathRequestedTick = Game.Instance.Player.GameTime.Ticks;
		forcedPath.CompleteState = PathCompleteState.Complete;
		return forcedPath;
	}

	public static ForcedPath Construct(Path sourcePath)
	{
		if (sourcePath.error)
		{
			return ErrorPath;
		}
		ForcedPath forcedPath = PathPool.GetPath<ForcedPath>();
		forcedPath.vectorPath = ListPool<Vector3>.Claim();
		forcedPath.path = ListPool<GraphNode>.Claim();
		if (sourcePath.GetTotalLength() < float.PositiveInfinity)
		{
			forcedPath.vectorPath.AddRange(sourcePath.vectorPath);
			forcedPath.path.AddRange(sourcePath.path);
		}
		forcedPath.pathRequestedTick = sourcePath.pathRequestedTick;
		forcedPath.CompleteState = PathCompleteState.Complete;
		return forcedPath;
	}

	public static ForcedPath Construct(IEnumerable<GraphNode> nodes)
	{
		ForcedPath forcedPath = PathPool.GetPath<ForcedPath>();
		forcedPath.vectorPath = ListPool<Vector3>.Claim();
		forcedPath.path = ListPool<GraphNode>.Claim();
		forcedPath.vectorPath.AddRange(nodes.Select((GraphNode i) => i.Vector3Position()));
		forcedPath.path.AddRange(nodes);
		forcedPath.pathRequestedTick = Game.Instance.Player.GameTime.Ticks;
		forcedPath.CompleteState = PathCompleteState.Complete;
		return forcedPath;
	}

	public static ForcedPath Construct(IEnumerable<Vector3> points, IEnumerable<GraphNode> nodes)
	{
		ForcedPath forcedPath = PathPool.GetPath<ForcedPath>();
		forcedPath.vectorPath = ListPool<Vector3>.Claim();
		forcedPath.path = ListPool<GraphNode>.Claim();
		forcedPath.vectorPath.AddRange(points);
		forcedPath.path.AddRange(nodes);
		forcedPath.pathRequestedTick = Game.Instance.Player.GameTime.Ticks;
		forcedPath.CompleteState = PathCompleteState.Complete;
		return forcedPath;
	}

	protected override void OnHeapExhausted()
	{
	}

	protected override void OnFoundEndNode(uint pathNode, uint hScore, uint gScore)
	{
	}

	protected override void Prepare()
	{
	}

	protected override void CalculateStep(long targetTick)
	{
	}

	protected override void OnEnterPool()
	{
		base.OnEnterPool();
		if (vectorPath != null)
		{
			ListPool<Vector3>.Release(vectorPath);
			vectorPath = null;
		}
		if (path != null)
		{
			ListPool<GraphNode>.Release(path);
			path = null;
		}
		pathRequestedTick = 0L;
	}

	protected override void Reset()
	{
		if ((object)AstarPath.active != null)
		{
			base.Reset();
			return;
		}
		hasBeenReset = true;
		pathHandler = null;
		callback = null;
		immediateCallback = null;
		completeState = PathCompleteState.NotCalculated;
		duration = 0f;
		(nnConstraint as ConstraintWithRespectToTraversalProvider)?.Reset();
		enabledTags = -1;
		base.tagPenalties = null;
		hTargetNode = null;
		traversalProvider = null;
	}

	private bool TryGetLinkNodes(GridNode a, GridNode b, out LinkNode start, out LinkNode end)
	{
		start = null;
		end = null;
		if (a.connections == null)
		{
			return false;
		}
		Connection[] connections = a.connections;
		for (int i = 0; i < connections.Length; i++)
		{
			if (!(connections[i].node is LinkNode { connections: var connections2 } linkNode))
			{
				continue;
			}
			for (int j = 0; j < connections2.Length; j++)
			{
				if (!(connections2[j].node is LinkNode { connections: var connections3 } linkNode2))
				{
					continue;
				}
				for (int k = 0; k < connections3.Length; k++)
				{
					if (connections3[k].node == b)
					{
						start = linkNode;
						end = linkNode2;
						return true;
					}
				}
			}
		}
		return false;
	}

	public void Repair()
	{
		if ((path != null && path.Count == vectorPath.Count) || AstarPath.active?.data?.gridGraph == null || !AstarPath.active.data.gridGraph.isScanned)
		{
			return;
		}
		GridGraph gridGraph = AstarPath.active.data.gridGraph;
		path = new List<GraphNode>(vectorPath.Count);
		for (int i = 0; i < vectorPath.Count; i++)
		{
			Vector3 position = vectorPath[i];
			if (!(gridGraph.GetNearest(position).node is GridNode gridNode))
			{
				PFLog.Pathfinding.Error($"Coordinates ({position.x},{position.y},{position.z}) do not correspond to a node. ForcedPath from another location, or location changed too much.");
				break;
			}
			if (i == vectorPath.Count - 1)
			{
				path.Add(gridNode);
				continue;
			}
			if (!(gridGraph.GetNearest(vectorPath[i + 1]).node is GridNode b))
			{
				PFLog.Pathfinding.Error($"Coordinates ({position.x},{position.y},{position.z}) do not correspond to a node. ForcedPath from another location, or location changed too much.");
				break;
			}
			if (TryGetLinkNodes(gridNode, b, out var start, out var end))
			{
				path.Add(start);
				path.Add(end);
				i++;
			}
			else
			{
				path.Add(gridNode);
			}
		}
	}

	public ForcedPath Clone()
	{
		ForcedPath forcedPath = PathPool.GetPath<ForcedPath>();
		forcedPath.vectorPath = ((vectorPath != null) ? new List<Vector3>(vectorPath) : null);
		forcedPath.path = ((path != null) ? new List<GraphNode>(path) : null);
		forcedPath.UserTag = UserTag;
		forcedPath.CompleteState = base.CompleteState;
		forcedPath.pathRequestedTick = pathRequestedTick;
		forcedPath.duration = duration;
		forcedPath.searchedNodes = base.searchedNodes;
		forcedPath.enabledTags = enabledTags;
		forcedPath.heuristic = heuristic;
		forcedPath.heuristicScale = heuristicScale;
		forcedPath.tagPenalties = ((base.tagPenalties != null) ? ((int[])base.tagPenalties.Clone()) : null);
		return forcedPath;
	}
}
