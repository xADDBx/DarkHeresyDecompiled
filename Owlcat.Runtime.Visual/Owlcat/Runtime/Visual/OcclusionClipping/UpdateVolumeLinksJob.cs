using Owlcat.Runtime.Visual.Experimental.Geometry;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

[BurstCompile]
internal struct UpdateVolumeLinksJob : IJob
{
	private struct OverlapRendererHandler : NativeBvhQueryContext<RendererNode>.IHandler<Obb>
	{
		private NativeHashSet<int> m_OverlapRendererIds;

		public OverlapRendererHandler(NativeHashSet<int> overlapRendererIds)
		{
			m_OverlapRendererIds = overlapRendererIds;
		}

		public void OnOverlap(ref Obb volumeBox, RendererNode data)
		{
			if (GeometryMath.Overlaps(data.Box, volumeBox))
			{
				m_OverlapRendererIds.Add(data.RendererId);
			}
		}
	}

	public State State;

	public void Execute()
	{
		using NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(16, Allocator.Temp);
		using NativeBvhQueryContext<RendererNode> nativeBvhQueryContext = State.RendererBvh.Query();
		OverlapRendererHandler handler = new OverlapRendererHandler(nativeHashSet);
		foreach (int dirtyLinkVolumeId in State.DirtyLinkVolumeIds)
		{
			if (State.Volumes.TryGetValue(dirtyLinkVolumeId, out var item))
			{
				nativeBvhQueryContext.Overlap(item.Box.Bounds, ref item.Box, ref handler);
			}
			State.HandleVolumeOverlaps(dirtyLinkVolumeId, nativeHashSet);
			nativeHashSet.Clear();
		}
	}
}
