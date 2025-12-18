using System;
using Pathfinding;

namespace Kingmaker.Pathfinding;

public class WarhammerTraversalProvider : IWarhammerTraversalProvider, ITraversalProvider
{
	private bool m_IsPlayersEnemy;

	private readonly IntRect m_SizeRect;

	private bool m_IsSoftUnit;

	private readonly WarhammerSingleNodeBlocker m_Executor;

	public WarhammerTraversalProvider(WarhammerSingleNodeBlocker executor, IntRect sizeRect, bool isPlayersEnemy, bool isSoftUnit = false)
	{
		m_Executor = executor ?? throw new ArgumentNullException("executor");
		m_IsPlayersEnemy = isPlayersEnemy;
		m_SizeRect = sizeRect;
		m_IsSoftUnit = isSoftUnit;
	}

	public bool CanTraverse(Path path, GraphNode node)
	{
		if (m_IsSoftUnit)
		{
			return true;
		}
		foreach (GridNodeBase node2 in GridAreaHelper.GetNodes(node, m_SizeRect))
		{
			if (!CanTraverseSingleCell(path, node2, m_Executor, m_IsPlayersEnemy))
			{
				return false;
			}
		}
		if (!GridAreaHelper.AllNodesConnectedToNeighbours(m_SizeRect, node))
		{
			return false;
		}
		return true;
	}

	private static bool CanTraverseSingleCell(Path path, GraphNode node, WarhammerSingleNodeBlocker executor, bool enemy)
	{
		if (node == null || !node.Walkable || ((path.enabledTags >> (int)node.Tag) & 1) == 0)
		{
			return false;
		}
		if (path is ABPath aBPath && node == aBPath.endNode)
		{
			return !WarhammerBlockManager.Instance.NodeContainsAny(node);
		}
		if (!(path is IPathBlockModeOwner pathBlockModeOwner))
		{
			return true;
		}
		switch (pathBlockModeOwner.PathBlockMode)
		{
		case BlockMode.AllExceptSelector:
		{
			WarhammerBlockManager instance = WarhammerBlockManager.Instance;
			bool enemies = !enemy;
			int passThroughSmallUnits;
			if (pathBlockModeOwner != null)
			{
				IPathBlockModeOwner pathBlockModeOwner2 = pathBlockModeOwner;
				passThroughSmallUnits = (pathBlockModeOwner2.PassThroughSmallUnits ? 1 : 0);
			}
			else
			{
				passThroughSmallUnits = 0;
			}
			return !instance.NodeContainsAnyExcept(node, executor, enemies, (byte)passThroughSmallUnits != 0);
		}
		case BlockMode.OnlySelector:
			return !WarhammerBlockManager.Instance.NodeContains(node, executor, enemy);
		case BlockMode.Ignore:
			return true;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public uint GetTraversalCost(Path path, GraphNode node)
	{
		return path.GetTagPenalty((int)node.Tag) + node.Penalty;
	}

	public bool CanTraverseEndNode(GraphNode node, int direction)
	{
		if (node is LinkNode)
		{
			return false;
		}
		if (m_IsSoftUnit)
		{
			return true;
		}
		foreach (GridNodeBase node2 in GridAreaHelper.GetNodes(node, m_SizeRect, direction))
		{
			if (WarhammerBlockManager.Instance.NodeContainsAnyExcept(node2, m_Executor))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanTraverseAlongDirection(Path path, GraphNode node, int direction)
	{
		GridNodeBase neighbourAlongDirection = GetNeighbourAlongDirection((GridNodeBase)node, m_SizeRect, direction, checkConnectivity: true);
		if (neighbourAlongDirection == null || !CanTraverse(path, neighbourAlongDirection))
		{
			return false;
		}
		return true;
	}

	private static GridNodeBase GetNeighbourAlongDirection(GraphNode node, IntRect sizeRect, int direction, bool checkConnectivity)
	{
		GridNodeBase result = null;
		foreach (GridNodeBase node2 in GridAreaHelper.GetNodes(node, sizeRect, direction))
		{
			GridNodeBase neighbourAlongDirection = node2.GetNeighbourAlongDirection(direction, checkConnectivity);
			if (neighbourAlongDirection == null)
			{
				return null;
			}
			if (node2 == node)
			{
				result = neighbourAlongDirection;
			}
		}
		return result;
	}

	public void SetIsPlayerEnemy(bool enemy)
	{
		m_IsPlayersEnemy = enemy;
	}
}
