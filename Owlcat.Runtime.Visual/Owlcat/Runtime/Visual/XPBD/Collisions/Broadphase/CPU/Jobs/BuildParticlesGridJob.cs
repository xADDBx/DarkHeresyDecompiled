using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU.Jobs;

[BurstCompile]
public struct BuildParticlesGridJob : IJobParallelFor
{
	public int2 ConstraintsRange;

	public int2 SimplexConstraintsRange;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeReference<int> ObjectSizeSum;

	public int HashmapSize;

	public int ObjectsCount;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<int> HashmapKeys;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	[WriteOnly]
	public NativeArray<int> HashmapValues;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<float4> SimplexParameters0;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<float4> SimplexParameters1;

	public unsafe void Execute(int index)
	{
		float spacing = SpatialHashGrid.CalculateSpacing(ref ObjectSizeSum, in ObjectsCount);
		int num = ConstraintsRange.x + SimplexConstraintsRange.x + index;
		float3 coords = SimplexParameters0[num].xyz;
		float3 coords2 = SimplexParameters1[num].xyz;
		int3 @int = SpatialHashGrid.IntCoords(in coords, in spacing);
		int3 int2 = SpatialHashGrid.IntCoords(in coords2, in spacing) + 1;
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
