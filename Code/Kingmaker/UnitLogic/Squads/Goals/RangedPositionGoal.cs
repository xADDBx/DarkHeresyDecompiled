using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Pathfinding;

namespace Kingmaker.UnitLogic.Squads.Goals;

public sealed class RangedPositionGoal : IMovementGoal
{
	private readonly GridNodeBase m_TargetNode;

	private readonly int m_Radius;

	public RangedPositionGoal(GridNodeBase targetNode)
		: this(targetNode, ConfigRoot.Instance.CombatRoot.SquadRangedClusterRadius)
	{
	}

	public RangedPositionGoal(GridNodeBase targetNode, int radius)
	{
		m_TargetNode = targetNode;
		m_Radius = ((radius < 1) ? 1 : radius);
	}

	public IReadOnlyList<SquadCandidateCell> GetCandidates(UnitSquad squad)
	{
		if (m_TargetNode == null || squad == null)
		{
			return Array.Empty<SquadCandidateCell>();
		}
		if (!(m_TargetNode.Graph is GridGraph gridGraph))
		{
			return Array.Empty<SquadCandidateCell>();
		}
		int xCoordinateInGrid = m_TargetNode.XCoordinateInGrid;
		int zCoordinateInGrid = m_TargetNode.ZCoordinateInGrid;
		int num = 2 * m_Radius + 1;
		List<SquadCandidateCell> list = new List<SquadCandidateCell>(num * num);
		for (int i = -m_Radius; i <= m_Radius; i++)
		{
			for (int j = -m_Radius; j <= m_Radius; j++)
			{
				GridNodeBase node = gridGraph.GetNode(xCoordinateInGrid + i, zCoordinateInGrid + j);
				if (node != null && node.Walkable)
				{
					list.Add(new SquadCandidateCell(node, 1f));
				}
			}
		}
		return list;
	}
}
