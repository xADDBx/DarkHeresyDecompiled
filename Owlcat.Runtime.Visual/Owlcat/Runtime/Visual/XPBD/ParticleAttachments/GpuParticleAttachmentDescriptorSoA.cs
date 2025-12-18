using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.ParticleAttachments;

public class GpuParticleAttachmentDescriptorSoA : GpuStructureOfArrays<ParticleAttachmentDescriptor, ParticleAttachmentDescriptorSoA>
{
	public GraphicsBufferWrapper<int2> BodyParticlesRange;

	public GraphicsBufferWrapper<int2> ParticleDataRange;

	public GraphicsBufferWrapper<float4x4> LocalToWorld;

	public GpuParticleAttachmentDescriptorSoA(int size)
		: base(size)
	{
		BodyParticlesRange = new GraphicsBufferWrapper<int2>("_XpbdParticleAttachmentDescriptorBodyParticlesRangeBuffer", size);
		m_Buffers.Add(BodyParticlesRange);
		ParticleDataRange = new GraphicsBufferWrapper<int2>("_XpbdParticleAttachmentDescriptorParticleDataRangeBuffer", size);
		m_Buffers.Add(ParticleDataRange);
		LocalToWorld = new GraphicsBufferWrapper<float4x4>("_XpbdParticleAttachmentDescriptorLocalToWorldBuffer", size);
		m_Buffers.Add(LocalToWorld);
	}

	public override void SetData(ParticleAttachmentDescriptorSoA data)
	{
		BodyParticlesRange.SetData(data.BodyParticlesRange);
		ParticleDataRange.SetData(data.ParticleDataRange);
		LocalToWorld.SetData(data.LocalToWorld);
	}

	public override void SetData(ParticleAttachmentDescriptorSoA data, int offset, int count)
	{
		BodyParticlesRange.SetData(data.BodyParticlesRange, offset, offset, count);
		ParticleDataRange.SetData(data.ParticleDataRange, offset, offset, count);
		LocalToWorld.SetData(data.LocalToWorld, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, ParticleAttachmentDescriptorSoA data)
	{
		cmd.SetBufferData(BodyParticlesRange.Buffer, data.BodyParticlesRange);
		cmd.SetBufferData(ParticleDataRange.Buffer, data.ParticleDataRange);
		cmd.SetBufferData(LocalToWorld.Buffer, data.LocalToWorld);
	}

	public override void SetData(CommandBuffer cmd, ParticleAttachmentDescriptorSoA data, int offset, int count)
	{
		cmd.SetBufferData(BodyParticlesRange.Buffer, data.BodyParticlesRange, offset, offset, count);
		cmd.SetBufferData(ParticleDataRange.Buffer, data.ParticleDataRange, offset, offset, count);
		cmd.SetBufferData(LocalToWorld.Buffer, data.LocalToWorld, offset, offset, count);
	}
}
