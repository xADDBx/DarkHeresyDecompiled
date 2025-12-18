using Owlcat.Runtime.Visual.Experimental.Geometry;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

[BurstCompile]
internal struct UpdateRendererLinksJob : IJob
{
	private struct OverlapVolumeHandler : NativeBvhQueryContext<VolumeNode>.IHandler<Obb>
	{
		private NativeHashSet<int> m_OverlapVolumeIds;

		public OverlapVolumeHandler(NativeHashSet<int> overlapVolumeIds)
		{
			m_OverlapVolumeIds = overlapVolumeIds;
		}

		public void OnOverlap(ref Obb rendererBox, VolumeNode data)
		{
			if (GeometryMath.Overlaps(data.Box, rendererBox))
			{
				m_OverlapVolumeIds.Add(data.VolumeId);
			}
		}
	}

	public State State;

	public void Execute()
	{
		using NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(16, Allocator.Temp);
		using NativeBvhQueryContext<VolumeNode> nativeBvhQueryContext = State.VolumeBvh.Query();
		OverlapVolumeHandler handler = new OverlapVolumeHandler(nativeHashSet);
		foreach (int dirtyLinkRendererId in State.DirtyLinkRendererIds)
		{
			if (State.Renderers.TryGetValue(dirtyLinkRendererId, out var item))
			{
				nativeBvhQueryContext.Overlap(item.Box.Bounds, ref item.Box, ref handler);
			}
			State.HandleRendererOverlaps(dirtyLinkRendererId, nativeHashSet);
			nativeHashSet.Clear();
		}
	}
}
