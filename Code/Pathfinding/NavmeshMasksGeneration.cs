using System;
using System.Collections.Generic;
using Kingmaker.Pathfinding;
using Unity.Collections;
using Unity.Jobs;

namespace Pathfinding;

public readonly struct NavmeshMasksGeneration : IEquatable<NavmeshMasksGeneration>
{
	private readonly NativeArray<NavmeshMask> Data;

	private readonly int Height;

	private readonly int Width;

	public NavmeshMasksGeneration(NavmeshMask[] data, int width, int height)
	{
		Data = new NativeArray<NavmeshMask>(data, Allocator.TempJob);
		Height = height;
		Width = width;
	}

	public void Dispose(JobHandle handle)
	{
		if (Data.IsCreated)
		{
			Data.Dispose(handle);
		}
	}

	public bool IsRemoved(int x, int z)
	{
		return GetCell(x, z).IsAnySet(NavmeshMask.Removed);
	}

	public bool IsAdded(int x, int z)
	{
		return GetCell(x, z).IsAnySet(NavmeshMask.AllowArea);
	}

	public bool IsFloor(int x, int z)
	{
		return GetCell(x, z).IsAnySet(NavmeshMask.LosBlockingFloor);
	}

	private NavmeshMask GetCell(int x, int z)
	{
		if (Data.IsCreated && x >= 0 && z >= 0 && x < Width && z < Height)
		{
			return Data[z * Width + x];
		}
		return (NavmeshMask)0;
	}

	public override bool Equals(object obj)
	{
		if (obj is NavmeshMasksGeneration other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(NavmeshMasksGeneration other)
	{
		if (EqualityComparer<NativeArray<NavmeshMask>>.Default.Equals(Data, other.Data) && Height == other.Height)
		{
			return Width == other.Width;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = (-1098418262 * -1521134295 + EqualityComparer<NativeArray<NavmeshMask>>.Default.GetHashCode(Data)) * -1521134295;
		int height = Height;
		int num2 = (num + height.GetHashCode()) * -1521134295;
		height = Width;
		return num2 + height.GetHashCode();
	}

	public static bool operator ==(NavmeshMasksGeneration left, NavmeshMasksGeneration right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(NavmeshMasksGeneration left, NavmeshMasksGeneration right)
	{
		return !(left == right);
	}
}
