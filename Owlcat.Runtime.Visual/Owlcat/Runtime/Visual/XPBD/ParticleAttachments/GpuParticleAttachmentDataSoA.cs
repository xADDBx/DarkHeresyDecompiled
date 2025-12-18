using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.ParticleAttachments;

public class GpuParticleAttachmentDataSoA : GpuStructureOfArrays<ParticleAttachmentData, ParticleAttachmentDataSoA>
{
	public GraphicsBufferWrapper<int> IndexInBody;

	public GraphicsBufferWrapper<float3> PositionOffset;

	public GpuParticleAttachmentDataSoA(int size)
		: base(size)
	{
		IndexInBody = new GraphicsBufferWrapper<int>("_XpbdParticleAttachmentDataIndexInBodyBuffer", size);
		m_Buffers.Add(IndexInBody);
		PositionOffset = new GraphicsBufferWrapper<float3>("_XpbdParticleAttachmentDataPositionOffsetBuffer", size);
		m_Buffers.Add(PositionOffset);
	}

	public override void SetData(ParticleAttachmentDataSoA data)
	{
		IndexInBody.SetData(data.IndexInBody);
		PositionOffset.SetData(data.PositionOffset);
	}

	public override void SetData(ParticleAttachmentDataSoA data, int offset, int count)
	{
		IndexInBody.SetData(data.IndexInBody, offset, offset, count);
		PositionOffset.SetData(data.PositionOffset, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, ParticleAttachmentDataSoA data)
	{
		cmd.SetBufferData(IndexInBody.Buffer, data.IndexInBody);
		cmd.SetBufferData(PositionOffset.Buffer, data.PositionOffset);
	}

	public override void SetData(CommandBuffer cmd, ParticleAttachmentDataSoA data, int offset, int count)
	{
		cmd.SetBufferData(IndexInBody.Buffer, data.IndexInBody, offset, offset, count);
		cmd.SetBufferData(PositionOffset.Buffer, data.PositionOffset, offset, offset, count);
	}
}
