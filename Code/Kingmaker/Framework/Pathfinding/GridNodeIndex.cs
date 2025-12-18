using System;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Framework.Pathfinding;

public readonly struct GridNodeIndex : IEquatable<GridNodeIndex>
{
	public readonly int x;

	public readonly int z;

	public GridNodeIndex(int x, int z)
	{
		this.x = x;
		this.z = z;
	}

	public bool Equals(GridNodeIndex other)
	{
		if (x == other.x)
		{
			return z == other.z;
		}
		return false;
	}

	public bool Equals(GridNodeBase other)
	{
		if (x == other.XCoordinateInGrid)
		{
			return z == other.ZCoordinateInGrid;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is GridNodeIndex other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(x, z);
	}

	public static GridNodeIndex operator +(GridNodeIndex i1, GridNodeIndex i2)
	{
		return new GridNodeIndex(i1.x + i2.x, i1.z + i2.z);
	}

	public static GridNodeIndex operator -(GridNodeIndex i1, GridNodeIndex i2)
	{
		return new GridNodeIndex(i1.x - i2.x, i1.z - i2.z);
	}

	public static implicit operator Vector2(GridNodeIndex index)
	{
		return new Vector2(index.x, index.z);
	}

	public static implicit operator Vector3(GridNodeIndex index)
	{
		return new Vector3(index.x, 0f, index.z);
	}

	public static implicit operator GridNodeIndex(GridNodeBase node)
	{
		return new GridNodeIndex(node.XCoordinateInGrid, node.ZCoordinateInGrid);
	}

	public override string ToString()
	{
		return $"({x}, {z})";
	}
}
