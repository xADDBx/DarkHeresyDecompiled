using Owlcat.Runtime.Core.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.Atlas.Jobs;

[BurstCompile]
public struct FindMaxMipCountJob : IJob
{
	public NativeReference<int> MaxMipCount;

	public NativeList<VirtualAtlasEntry> Entries;

	public void Execute()
	{
		int num = 0;
		for (int i = 0; i < Entries.Length; i++)
		{
			num = math.max(num, UnsafeCollectionExtensions.ElementAsRef(in Entries, i).MipCount);
		}
		MaxMipCount.Value = num;
	}
}
