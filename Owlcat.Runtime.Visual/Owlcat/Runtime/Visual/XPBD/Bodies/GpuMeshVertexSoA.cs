using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public class GpuMeshVertexSoA : GpuStructureOfArrays<MeshVertex, MeshVertexSoA>
{
	public GraphicsBufferWrapper<float3> Position;

	public GraphicsBufferWrapper<float3> Normal;

	public GpuMeshVertexSoA(int size)
		: base(size)
	{
		Position = new GraphicsBufferWrapper<float3>("_XpbdMeshVertexPositionBuffer", size);
		m_Buffers.Add(Position);
		Normal = new GraphicsBufferWrapper<float3>("_XpbdMeshVertexNormalBuffer", size);
		m_Buffers.Add(Normal);
	}

	public override void SetData(MeshVertexSoA data)
	{
		Position.SetData(data.Position);
		Normal.SetData(data.Normal);
	}

	public override void SetData(MeshVertexSoA data, int offset, int count)
	{
		Position.SetData(data.Position, offset, offset, count);
		Normal.SetData(data.Normal, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, MeshVertexSoA data)
	{
		cmd.SetBufferData(Position.Buffer, data.Position);
		cmd.SetBufferData(Normal.Buffer, data.Normal);
	}

	public override void SetData(CommandBuffer cmd, MeshVertexSoA data, int offset, int count)
	{
		cmd.SetBufferData(Position.Buffer, data.Position, offset, offset, count);
		cmd.SetBufferData(Normal.Buffer, data.Normal, offset, offset, count);
	}
}
