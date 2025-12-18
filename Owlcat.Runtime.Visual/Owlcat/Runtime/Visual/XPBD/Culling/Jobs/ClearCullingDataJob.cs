using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.XPBD.Culling.Jobs;

[BurstCompile]
public struct ClearCullingDataJob : IJobParallelFor
{
	public bool CullingEnabled;

	[WriteOnly]
	public NativeArray<int> BodyVisibility;

	public void Execute(int index)
	{
		BodyVisibility[index] = ((!CullingEnabled) ? 1 : 0);
	}
}
