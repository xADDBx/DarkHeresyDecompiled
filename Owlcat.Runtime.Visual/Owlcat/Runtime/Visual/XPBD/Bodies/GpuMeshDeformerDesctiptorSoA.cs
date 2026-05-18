using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public class GpuMeshDeformerDesctiptorSoA : GpuStructureOfArrays<MeshDeformerDescriptor, MeshDeformerDescriptorSoA>
{
	public GraphicsBufferWrapper<float4x4> WorldToLocal;

	public GraphicsBufferWrapper<int2> BindingsRange;

	public GraphicsBufferWrapper<float4x4> LocalToWorld;

	public GraphicsBufferWrapper<int2> VertexToSkinnedVertexMapRange;

	public GraphicsBufferWrapper<int2> DeformableVerticesRange;

	public GraphicsBufferWrapper<int2> SkinnedVerticesRange;

	public GpuMeshDeformerDesctiptorSoA(int size)
		: base(size)
	{
		WorldToLocal = new GraphicsBufferWrapper<float4x4>("_XpbdMeshDeformerDescriptorWorldToLocalBuffer", size);
		m_Buffers.Add(WorldToLocal);
		BindingsRange = new GraphicsBufferWrapper<int2>("_XpbdMeshDeformerDescriptorBindingsRangeBuffer", size);
		m_Buffers.Add(BindingsRange);
		LocalToWorld = new GraphicsBufferWrapper<float4x4>("_XpbdMeshDeformerDescriptorLocalToWorldBuffer", size);
		m_Buffers.Add(LocalToWorld);
		VertexToSkinnedVertexMapRange = new GraphicsBufferWrapper<int2>("_XpbdMeshDeformerDescriptorVertexToSkinnedVertexMapRangeBuffer", size);
		m_Buffers.Add(VertexToSkinnedVertexMapRange);
		DeformableVerticesRange = new GraphicsBufferWrapper<int2>("_XpbdMeshDeformerDescriptorDeformableVerticesRangeBuffer", size);
		m_Buffers.Add(DeformableVerticesRange);
		SkinnedVerticesRange = new GraphicsBufferWrapper<int2>("_XpbdMeshDeformerDescriptorSkinnedVerticesRangeBuffer", size);
		m_Buffers.Add(SkinnedVerticesRange);
	}

	public override void SetData(MeshDeformerDescriptorSoA data)
	{
		WorldToLocal.SetData(data.WorldToLocal);
		BindingsRange.SetData(data.BindingsRange);
		LocalToWorld.SetData(data.LocalToWorld);
		VertexToSkinnedVertexMapRange.SetData(data.VertexToSkinnedVertexMapRange);
		DeformableVerticesRange.SetData(data.DeformableVerticesRange);
		SkinnedVerticesRange.SetData(data.SkinnedVerticesRange);
	}

	public override void SetData(MeshDeformerDescriptorSoA data, int offset, int count)
	{
		WorldToLocal.SetData(data.WorldToLocal, offset, offset, count);
		BindingsRange.SetData(data.BindingsRange, offset, offset, count);
		LocalToWorld.SetData(data.LocalToWorld, offset, offset, count);
		VertexToSkinnedVertexMapRange.SetData(data.VertexToSkinnedVertexMapRange, offset, offset, count);
		DeformableVerticesRange.SetData(data.DeformableVerticesRange, offset, offset, count);
		SkinnedVerticesRange.SetData(data.SkinnedVerticesRange, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, MeshDeformerDescriptorSoA data)
	{
		cmd.SetBufferData(WorldToLocal.Buffer, data.WorldToLocal);
		cmd.SetBufferData(BindingsRange.Buffer, data.BindingsRange);
		cmd.SetBufferData(LocalToWorld.Buffer, data.LocalToWorld);
		cmd.SetBufferData(VertexToSkinnedVertexMapRange.Buffer, data.VertexToSkinnedVertexMapRange);
		cmd.SetBufferData(DeformableVerticesRange.Buffer, data.DeformableVerticesRange);
		cmd.SetBufferData(SkinnedVerticesRange.Buffer, data.SkinnedVerticesRange);
	}

	public override void SetData(CommandBuffer cmd, MeshDeformerDescriptorSoA data, int offset, int count)
	{
		cmd.SetBufferData(WorldToLocal.Buffer, data.WorldToLocal, offset, offset, count);
		cmd.SetBufferData(BindingsRange.Buffer, data.BindingsRange, offset, offset, count);
		cmd.SetBufferData(LocalToWorld.Buffer, data.LocalToWorld, offset, offset, count);
		cmd.SetBufferData(VertexToSkinnedVertexMapRange.Buffer, data.VertexToSkinnedVertexMapRange, offset, offset, count);
		cmd.SetBufferData(DeformableVerticesRange.Buffer, data.DeformableVerticesRange, offset, offset, count);
		cmd.SetBufferData(SkinnedVerticesRange.Buffer, data.SkinnedVerticesRange, offset, offset, count);
	}
}
