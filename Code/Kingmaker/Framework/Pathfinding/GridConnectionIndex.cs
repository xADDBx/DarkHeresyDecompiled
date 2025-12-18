using System;

namespace Kingmaker.Framework.Pathfinding;

public readonly struct GridConnectionIndex : IEquatable<GridConnectionIndex>
{
	public readonly GridNodeIndex from;

	public readonly GridNodeDirection direction;

	public GridNodeIndex to => from.GetNeighbourAlongDirection(direction);

	public GridConnectionIndex(GridNodeIndex from, GridNodeDirection direction)
	{
		this.from = from;
		this.direction = direction;
	}

	public GridConnectionIndex Inverse()
	{
		return new GridConnectionIndex(from, direction.GetOppositeDirection());
	}

	public bool Equals(GridConnectionIndex other)
	{
		if (from.Equals(other.from))
		{
			return direction.Equals(other.direction);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is GridConnectionIndex other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(from, direction);
	}

	public override string ToString()
	{
		return $"{from}->{to}";
	}
}
