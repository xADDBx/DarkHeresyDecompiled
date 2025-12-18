using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU.Jobs;

[BurstCompile]
public struct ClearGridJob : IJobParallelFor
{
	[WriteOnly]
	public NativeArray<int> HashmapKeys;

	[WriteOnly]
	public NativeArray<int> HashmapValues;

	public void Execute(int index)
	{
		HashmapKeys[index] = -1;
		HashmapValues[index] = -1;
	}
}
