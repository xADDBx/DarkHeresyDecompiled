using System;
using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.DotNetExtensions;
using Pathfinding.Graphs.Grid;
using Unity.Mathematics;
using UnityEngine;

namespace Pathfinding;

public static class AStarExtensions
{
	public static GridNodeBase GetNeighbourAlongDirection(this GridNodeBase node, int direction, bool checkConnectivity = true)
	{
		if (node.HasConnectionInDirection(direction) || !checkConnectivity)
		{
			GridGraph gridGraph = GridNode.GetGridGraph(node.GraphIndex);
			int num = node.NodeInGridIndex + gridGraph.neighbourOffsets[direction];
			int num2 = num % gridGraph.width;
			if (num >= 0 && num < gridGraph.nodes.Length && Mathf.Abs(num2 - node.XCoordinateInGrid) < 2)
			{
				return gridGraph.nodes[num];
			}
			return null;
		}
		return null;
	}

	public static Vector3 Vector3Position(this GraphNode node)
	{
		return (Vector3)node.position;
	}

	public static int CellDistanceTo(this GridNodeBase a, GridNodeBase b)
	{
		return GraphHelper.GetWarhammerCellDistance(a, b);
	}

	public static int2 GetNearestNodeCoords(this GridGraph graph, Vector3 position)
	{
		position = graph.transform.InverseTransform(position);
		return new int2(Mathf.Clamp((int)position.x, 0, graph.width - 1), Mathf.Clamp((int)position.z, 0, graph.depth - 1));
	}

	public static Vector3 CheckHeight(this GraphCollision collision, Vector3 position, out RaycastHit hit, out bool walkable, float height)
	{
		float fromHeight = collision.fromHeight;
		try
		{
			collision.fromHeight = height;
			return collision.CheckHeight(position, out hit, out walkable);
		}
		finally
		{
			collision.fromHeight = fromHeight;
		}
	}

	public static bool ContainsUnit(this GridNodeBase node)
	{
		return Game.Instance.GridNodeToEntityCache.GetFirstEntity<BaseUnitEntity>(node) != null;
	}

	public static bool ContainsUnit(this GridNodeBase node, BaseUnitEntity unit)
	{
		return Game.Instance.GridNodeToEntityCache.ContainsEntity(node, unit);
	}

	public static bool ContainsEntity(this GridNodeBase node, MechanicEntity entity)
	{
		return Game.Instance.GridNodeToEntityCache.ContainsEntity(node, entity);
	}

	public static bool TryGetFirstUnit(this GridNodeBase node, out BaseUnitEntity unit)
	{
		unit = Game.Instance.GridNodeToEntityCache.GetFirstEntity<BaseUnitEntity>(node);
		return unit != null;
	}

	[CanBeNull]
	public static BaseUnitEntity GetFirstUnit(this GridNodeBase node, [CanBeNull] Func<BaseUnitEntity, bool> pred = null)
	{
		return Game.Instance.GridNodeToEntityCache.GetFirstEntity(node, pred);
	}

	[CanBeNull]
	public static MechanicEntity GetFirstEntity(this GridNodeBase node, [CanBeNull] Func<MechanicEntity, bool> pred = null)
	{
		return Game.Instance.GridNodeToEntityCache.GetFirstEntity(node, pred);
	}

	public static FilteredList<MechanicEntity> GetEntities(this GridNodeBase node, [CanBeNull] Func<MechanicEntity, bool> pred = null)
	{
		return Game.Instance.GridNodeToEntityCache.GetEntities(node, pred);
	}

	public static FilteredList<DestructibleEntity> GetDestructibleEntities(this GridNodeBase node, [CanBeNull] Func<DestructibleEntity, bool> pred = null)
	{
		return Game.Instance.GridNodeToEntityCache.GetEntities(node, pred);
	}

	public static FilteredList<BaseUnitEntity> GetUnits(this GridNodeBase node, [CanBeNull] Func<BaseUnitEntity, bool> pred = null)
	{
		return Game.Instance.GridNodeToEntityCache.GetEntities(node, pred);
	}

	public static FilteredList<AreaEffectEntity> GetAreaEffects(this GridNodeBase node, [CanBeNull] Func<AreaEffectEntity, bool> pred = null)
	{
		return Game.Instance.GridNodeToEntityCache.GetEntities(node, pred);
	}

	public static GridNodeBase GetNearestDirect(this GridGraph gg, Vector3 position)
	{
		if (gg.nodes == null || gg.depth * gg.width != gg.nodes.Length)
		{
			return null;
		}
		position = gg.transform.InverseTransform(position);
		float x = position.x;
		float z = position.z;
		int num = (int)x;
		int num2 = (int)z;
		if (num < 0 || num > gg.width - 1)
		{
			return null;
		}
		if (num2 < 0 || num2 > gg.depth - 1)
		{
			return null;
		}
		return gg.nodes[num2 * gg.width + num];
	}
}
