using System.Threading;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU.Jobs;

[BurstCompile]
public struct CalculateGridSpacingJob : IJobParallelFor
{
	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> AabbMap;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<Aabb> Aabbs;

	[NativeDisableParallelForRestriction]
	public NativeReference<int> ObjectSizeSum;

	public void Execute(int index)
	{
		int index2 = AabbMap[index];
		float3 size = Aabbs[index2].Size;
		int value = (int)(math.max(size.x, math.max(size.y, size.z)) * 100f);
		Interlocked.Add(ref UnsafeCollectionExtensions.AsRef(in ObjectSizeSum), value);
	}
}
