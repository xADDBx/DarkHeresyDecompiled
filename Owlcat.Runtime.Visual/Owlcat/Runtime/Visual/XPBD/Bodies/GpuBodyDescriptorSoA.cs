using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public class GpuBodyDescriptorSoA : GpuStructureOfArrays<BodyDescriptor, BodyDescriptorSoA>
{
	public GraphicsBufferWrapper<float4x4> PrevWorldToLocal;

	public GraphicsBufferWrapper<int2> ConstraintSettingsRange;

	public GraphicsBufferWrapper<int2> SkinBufferRange;

	public GraphicsBufferWrapper<int2> VertexTrianglesMapRangesRange;

	public GraphicsBufferWrapper<InertialFrame> InertialFrame;

	public GraphicsBufferWrapper<int2> ConstraintBatchesRange;

	public GraphicsBufferWrapper<InertialForces> InertialForces;

	public GraphicsBufferWrapper<int2> BoneIndicesMapRange;

	public GraphicsBufferWrapper<Aabb> Aabb;

	public GraphicsBufferWrapper<int2> TrianglesRange;

	public GraphicsBufferWrapper<int2> SimplexConstraintsRange;

	public GraphicsBufferWrapper<float4x4> LocalToWorld;

	public GraphicsBufferWrapper<int2> VertexTrianglesMapRange;

	public GraphicsBufferWrapper<int> Layer;

	public GraphicsBufferWrapper<int2> BonesRange;

	public GraphicsBufferWrapper<int2> VerticesRange;

	public GraphicsBufferWrapper<int2> BoneIndicesMapRangesRange;

	public GraphicsBufferWrapper<int2> VertexToParticleRange;

	public GraphicsBufferWrapper<int> Enabled;

	public GraphicsBufferWrapper<int2> MeshLocalVerticesRange;

	public GraphicsBufferWrapper<float4x4> WorldToLocal;

	public GraphicsBufferWrapper<int2> ParticlesRange;

	public GraphicsBufferWrapper<int2> ConstraintsRange;

	public GraphicsBufferWrapper<uint> EnabledConstraintTypeMask;

	public GraphicsBufferWrapper<int2> ParticleToVertexRange;

	public GraphicsBufferWrapper<float4> BodySimulationParameters;

	public GpuBodyDescriptorSoA(int size)
		: base(size)
	{
		PrevWorldToLocal = new GraphicsBufferWrapper<float4x4>("_XpbdBodyDescriptorPrevWorldToLocalBuffer", size);
		m_Buffers.Add(PrevWorldToLocal);
		ConstraintSettingsRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorConstraintSettingsRangeBuffer", size);
		m_Buffers.Add(ConstraintSettingsRange);
		SkinBufferRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorSkinBufferRangeBuffer", size);
		m_Buffers.Add(SkinBufferRange);
		VertexTrianglesMapRangesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorVertexTrianglesMapRangesRangeBuffer", size);
		m_Buffers.Add(VertexTrianglesMapRangesRange);
		InertialFrame = new GraphicsBufferWrapper<InertialFrame>("_XpbdBodyDescriptorInertialFrameBuffer", size);
		m_Buffers.Add(InertialFrame);
		ConstraintBatchesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorConstraintBatchesRangeBuffer", size);
		m_Buffers.Add(ConstraintBatchesRange);
		InertialForces = new GraphicsBufferWrapper<InertialForces>("_XpbdBodyDescriptorInertialForcesBuffer", size);
		m_Buffers.Add(InertialForces);
		BoneIndicesMapRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorBoneIndicesMapRangeBuffer", size);
		m_Buffers.Add(BoneIndicesMapRange);
		Aabb = new GraphicsBufferWrapper<Aabb>("_XpbdBodyDescriptorAabbBuffer", size);
		m_Buffers.Add(Aabb);
		TrianglesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorTrianglesRangeBuffer", size);
		m_Buffers.Add(TrianglesRange);
		SimplexConstraintsRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorSimplexConstraintsRangeBuffer", size);
		m_Buffers.Add(SimplexConstraintsRange);
		LocalToWorld = new GraphicsBufferWrapper<float4x4>("_XpbdBodyDescriptorLocalToWorldBuffer", size);
		m_Buffers.Add(LocalToWorld);
		VertexTrianglesMapRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorVertexTrianglesMapRangeBuffer", size);
		m_Buffers.Add(VertexTrianglesMapRange);
		Layer = new GraphicsBufferWrapper<int>("_XpbdBodyDescriptorLayerBuffer", size);
		m_Buffers.Add(Layer);
		BonesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorBonesRangeBuffer", size);
		m_Buffers.Add(BonesRange);
		VerticesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorVerticesRangeBuffer", size);
		m_Buffers.Add(VerticesRange);
		BoneIndicesMapRangesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorBoneIndicesMapRangesRangeBuffer", size);
		m_Buffers.Add(BoneIndicesMapRangesRange);
		VertexToParticleRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorVertexToParticleRangeBuffer", size);
		m_Buffers.Add(VertexToParticleRange);
		Enabled = new GraphicsBufferWrapper<int>("_XpbdBodyDescriptorEnabledBuffer", size);
		m_Buffers.Add(Enabled);
		MeshLocalVerticesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorMeshLocalVerticesRangeBuffer", size);
		m_Buffers.Add(MeshLocalVerticesRange);
		WorldToLocal = new GraphicsBufferWrapper<float4x4>("_XpbdBodyDescriptorWorldToLocalBuffer", size);
		m_Buffers.Add(WorldToLocal);
		ParticlesRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorParticlesRangeBuffer", size);
		m_Buffers.Add(ParticlesRange);
		ConstraintsRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorConstraintsRangeBuffer", size);
		m_Buffers.Add(ConstraintsRange);
		EnabledConstraintTypeMask = new GraphicsBufferWrapper<uint>("_XpbdBodyDescriptorEnabledConstraintTypeMaskBuffer", size);
		m_Buffers.Add(EnabledConstraintTypeMask);
		ParticleToVertexRange = new GraphicsBufferWrapper<int2>("_XpbdBodyDescriptorParticleToVertexRangeBuffer", size);
		m_Buffers.Add(ParticleToVertexRange);
		BodySimulationParameters = new GraphicsBufferWrapper<float4>("_XpbdBodyDescriptorBodySimulationParametersBuffer", size);
		m_Buffers.Add(BodySimulationParameters);
	}

	public override void SetData(BodyDescriptorSoA data)
	{
		PrevWorldToLocal.SetData(data.PrevWorldToLocal);
		ConstraintSettingsRange.SetData(data.ConstraintSettingsRange);
		SkinBufferRange.SetData(data.SkinBufferRange);
		VertexTrianglesMapRangesRange.SetData(data.VertexTrianglesMapRangesRange);
		InertialFrame.SetData(data.InertialFrame);
		ConstraintBatchesRange.SetData(data.ConstraintBatchesRange);
		InertialForces.SetData(data.InertialForces);
		BoneIndicesMapRange.SetData(data.BoneIndicesMapRange);
		Aabb.SetData(data.Aabb);
		TrianglesRange.SetData(data.TrianglesRange);
		SimplexConstraintsRange.SetData(data.SimplexConstraintsRange);
		LocalToWorld.SetData(data.LocalToWorld);
		VertexTrianglesMapRange.SetData(data.VertexTrianglesMapRange);
		Layer.SetData(data.Layer);
		BonesRange.SetData(data.BonesRange);
		VerticesRange.SetData(data.VerticesRange);
		BoneIndicesMapRangesRange.SetData(data.BoneIndicesMapRangesRange);
		VertexToParticleRange.SetData(data.VertexToParticleRange);
		Enabled.SetData(data.Enabled);
		MeshLocalVerticesRange.SetData(data.MeshLocalVerticesRange);
		WorldToLocal.SetData(data.WorldToLocal);
		ParticlesRange.SetData(data.ParticlesRange);
		ConstraintsRange.SetData(data.ConstraintsRange);
		EnabledConstraintTypeMask.SetData(data.EnabledConstraintTypeMask);
		ParticleToVertexRange.SetData(data.ParticleToVertexRange);
		BodySimulationParameters.SetData(data.BodySimulationParameters);
	}

	public override void SetData(BodyDescriptorSoA data, int offset, int count)
	{
		PrevWorldToLocal.SetData(data.PrevWorldToLocal, offset, offset, count);
		ConstraintSettingsRange.SetData(data.ConstraintSettingsRange, offset, offset, count);
		SkinBufferRange.SetData(data.SkinBufferRange, offset, offset, count);
		VertexTrianglesMapRangesRange.SetData(data.VertexTrianglesMapRangesRange, offset, offset, count);
		InertialFrame.SetData(data.InertialFrame, offset, offset, count);
		ConstraintBatchesRange.SetData(data.ConstraintBatchesRange, offset, offset, count);
		InertialForces.SetData(data.InertialForces, offset, offset, count);
		BoneIndicesMapRange.SetData(data.BoneIndicesMapRange, offset, offset, count);
		Aabb.SetData(data.Aabb, offset, offset, count);
		TrianglesRange.SetData(data.TrianglesRange, offset, offset, count);
		SimplexConstraintsRange.SetData(data.SimplexConstraintsRange, offset, offset, count);
		LocalToWorld.SetData(data.LocalToWorld, offset, offset, count);
		VertexTrianglesMapRange.SetData(data.VertexTrianglesMapRange, offset, offset, count);
		Layer.SetData(data.Layer, offset, offset, count);
		BonesRange.SetData(data.BonesRange, offset, offset, count);
		VerticesRange.SetData(data.VerticesRange, offset, offset, count);
		BoneIndicesMapRangesRange.SetData(data.BoneIndicesMapRangesRange, offset, offset, count);
		VertexToParticleRange.SetData(data.VertexToParticleRange, offset, offset, count);
		Enabled.SetData(data.Enabled, offset, offset, count);
		MeshLocalVerticesRange.SetData(data.MeshLocalVerticesRange, offset, offset, count);
		WorldToLocal.SetData(data.WorldToLocal, offset, offset, count);
		ParticlesRange.SetData(data.ParticlesRange, offset, offset, count);
		ConstraintsRange.SetData(data.ConstraintsRange, offset, offset, count);
		EnabledConstraintTypeMask.SetData(data.EnabledConstraintTypeMask, offset, offset, count);
		ParticleToVertexRange.SetData(data.ParticleToVertexRange, offset, offset, count);
		BodySimulationParameters.SetData(data.BodySimulationParameters, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, BodyDescriptorSoA data)
	{
		cmd.SetBufferData(PrevWorldToLocal.Buffer, data.PrevWorldToLocal);
		cmd.SetBufferData(ConstraintSettingsRange.Buffer, data.ConstraintSettingsRange);
		cmd.SetBufferData(SkinBufferRange.Buffer, data.SkinBufferRange);
		cmd.SetBufferData(VertexTrianglesMapRangesRange.Buffer, data.VertexTrianglesMapRangesRange);
		cmd.SetBufferData(InertialFrame.Buffer, data.InertialFrame);
		cmd.SetBufferData(ConstraintBatchesRange.Buffer, data.ConstraintBatchesRange);
		cmd.SetBufferData(InertialForces.Buffer, data.InertialForces);
		cmd.SetBufferData(BoneIndicesMapRange.Buffer, data.BoneIndicesMapRange);
		cmd.SetBufferData(Aabb.Buffer, data.Aabb);
		cmd.SetBufferData(TrianglesRange.Buffer, data.TrianglesRange);
		cmd.SetBufferData(SimplexConstraintsRange.Buffer, data.SimplexConstraintsRange);
		cmd.SetBufferData(LocalToWorld.Buffer, data.LocalToWorld);
		cmd.SetBufferData(VertexTrianglesMapRange.Buffer, data.VertexTrianglesMapRange);
		cmd.SetBufferData(Layer.Buffer, data.Layer);
		cmd.SetBufferData(BonesRange.Buffer, data.BonesRange);
		cmd.SetBufferData(VerticesRange.Buffer, data.VerticesRange);
		cmd.SetBufferData(BoneIndicesMapRangesRange.Buffer, data.BoneIndicesMapRangesRange);
		cmd.SetBufferData(VertexToParticleRange.Buffer, data.VertexToParticleRange);
		cmd.SetBufferData(Enabled.Buffer, data.Enabled);
		cmd.SetBufferData(MeshLocalVerticesRange.Buffer, data.MeshLocalVerticesRange);
		cmd.SetBufferData(WorldToLocal.Buffer, data.WorldToLocal);
		cmd.SetBufferData(ParticlesRange.Buffer, data.ParticlesRange);
		cmd.SetBufferData(ConstraintsRange.Buffer, data.ConstraintsRange);
		cmd.SetBufferData(EnabledConstraintTypeMask.Buffer, data.EnabledConstraintTypeMask);
		cmd.SetBufferData(ParticleToVertexRange.Buffer, data.ParticleToVertexRange);
		cmd.SetBufferData(BodySimulationParameters.Buffer, data.BodySimulationParameters);
	}

	public override void SetData(CommandBuffer cmd, BodyDescriptorSoA data, int offset, int count)
	{
		cmd.SetBufferData(PrevWorldToLocal.Buffer, data.PrevWorldToLocal, offset, offset, count);
		cmd.SetBufferData(ConstraintSettingsRange.Buffer, data.ConstraintSettingsRange, offset, offset, count);
		cmd.SetBufferData(SkinBufferRange.Buffer, data.SkinBufferRange, offset, offset, count);
		cmd.SetBufferData(VertexTrianglesMapRangesRange.Buffer, data.VertexTrianglesMapRangesRange, offset, offset, count);
		cmd.SetBufferData(InertialFrame.Buffer, data.InertialFrame, offset, offset, count);
		cmd.SetBufferData(ConstraintBatchesRange.Buffer, data.ConstraintBatchesRange, offset, offset, count);
		cmd.SetBufferData(InertialForces.Buffer, data.InertialForces, offset, offset, count);
		cmd.SetBufferData(BoneIndicesMapRange.Buffer, data.BoneIndicesMapRange, offset, offset, count);
		cmd.SetBufferData(Aabb.Buffer, data.Aabb, offset, offset, count);
		cmd.SetBufferData(TrianglesRange.Buffer, data.TrianglesRange, offset, offset, count);
		cmd.SetBufferData(SimplexConstraintsRange.Buffer, data.SimplexConstraintsRange, offset, offset, count);
		cmd.SetBufferData(LocalToWorld.Buffer, data.LocalToWorld, offset, offset, count);
		cmd.SetBufferData(VertexTrianglesMapRange.Buffer, data.VertexTrianglesMapRange, offset, offset, count);
		cmd.SetBufferData(Layer.Buffer, data.Layer, offset, offset, count);
		cmd.SetBufferData(BonesRange.Buffer, data.BonesRange, offset, offset, count);
		cmd.SetBufferData(VerticesRange.Buffer, data.VerticesRange, offset, offset, count);
		cmd.SetBufferData(BoneIndicesMapRangesRange.Buffer, data.BoneIndicesMapRangesRange, offset, offset, count);
		cmd.SetBufferData(VertexToParticleRange.Buffer, data.VertexToParticleRange, offset, offset, count);
		cmd.SetBufferData(Enabled.Buffer, data.Enabled, offset, offset, count);
		cmd.SetBufferData(MeshLocalVerticesRange.Buffer, data.MeshLocalVerticesRange, offset, offset, count);
		cmd.SetBufferData(WorldToLocal.Buffer, data.WorldToLocal, offset, offset, count);
		cmd.SetBufferData(ParticlesRange.Buffer, data.ParticlesRange, offset, offset, count);
		cmd.SetBufferData(ConstraintsRange.Buffer, data.ConstraintsRange, offset, offset, count);
		cmd.SetBufferData(EnabledConstraintTypeMask.Buffer, data.EnabledConstraintTypeMask, offset, offset, count);
		cmd.SetBufferData(ParticleToVertexRange.Buffer, data.ParticleToVertexRange, offset, offset, count);
		cmd.SetBufferData(BodySimulationParameters.Buffer, data.BodySimulationParameters, offset, offset, count);
	}
}
