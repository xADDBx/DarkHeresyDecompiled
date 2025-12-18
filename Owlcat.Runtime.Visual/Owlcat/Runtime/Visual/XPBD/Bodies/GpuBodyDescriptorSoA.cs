using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public class GpuBodyDescriptorSoA : GpuStructureOfArrays<BodyDescriptor, BodyDescriptorSoA>
{
	public GraphicsBufferWrapper<int2> BoneIndicesMapRange;

	public GraphicsBufferWrapper<int2> VerticesRange;

	public GraphicsBufferWrapper<int2> SkinBufferRange;

	public GraphicsBufferWrapper<int> Enabled;

	public GraphicsBufferWrapper<Aabb> Aabb;

	public GraphicsBufferWrapper<int2> VertexTrianglesMapRange;

	public GraphicsBufferWrapper<int2> VertexToParticleRange;

	public GraphicsBufferWrapper<int2> SimplexConstraintsRange;

	public GraphicsBufferWrapper<int2> ParticlesRange;

	public GraphicsBufferWrapper<int> Layer;

	public GraphicsBufferWrapper<InertialForces> InertialForces;

	public GraphicsBufferWrapper<int2> ParticleToVertexRange;

	public GraphicsBufferWrapper<int2> ConstraintSettingsRange;

	public GraphicsBufferWrapper<InertialFrame> InertialFrame;

	public GraphicsBufferWrapper<int2> ConstraintBatchesRange;

	public GraphicsBufferWrapper<float4> BodySimulationParameters;

	public GraphicsBufferWrapper<float4x4> PrevWorldToLocal;

	public GraphicsBufferWrapper<int2> ConstraintsRange;

	public GraphicsBufferWrapper<float4x4> WorldToLocal;

	public GraphicsBufferWrapper<int2> TrianglesRange;

	public GraphicsBufferWrapper<int2> BonesRange;

	public GraphicsBufferWrapper<int2> VertexTrianglesMapRangesRange;

	public GraphicsBufferWrapper<int2> BoneIndicesMapRangesRange;

	public GraphicsBufferWrapper<float4x4> LocalToWorld;

	public GraphicsBufferWrapper<uint> EnabledConstraintTypeMask;

	public GraphicsBufferWrapper<int2> MeshLocalVerticesRange;

	public GpuBodyDescriptorSoA(int size)
		: base(size)
	{
		BoneIndicesMapRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorBoneIndicesMapRangeBuffer", size);
		m_Buffers.Add(BoneIndicesMapRange);
		VerticesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorVerticesRangeBuffer", size);
		m_Buffers.Add(VerticesRange);
		SkinBufferRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorSkinBufferRangeBuffer", size);
		m_Buffers.Add(SkinBufferRange);
		Enabled = new GraphicsBufferWrapper<int>("_XpbdBodyDescriptorEnabledBuffer", size);
		m_Buffers.Add(Enabled);
		Aabb = new GraphicsBufferWrapper<Aabb>("_XpbdBodyDescriptorAabbBuffer", size);
		m_Buffers.Add(Aabb);
		VertexTrianglesMapRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorVertexTrianglesMapRangeBuffer", size);
		m_Buffers.Add(VertexTrianglesMapRange);
		VertexToParticleRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorVertexToParticleRangeBuffer", size);
		m_Buffers.Add(VertexToParticleRange);
		SimplexConstraintsRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorSimplexConstraintsRangeBuffer", size);
		m_Buffers.Add(SimplexConstraintsRange);
		ParticlesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorParticlesRangeBuffer", size);
		m_Buffers.Add(ParticlesRange);
		Layer = new GraphicsBufferWrapper<int>("_XpbdBodyDescriptorLayerBuffer", size);
		m_Buffers.Add(Layer);
		InertialForces = new GraphicsBufferWrapper<InertialForces>("_XpbdBodyDescriptorInertialForcesBuffer", size);
		m_Buffers.Add(InertialForces);
		ParticleToVertexRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorParticleToVertexRangeBuffer", size);
		m_Buffers.Add(ParticleToVertexRange);
		ConstraintSettingsRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorConstraintSettingsRangeBuffer", size);
		m_Buffers.Add(ConstraintSettingsRange);
		InertialFrame = new GraphicsBufferWrapper<InertialFrame>("_XpbdBodyDescriptorInertialFrameBuffer", size);
		m_Buffers.Add(InertialFrame);
		ConstraintBatchesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorConstraintBatchesRangeBuffer", size);
		m_Buffers.Add(ConstraintBatchesRange);
		BodySimulationParameters = new GraphicsBufferWrapper<float4>("_XpbdBodyDescriptorBodySimulationParametersBuffer", size);
		m_Buffers.Add(BodySimulationParameters);
		PrevWorldToLocal = new GraphicsBufferWrapper<float4x4>("_XpbdBodyDescriptorPrevWorldToLocalBuffer", size);
		m_Buffers.Add(PrevWorldToLocal);
		ConstraintsRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorConstraintsRangeBuffer", size);
		m_Buffers.Add(ConstraintsRange);
		WorldToLocal = new GraphicsBufferWrapper<float4x4>("_XpbdBodyDescriptorWorldToLocalBuffer", size);
		m_Buffers.Add(WorldToLocal);
		TrianglesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorTrianglesRangeBuffer", size);
		m_Buffers.Add(TrianglesRange);
		BonesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorBonesRangeBuffer", size);
		m_Buffers.Add(BonesRange);
		VertexTrianglesMapRangesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorVertexTrianglesMapRangesRangeBuffer", size);
		m_Buffers.Add(VertexTrianglesMapRangesRange);
		BoneIndicesMapRangesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorBoneIndicesMapRangesRangeBuffer", size);
		m_Buffers.Add(BoneIndicesMapRangesRange);
		LocalToWorld = new GraphicsBufferWrapper<float4x4>("_XpbdBodyDescriptorLocalToWorldBuffer", size);
		m_Buffers.Add(LocalToWorld);
		EnabledConstraintTypeMask = new GraphicsBufferWrapper<uint>("_XpbdBodyDescriptorEnabledConstraintTypeMaskBuffer", size);
		m_Buffers.Add(EnabledConstraintTypeMask);
		MeshLocalVerticesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorMeshLocalVerticesRangeBuffer", size);
		m_Buffers.Add(MeshLocalVerticesRange);
	}

	public override void SetData(BodyDescriptorSoA data)
	{
		BoneIndicesMapRange.SetData(data.BoneIndicesMapRange);
		VerticesRange.SetData(data.VerticesRange);
		SkinBufferRange.SetData(data.SkinBufferRange);
		Enabled.SetData(data.Enabled);
		Aabb.SetData(data.Aabb);
		VertexTrianglesMapRange.SetData(data.VertexTrianglesMapRange);
		VertexToParticleRange.SetData(data.VertexToParticleRange);
		SimplexConstraintsRange.SetData(data.SimplexConstraintsRange);
		ParticlesRange.SetData(data.ParticlesRange);
		Layer.SetData(data.Layer);
		InertialForces.SetData(data.InertialForces);
		ParticleToVertexRange.SetData(data.ParticleToVertexRange);
		ConstraintSettingsRange.SetData(data.ConstraintSettingsRange);
		InertialFrame.SetData(data.InertialFrame);
		ConstraintBatchesRange.SetData(data.ConstraintBatchesRange);
		BodySimulationParameters.SetData(data.BodySimulationParameters);
		PrevWorldToLocal.SetData(data.PrevWorldToLocal);
		ConstraintsRange.SetData(data.ConstraintsRange);
		WorldToLocal.SetData(data.WorldToLocal);
		TrianglesRange.SetData(data.TrianglesRange);
		BonesRange.SetData(data.BonesRange);
		VertexTrianglesMapRangesRange.SetData(data.VertexTrianglesMapRangesRange);
		BoneIndicesMapRangesRange.SetData(data.BoneIndicesMapRangesRange);
		LocalToWorld.SetData(data.LocalToWorld);
		EnabledConstraintTypeMask.SetData(data.EnabledConstraintTypeMask);
		MeshLocalVerticesRange.SetData(data.MeshLocalVerticesRange);
	}

	public override void SetData(BodyDescriptorSoA data, int offset, int count)
	{
		BoneIndicesMapRange.SetData(data.BoneIndicesMapRange, offset, offset, count);
		VerticesRange.SetData(data.VerticesRange, offset, offset, count);
		SkinBufferRange.SetData(data.SkinBufferRange, offset, offset, count);
		Enabled.SetData(data.Enabled, offset, offset, count);
		Aabb.SetData(data.Aabb, offset, offset, count);
		VertexTrianglesMapRange.SetData(data.VertexTrianglesMapRange, offset, offset, count);
		VertexToParticleRange.SetData(data.VertexToParticleRange, offset, offset, count);
		SimplexConstraintsRange.SetData(data.SimplexConstraintsRange, offset, offset, count);
		ParticlesRange.SetData(data.ParticlesRange, offset, offset, count);
		Layer.SetData(data.Layer, offset, offset, count);
		InertialForces.SetData(data.InertialForces, offset, offset, count);
		ParticleToVertexRange.SetData(data.ParticleToVertexRange, offset, offset, count);
		ConstraintSettingsRange.SetData(data.ConstraintSettingsRange, offset, offset, count);
		InertialFrame.SetData(data.InertialFrame, offset, offset, count);
		ConstraintBatchesRange.SetData(data.ConstraintBatchesRange, offset, offset, count);
		BodySimulationParameters.SetData(data.BodySimulationParameters, offset, offset, count);
		PrevWorldToLocal.SetData(data.PrevWorldToLocal, offset, offset, count);
		ConstraintsRange.SetData(data.ConstraintsRange, offset, offset, count);
		WorldToLocal.SetData(data.WorldToLocal, offset, offset, count);
		TrianglesRange.SetData(data.TrianglesRange, offset, offset, count);
		BonesRange.SetData(data.BonesRange, offset, offset, count);
		VertexTrianglesMapRangesRange.SetData(data.VertexTrianglesMapRangesRange, offset, offset, count);
		BoneIndicesMapRangesRange.SetData(data.BoneIndicesMapRangesRange, offset, offset, count);
		LocalToWorld.SetData(data.LocalToWorld, offset, offset, count);
		EnabledConstraintTypeMask.SetData(data.EnabledConstraintTypeMask, offset, offset, count);
		MeshLocalVerticesRange.SetData(data.MeshLocalVerticesRange, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, BodyDescriptorSoA data)
	{
		cmd.SetBufferData(BoneIndicesMapRange.Buffer, data.BoneIndicesMapRange);
		cmd.SetBufferData(VerticesRange.Buffer, data.VerticesRange);
		cmd.SetBufferData(SkinBufferRange.Buffer, data.SkinBufferRange);
		cmd.SetBufferData(Enabled.Buffer, data.Enabled);
		cmd.SetBufferData(Aabb.Buffer, data.Aabb);
		cmd.SetBufferData(VertexTrianglesMapRange.Buffer, data.VertexTrianglesMapRange);
		cmd.SetBufferData(VertexToParticleRange.Buffer, data.VertexToParticleRange);
		cmd.SetBufferData(SimplexConstraintsRange.Buffer, data.SimplexConstraintsRange);
		cmd.SetBufferData(ParticlesRange.Buffer, data.ParticlesRange);
		cmd.SetBufferData(Layer.Buffer, data.Layer);
		cmd.SetBufferData(InertialForces.Buffer, data.InertialForces);
		cmd.SetBufferData(ParticleToVertexRange.Buffer, data.ParticleToVertexRange);
		cmd.SetBufferData(ConstraintSettingsRange.Buffer, data.ConstraintSettingsRange);
		cmd.SetBufferData(InertialFrame.Buffer, data.InertialFrame);
		cmd.SetBufferData(ConstraintBatchesRange.Buffer, data.ConstraintBatchesRange);
		cmd.SetBufferData(BodySimulationParameters.Buffer, data.BodySimulationParameters);
		cmd.SetBufferData(PrevWorldToLocal.Buffer, data.PrevWorldToLocal);
		cmd.SetBufferData(ConstraintsRange.Buffer, data.ConstraintsRange);
		cmd.SetBufferData(WorldToLocal.Buffer, data.WorldToLocal);
		cmd.SetBufferData(TrianglesRange.Buffer, data.TrianglesRange);
		cmd.SetBufferData(BonesRange.Buffer, data.BonesRange);
		cmd.SetBufferData(VertexTrianglesMapRangesRange.Buffer, data.VertexTrianglesMapRangesRange);
		cmd.SetBufferData(BoneIndicesMapRangesRange.Buffer, data.BoneIndicesMapRangesRange);
		cmd.SetBufferData(LocalToWorld.Buffer, data.LocalToWorld);
		cmd.SetBufferData(EnabledConstraintTypeMask.Buffer, data.EnabledConstraintTypeMask);
		cmd.SetBufferData(MeshLocalVerticesRange.Buffer, data.MeshLocalVerticesRange);
	}

	public override void SetData(CommandBuffer cmd, BodyDescriptorSoA data, int offset, int count)
	{
		cmd.SetBufferData(BoneIndicesMapRange.Buffer, data.BoneIndicesMapRange, offset, offset, count);
		cmd.SetBufferData(VerticesRange.Buffer, data.VerticesRange, offset, offset, count);
		cmd.SetBufferData(SkinBufferRange.Buffer, data.SkinBufferRange, offset, offset, count);
		cmd.SetBufferData(Enabled.Buffer, data.Enabled, offset, offset, count);
		cmd.SetBufferData(Aabb.Buffer, data.Aabb, offset, offset, count);
		cmd.SetBufferData(VertexTrianglesMapRange.Buffer, data.VertexTrianglesMapRange, offset, offset, count);
		cmd.SetBufferData(VertexToParticleRange.Buffer, data.VertexToParticleRange, offset, offset, count);
		cmd.SetBufferData(SimplexConstraintsRange.Buffer, data.SimplexConstraintsRange, offset, offset, count);
		cmd.SetBufferData(ParticlesRange.Buffer, data.ParticlesRange, offset, offset, count);
		cmd.SetBufferData(Layer.Buffer, data.Layer, offset, offset, count);
		cmd.SetBufferData(InertialForces.Buffer, data.InertialForces, offset, offset, count);
		cmd.SetBufferData(ParticleToVertexRange.Buffer, data.ParticleToVertexRange, offset, offset, count);
		cmd.SetBufferData(ConstraintSettingsRange.Buffer, data.ConstraintSettingsRange, offset, offset, count);
		cmd.SetBufferData(InertialFrame.Buffer, data.InertialFrame, offset, offset, count);
		cmd.SetBufferData(ConstraintBatchesRange.Buffer, data.ConstraintBatchesRange, offset, offset, count);
		cmd.SetBufferData(BodySimulationParameters.Buffer, data.BodySimulationParameters, offset, offset, count);
		cmd.SetBufferData(PrevWorldToLocal.Buffer, data.PrevWorldToLocal, offset, offset, count);
		cmd.SetBufferData(ConstraintsRange.Buffer, data.ConstraintsRange, offset, offset, count);
		cmd.SetBufferData(WorldToLocal.Buffer, data.WorldToLocal, offset, offset, count);
		cmd.SetBufferData(TrianglesRange.Buffer, data.TrianglesRange, offset, offset, count);
		cmd.SetBufferData(BonesRange.Buffer, data.BonesRange, offset, offset, count);
		cmd.SetBufferData(VertexTrianglesMapRangesRange.Buffer, data.VertexTrianglesMapRangesRange, offset, offset, count);
		cmd.SetBufferData(BoneIndicesMapRangesRange.Buffer, data.BoneIndicesMapRangesRange, offset, offset, count);
		cmd.SetBufferData(LocalToWorld.Buffer, data.LocalToWorld, offset, offset, count);
		cmd.SetBufferData(EnabledConstraintTypeMask.Buffer, data.EnabledConstraintTypeMask, offset, offset, count);
		cmd.SetBufferData(MeshLocalVerticesRange.Buffer, data.MeshLocalVerticesRange, offset, offset, count);
	}
}
