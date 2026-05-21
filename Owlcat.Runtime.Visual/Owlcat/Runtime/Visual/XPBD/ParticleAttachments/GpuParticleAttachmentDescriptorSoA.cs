using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.ParticleAttachments;

public class GpuParticleAttachmentDescriptorSoA : GpuStructureOfArrays<ParticleAttachmentDescriptor, ParticleAttachmentDescriptorSoA>
{
	public GraphicsBufferWrapper<float4x4> LocalToWorld;

	public GraphicsBufferWrapper<int2> ParticleDataRange;

	public GraphicsBufferWrapper<int2> BodyParticlesRange;

	public GpuParticleAttachmentDescriptorSoA(int size)
		: base(size)
	{
		LocalToWorld = new GraphicsBufferWrapper<float4x4>("_XpbdParticleAttachmentDescriptorLocalToWorldBuffer", size);
		m_Buffers.Add(LocalToWorld);
		ParticleDataRange = new GraphicsBufferWrapper<int2>("_XpbdParticleAttachmentDescriptorParticleDataRangeBuffer", size);
		m_Buffers.Add(ParticleDataRange);
		BodyParticlesRange = new GraphicsBufferWrapper<int2>("_XpbdParticleAttachmentDescriptorBodyParticlesRangeBuffer", size);
		m_Buffers.Add(BodyParticlesRange);
	}

	public override void SetData(ParticleAttachmentDescriptorSoA data)
	{
		LocalToWorld.SetData(data.LocalToWorld);
		ParticleDataRange.SetData(data.ParticleDataRange);
		BodyParticlesRange.SetData(data.BodyParticlesRange);
	}

	public override void SetData(ParticleAttachmentDescriptorSoA data, int offset, int count)
	{
		LocalToWorld.SetData(data.LocalToWorld, offset, offset, count);
		ParticleDataRange.SetData(data.ParticleDataRange, offset, offset, count);
		BodyParticlesRange.SetData(data.BodyParticlesRange, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, ParticleAttachmentDescriptorSoA data)
	{
		cmd.SetBufferData(LocalToWorld.Buffer, data.LocalToWorld);
		cmd.SetBufferData(ParticleDataRange.Buffer, data.ParticleDataRange);
		cmd.SetBufferData(BodyParticlesRange.Buffer, data.BodyParticlesRange);
	}

	public override void SetData(CommandBuffer cmd, ParticleAttachmentDescriptorSoA data, int offset, int count)
	{
		cmd.SetBufferData(LocalToWorld.Buffer, data.LocalToWorld, offset, offset, count);
		cmd.SetBufferData(ParticleDataRange.Buffer, data.ParticleDataRange, offset, offset, count);
		cmd.SetBufferData(BodyParticlesRange.Buffer, data.BodyParticlesRange, offset, offset, count);
	}
}
