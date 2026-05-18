using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting;

[BurstCompile]
internal struct CountJob : IJob
{
	[ReadOnly]
	public NativeSlice<TestPlanesResults> BoundsVisibility;

	[WriteOnly]
	public NativeReference<int> Count;

	public void Execute()
	{
		int num = 0;
		foreach (TestPlanesResults item in BoundsVisibility)
		{
			if (item != TestPlanesResults.Outside)
			{
				num++;
			}
		}
		Count.Value = num;
	}
}
