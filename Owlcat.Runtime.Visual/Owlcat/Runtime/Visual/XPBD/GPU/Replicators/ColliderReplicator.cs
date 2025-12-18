using Owlcat.Runtime.Visual.XPBD.Collisions;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Owlcat.Runtime.Visual.XPBD.Stats;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.GPU.Replicators;

public class ColliderReplicator : ReplicatorBase<ColliderWorld>
{
	private bool m_NeedReplicate;

	private GraphicsBufferWrapper<int> m_ColliderIndicesMapBuffer;

	private GpuColliderDescriptorSoA m_ColliderDescriptorSoA;

	public GraphicsBufferWrapper<int> ColliderIndicesMapBuffer => m_ColliderIndicesMapBuffer;

	public GpuColliderDescriptorSoA ColliderDescriptorSoA => m_ColliderDescriptorSoA;

	public override bool IsEmpty => base.Allocator.IsEmpty;

	public ColliderReplicator(ColliderWorld allocator)
		: base(allocator)
	{
		m_ColliderIndicesMapBuffer = new GraphicsBufferWrapper<int>("_XpbdColliderIndicesMapBuffer", 128);
		m_ColliderDescriptorSoA = CreateSoA(base.Allocator.Allocations.Capacity, (int size) => new GpuColliderDescriptorSoA(size));
	}

	public override void Dispose()
	{
		base.Dispose();
		m_ColliderIndicesMapBuffer?.Dispose();
	}

	public override bool Replicate(CommandBuffer cmd)
	{
		bool result = false;
		if (m_NeedReplicate)
		{
			cmd.SetBufferData(m_ColliderIndicesMapBuffer.Buffer, base.Allocator.IndicesMap);
			m_ColliderDescriptorSoA.SetData(cmd, base.Allocator.ColliderDescriptorSoA);
			result = true;
			m_NeedReplicate = false;
		}
		return result;
	}

	protected override void OnAfterAlloc()
	{
		m_ColliderIndicesMapBuffer.Resize(base.Allocator.IndicesMap.Length);
		m_ColliderDescriptorSoA.Resize(base.Allocator.ColliderDescriptorSoA.Capacity);
		m_NeedReplicate = base.Allocator.AddedEntities.Count > 0 || base.Allocator.RemovedEntities.Count > 0;
	}

	public override MemoryStat GetMemoryStat()
	{
		MemoryStat memoryStat = base.GetMemoryStat();
		memoryStat.Gpu += m_ColliderIndicesMapBuffer.GetSizeInBytes();
		return memoryStat;
	}
}
