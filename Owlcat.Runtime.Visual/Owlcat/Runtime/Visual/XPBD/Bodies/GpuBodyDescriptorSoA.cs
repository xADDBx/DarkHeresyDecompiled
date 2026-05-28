using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public class GpuBodyDescriptorSoA : GpuStructureOfArrays<BodyDescriptor, BodyDescriptorSoA>
{
	public GraphicsBufferWrapper<float4x4> WorldToLocal;

	public GraphicsBufferWrapper<int2> BoneIndicesMapRangesRange;

	public GraphicsBufferWrapper<int2> MeshLocalVerticesRange;

	public GraphicsBufferWrapper<int2> VertexToParticleRange;

	public GraphicsBufferWrapper<float4x4> LocalToWorld;

	public GraphicsBufferWrapper<int> Layer;

	public GraphicsBufferWrapper<int2> BonesRange;

	public GraphicsBufferWrapper<int2> SkinBufferRange;

	public GraphicsBufferWrapper<int2> BoneIndicesMapRange;

	public GraphicsBufferWrapper<int2> ConstraintSettingsRange;

	public GraphicsBufferWrapper<int2> SimplexConstraintsRange;

	public GraphicsBufferWrapper<int2> ConstraintBatchesRange;

	public GraphicsBufferWrapper<int2> ParticleToVertexRange;

	public GraphicsBufferWrapper<int2> ParticlesRange;

	public GraphicsBufferWrapper<InertialForces> InertialForces;

	public GraphicsBufferWrapper<Aabb> Aabb;

	public GraphicsBufferWrapper<int2> VerticesRange;

	public GraphicsBufferWrapper<InertialFrame> InertialFrame;

	public GraphicsBufferWrapper<int2> TrianglesRange;

	public GraphicsBufferWrapper<float4x4> PrevWorldToLocal;

	public GraphicsBufferWrapper<int2> ConstraintsRange;

	public GraphicsBufferWrapper<float4> BodySimulationParameters;

	public GraphicsBufferWrapper<uint> EnabledConstraintTypeMask;

	public GraphicsBufferWrapper<int2> VertexTrianglesMapRangesRange;

	public GraphicsBufferWrapper<int> Enabled;

	public GraphicsBufferWrapper<int2> VertexTrianglesMapRange;

	public GpuBodyDescriptorSoA(int size)
		: base(size)
	{
		WorldToLocal = new GraphicsBufferWrapper<float4x4>("_XpbdBodyDescriptorWorldToLocalBuffer", size);
		m_Buffers.Add(WorldToLocal);
		BoneIndicesMapRangesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorBoneIndicesMapRangesRangeBuffer", size);
		m_Buffers.Add(BoneIndicesMapRangesRange);
		MeshLocalVerticesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorMeshLocalVerticesRangeBuffer", size);
		m_Buffers.Add(MeshLocalVerticesRange);
		VertexToParticleRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorVertexToParticleRangeBuffer", size);
		m_Buffers.Add(VertexToParticleRange);
		LocalToWorld = new GraphicsBufferWrapper<float4x4>("_XpbdBodyDescriptorLocalToWorldBuffer", size);
		m_Buffers.Add(LocalToWorld);
		Layer = new GraphicsBufferWrapper<int>("_XpbdBodyDescriptorLayerBuffer", size);
		m_Buffers.Add(Layer);
		BonesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorBonesRangeBuffer", size);
		m_Buffers.Add(BonesRange);
		SkinBufferRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorSkinBufferRangeBuffer", size);
		m_Buffers.Add(SkinBufferRange);
		BoneIndicesMapRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorBoneIndicesMapRangeBuffer", size);
		m_Buffers.Add(BoneIndicesMapRange);
		ConstraintSettingsRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorConstraintSettingsRangeBuffer", size);
		m_Buffers.Add(ConstraintSettingsRange);
		SimplexConstraintsRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorSimplexConstraintsRangeBuffer", size);
		m_Buffers.Add(SimplexConstraintsRange);
		ConstraintBatchesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorConstraintBatchesRangeBuffer", size);
		m_Buffers.Add(ConstraintBatchesRange);
		ParticleToVertexRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorParticleToVertexRangeBuffer", size);
		m_Buffers.Add(ParticleToVertexRange);
		ParticlesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorParticlesRangeBuffer", size);
		m_Buffers.Add(ParticlesRange);
		InertialForces = new GraphicsBufferWrapper<InertialForces>("_XpbdBodyDescriptorInertialForcesBuffer", size);
		m_Buffers.Add(InertialForces);
		Aabb = new GraphicsBufferWrapper<Aabb>("_XpbdBodyDescriptorAabbBuffer", size);
		m_Buffers.Add(Aabb);
		VerticesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorVerticesRangeBuffer", size);
		m_Buffers.Add(VerticesRange);
		InertialFrame = new GraphicsBufferWrapper<InertialFrame>("_XpbdBodyDescriptorInertialFrameBuffer", size);
		m_Buffers.Add(InertialFrame);
		TrianglesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorTrianglesRangeBuffer", size);
		m_Buffers.Add(TrianglesRange);
		PrevWorldToLocal = new GraphicsBufferWrapper<float4x4>("_XpbdBodyDescriptorPrevWorldToLocalBuffer", size);
		m_Buffers.Add(PrevWorldToLocal);
		ConstraintsRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorConstraintsRangeBuffer", size);
		m_Buffers.Add(ConstraintsRange);
		BodySimulationParameters = new GraphicsBufferWrapper<float4>("_XpbdBodyDescriptorBodySimulationParametersBuffer", size);
		m_Buffers.Add(BodySimulationParameters);
		EnabledConstraintTypeMask = new GraphicsBufferWrapper<uint>("_XpbdBodyDescriptorEnabledConstraintTypeMaskBuffer", size);
		m_Buffers.Add(EnabledConstraintTypeMask);
		VertexTrianglesMapRangesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorVertexTrianglesMapRangesRangeBuffer", size);
		m_Buffers.Add(VertexTrianglesMapRangesRange);
		Enabled = new GraphicsBufferWrapper<int>("_XpbdBodyDescriptorEnabledBuffer", size);
		m_Buffers.Add(Enabled);
		VertexTrianglesMapRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorVertexTrianglesMapRangeBuffer", size);
		m_Buffers.Add(VertexTrianglesMapRange);
	}

	public override void SetData(BodyDescriptorSoA data)
	{
		WorldToLocal.SetData(data.WorldToLocal);
		BoneIndicesMapRangesRange.SetData(data.BoneIndicesMapRangesRange);
		MeshLocalVerticesRange.SetData(data.MeshLocalVerticesRange);
		VertexToParticleRange.SetData(data.VertexToParticleRange);
		LocalToWorld.SetData(data.LocalToWorld);
		Layer.SetData(data.Layer);
		BonesRange.SetData(data.BonesRange);
		SkinBufferRange.SetData(data.SkinBufferRange);
		BoneIndicesMapRange.SetData(data.BoneIndicesMapRange);
		ConstraintSettingsRange.SetData(data.ConstraintSettingsRange);
		SimplexConstraintsRange.SetData(data.SimplexConstraintsRange);
		ConstraintBatchesRange.SetData(data.ConstraintBatchesRange);
		ParticleToVertexRange.SetData(data.ParticleToVertexRange);
		ParticlesRange.SetData(data.ParticlesRange);
		InertialForces.SetData(data.InertialForces);
		Aabb.SetData(data.Aabb);
		VerticesRange.SetData(data.VerticesRange);
		InertialFrame.SetData(data.InertialFrame);
		TrianglesRange.SetData(data.TrianglesRange);
		PrevWorldToLocal.SetData(data.PrevWorldToLocal);
		ConstraintsRange.SetData(data.ConstraintsRange);
		BodySimulationParameters.SetData(data.BodySimulationParameters);
		EnabledConstraintTypeMask.SetData(data.EnabledConstraintTypeMask);
		VertexTrianglesMapRangesRange.SetData(data.VertexTrianglesMapRangesRange);
		Enabled.SetData(data.Enabled);
		VertexTrianglesMapRange.SetData(data.VertexTrianglesMapRange);
	}

	public override void SetData(BodyDescriptorSoA data, int offset, int count)
	{
		WorldToLocal.SetData(data.WorldToLocal, offset, offset, count);
		BoneIndicesMapRangesRange.SetData(data.BoneIndicesMapRangesRange, offset, offset, count);
		MeshLocalVerticesRange.SetData(data.MeshLocalVerticesRange, offset, offset, count);
		VertexToParticleRange.SetData(data.VertexToParticleRange, offset, offset, count);
		LocalToWorld.SetData(data.LocalToWorld, offset, offset, count);
		Layer.SetData(data.Layer, offset, offset, count);
		BonesRange.SetData(data.BonesRange, offset, offset, count);
		SkinBufferRange.SetData(data.SkinBufferRange, offset, offset, count);
		BoneIndicesMapRange.SetData(data.BoneIndicesMapRange, offset, offset, count);
		ConstraintSettingsRange.SetData(data.ConstraintSettingsRange, offset, offset, count);
		SimplexConstraintsRange.SetData(data.SimplexConstraintsRange, offset, offset, count);
		ConstraintBatchesRange.SetData(data.ConstraintBatchesRange, offset, offset, count);
		ParticleToVertexRange.SetData(data.ParticleToVertexRange, offset, offset, count);
		ParticlesRange.SetData(data.ParticlesRange, offset, offset, count);
		InertialForces.SetData(data.InertialForces, offset, offset, count);
		Aabb.SetData(data.Aabb, offset, offset, count);
		VerticesRange.SetData(data.VerticesRange, offset, offset, count);
		InertialFrame.SetData(data.InertialFrame, offset, offset, count);
		TrianglesRange.SetData(data.TrianglesRange, offset, offset, count);
		PrevWorldToLocal.SetData(data.PrevWorldToLocal, offset, offset, count);
		ConstraintsRange.SetData(data.ConstraintsRange, offset, offset, count);
		BodySimulationParameters.SetData(data.BodySimulationParameters, offset, offset, count);
		EnabledConstraintTypeMask.SetData(data.EnabledConstraintTypeMask, offset, offset, count);
		VertexTrianglesMapRangesRange.SetData(data.VertexTrianglesMapRangesRange, offset, offset, count);
		Enabled.SetData(data.Enabled, offset, offset, count);
		VertexTrianglesMapRange.SetData(data.VertexTrianglesMapRange, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, BodyDescriptorSoA data)
	{
		cmd.SetBufferData(WorldToLocal.Buffer, data.WorldToLocal);
		cmd.SetBufferData(BoneIndicesMapRangesRange.Buffer, data.BoneIndicesMapRangesRange);
		cmd.SetBufferData(MeshLocalVerticesRange.Buffer, data.MeshLocalVerticesRange);
		cmd.SetBufferData(VertexToParticleRange.Buffer, data.VertexToParticleRange);
		cmd.SetBufferData(LocalToWorld.Buffer, data.LocalToWorld);
		cmd.SetBufferData(Layer.Buffer, data.Layer);
		cmd.SetBufferData(BonesRange.Buffer, data.BonesRange);
		cmd.SetBufferData(SkinBufferRange.Buffer, data.SkinBufferRange);
		cmd.SetBufferData(BoneIndicesMapRange.Buffer, data.BoneIndicesMapRange);
		cmd.SetBufferData(ConstraintSettingsRange.Buffer, data.ConstraintSettingsRange);
		cmd.SetBufferData(SimplexConstraintsRange.Buffer, data.SimplexConstraintsRange);
		cmd.SetBufferData(ConstraintBatchesRange.Buffer, data.ConstraintBatchesRange);
		cmd.SetBufferData(ParticleToVertexRange.Buffer, data.ParticleToVertexRange);
		cmd.SetBufferData(ParticlesRange.Buffer, data.ParticlesRange);
		cmd.SetBufferData(InertialForces.Buffer, data.InertialForces);
		cmd.SetBufferData(Aabb.Buffer, data.Aabb);
		cmd.SetBufferData(VerticesRange.Buffer, data.VerticesRange);
		cmd.SetBufferData(InertialFrame.Buffer, data.InertialFrame);
		cmd.SetBufferData(TrianglesRange.Buffer, data.TrianglesRange);
		cmd.SetBufferData(PrevWorldToLocal.Buffer, data.PrevWorldToLocal);
		cmd.SetBufferData(ConstraintsRange.Buffer, data.ConstraintsRange);
		cmd.SetBufferData(BodySimulationParameters.Buffer, data.BodySimulationParameters);
		cmd.SetBufferData(EnabledConstraintTypeMask.Buffer, data.EnabledConstraintTypeMask);
		cmd.SetBufferData(VertexTrianglesMapRangesRange.Buffer, data.VertexTrianglesMapRangesRange);
		cmd.SetBufferData(Enabled.Buffer, data.Enabled);
		cmd.SetBufferData(VertexTrianglesMapRange.Buffer, data.VertexTrianglesMapRange);
	}

	public override void SetData(CommandBuffer cmd, BodyDescriptorSoA data, int offset, int count)
	{
		cmd.SetBufferData(WorldToLocal.Buffer, data.WorldToLocal, offset, offset, count);
		cmd.SetBufferData(BoneIndicesMapRangesRange.Buffer, data.BoneIndicesMapRangesRange, offset, offset, count);
		cmd.SetBufferData(MeshLocalVerticesRange.Buffer, data.MeshLocalVerticesRange, offset, offset, count);
		cmd.SetBufferData(VertexToParticleRange.Buffer, data.VertexToParticleRange, offset, offset, count);
		cmd.SetBufferData(LocalToWorld.Buffer, data.LocalToWorld, offset, offset, count);
		cmd.SetBufferData(Layer.Buffer, data.Layer, offset, offset, count);
		cmd.SetBufferData(BonesRange.Buffer, data.BonesRange, offset, offset, count);
		cmd.SetBufferData(SkinBufferRange.Buffer, data.SkinBufferRange, offset, offset, count);
		cmd.SetBufferData(BoneIndicesMapRange.Buffer, data.BoneIndicesMapRange, offset, offset, count);
		cmd.SetBufferData(ConstraintSettingsRange.Buffer, data.ConstraintSettingsRange, offset, offset, count);
		cmd.SetBufferData(SimplexConstraintsRange.Buffer, data.SimplexConstraintsRange, offset, offset, count);
		cmd.SetBufferData(ConstraintBatchesRange.Buffer, data.ConstraintBatchesRange, offset, offset, count);
		cmd.SetBufferData(ParticleToVertexRange.Buffer, data.ParticleToVertexRange, offset, offset, count);
		cmd.SetBufferData(ParticlesRange.Buffer, data.ParticlesRange, offset, offset, count);
		cmd.SetBufferData(InertialForces.Buffer, data.InertialForces, offset, offset, count);
		cmd.SetBufferData(Aabb.Buffer, data.Aabb, offset, offset, count);
		cmd.SetBufferData(VerticesRange.Buffer, data.VerticesRange, offset, offset, count);
		cmd.SetBufferData(InertialFrame.Buffer, data.InertialFrame, offset, offset, count);
		cmd.SetBufferData(TrianglesRange.Buffer, data.TrianglesRange, offset, offset, count);
		cmd.SetBufferData(PrevWorldToLocal.Buffer, data.PrevWorldToLocal, offset, offset, count);
		cmd.SetBufferData(ConstraintsRange.Buffer, data.ConstraintsRange, offset, offset, count);
		cmd.SetBufferData(BodySimulationParameters.Buffer, data.BodySimulationParameters, offset, offset, count);
		cmd.SetBufferData(EnabledConstraintTypeMask.Buffer, data.EnabledConstraintTypeMask, offset, offset, count);
		cmd.SetBufferData(VertexTrianglesMapRangesRange.Buffer, data.VertexTrianglesMapRangesRange, offset, offset, count);
		cmd.SetBufferData(Enabled.Buffer, data.Enabled, offset, offset, count);
		cmd.SetBufferData(VertexTrianglesMapRange.Buffer, data.VertexTrianglesMapRange, offset, offset, count);
	}
}
