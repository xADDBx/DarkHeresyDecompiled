using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.View.Mechanics;
using Kingmaker.View.Scene.Mechanics.Entities;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View.MapObjects.SriptZones;

public static class EditorGridHelper
{
	public static IEnumerable<Vector3> GetPointsInsideScriptZone(IScriptZoneShape shape, Vector3 point)
	{
		AstarPath.FindAstarPath();
		if (!AstarPath.active || !(AstarPath.active.data.graphs[0] is GridGraph gridGraph))
		{
			return null;
		}
		Int3 centerNode = ConvertPointToGridCoordinates(point, gridGraph);
		if (gridGraph.nodes != null)
		{
			return FindPointsInsideScriptZoneNavmesh(shape, gridGraph);
		}
		return FindPointsInsideScriptZoneSimple(shape, centerNode, gridGraph);
	}

	public static IEnumerable<GridNodeBase> GetNodesInsideScriptZone(IScriptZoneShape shape)
	{
		AstarPath.FindAstarPath();
		if (!AstarPath.active || !(AstarPath.active.data.graphs[0] is GridGraph graph))
		{
			return null;
		}
		return FindNodesInsideScriptZoneNavmesh(shape, graph);
	}

	private static IEnumerable<GridNodeBase> FindNodesInsideScriptZoneNavmesh(IScriptZoneShape shape, GridGraph graph)
	{
		List<GraphNode> nodesInRegion = graph.GetNodesInRegion(shape.GetBounds());
		foreach (GraphNode item in nodesInRegion)
		{
			if (shape.Contains(item.Vector3Position()))
			{
				yield return item as GridNodeBase;
			}
		}
	}

	private static IEnumerable<Vector3> FindPointsInsideScriptZoneNavmesh(IScriptZoneShape shape, GridGraph graph)
	{
		foreach (GridNodeBase item in FindNodesInsideScriptZoneNavmesh(shape, graph))
		{
			yield return item.Vector3Position();
		}
	}

	private static IEnumerable<Vector3> FindPointsInsideScriptZoneSimple(IScriptZoneShape shape, Int3 centerNode, GridGraph graph)
	{
		HashSet<Int3> searchList = new HashSet<Int3> { centerNode };
		HashSet<Int3> doneNodes = new HashSet<Int3>();
		float[,] neighborStepDirection = new float[4, 2]
		{
			{ 0f, graph.nodeSize },
			{
				0f,
				0f - graph.nodeSize
			},
			{ graph.nodeSize, 0f },
			{
				0f - graph.nodeSize,
				0f
			}
		};
		while (searchList.Count > 0)
		{
			Int3 currentNode = searchList.FirstOrDefault();
			Vector3 currentNodePosition = FindWorldPositionOfNode(currentNode, graph, shape.Center().y);
			if (!shape.Contains(currentNodePosition))
			{
				searchList.Remove(currentNode);
				continue;
			}
			yield return currentNodePosition;
			for (int i = 0; i < 4; i++)
			{
				Int3 item = ConvertPointToGridCoordinates(new Vector3(currentNodePosition.x + neighborStepDirection[i, 0], currentNodePosition.y, currentNodePosition.z + neighborStepDirection[i, 1]), graph);
				if (!doneNodes.Contains(item) && !searchList.Contains(item))
				{
					searchList.Add(item);
				}
			}
			doneNodes.Add(currentNode);
			searchList.Remove(currentNode);
		}
	}

	private static Int3 ConvertPointToGridCoordinates(Vector3 point, GridGraph graph)
	{
		int value = Mathf.FloorToInt((point.x - graph.size.x / 2f) / 1.Cells().Meters);
		return new Int3(_z: Math.Abs(Mathf.FloorToInt((point.z - graph.size.y / 2f) / 1.Cells().Meters)), _x: Math.Abs(value), _y: 0);
	}

	private static Vector3 FindWorldPositionOfNode(Int3 graphPosition, GridGraph graph, float height)
	{
		float num = graph.size.x / 2f - graph.center.x;
		float num2 = graph.size.y / 2f - graph.center.z;
		float x = num + 1.Cells().Meters / 2f - 1.Cells().Meters * (float)graphPosition.x;
		float z = num2 + 1.Cells().Meters / 2f - 1.Cells().Meters * (float)graphPosition.z;
		return new Vector3(x, height, z);
	}

	public static IEnumerable<Vector3> GetCellsCoveredByMechanicEntity([CanBeNull] MechanicEntity data, MechanicEntityView view)
	{
		if (data == null)
		{
			AbstractDestructibleEntityView abstractDestructibleEntityView = view as AbstractDestructibleEntityView;
			if (abstractDestructibleEntityView == null)
			{
				return null;
			}
			AstarPath.FindAstarPath();
			if (!AstarPath.active || !(AstarPath.active.data.graphs[0] is GridGraph))
			{
				return null;
			}
			IntRect sizeRect = DestructibleEntity.BoundsToSizeRect(abstractDestructibleEntityView.Bounds);
			Vector3 vector = abstractDestructibleEntityView.transform.position + MechanicEntity.GetViewToEntityPositionOffset(sizeRect);
			List<Vector3> list = new List<Vector3>();
			for (int j = sizeRect.xmin; j <= sizeRect.xmax; j++)
			{
				for (int k = sizeRect.ymin; k <= sizeRect.ymax; k++)
				{
					list.Add(vector + new Vector3(j, 0f, k) * 1.Cells().Meters);
				}
			}
			return list;
		}
		return from i in data.GetOccupiedNodes()
			select i.Vector3Position();
	}

	private static Vector3 GetStarterNode(AbstractDestructibleEntityView destructibleEntity, GridGraph graph)
	{
		Vector2 min = destructibleEntity.Bounds.min;
		return FindWorldPositionOfNode(ConvertPointToGridCoordinates(new Vector3(min.x, 0f, min.y), graph), graph, destructibleEntity.ViewTransform.position.y);
	}
}
