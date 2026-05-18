using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Kingmaker.View;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class ForcedPath : Path, IMemoryPackable<ForcedPath>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ForcedPathFormatter : MemoryPackFormatter<ForcedPath>
	{
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref ForcedPath? value)
		{
			ForcedPath.Serialize(ref writer, ref value);
		}

		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref ForcedPath? value)
		{
			ForcedPath.Serialize(ref writer, ref value);
		}

		public override void Deserialize(ref MemoryPackReader reader, ref ForcedPath? value)
		{
			ForcedPath.Deserialize(ref reader, ref value);
		}
	}

	public static readonly ForcedPath ErrorPath = new ForcedPath
	{
		CompleteState = PathCompleteState.Error
	};

	public string UserTag { get; set; }

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ForcedPath>())
		{
			MemoryPackFormatterProvider.Register(new ForcedPathFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ForcedPath[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ForcedPath>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PathCompleteState>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<PathCompleteState>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<Vector3>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<Vector3>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<NodeIndexData>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<NodeIndexData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackJsonWriter writer, ref ForcedPath? value)
	{
		if (value == null)
		{
			writer.WriteNullObject();
			return;
		}
		List<NodeIndexData> value2 = PathToIndices(value.path);
		writer.WriteValue(value.CompleteState);
		writer.WriteValue(value.vectorPath);
		writer.WriteValue(value2);
		writer.WriteUnmanaged(value.pathRequestedTick);
		writer.WriteUnmanaged(value.persistentPath);
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref ForcedPath? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		List<NodeIndexData> value2 = PathToIndices(value.path);
		PathCompleteState value3 = value.CompleteState;
		writer.WriteUnmanagedWithObjectHeader(5, in value3);
		writer.WriteValue(in value.vectorPath);
		writer.WriteValue(in value2);
		writer.WriteUnmanaged(in value.pathRequestedTick, in value.persistentPath);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ForcedPath? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		byte b = 5;
		PathCompleteState value2;
		List<Vector3> value3;
		List<NodeIndexData> value4;
		long value5;
		bool value6;
		if (memberCount == b)
		{
			if (value != null)
			{
				value2 = value.CompleteState;
				value3 = value.vectorPath;
				value4 = new List<NodeIndexData>();
				value5 = value.pathRequestedTick;
				value6 = value.persistentPath;
				reader.ReadUnmanaged<PathCompleteState>(out value2);
				reader.ReadValue(ref value3);
				reader.ReadValue(ref value4);
				reader.ReadUnmanaged<long>(out value5);
				reader.ReadUnmanaged<bool>(out value6);
				goto IL_0132;
			}
			reader.ReadUnmanaged<PathCompleteState>(out value2);
			value3 = reader.ReadValue<List<Vector3>>();
			value4 = reader.ReadValue<List<NodeIndexData>>();
			reader.ReadUnmanaged<long, bool>(out value5, out value6);
		}
		else
		{
			if (memberCount > b)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ForcedPath), b, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = PathCompleteState.NotCalculated;
				value3 = null;
				value4 = null;
				value5 = 0L;
				value6 = false;
			}
			else
			{
				value2 = value.CompleteState;
				value3 = value.vectorPath;
				value4 = PathToIndices(value.path);
				value5 = value.pathRequestedTick;
				value6 = value.persistentPath;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<PathCompleteState>(out value2);
				if (memberCount != 1)
				{
					reader.ReadValue(ref value3);
					if (memberCount != 2)
					{
						reader.ReadValue(ref value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<long>(out value5);
							if (memberCount != 4)
							{
								reader.ReadUnmanaged<bool>(out value6);
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_0132;
			}
		}
		value = new ForcedPath
		{
			CompleteState = value2,
			vectorPath = value3,
			path = IndicesToPath(value4),
			pathRequestedTick = value5,
			persistentPath = value6
		};
		return;
		IL_0132:
		value.CompleteState = value2;
		value.vectorPath = value3;
		value.path = IndicesToPath(value4);
		value.pathRequestedTick = value5;
		value.persistentPath = value6;
	}

	private static List<NodeIndexData> PathToIndices(List<GraphNode> graphNodes)
	{
		List<NodeIndexData> list = new List<NodeIndexData>();
		for (int i = 0; i < graphNodes.Count; i++)
		{
			GraphNode graphNode = graphNodes[i];
			if (graphNode != null)
			{
				if (!(graphNode is GridNode gridNode))
				{
					if (graphNode is LinkNode linkNode)
					{
						NodeIndexData item = NodeIndexData.LinkNode(linkNode);
						list.Add(item);
					}
				}
				else
				{
					NodeIndexData item2 = NodeIndexData.GridNode(gridNode);
					list.Add(item2);
				}
			}
			else
			{
				list.Add(NodeIndexData.Null());
			}
		}
		return list;
	}

	private static List<GraphNode> IndicesToPath(List<NodeIndexData> indices)
	{
		List<GraphNode> result = new List<GraphNode>();
		if (AstarPath.active == null)
		{
			return result;
		}
		NavGraph[] graphs = AstarPath.active.data.graphs;
		for (int i = 0; i < indices.Count; i++)
		{
			NodeIndexData indexData = indices[i];
			TryFindNode(i, indexData, graphs, result);
		}
		return result;
	}

	private static void TryFindNode(int idx, NodeIndexData indexData, NavGraph[] graphs, List<GraphNode> result)
	{
		if (indexData.IsLink)
		{
			TryFindLinkNode(idx, graphs.OfType<LinkGraph>().First(), indexData, result);
		}
		else if (indexData.IsGrid)
		{
			TryFindGridNode(idx, indexData, graphs.OfType<GridGraph>().First(), result);
		}
		else
		{
			result.Add(null);
		}
	}

	private static void TryFindLinkNode(int idx, NavGraph graph, NodeIndexData indexData, List<GraphNode> result)
	{
		List<GraphNode> result = result;
		Vector2Int coords = indexData.CoordinatesInGrid;
		bool found = false;
		graph.GetNodes(delegate(GraphNode node)
		{
			if (!(node is LinkNode linkNode))
			{
				return true;
			}
			GridNode gridNode = (GridNode)linkNode.linkConcrete.startNodes[0];
			GridNode gridNode2 = (GridNode)linkNode.linkConcrete.endNodes[0];
			if (gridNode.CoordinatesInGrid != coords || gridNode2.CoordinatesInGrid != indexData.EndCoordinatesInGrid || linkNode.position != indexData.Position)
			{
				return true;
			}
			result.Add(node);
			found = true;
			return false;
		});
		_ = found;
	}

	private static void TryFindGridNode(int idx, NodeIndexData indexData, GridGraph graph, List<GraphNode> result)
	{
		List<GraphNode> result = result;
		Vector2Int coords = indexData.CoordinatesInGrid;
		bool found = false;
		if (!TryFindRequiredNode(graph.GetNode(coords.x, coords.y)))
		{
			graph.GetNodes((GraphNode node) => !TryFindRequiredNode(node));
		}
		_ = found;
		bool TryFindRequiredNode(GraphNode node)
		{
			if (!(node is GridNode gridNode) || gridNode.CoordinatesInGrid != coords || gridNode.position != indexData.Position)
			{
				return false;
			}
			result.Add(gridNode);
			found = true;
			return true;
		}
	}

	[Conditional("REPLAY_LOG_ENABLED")]
	private static void ReportNodeNotFound(NodeIndexData indexData)
	{
	}

	[Conditional("REPLAY_LOG_ENABLED")]
	private static void LogVectorPath(string prefix, List<Vector3> points)
	{
		for (int i = 0; i < points.Count; i++)
		{
		}
	}

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
