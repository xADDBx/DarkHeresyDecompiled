using System.Threading;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU.Jobs;

[BurstCompile]
public struct BuildGridJob : IJobParallelFor
{
	[NativeDisableParallelForRestriction]
	public NativeReference<int> ObjectSizeSum;

	public int HashmapSize;

	public int ObjectsCount;

	[NativeDisableParallelForRestriction]
	public NativeArray<int> HashmapKeys;

	[NativeDisableParallelForRestriction]
	public NativeArray<int> HashmapValues;

	[ReadOnly]
	public NativeArray<Aabb> Aabbs;

	[ReadOnly]
	public NativeArray<int> AabbMap;

	public unsafe void Execute(int index)
	{
		float spacing = SpatialHashGrid.CalculateSpacing(ref ObjectSizeSum, in ObjectsCount);
		int num = AabbMap[index];
		Aabb aabb = Aabbs[num];
		int3 @int = SpatialHashGrid.IntCoords(in aabb.Min, in spacing);
		int3 int2 = SpatialHashGrid.IntCoords(in aabb.Max, in spacing) + 1;
		for (int i = @int.x; i < int2.x; i++)
		{
			for (int j = @int.y; j < int2.y; j++)
			{
				for (int k = @int.z; k < int2.z; k++)
				{
					int num2 = SpatialHashGrid.HashCoords(ref i, ref j, ref k);
					int num3 = num2 % HashmapSize;
					int num4 = 0;
					while (num4 < 100)
					{
						if (Interlocked.CompareExchange(ref *(int*)((byte*)HashmapKeys.GetUnsafePtr() + (nint)num3 * (nint)4), num2, -1) == -1)
						{
							HashmapValues[num3] = num;
							break;
						}
						num4++;
						num3 = (num3 + 1) % HashmapSize;
					}
				}
			}
		}
	}
}
