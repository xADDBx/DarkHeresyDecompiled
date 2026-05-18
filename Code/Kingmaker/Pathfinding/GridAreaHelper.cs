using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using Unity.Mathematics;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public static class GridAreaHelper
{
	private readonly struct OffsetsKey : IEquatable<OffsetsKey>
	{
		public readonly IntRect Rect;

		public readonly int Direction;

		public OffsetsKey(IntRect rect, int direction)
		{
			Rect = rect;
			Direction = direction;
		}

		public bool Equals(OffsetsKey other)
		{
			if (Rect == other.Rect)
			{
				return Direction == other.Direction;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is OffsetsKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Rect, Direction);
		}
	}

	private static readonly StaticCache<OffsetsKey, PatternGridData> NodeOffsets = new StaticCache<OffsetsKey, PatternGridData>(CalcOffsets);

	private static readonly StaticCache<OffsetsKey, PatternGridData> NodeBorderOffsets = new StaticCache<OffsetsKey, PatternGridData>(CalcBorderOffsets);

	private static readonly ThreadLocal<HashSet<Vector2Int>> RectCache = new ThreadLocal<HashSet<Vector2Int>>(() => new HashSet<Vector2Int>());

	public static int2 GetNearestNodeCoords(this Vector3 position)
	{
		return ObstacleAnalyzer.GetNearestNodeCoords(position);
	}

	[CanBeNull]
	public static GridNodeBase GetNearestNodeXZ(this Vector3 position)
	{
		return (GridNodeBase)ObstacleAnalyzer.GetNearestNode(position, null, ObstacleAnalyzer.DefaultXZConstraint).node;
	}

	[CanBeNull]
	public static GridNodeBase GetNearestNodeXZUnwalkable(this Vector3 position)
	{
		return ObstacleAnalyzer.GetNearestNodeXZUnwalkable(position);
	}

	public static int2 GetNearestNodeCoords(this MechanicEntity entity)
	{
		return entity.Position.GetNearestNodeCoords();
	}

	[CanBeNull]
	public static GridNodeBase GetNearestNodeXZ(this MechanicEntity entity)
	{
		return entity.Position.GetNearestNodeXZ();
	}

	public static GridNodeBase GetInnerNodeNearestToTarget(this MechanicEntity unit, Vector3 targetPosition)
	{
		return GetInnerNodeNearestToTarget(unit.GetOccupiedNodes(), targetPosition);
	}

	public static GridNodeBase GetInnerNodeNearestToTarget(this MechanicEntity unit, GridNodeBase origin, Vector3 targetPosition)
	{
		return GetInnerNodeNearestToTarget(GetNodes(origin, unit.SizeRect), targetPosition);
	}

	public static GridNodeBase GetOuterNodeNearestToTarget(this MechanicEntity unit, Vector3 targetPosition)
	{
		return unit.GetOuterNodeNearestToTarget(unit.CurrentUnwalkableNode, targetPosition);
	}

	public static GridNodeBase GetOuterNodeNearestToTarget(this MechanicEntity unit, GridNodeBase origin, Vector3 targetPosition)
	{
		GridNodeBase innerNodeNearestToTarget = GetInnerNodeNearestToTarget(GetNodes(origin, unit.SizeRect), targetPosition);
		Vector3 vector = targetPosition - innerNodeNearestToTarget.Vector3Position();
		vector.y = 0f;
		vector = vector.normalized;
		return (innerNodeNearestToTarget.Vector3Position() + vector * 1.Cells().Meters).GetNearestNodeXZUnwalkable();
	}

	public static IEnumerable<GridNodeBase> GetSortedNodesByDistanceToTarget(this MechanicEntity unit, GridNodeBase origin, Vector3 targetPosition)
	{
		return from node in GetNodes(origin, unit.SizeRect)
			orderby (targetPosition - node.Vector3Position()).magnitude
			select node;
	}

	private static GridNodeBase GetInnerNodeNearestToTarget(NodeList nodes, Vector3 target)
	{
		float num = float.MaxValue;
		GridNodeBase result = null;
		foreach (GridNodeBase item in nodes)
		{
			float sqrMagnitude = (item.Vector3Position() - target).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = item;
			}
		}
		return result;
	}

	public static bool IsUnitPositionContainsNode(this MechanicEntity unit, Vector3 desiredPosition, GridNodeBase targetMode)
	{
		return GetNodes(desiredPosition, unit.SizeRect).Contains(targetMode);
	}

	public static NodeList GetNodes(Vector3 position, IntRect rect)
	{
		GridNodeBase nearestNodeXZUnwalkable = position.GetNearestNodeXZUnwalkable();
		if (nearestNodeXZUnwalkable != null)
		{
			return GetNodes(nearestNodeXZUnwalkable, rect);
		}
		return NodeList.Empty;
	}

	public static NodeList GetNodes(Vector3 position, IntRect rect, Vector3 forward)
	{
		GridNodeBase nearestNodeXZUnwalkable = position.GetNearestNodeXZUnwalkable();
		if (nearestNodeXZUnwalkable == null || !nearestNodeXZUnwalkable.ContainsPoint(position))
		{
			return NodeList.Empty;
		}
		return GetNodes(nearestNodeXZUnwalkable, rect, forward);
	}

	public static NodeList GetNodes(GraphNode node, IntRect rect)
	{
		return GetNodes(node, rect, 0);
	}

	public static NodeList GetNodes(GraphNode node, IntRect rect, Vector3 forward)
	{
		return GetNodes(node, rect, GraphHelper.GuessDirection(forward));
	}

	private static PatternGridData GetOffset(OffsetsKey key)
	{
		return NodeOffsets.Get(key);
	}

	private static HashSet<Vector2Int> CalculateArea(IntRect rect, int direction)
	{
		HashSet<Vector2Int> value = RectCache.Value;
		for (int i = rect.xmin; i <= rect.xmax; i++)
		{
			for (int j = rect.ymin; j <= rect.ymax; j++)
			{
				value.Add(new Vector2Int(i, j));
			}
		}
		return value;
	}

	private static HashSet<Vector2Int> CalculateBorderArea(IntRect rect, int sideDirection)
	{
		HashSet<Vector2Int> value = RectCache.Value;
		IntRect intRect = rect;
		intRect.ymax = intRect.ymin + intRect.Width - 1;
		Vector2Int vector2Int = new Vector2Int(0, 0);
		int num = rect.Height - rect.Width;
		int num2 = 0;
		int num3 = 1;
		for (int i = 0; i <= num; i++)
		{
			value.Add(vector2Int + new Vector2Int(intRect.xmin, intRect.ymin));
			value.Add(vector2Int + new Vector2Int(intRect.xmin, intRect.ymax));
			value.Add(vector2Int + new Vector2Int(intRect.xmax, intRect.ymin));
			value.Add(vector2Int + new Vector2Int(intRect.xmax, intRect.ymax));
			if (sideDirection == 7 || sideDirection == 3 || sideDirection == 6 || sideDirection == -1)
			{
				int xmin = intRect.xmin;
				for (int j = intRect.ymin + 1; j < intRect.ymax; j++)
				{
					value.Add(vector2Int + new Vector2Int(xmin, j));
				}
			}
			if (sideDirection == 5 || sideDirection == 1 || sideDirection == 4 || sideDirection == -1)
			{
				int xmin = intRect.xmax;
				for (int j = intRect.ymin + 1; j < intRect.ymax; j++)
				{
					value.Add(vector2Int + new Vector2Int(xmin, j));
				}
			}
			for (int xmin = intRect.xmin + 1; xmin <= intRect.xmax - 1; xmin++)
			{
				switch (sideDirection)
				{
				case -1:
				case 0:
				case 4:
				case 7:
					value.Add(vector2Int + new Vector2Int(xmin, intRect.ymin));
					break;
				case 2:
				case 5:
				case 6:
					value.Add(vector2Int + new Vector2Int(xmin, intRect.ymax));
					break;
				}
			}
			vector2Int.x += num2;
			vector2Int.y += num3;
		}
		return value;
	}

	public static IntRect ShiftRectByDirection(this IntRect originRect, int direction)
	{
		int num;
		switch (direction)
		{
		case 1:
		case 4:
		case 5:
			num = -1;
			break;
		case 0:
		case 2:
			num = 0;
			break;
		default:
			num = 1;
			break;
		}
		int x = num;
		switch (direction)
		{
		case 2:
		case 5:
		case 6:
			num = -1;
			break;
		case 1:
		case 3:
			num = 0;
			break;
		default:
			num = 1;
			break;
		}
		int y = num;
		return originRect.Move(new Vector2Int(x, y));
	}

	public static IntRect Move(this IntRect rect, Vector2Int shift)
	{
		return new IntRect(rect.xmin + shift.x, rect.ymin + shift.y, rect.xmax + shift.x, rect.ymax + shift.y);
	}

	private static PatternGridData CalcOffsets(OffsetsKey key)
	{
		try
		{
			IntRect rect = key.Rect;
			int direction = key.Direction;
			HashSet<Vector2Int> hashSet = CalculateArea(rect, direction);
			return (hashSet.Count == 0) ? PatternGridData.Empty : PatternGridData.Create(hashSet, disposable: false);
		}
		finally
		{
			RectCache.Value.Clear();
		}
	}

	private static PatternGridData CalcBorderOffsets(OffsetsKey key)
	{
		try
		{
			IntRect rect = key.Rect;
			int direction = key.Direction;
			HashSet<Vector2Int> hashSet = ((rect.Width != rect.Height) ? ((direction > 3) ? GetNodesDiagonalAngles(rect, 0) : GetNodesOrthogonalAngles(rect, 0)) : CalculateBorderArea(rect, direction));
			return (hashSet.Count == 0) ? PatternGridData.Empty : PatternGridData.Create(hashSet, disposable: false);
		}
		finally
		{
			RectCache.Value.Clear();
		}
	}

	public static NodeList GetNodes(GraphNode node, IntRect rect, int direction)
	{
		if (!(node?.Graph is GridGraph graph) || !(node is GridNodeBase gridNodeBase))
		{
			return NodeList.Empty;
		}
		PatternGridData offset = GetOffset(new OffsetsKey(rect, direction));
		PatternGridData pattern = offset.Move(gridNodeBase.CoordinatesInGrid);
		return new NodeList(graph, in pattern);
	}

	public static NodeList GetNodes(IEnumerable<GridNodeBase> nodes)
	{
		if (!(nodes.First()?.Graph is GridGraph graph))
		{
			return NodeList.Empty;
		}
		PatternGridData pattern = GridPatterns.ConstructPattern(nodes);
		return new NodeList(graph, in pattern);
	}

	public static NodeList GetBorderNodes(GraphNode node, IntRect rect)
	{
		if (!(node?.Graph is GridGraph graph) || !(node is GridNodeBase gridNodeBase))
		{
			return NodeList.Empty;
		}
		PatternGridData patternGridData = NodeBorderOffsets.Get(new OffsetsKey(rect, -1));
		PatternGridData pattern = patternGridData.Move(gridNodeBase.CoordinatesInGrid);
		return new NodeList(graph, in pattern);
	}

	public static NodeList GetBorderNodes(GraphNode node, IntRect rect, int sideDirection)
	{
		if (!(node?.Graph is GridGraph graph) || !(node is GridNodeBase gridNodeBase))
		{
			return NodeList.Empty;
		}
		PatternGridData patternGridData = NodeBorderOffsets.Get(new OffsetsKey(rect, sideDirection));
		PatternGridData pattern = patternGridData.Move(gridNodeBase.CoordinatesInGrid);
		return new NodeList(graph, in pattern);
	}

	private static HashSet<Vector2Int> GetNodesOrthogonalAngles(IntRect rect, int direction)
	{
		HashSet<Vector2Int> value = RectCache.Value;
		int num = rect.Height / 2 - 1;
		int num2 = 0;
		int num3 = -1;
		for (int i = 0; i < rect.Width; i++)
		{
			for (int j = 0; j < rect.Height; j++)
			{
				Vector2Int item = RotateNode(direction, num2 * num3, num - j);
				value.Add(item);
			}
			if (num3 < 0)
			{
				num2++;
			}
			num3 *= -1;
		}
		return value;
	}

	private static HashSet<Vector2Int> GetNodesDiagonalAngles(IntRect rect, int direction)
	{
		HashSet<Vector2Int> value = RectCache.Value;
		int num = rect.Height / 2 - 1;
		int num2 = num;
		for (int i = 0; i < rect.Width; i++)
		{
			for (int j = 0; j < rect.Height - i; j++)
			{
				Vector2Int item = RotateNode(direction, num2 - j, num - j);
				value.Add(item);
				if (i != 0)
				{
					value.Add((direction == 5 || direction == 7) ? new Vector2Int(item.y, item.x) : new Vector2Int(-item.y, -item.x));
				}
			}
			num--;
		}
		return value;
	}

	private static Vector2Int RotateNode(int direction, int xIndex, int yIndex)
	{
		return direction switch
		{
			0 => new Vector2Int(-xIndex, -yIndex), 
			1 => new Vector2Int(yIndex, -xIndex), 
			2 => new Vector2Int(xIndex, yIndex), 
			3 => new Vector2Int(-yIndex, xIndex), 
			4 => new Vector2Int(xIndex, -yIndex), 
			5 => new Vector2Int(xIndex, yIndex), 
			6 => new Vector2Int(-xIndex, yIndex), 
			7 => new Vector2Int(-xIndex, -yIndex), 
			_ => throw new Exception($"Unknown angle met with direction {direction}"), 
		};
	}

	[ItemNotNull]
	public static NodeList GetOccupiedNodes(this MechanicEntity unit, GraphNode node)
	{
		IntRect rect = ((!unit.IsInCombat && unit.IsInPlayerParty) ? new IntRect(0, 0, 0, 0) : unit.SizeRect);
		return GetNodes(node, rect);
	}

	[ItemNotNull]
	public static NodeList GetOccupiedNodes(this MechanicEntity unit)
	{
		IntRect rect = ((!unit.IsInCombat && unit.IsInPlayerParty) ? new IntRect(0, 0, 0, 0) : unit.SizeRect);
		return GetNodes(unit.Position, rect, unit.Forward);
	}

	[ItemNotNull]
	public static NodeList GetOccupiedNodes(this MechanicEntity unit, Vector3 position)
	{
		return GetNodes(position, unit.SizeRect, unit.Forward);
	}

	[ItemNotNull]
	public static NodeList GetOccupiedNodes(GridNodeBase node, IntRect size)
	{
		return GetNodes(node, size, Vector3.forward);
	}

	public static IEnumerable<GridNodeBase> GetNodesSpiralAround(GridNodeBase node, IntRect rect, int radius)
	{
		if (radius < 1)
		{
			yield break;
		}
		GridGraph graph = (GridGraph)node.Graph;
		int currentRadius = 1;
		int x = rect.xmin - currentRadius;
		int y = rect.ymax + currentRadius;
		int2 dir = new int2(1, 0);
		while (currentRadius <= radius)
		{
			int x2 = node.XCoordinateInGrid + x;
			int z = node.ZCoordinateInGrid + y;
			GridNodeBase node2 = graph.GetNode(x2, z);
			if (node2 != null)
			{
				yield return node2;
			}
			if (dir.x != 0)
			{
				if (x + dir.x > rect.xmax + currentRadius)
				{
					dir = new int2(0, -1);
				}
				if (x + dir.x < rect.xmin - currentRadius)
				{
					dir = new int2(0, 1);
				}
			}
			if (dir.y != 0)
			{
				if (y + dir.y < rect.ymin - currentRadius)
				{
					dir = new int2(-1, 0);
				}
				if (y + dir.y >= rect.ymax + currentRadius && x == rect.xmin - currentRadius)
				{
					currentRadius++;
					x--;
					y += dir.y + 1;
					dir = new int2(1, 0);
					continue;
				}
			}
			x += dir.x;
			y += dir.y;
		}
	}

	public static IEnumerable<GridNodeBase> GetNodesAround(GridNodeBase node, IntRect rect, IntRect size)
	{
		GridGraph graph = (GridGraph)node.Graph;
		int x = node.XCoordinateInGrid - size.Width;
		int y = node.ZCoordinateInGrid - size.Height;
		int width = rect.Width + size.Width;
		int height = rect.Height + size.Height;
		for (int i = 0; i < width; i++)
		{
			int num = x + 1;
			x = num;
			GridNodeBase node2 = graph.GetNode(num, y);
			if (node2 != null)
			{
				yield return node2;
			}
		}
		for (int i = 0; i < height; i++)
		{
			int x2 = x;
			int num = y + 1;
			y = num;
			GridNodeBase node3 = graph.GetNode(x2, num);
			if (node3 != null)
			{
				yield return node3;
			}
		}
		for (int i = 0; i < width; i++)
		{
			int num = x - 1;
			x = num;
			GridNodeBase node4 = graph.GetNode(num, y);
			if (node4 != null)
			{
				yield return node4;
			}
		}
		for (int i = 0; i < height; i++)
		{
			int x3 = x;
			int num = y - 1;
			y = num;
			GridNodeBase node5 = graph.GetNode(x3, num);
			if (node5 != null)
			{
				yield return node5;
			}
		}
	}

	[CanBeNull]
	public static GridNodeBase ToNode(this Vector3 nodePosition, GridGraph graph)
	{
		Vector3 nodePositionProcessed = (Vector3)new Int3(nodePosition.ToXZ());
		return graph.nodes.FirstOrDefault((GridNodeBase x) => x.Vector3Position().ToXZ() == nodePositionProcessed);
	}

	public static Vector3 DirectionTo(this GridNodeBase from, GridNodeBase to)
	{
		return from.Vector3Position().DirectionTo(to.Vector3Position());
	}

	public static Vector3 DirectionTo(this GridNodeBase from, Vector3 to)
	{
		return from.Vector3Position().DirectionTo(to);
	}

	public static Vector3 DirectionTo(this Vector3 from, Vector3 to)
	{
		return (to - from).normalized;
	}

	public static int SimplifiedDirection(this Vector2Int from, Vector2Int to)
	{
		if (to.x >= from.x)
		{
			if (to.y >= from.y)
			{
				return 5;
			}
			return 4;
		}
		if (to.y >= from.y)
		{
			return 6;
		}
		return 7;
	}

	public static bool AllNodesConnectedToNeighbours(IntRect rect, GraphNode node)
	{
		if (rect.Width == 1 && rect.Height == 1)
		{
			return true;
		}
		GridGraph gridGraph = (GridGraph)node.Graph;
		Vector2Int coordinatesInGrid = ((GridNodeBase)node).CoordinatesInGrid;
		for (int i = rect.xmin; i <= rect.xmax; i++)
		{
			for (int j = rect.ymin; j <= rect.ymax; j++)
			{
				GridNodeBase node2 = gridGraph.GetNode(coordinatesInGrid.x + i, coordinatesInGrid.y + j);
				if (i != rect.xmax && !node2.HasConnectionInDirection(1))
				{
					return false;
				}
				if (j != rect.ymax && !node2.HasConnectionInDirection(2))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool TryGetStandableNode(UnitEntity targetEntity, GridNodeBase endPoint, int maxDistance, out GridNodeBase targetNode)
	{
		targetNode = null;
		if (endPoint.Walkable && WarhammerBlockManager.Instance.CanUnitStandOnNode(targetEntity, endPoint))
		{
			targetNode = endPoint;
		}
		else
		{
			IEnumerable<GridNodeBase> nodesSpiralAround = GetNodesSpiralAround(endPoint, targetEntity.SizeRect, maxDistance);
			int num = maxDistance;
			foreach (GridNodeBase item in nodesSpiralAround)
			{
				int num2 = item.CellDistanceTo(endPoint);
				int num3 = targetEntity.DistanceToInCells(item.Vector3Position(), default(IntRect));
				if (item.Walkable && num2 < num && num3 <= maxDistance && WarhammerBlockManager.Instance.CanUnitStandOnNode(targetEntity, item))
				{
					targetNode = item;
					num = num2;
				}
			}
		}
		return targetNode != null;
	}

	public static GridNodeBase GetNeighbourAlongDirection(this GridNodeBase node, Vector3 direction)
	{
		return (node.Vector3Position() + direction.normalized * 1.Cells().Meters).GetNearestNodeXZUnwalkable();
	}
}
