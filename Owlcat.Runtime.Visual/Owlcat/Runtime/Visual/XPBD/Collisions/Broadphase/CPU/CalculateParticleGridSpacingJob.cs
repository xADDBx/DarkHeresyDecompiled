using System.Threading;
using Owlcat.Runtime.Core.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU;

[BurstCompile]
public struct CalculateParticleGridSpacingJob : IJobParallelFor
{
	public int2 ConstraintsRange;

	public int2 SimplexConstraintsRange;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	[ReadOnly]
	public NativeArray<float4> SimplexParameters0;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	[ReadOnly]
	public NativeArray<float4> SimplexParameters1;

	[NativeDisableParallelForRestriction]
	[NativeDisableContainerSafetyRestriction]
	public NativeReference<int> ObjectSizeSum;

	public void Execute(int index)
	{
		int index2 = ConstraintsRange.x + SimplexConstraintsRange.x + index;
		float3 xyz = SimplexParameters0[index2].xyz;
		float3 @float = SimplexParameters1[index2].xyz - xyz;
		int value = (int)(math.max(@float.x, math.max(@float.y, @float.z)) * 100f);
		Interlocked.Add(ref UnsafeCollectionExtensions.AsRef(in ObjectSizeSum), value);
	}
}
