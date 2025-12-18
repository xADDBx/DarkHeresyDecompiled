using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.VirtualTexture;

[BurstCompile]
public struct SortListJob<T> : IJob where T : unmanaged, IComparable<T>
{
	public NativeList<T> List;

	public void Execute()
	{
		List.Sort();
	}
}
