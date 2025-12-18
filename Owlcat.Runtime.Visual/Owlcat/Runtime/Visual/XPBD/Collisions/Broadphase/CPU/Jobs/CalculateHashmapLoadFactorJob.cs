using System.Threading;
using Owlcat.Runtime.Core.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU.Jobs;

[BurstCompile]
public struct CalculateHashmapLoadFactorJob : IJobParallelFor
{
	[ReadOnly]
	public NativeArray<int> HashmapKeys;

	[ReadOnly]
	public NativeArray<int> HashmapValues;

	[NativeDisableParallelForRestriction]
	public NativeReference<int> OccupiedCellsCount;

	public void Execute(int index)
	{
		int num = HashmapKeys[index];
		int num2 = HashmapValues[index];
		if (num != -1 && num2 != -1)
		{
			Interlocked.Increment(ref UnsafeCollectionExtensions.AsRef(in OccupiedCellsCount));
		}
	}
}
