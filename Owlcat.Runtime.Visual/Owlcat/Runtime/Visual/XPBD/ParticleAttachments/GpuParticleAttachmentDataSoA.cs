using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.ParticleAttachments;

public class GpuParticleAttachmentDataSoA : GpuStructureOfArrays<ParticleAttachmentData, ParticleAttachmentDataSoA>
{
	public GraphicsBufferWrapper<float3> PositionOffset;

	public GraphicsBufferWrapper<int> IndexInBody;

	public GpuParticleAttachmentDataSoA(int size)
		: base(size)
	{
		PositionOffset = new GraphicsBufferWrapper<float3>("_XpbdParticleAttachmentDataPositionOffsetBuffer", size);
		m_Buffers.Add(PositionOffset);
		IndexInBody = new GraphicsBufferWrapper<int>("_XpbdParticleAttachmentDataIndexInBodyBuffer", size);
		m_Buffers.Add(IndexInBody);
	}

	public override void SetData(ParticleAttachmentDataSoA data)
	{
		PositionOffset.SetData(data.PositionOffset);
		IndexInBody.SetData(data.IndexInBody);
	}

	public override void SetData(ParticleAttachmentDataSoA data, int offset, int count)
	{
		PositionOffset.SetData(data.PositionOffset, offset, offset, count);
		IndexInBody.SetData(data.IndexInBody, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, ParticleAttachmentDataSoA data)
	{
		cmd.SetBufferData(PositionOffset.Buffer, data.PositionOffset);
		cmd.SetBufferData(IndexInBody.Buffer, data.IndexInBody);
	}

	public override void SetData(CommandBuffer cmd, ParticleAttachmentDataSoA data, int offset, int count)
	{
		cmd.SetBufferData(PositionOffset.Buffer, data.PositionOffset, offset, offset, count);
		cmd.SetBufferData(IndexInBody.Buffer, data.IndexInBody, offset, offset, count);
	}
}
