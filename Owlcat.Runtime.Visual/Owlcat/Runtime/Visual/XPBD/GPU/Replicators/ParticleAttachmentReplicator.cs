using Owlcat.Runtime.Visual.XPBD.ParticleAttachments;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Owlcat.Runtime.Visual.XPBD.Stats;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.GPU.Replicators;

public class ParticleAttachmentReplicator : ReplicatorBase<ParticleAttachmentAllocator>
{
	private bool m_NeedReplicate;

	private GraphicsBufferWrapper<int> m_ParticleAttachmentDeactivatedParticleIndices;

	private GraphicsBufferWrapper<float> m_ParticleAttachmentDeactivatedParticleInvMass;

	private GraphicsBufferWrapper<int> m_ParticleAttachmentMapBuffer;

	private GpuParticleAttachmentDescriptorSoA m_ParticleAttachmentDescriptorSoA;

	private GpuParticleAttachmentDataSoA m_ParticleAttachmentDataSoA;

	public GraphicsBufferWrapper<int> ParticleAttachmentDeactivatedParticleIndices => m_ParticleAttachmentDeactivatedParticleIndices;

	public GraphicsBufferWrapper<float> ParticleAttachmentDeactivatedParticleInvMass => m_ParticleAttachmentDeactivatedParticleInvMass;

	public GraphicsBufferWrapper<int> ParticleAttachmentMapBuffer => m_ParticleAttachmentMapBuffer;

	public GpuParticleAttachmentDescriptorSoA ParticleAttachmentDescriptorSoA => m_ParticleAttachmentDescriptorSoA;

	public GpuParticleAttachmentDataSoA ParticleAttachmentDataSoA => m_ParticleAttachmentDataSoA;

	public override bool IsEmpty => base.Allocator.IsEmpty;

	public ParticleAttachmentReplicator(ParticleAttachmentAllocator allocator)
		: base(allocator)
	{
		m_ParticleAttachmentDeactivatedParticleIndices = new GraphicsBufferWrapper<int>("_XpbdParticleAttachmentDeactivatedParticleIndices", 64);
		m_ParticleAttachmentDeactivatedParticleInvMass = new GraphicsBufferWrapper<float>("_XpbdParticleAttachmentDeactivatedParticleInvMass", 64);
		m_ParticleAttachmentMapBuffer = new GraphicsBufferWrapper<int>("_XpbdParticleAttachmentIndicesMapBuffer", 128);
		m_ParticleAttachmentDescriptorSoA = CreateSoA(base.Allocator.DescriptorSoA.Capacity, (int size) => new GpuParticleAttachmentDescriptorSoA(size));
		m_ParticleAttachmentDataSoA = CreateSoA(base.Allocator.DataSoA.Capacity, (int size) => new GpuParticleAttachmentDataSoA(size));
	}

	public override void Dispose()
	{
		base.Dispose();
		m_ParticleAttachmentDeactivatedParticleIndices?.Dispose();
		m_ParticleAttachmentDeactivatedParticleInvMass?.Dispose();
		m_ParticleAttachmentMapBuffer?.Dispose();
	}

	public override bool Replicate(CommandBuffer cmd)
	{
		if (m_NeedReplicate)
		{
			cmd.SetBufferData(m_ParticleAttachmentMapBuffer.Buffer, base.Allocator.IndicesMap);
			m_ParticleAttachmentDescriptorSoA.SetData(cmd, base.Allocator.DescriptorSoA);
			m_ParticleAttachmentDataSoA.SetData(cmd, base.Allocator.DataSoA);
		}
		m_NeedReplicate = false;
		return false;
	}

	protected override void OnAfterAlloc()
	{
		m_ParticleAttachmentMapBuffer.Resize(base.Allocator.IndicesMap.Length);
		m_ParticleAttachmentDescriptorSoA.Resize(base.Allocator.DescriptorSoA.Capacity);
		m_ParticleAttachmentDataSoA.Resize(base.Allocator.DataSoA.Capacity);
		m_NeedReplicate = base.Allocator.AddedEntities.Count > 0 || base.Allocator.RemovedEntities.Count > 0;
	}

	public override MemoryStat GetMemoryStat()
	{
		MemoryStat memoryStat = base.GetMemoryStat();
		memoryStat.Gpu += m_ParticleAttachmentMapBuffer.GetSizeInBytes();
		memoryStat.Gpu += m_ParticleAttachmentDeactivatedParticleIndices.GetSizeInBytes();
		memoryStat.Gpu += m_ParticleAttachmentDeactivatedParticleInvMass.GetSizeInBytes();
		return memoryStat;
	}
}
