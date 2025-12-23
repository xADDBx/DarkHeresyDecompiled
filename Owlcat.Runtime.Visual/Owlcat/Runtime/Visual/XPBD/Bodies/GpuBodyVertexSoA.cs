using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public class GpuBodyVertexSoA : GpuStructureOfArrays<BodyVertex, BodyVertexSoA>
{
	public GraphicsBufferWrapper<float3> RestNormal;

	public GraphicsBufferWrapper<float3> Position;

	public GraphicsBufferWrapper<float3> Normal;

	public GpuBodyVertexSoA(int size)
		: base(size)
	{
		RestNormal = new GraphicsBufferWrapper<float3>("_XpbdBodyVertexRestNormalBuffer", size);
		m_Buffers.Add(RestNormal);
		Position = new GraphicsBufferWrapper<float3>("_XpbdBodyVertexPositionBuffer", size);
		m_Buffers.Add(Position);
		Normal = new GraphicsBufferWrapper<float3>("_XpbdBodyVertexNormalBuffer", size);
		m_Buffers.Add(Normal);
	}

	public override void SetData(BodyVertexSoA data)
	{
		RestNormal.SetData(data.RestNormal);
		Position.SetData(data.Position);
		Normal.SetData(data.Normal);
	}

	public override void SetData(BodyVertexSoA data, int offset, int count)
	{
		RestNormal.SetData(data.RestNormal, offset, offset, count);
		Position.SetData(data.Position, offset, offset, count);
		Normal.SetData(data.Normal, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, BodyVertexSoA data)
	{
		cmd.SetBufferData(RestNormal.Buffer, data.RestNormal);
		cmd.SetBufferData(Position.Buffer, data.Position);
		cmd.SetBufferData(Normal.Buffer, data.Normal);
	}

	public override void SetData(CommandBuffer cmd, BodyVertexSoA data, int offset, int count)
	{
		cmd.SetBufferData(RestNormal.Buffer, data.RestNormal, offset, offset, count);
		cmd.SetBufferData(Position.Buffer, data.Position, offset, offset, count);
		cmd.SetBufferData(Normal.Buffer, data.Normal, offset, offset, count);
	}
}
