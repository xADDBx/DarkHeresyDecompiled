using Owlcat.Runtime.Visual.Experimental.Geometry;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

[BurstCompile]
internal struct UpdateTriggersJob : IJob
{
	private struct OverlapTriggerHandler : NativeBvhQueryContext<TriggerNode>.IHandler<OverlapContext>
	{
		private NativeHashSet<int> m_OverlapProbeIds;

		private NativeHashSet<int> m_OverlapTriggerIds;

		public OverlapTriggerHandler(NativeHashSet<int> overlapProbeIds, NativeHashSet<int> overlapTriggerIds)
		{
			m_OverlapProbeIds = overlapProbeIds;
			m_OverlapTriggerIds = overlapTriggerIds;
		}

		public void OnOverlap(ref OverlapContext context, TriggerNode data)
		{
			if (GeometryMath.Overlaps(data.Box, context.SphereSq))
			{
				m_OverlapProbeIds.Add(context.ProbeId);
				m_OverlapTriggerIds.Add(data.TriggerId);
			}
		}
	}

	private struct OverlapContext
	{
		public int ProbeId;

		public float4 SphereSq;
	}

	public State State;

	public NativeArray<ProbeData> Probes;

	public float ActiveProbeExtension;

	public void Execute()
	{
		using NativeBvhQueryContext<TriggerNode> nativeBvhQueryContext = State.TriggerBvh.Query();
		using NativeHashSet<int> overlapProbeIds = new NativeHashSet<int>(16, Allocator.Temp);
		using NativeHashSet<int> overlapTriggerIds = new NativeHashSet<int>(16, Allocator.Temp);
		OverlapTriggerHandler handler = new OverlapTriggerHandler(overlapProbeIds, overlapTriggerIds);
		foreach (ProbeData probe in Probes)
		{
			float4 sphere = probe.Sphere;
			if (State.ActiveProbeIds.Contains(probe.ProbeId))
			{
				sphere.w += ActiveProbeExtension;
			}
			float4 sphereSq = new float4(sphere.xyz, sphere.w * sphere.w);
			float3 xyz = sphere.xyz;
			float3 @float = new float3(sphere.w);
			Aabb box = new Aabb(xyz - @float, xyz + @float);
			OverlapContext overlapContext = default(OverlapContext);
			overlapContext.ProbeId = probe.ProbeId;
			overlapContext.SphereSq = sphereSq;
			OverlapContext context = overlapContext;
			nativeBvhQueryContext.Overlap(box, ref context, ref handler);
		}
		State.HandleProbeOverlaps(overlapProbeIds, overlapTriggerIds);
	}
}
