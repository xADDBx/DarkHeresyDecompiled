using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public class GpuMeshVertexSoA : GpuStructureOfArrays<MeshVertex, MeshVertexSoA>
{
	public GraphicsBufferWrapper<float3> Normal;

	public GraphicsBufferWrapper<float3> Position;

	public GpuMeshVertexSoA(int size)
		: base(size)
	{
		Normal = new GraphicsBufferWrapper<float3>("_XpbdMeshVertexNormalBuffer", size);
		m_Buffers.Add(Normal);
		Position = new GraphicsBufferWrapper<float3>("_XpbdMeshVertexPositionBuffer", size);
		m_Buffers.Add(Position);
	}

	public override void SetData(MeshVertexSoA data)
	{
		Normal.SetData(data.Normal);
		Position.SetData(data.Position);
	}

	public override void SetData(MeshVertexSoA data, int offset, int count)
	{
		Normal.SetData(data.Normal, offset, offset, count);
		Position.SetData(data.Position, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, MeshVertexSoA data)
	{
		cmd.SetBufferData(Normal.Buffer, data.Normal);
		cmd.SetBufferData(Position.Buffer, data.Position);
	}

	public override void SetData(CommandBuffer cmd, MeshVertexSoA data, int offset, int count)
	{
		cmd.SetBufferData(Normal.Buffer, data.Normal, offset, offset, count);
		cmd.SetBufferData(Position.Buffer, data.Position, offset, offset, count);
	}
}
