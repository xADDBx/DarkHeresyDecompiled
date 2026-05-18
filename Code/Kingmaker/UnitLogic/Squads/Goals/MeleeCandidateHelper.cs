using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Kingmaker.UnitLogic.Squads.Goals;

public static class MeleeCandidateHelper
{
	public static HashSet<GridNodeBase> GetNodesNearTargetEntity(BaseUnitEntity agent, MechanicEntity targetEntity, PreferredPositionNearEntity preferredPositionNearEntity)
	{
		HashSet<GridNodeBase> hashSet = EnumerateNodesNearEntity(targetEntity, preferredPositionNearEntity).ToHashSet();
		IntRect sizeRect = agent.SizeRect;
		if (sizeRect.Width == 1 && sizeRect.Height == 1)
		{
			return hashSet;
		}
		if (hashSet.Count == 0)
		{
			return hashSet;
		}
		GridGraph gridGraph = hashSet.First().Graph as GridGraph;
		foreach (GridNodeBase item in hashSet.ToTempList())
		{
			for (int i = 0; i < sizeRect.Width; i++)
			{
				for (int j = 0; j < sizeRect.Height; j++)
				{
					hashSet.Add(gridGraph.GetNode(item.XCoordinateInGrid - i, item.ZCoordinateInGrid - j));
				}
			}
		}
		return hashSet;
	}

	public static IEnumerable<GridNodeBase> EnumerateNodesNearEntity(MechanicEntity entity, PreferredPositionNearEntity preferredPositionNearEntity)
	{
		return EnumerateNodesNearEntity(entity.GetNearestNodeXZ(), entity.SizeRect, preferredPositionNearEntity);
	}

	public static IEnumerable<GridNodeBase> EnumerateNodesNearEntity(GridNodeBase node, IntRect sizeRect, PreferredPositionNearEntity preferredPositionNearEntity)
	{
		List<GridNodeBase> entityNodes = GridAreaHelper.GetOccupiedNodes(node, sizeRect).ToList();
		int xFrom = entityNodes.Min((GridNodeBase n) => n.XCoordinateInGrid) - 1;
		int xTo = entityNodes.Max((GridNodeBase n) => n.XCoordinateInGrid) + 1;
		int zFrom = entityNodes.Min((GridNodeBase n) => n.ZCoordinateInGrid) - 1;
		int zTo = entityNodes.Max((GridNodeBase n) => n.ZCoordinateInGrid) + 1;
		GridGraph graph = entityNodes[0].Graph as GridGraph;
		foreach (GridNodeBase entityNode in entityNodes)
		{
			for (int x = xFrom; x <= xTo; x++)
			{
				for (int z = zFrom; z <= zTo; z++)
				{
					bool flag = (x == xFrom || x == xTo) && (z == zFrom || z == zTo);
					if (!(preferredPositionNearEntity == PreferredPositionNearEntity.AdjacentGraphNodes && flag) && (preferredPositionNearEntity != PreferredPositionNearEntity.DiagonalGraphNodes || flag))
					{
						GridNodeBase node2 = graph.GetNode(x, z);
						if (node2 != null && !entityNodes.Contains(node2) && entityNode.ContainsConnection(node2))
						{
							yield return node2;
						}
					}
				}
			}
		}
	}
}
