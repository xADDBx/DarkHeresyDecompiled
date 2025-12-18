using System;
using JetBrains.Annotations;
using Pathfinding;

namespace Kingmaker.Framework.Pathfinding;

public static class GridNodeDirectionExtensions
{
	public static bool IsDiagonal(this GridNodeDirection direction)
	{
		if (direction != GridNodeDirection.SE && direction != GridNodeDirection.NE && direction != GridNodeDirection.NW)
		{
			return direction == GridNodeDirection.SW;
		}
		return true;
	}

	public static GridNodeIndex GetNeighbourAlongDirection(this GridNodeIndex node, GridNodeDirection direction)
	{
		return direction switch
		{
			GridNodeDirection.S => new GridNodeIndex(node.x, node.z - 1), 
			GridNodeDirection.E => new GridNodeIndex(node.x + 1, node.z), 
			GridNodeDirection.N => new GridNodeIndex(node.x, node.z + 1), 
			GridNodeDirection.W => new GridNodeIndex(node.x - 1, node.z), 
			GridNodeDirection.SE => new GridNodeIndex(node.x + 1, node.z - 1), 
			GridNodeDirection.NE => new GridNodeIndex(node.x + 1, node.z + 1), 
			GridNodeDirection.NW => new GridNodeIndex(node.x - 1, node.z + 1), 
			GridNodeDirection.SW => new GridNodeIndex(node.x - 1, node.z - 1), 
			_ => throw new ArgumentOutOfRangeException("direction", direction, null), 
		};
	}

	public static GridNodeDirection GetOppositeDirection(this GridNodeDirection direction)
	{
		return direction switch
		{
			GridNodeDirection.S => GridNodeDirection.N, 
			GridNodeDirection.E => GridNodeDirection.W, 
			GridNodeDirection.N => GridNodeDirection.S, 
			GridNodeDirection.W => GridNodeDirection.E, 
			GridNodeDirection.SE => GridNodeDirection.NW, 
			GridNodeDirection.NE => GridNodeDirection.SW, 
			GridNodeDirection.NW => GridNodeDirection.SE, 
			GridNodeDirection.SW => GridNodeDirection.NE, 
			_ => throw new ArgumentOutOfRangeException("direction", direction, null), 
		};
	}

	public static GridNodeDirection GetDirection(this GridNodeIndex from, GridNodeIndex to)
	{
		int num = to.x - from.x;
		int num2 = to.z - from.z;
		if (num == 0)
		{
			if (num2 <= 0)
			{
				return GridNodeDirection.S;
			}
			return GridNodeDirection.N;
		}
		if (num2 == 0)
		{
			if (num <= 0)
			{
				return GridNodeDirection.W;
			}
			return GridNodeDirection.E;
		}
		if (num > 0)
		{
			if (num2 <= 0)
			{
				return GridNodeDirection.SE;
			}
			return GridNodeDirection.NE;
		}
		if (num2 <= 0)
		{
			return GridNodeDirection.SW;
		}
		return GridNodeDirection.NW;
	}

	public static GridNodeDirection GetDirection([NotNull] this GridNodeBase from, [NotNull] GridNodeBase to)
	{
		return ((GridNodeIndex)from).GetDirection((GridNodeIndex)to);
	}
}
