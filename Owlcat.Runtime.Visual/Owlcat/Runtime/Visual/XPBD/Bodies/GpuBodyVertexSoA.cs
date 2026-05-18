using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public class GpuBodyVertexSoA : GpuStructureOfArrays<BodyVertex, BodyVertexSoA>
{
	public GraphicsBufferWrapper<float3> Normal;

	public GraphicsBufferWrapper<float3> RestNormal;

	public GraphicsBufferWrapper<float3> Position;

	public GpuBodyVertexSoA(int size)
		: base(size)
	{
		Normal = new GraphicsBufferWrapper<float3>("_XpbdBodyVertexNormalBuffer", size);
		m_Buffers.Add(Normal);
		RestNormal = new GraphicsBufferWrapper<float3>("_XpbdBodyVertexRestNormalBuffer", size);
		m_Buffers.Add(RestNormal);
		Position = new GraphicsBufferWrapper<float3>("_XpbdBodyVertexPositionBuffer", size);
		m_Buffers.Add(Position);
	}

	public override void SetData(BodyVertexSoA data)
	{
		Normal.SetData(data.Normal);
		RestNormal.SetData(data.RestNormal);
		Position.SetData(data.Position);
	}

	public override void SetData(BodyVertexSoA data, int offset, int count)
	{
		Normal.SetData(data.Normal, offset, offset, count);
		RestNormal.SetData(data.RestNormal, offset, offset, count);
		Position.SetData(data.Position, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, BodyVertexSoA data)
	{
		cmd.SetBufferData(Normal.Buffer, data.Normal);
		cmd.SetBufferData(RestNormal.Buffer, data.RestNormal);
		cmd.SetBufferData(Position.Buffer, data.Position);
	}

	public override void SetData(CommandBuffer cmd, BodyVertexSoA data, int offset, int count)
	{
		cmd.SetBufferData(Normal.Buffer, data.Normal, offset, offset, count);
		cmd.SetBufferData(RestNormal.Buffer, data.RestNormal, offset, offset, count);
		cmd.SetBufferData(Position.Buffer, data.Position, offset, offset, count);
	}
}
