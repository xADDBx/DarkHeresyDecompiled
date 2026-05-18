using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Core.WindSystem;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.Bodies;
using Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU;
using Owlcat.Runtime.Visual.XPBD.Constraints;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.Debug;
using Owlcat.Runtime.Visual.XPBD.Particles;
using Owlcat.Runtime.Visual.XPBD.Shaders;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Owlcat.Runtime.Visual.XPBD.Solvers.CPU.Jobs;
using Owlcat.Runtime.Visual.XPBD.Stats;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Solvers.CPU;

public class CpuSolverImpl : ISolverImpl
{
	private class DynamicData
	{
		private CpuSolverImpl m_SolverImpl;

		private NativeArray<float3> m_ParticleBasePosition;

		private NativeArray<float3> m_ParticlePrevPositioin;

		private NativeArray<float3> m_ParticlePosition;

		private NativeArray<float3> m_ParticleVelocity;

		private NativeArray<int2> m_ParticleRanges;

		public DynamicData(CpuSolverImpl solverImpl)
		{
			m_SolverImpl = solverImpl;
		}

		public void Store()
		{
			m_ParticleBasePosition = new NativeArray<float3>(m_SolverImpl.Solver.BodyAllocator.ParticleSoA.BasePosition, Allocator.Temp);
			m_ParticlePrevPositioin = new NativeArray<float3>(m_SolverImpl.Solver.BodyAllocator.ParticleSoA.PrevPosition, Allocator.Temp);
			m_ParticlePosition = new NativeArray<float3>(m_SolverImpl.Solver.BodyAllocator.ParticleSoA.Position, Allocator.Temp);
			m_ParticleVelocity = new NativeArray<float3>(m_SolverImpl.Solver.BodyAllocator.ParticleSoA.Velocity, Allocator.Temp);
			m_ParticleRanges = new NativeArray<int2>(m_SolverImpl.Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange, Allocator.Temp);
		}

		public void Roll()
		{
			foreach (KeyValuePair<AuthoringBase, int> item in m_SolverImpl.Solver.BodyAllocator.EntityAllocationMap)
			{
				AuthoringBase key = item.Key;
				if (key.IndexInSolver > -1)
				{
					int2 @int = m_ParticleRanges[key.IndexInSolver];
					int2 int2 = m_SolverImpl.Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange[item.Value];
					for (int i = 0; i < int2.y; i++)
					{
						Particle value = key.LayoutBase.BodyStructure.Particles[i];
						value.BasePosition = m_ParticleBasePosition[@int.x + i];
						value.PrevPosition = m_ParticlePrevPositioin[@int.x + i];
						value.Position = m_ParticlePosition[@int.x + i];
						value.Velocity = m_ParticleVelocity[@int.x + i];
						m_SolverImpl.Solver.BodyAllocator.ParticleSoA[int2.x + i] = value;
					}
				}
			}
			m_ParticleBasePosition.Dispose();
			m_ParticlePrevPositioin.Dispose();
			m_ParticlePosition.Dispose();
			m_ParticleVelocity.Dispose();
			m_ParticleRanges.Dispose();
		}
	}

	private class GpuUpdaters
	{
		private List<GpuBufferUpdater> m_Updaters;

		public GpuUpdaters(CpuSolverImpl solverImpl)
		{
			m_Updaters = new List<GpuBufferUpdater>
			{
				new GpuBufferUpdater<int>(() => solverImpl.Solver.BodyAllocator.VertexToParticleMapSoA.Array, solverImpl.m_VertexToParticleMapBuffer),
				new GpuBufferUpdater<float3>(() => solverImpl.Solver.BodyAllocator.VerticesSoA.Position, solverImpl.m_VertexPositionBuffer),
				new GpuBufferUpdater<float3>(() => solverImpl.Solver.BodyAllocator.VerticesSoA.Normal, solverImpl.m_VertexNormalBuffer),
				new GpuBufferUpdater<DeformableSkinnedVertex>(() => solverImpl.Solver.MeshDeformerAllocator.DeformableSkinnedVerticesSoA.Array, solverImpl.m_DeformableSkinnedVerticesBuffer),
				new GpuBufferUpdater<int>(() => solverImpl.Solver.MeshDeformerAllocator.VertexToSkinnedVertexMapSoA.Array, solverImpl.m_VertexToDeformableVertexMapBuffer),
				new GpuBufferUpdater<float4x4>(() => solverImpl.Solver.BodyAllocator.BonesSoA.SimulatedBindpose, solverImpl.m_BindposesBuffer),
				new GpuBufferUpdater<int>(() => solverImpl.Solver.BodyAllocator.BoneIndicesMapSoA.Array, solverImpl.m_BoneIndicesMapBuffer)
			};
		}

		public void Update(CommandBuffer cmd)
		{
			foreach (GpuBufferUpdater updater in m_Updaters)
			{
				updater.Update(cmd);
			}
		}
	}

	private int m_ParticlesCount;

	private NativeArray<int> m_ParticlesMap;

	private DynamicData m_DynamicData;

	private List<GraphicsBufferWrapper> m_Buffers;

	private GraphicsBufferWrapper<int> m_VertexToParticleMapBuffer;

	private GraphicsBufferWrapper<float3> m_VertexPositionBuffer;

	private GraphicsBufferWrapper<float3> m_VertexNormalBuffer;

	private GraphicsBufferWrapper<DeformableSkinnedVertex> m_DeformableSkinnedVerticesBuffer;

	private GraphicsBufferWrapper<int> m_VertexToDeformableVertexMapBuffer;

	private GraphicsBufferWrapper<float4x4> m_BindposesBuffer;

	private GraphicsBufferWrapper<int> m_BoneIndicesMapBuffer;

	private GpuUpdaters m_GpuUpdaters;

	private Solver m_Solver;

	internal JobCombiner<JobGroup> m_Jobs;

	private CpuGizmosImpl m_GizmosImpl;

	public Solver Solver => m_Solver;

	public IGizmosImpl GizmosImpl => m_GizmosImpl;

	public NativeArray<int> ParticlesMap => m_ParticlesMap;

	public CpuSolverImpl(Solver solver)
	{
		m_Solver = solver;
		m_DynamicData = new DynamicData(this);
		BodyAllocator bodyAllocator = m_Solver.BodyAllocator;
		bodyAllocator.AfterAlloc = (Action)Delegate.Combine(bodyAllocator.AfterAlloc, new Action(OnAfterAllocBody));
		BodyAllocator bodyAllocator2 = m_Solver.BodyAllocator;
		bodyAllocator2.BeforeGrow = (Action)Delegate.Combine(bodyAllocator2.BeforeGrow, new Action(OnBeforeGrowBody));
		BodyAllocator bodyAllocator3 = m_Solver.BodyAllocator;
		bodyAllocator3.AfterGrow = (Action)Delegate.Combine(bodyAllocator3.AfterGrow, new Action(OnAfterGrowBody));
		m_Jobs = new JobCombiner<JobGroup>();
		m_VertexToParticleMapBuffer = new GraphicsBufferWrapper<int>("_XpbdVertexToParticleMapBuffer", 128);
		m_VertexPositionBuffer = new GraphicsBufferWrapper<float3>("_XpbdVertexPositionBuffer", 128);
		m_VertexNormalBuffer = new GraphicsBufferWrapper<float3>("_XpbdVertexNormalBuffer", 128);
		m_BindposesBuffer = new GraphicsBufferWrapper<float4x4>("_XpbdBindposesBuffer", 128);
		m_BoneIndicesMapBuffer = new GraphicsBufferWrapper<int>("_XpbdBoneIndicesMapBuffer", 128);
		m_DeformableSkinnedVerticesBuffer = new GraphicsBufferWrapper<DeformableSkinnedVertex>("DeformableSkinnedVertex", 128);
		m_VertexToDeformableVertexMapBuffer = new GraphicsBufferWrapper<int>("_XpbdVertexToDeformableVertexMapBuffer", 128);
		m_Buffers = new List<GraphicsBufferWrapper> { m_VertexToParticleMapBuffer, m_VertexPositionBuffer, m_VertexNormalBuffer, m_BindposesBuffer, m_BoneIndicesMapBuffer, m_DeformableSkinnedVerticesBuffer, m_VertexToDeformableVertexMapBuffer };
		m_GpuUpdaters = new GpuUpdaters(this);
	}

	public void Dispose()
	{
		BodyAllocator bodyAllocator = m_Solver.BodyAllocator;
		bodyAllocator.AfterAlloc = (Action)Delegate.Remove(bodyAllocator.AfterAlloc, new Action(OnAfterAllocBody));
		BodyAllocator bodyAllocator2 = m_Solver.BodyAllocator;
		bodyAllocator2.BeforeGrow = (Action)Delegate.Remove(bodyAllocator2.BeforeGrow, new Action(OnBeforeGrowBody));
		BodyAllocator bodyAllocator3 = m_Solver.BodyAllocator;
		bodyAllocator3.AfterGrow = (Action)Delegate.Remove(bodyAllocator3.AfterGrow, new Action(OnAfterGrowBody));
		m_GizmosImpl?.Dispose();
		if (m_ParticlesMap.IsCreated)
		{
			m_ParticlesMap.Dispose();
		}
		foreach (GraphicsBufferWrapper buffer in m_Buffers)
		{
			buffer.Dispose();
		}
	}

	private void OnAfterAllocBody()
	{
		InitParticleMap();
	}

	private void OnBeforeGrowBody()
	{
		m_DynamicData.Store();
	}

	private void OnAfterGrowBody()
	{
		m_DynamicData.Roll();
	}

	private void InitParticleMap()
	{
		m_ParticlesCount = Solver.BodyAllocator.ParticleSoA.Count;
		if (!m_ParticlesMap.IsCreated || m_ParticlesMap.Length < m_ParticlesCount)
		{
			if (m_ParticlesMap.IsCreated)
			{
				m_ParticlesMap.Dispose();
			}
			m_ParticlesMap = new NativeArray<int>((int)((float)m_ParticlesCount * 1.5f), Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}
		int num = 0;
		foreach (KeyValuePair<AuthoringBase, int> item in m_Solver.BodyAllocator.EntityAllocationMap)
		{
			int2 @int = m_Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange[item.Value];
			for (int i = 0; i < @int.y; i++)
			{
				m_ParticlesMap[num++] = @int.x + i;
			}
		}
	}

	public void EnsureRenderBuffersInitialized(ScriptableRenderContext context)
	{
	}

	public void BeginStep(in UpdateContext context)
	{
		m_Jobs.Clear();
		UpdateBodyTransformsJob updateBodyTransformsJob = default(UpdateBodyTransformsJob);
		updateBodyTransformsJob.StepDt = context.StepDelta;
		updateBodyTransformsJob.DtBetweenSimulations = context.DeltaTimeBetweenSimulations;
		updateBodyTransformsJob.BodyIndicesMap = Solver.BodyAllocator.IndicesMap;
		updateBodyTransformsJob.BodyLocalToWorld = Solver.BodyAllocator.BodyDescriptorSoA.LocalToWorld;
		updateBodyTransformsJob.BodyWorldToLocal = Solver.BodyAllocator.BodyDescriptorSoA.WorldToLocal;
		updateBodyTransformsJob.BodyPrevWorldToLocal = Solver.BodyAllocator.BodyDescriptorSoA.PrevWorldToLocal;
		updateBodyTransformsJob.BodyInertialFrame = Solver.BodyAllocator.BodyDescriptorSoA.InertialFrame;
		updateBodyTransformsJob.BodyInertialForces = Solver.BodyAllocator.BodyDescriptorSoA.InertialForces;
		updateBodyTransformsJob.BodySimulationParameters = Solver.BodyAllocator.BodyDescriptorSoA.BodySimulationParameters;
		UpdateBodyTransformsJob jobData = updateBodyTransformsJob;
		m_Jobs.Handle = IJobParallelForTransformExtensions.ScheduleReadOnlyByRef(ref jobData, Solver.BodyAllocator.Transforms, 32, m_Jobs.Handle);
		if (!Solver.MeshDeformerAllocator.IsEmpty)
		{
			Solver.MeshDeformerAllocator.UpdateMasterIndices();
			UpdateMeshDeformerTransformsJob updateMeshDeformerTransformsJob = default(UpdateMeshDeformerTransformsJob);
			updateMeshDeformerTransformsJob.MeshDeformerIndicesMap = Solver.MeshDeformerAllocator.IndicesMap;
			updateMeshDeformerTransformsJob.MeshDeformerLocalToWorld = Solver.MeshDeformerAllocator.DescriptorsSoA.LocalToWorld;
			updateMeshDeformerTransformsJob.MeshDeformerWorldToLocal = Solver.MeshDeformerAllocator.DescriptorsSoA.WorldToLocal;
			updateMeshDeformerTransformsJob.MeshDeformerBindingsRange = Solver.MeshDeformerAllocator.DescriptorsSoA.BindingsRange;
			updateMeshDeformerTransformsJob.BindingsBodyToDeformer = Solver.MeshDeformerAllocator.BindingDescriptorsSoA.BodyToDeformer;
			updateMeshDeformerTransformsJob.BindingsMasterIndexInSimulation = Solver.MeshDeformerAllocator.BindingDescriptorsSoA.MasterIndexInSimulation;
			updateMeshDeformerTransformsJob.BodyLocalToWorld = Solver.BodyAllocator.BodyDescriptorSoA.LocalToWorld;
			UpdateMeshDeformerTransformsJob jobData2 = updateMeshDeformerTransformsJob;
			m_Jobs.Handle = IJobParallelForTransformExtensions.ScheduleReadOnlyByRef(ref jobData2, Solver.MeshDeformerAllocator.Transforms, 16, m_Jobs.Handle);
		}
		foreach (AuthoringBase key in Solver.BodyAllocator.EntityAllocationMap.Keys)
		{
			key.BeginStep(Solver);
		}
		m_Jobs.FlushGroup(JobGroup.PreSolve);
		TransformBodyJob transformBodyJob = default(TransformBodyJob);
		transformBodyJob.BodyDescriptorMap = Solver.BodyAllocator.IndicesMap;
		transformBodyJob.BodyDescriptorParticleRange = Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange;
		transformBodyJob.BodyDescriptorLocalToWorld = Solver.BodyAllocator.BodyDescriptorSoA.LocalToWorld;
		transformBodyJob.BodyDescriptorPrevWorldToLocal = Solver.BodyAllocator.BodyDescriptorSoA.PrevWorldToLocal;
		transformBodyJob.BodyDescriptorSkinBufferRange = Solver.BodyAllocator.BodyDescriptorSoA.SkinBufferRange;
		transformBodyJob.ParticlePosition = Solver.BodyAllocator.ParticleSoA.Position;
		transformBodyJob.ParticlePrevPosition = Solver.BodyAllocator.ParticleSoA.PrevPosition;
		TransformBodyJob jobData3 = transformBodyJob;
		m_Jobs.Handle = IJobParallelForExtensions.ScheduleByRef(ref jobData3, Solver.BodyAllocator.EntityAllocationMap.Count, 1, m_Jobs.Handle);
		JobHandle inputDep = Solver.ParticleAttachmentAllocator.UpdateAttachmentTransforms(Solver.BodyAllocator, m_Jobs.Handle);
		inputDep = Solver.ParticleAttachmentAllocator.RestoreParticles(Solver.BodyAllocator, inputDep);
		inputDep = Solver.ParticleAttachmentAllocator.UpdateAttachments(Solver.BodyAllocator, inputDep);
		m_Jobs.CombineWithGroup(JobGroup.PreSolve, inputDep);
		JobHandle handle = Solver.ColliderWorld.UpdateColliders(m_Jobs.Handle, Solver.Config.CollisionSettings.ColliderCCD);
		m_Jobs.CombineWithGroup(JobGroup.PreSolve, handle);
		m_Jobs.FlushGroup(JobGroup.PreSolve);
		Solver.BroadphaseImpl.CollisionDetection(context.StepDelta);
	}

	public void UpdateSkin(SkinnedMeshBody body, GraphicsBuffer vertexBuffer, bool recalculateNormals)
	{
		if (vertexBuffer != null)
		{
			int2 skinBufferRange = Solver.BodyAllocator.BodyDescriptorSoA.SkinBufferRange[body.IndexInSolver];
			int2 @int = Solver.BodyAllocator.BodyDescriptorSoA.VerticesRange[body.IndexInSolver];
			int2 particleToVertexMapRange = Solver.BodyAllocator.BodyDescriptorSoA.ParticleToVertexRange[body.IndexInSolver];
			int2 particlesRange = Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange[body.IndexInSolver];
			int stride = vertexBuffer.stride;
			int num = stride / 4;
			NativeArray<float> output = new NativeArray<float>(skinBufferRange.y * num, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			AsyncGPUReadback.RequestIntoNativeArray(ref output, vertexBuffer, skinBufferRange.y * stride, skinBufferRange.x * stride).WaitForCompletion();
			CopySkinnedVerticesJob copySkinnedVerticesJob = default(CopySkinnedVerticesJob);
			copySkinnedVerticesJob.BodyDescIndex = body.IndexInSolver;
			copySkinnedVerticesJob.RestNormalsRange = @int;
			copySkinnedVerticesJob.SkinBufferRange = skinBufferRange;
			copySkinnedVerticesJob.BodyLocalToWorld = Solver.BodyAllocator.BodyDescriptorSoA.LocalToWorld;
			copySkinnedVerticesJob.ParticleToVertexMapRange = particleToVertexMapRange;
			copySkinnedVerticesJob.ParticlesRange = particlesRange;
			copySkinnedVerticesJob.VertexStride = num;
			copySkinnedVerticesJob.ParticleToVertexMap = Solver.BodyAllocator.ParticleToVertexMapSoA.Array;
			copySkinnedVerticesJob.MeshVertices = output;
			copySkinnedVerticesJob.ParticleBasePosition = Solver.BodyAllocator.ParticleSoA.BasePosition;
			copySkinnedVerticesJob.ParticlePosition = Solver.BodyAllocator.ParticleSoA.Position;
			copySkinnedVerticesJob.ParticlePrevPosition = Solver.BodyAllocator.ParticleSoA.PrevPosition;
			copySkinnedVerticesJob.ParticleInvMass = Solver.BodyAllocator.ParticleSoA.InvMass;
			copySkinnedVerticesJob.RestNormal = Solver.BodyAllocator.VerticesSoA.RestNormal;
			CopySkinnedVerticesJob jobData = copySkinnedVerticesJob;
			JobHandle jobHandle = IJobParallelForExtensions.ScheduleByRef(ref jobData, particlesRange.y, 32, m_Jobs.Handle);
			if (recalculateNormals)
			{
				int2 trianglesRange = Solver.BodyAllocator.BodyDescriptorSoA.TrianglesRange[body.IndexInSolver];
				int2 vertexTrianglesMapRange = Solver.BodyAllocator.BodyDescriptorSoA.VertexTrianglesMapRange[body.IndexInSolver];
				int2 vertexTrianglesMapRangesRange = Solver.BodyAllocator.BodyDescriptorSoA.VertexTrianglesMapRangesRange[body.IndexInSolver];
				RecalculateSkinNormalsJob recalculateSkinNormalsJob = default(RecalculateSkinNormalsJob);
				recalculateSkinNormalsJob.ParticlesRange = particlesRange;
				recalculateSkinNormalsJob.TrianglesRange = trianglesRange;
				recalculateSkinNormalsJob.VerticesRange = @int;
				recalculateSkinNormalsJob.VertexTrianglesMapRange = vertexTrianglesMapRange;
				recalculateSkinNormalsJob.VertexTrianglesMapRangesRange = vertexTrianglesMapRangesRange;
				recalculateSkinNormalsJob.ParticleBasePosition = Solver.BodyAllocator.ParticleSoA.BasePosition;
				recalculateSkinNormalsJob.Triangles = Solver.BodyAllocator.TrianglesSoA.Array;
				recalculateSkinNormalsJob.RestNormals = Solver.BodyAllocator.VerticesSoA.RestNormal;
				recalculateSkinNormalsJob.VertexTrianglesMap = Solver.BodyAllocator.VertexTrianglesMapSoA.Array;
				recalculateSkinNormalsJob.VertexTrianglesMapRanges = Solver.BodyAllocator.VertexTrianglesMapRangesSoA.Array;
				RecalculateSkinNormalsJob jobData2 = recalculateSkinNormalsJob;
				jobHandle = IJobParallelForExtensions.ScheduleByRef(ref jobData2, @int.y, 32, jobHandle);
			}
			jobHandle = output.Dispose(jobHandle);
			m_Jobs.CombineWithGroup(JobGroup.PreSolve, jobHandle);
		}
	}

	public void UpdateBoneTranforms(SkeletonBody body, TransformAccessArray bones)
	{
		int2 bonesRange = Solver.BodyAllocator.BodyDescriptorSoA.BonesRange[body.IndexInSolver];
		int2 particlesRange = Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange[body.IndexInSolver];
		CopyBonesJob copyBonesJob = default(CopyBonesJob);
		copyBonesJob.BodyDescIndex = body.IndexInSolver;
		copyBonesJob.BodyWorldToLocal = Solver.BodyAllocator.BodyDescriptorSoA.WorldToLocal;
		copyBonesJob.BonesRange = bonesRange;
		copyBonesJob.ParticlesRange = particlesRange;
		copyBonesJob.Boneposes = Solver.BodyAllocator.BonesSoA.Bonepose;
		copyBonesJob.ParticleBasePosition = Solver.BodyAllocator.ParticleSoA.BasePosition;
		copyBonesJob.BoneToParticleMap = Solver.BodyAllocator.BonesSoA.ParticleIndex;
		CopyBonesJob jobData = copyBonesJob;
		JobHandle handle = IJobParallelForTransformExtensions.ScheduleReadOnlyByRef(ref jobData, bones, 32, m_Jobs.Handle);
		m_Jobs.CombineWithGroup(JobGroup.PreSolve, handle);
	}

	public void UpdateMeshBasePositions(MeshBody body)
	{
		int2 particlesRange = Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange[body.IndexInSolver];
		int2 restNormalsRange = Solver.BodyAllocator.BodyDescriptorSoA.VerticesRange[body.IndexInSolver];
		int2 meshLocalVerticesRange = Solver.BodyAllocator.BodyDescriptorSoA.MeshLocalVerticesRange[body.IndexInSolver];
		UpdateMeshBasePositionsJob updateMeshBasePositionsJob = default(UpdateMeshBasePositionsJob);
		updateMeshBasePositionsJob.BodyDescIndex = body.IndexInSolver;
		updateMeshBasePositionsJob.BodyLocalToWorld = Solver.BodyAllocator.BodyDescriptorSoA.LocalToWorld;
		updateMeshBasePositionsJob.ParticlesRange = particlesRange;
		updateMeshBasePositionsJob.RestNormalsRange = restNormalsRange;
		updateMeshBasePositionsJob.MeshLocalVerticesRange = meshLocalVerticesRange;
		updateMeshBasePositionsJob.MeshLocalVertexPosition = Solver.BodyAllocator.MeshLocalVerticesSoA.Position;
		updateMeshBasePositionsJob.MeshLocalVertexNormal = Solver.BodyAllocator.MeshLocalVerticesSoA.Normal;
		updateMeshBasePositionsJob.ParticleBasePosition = Solver.BodyAllocator.ParticleSoA.BasePosition;
		updateMeshBasePositionsJob.RestNormals = Solver.BodyAllocator.VerticesSoA.RestNormal;
		UpdateMeshBasePositionsJob jobData = updateMeshBasePositionsJob;
		JobHandle handle = IJobParallelForExtensions.ScheduleByRef(ref jobData, particlesRange.y, 16, m_Jobs.Handle);
		m_Jobs.CombineWithGroup(JobGroup.PreSolve, handle);
	}

	public void Step(in UpdateContext context)
	{
		float stepDelta = context.StepDelta;
		bool windEnabled = false;
		AmbientWindParameters windParams = default(AmbientWindParameters);
		if (AmbientWind.Instance != null)
		{
			windParams = GetWind();
			windEnabled = true;
		}
		ApplyExternalForcesJob applyExternalForcesJob = default(ApplyExternalForcesJob);
		applyExternalForcesJob.Dt = stepDelta;
		applyExternalForcesJob.WindEnabled = windEnabled;
		applyExternalForcesJob.WindParams = windParams;
		applyExternalForcesJob.VisibleBodyIndices = m_Solver.Culler.VisibleBodyIndices.AsArray();
		applyExternalForcesJob.BodyDescriptorParticleRange = m_Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange;
		applyExternalForcesJob.BodyDescriptorInertialForces = m_Solver.BodyAllocator.BodyDescriptorSoA.InertialForces;
		applyExternalForcesJob.BodyDescriptorWorldToLocal = m_Solver.BodyAllocator.BodyDescriptorSoA.WorldToLocal;
		applyExternalForcesJob.BodyDescriptorLocalToWorld = m_Solver.BodyAllocator.BodyDescriptorSoA.LocalToWorld;
		applyExternalForcesJob.BodyDescriptorSimulationParameters = m_Solver.BodyAllocator.BodyDescriptorSoA.BodySimulationParameters;
		applyExternalForcesJob.BodyDescriptorEnabledConstraints = m_Solver.BodyAllocator.BodyDescriptorSoA.EnabledConstraintTypeMask;
		applyExternalForcesJob.BodyDescriptorConstraintSettingsRange = m_Solver.BodyAllocator.BodyDescriptorSoA.ConstraintSettingsRange;
		applyExternalForcesJob.BodyDescriptorVerticesRange = m_Solver.BodyAllocator.BodyDescriptorSoA.VerticesRange;
		applyExternalForcesJob.ParticlePosition = m_Solver.BodyAllocator.ParticleSoA.Position;
		applyExternalForcesJob.ParticleInvMass = m_Solver.BodyAllocator.ParticleSoA.InvMass;
		applyExternalForcesJob.ParticleVelocity = m_Solver.BodyAllocator.ParticleSoA.Velocity;
		applyExternalForcesJob.ConstraintSettings = m_Solver.BodyAllocator.ConstraintSettingsSoA.Array;
		applyExternalForcesJob.VertexNormal = m_Solver.BodyAllocator.VerticesSoA.Normal;
		ApplyExternalForcesJob jobData = applyExternalForcesJob;
		m_Jobs.Handle = IJobParallelForExtensions.ScheduleByRef(ref jobData, m_Solver.Culler.VisibleBodyIndices.Length, 1, m_Jobs.Handle);
		Solve(context);
	}

	private void Solve(UpdateContext context)
	{
		CpuBroadphaseGlobal cpuBroadphaseGlobal = (CpuBroadphaseGlobal)Solver.BroadphaseImpl;
		float deltaTimeRcpSqr = math.rcp(context.SubstepTime * context.SubstepTime);
		int num = (context.SimulationSteps - context.StepIndex) * Solver.Config.SimulationSettings.Substeps;
		for (int i = 0; i < Solver.Config.SimulationSettings.Substeps; i++)
		{
			SolveJob solveJob = default(SolveJob);
			solveJob.SubstepDt = context.SubstepTime;
			solveJob.Gravity = Solver.Config.SimulationSettings.Gravity;
			solveJob.SleepThreshold = Solver.Config.SimulationSettings.SleepThreshold;
			solveJob.SubstepIndex = i;
			solveJob.DeltaTimeRcpSqr = deltaTimeRcpSqr;
			solveJob.Substeps = Solver.Config.SimulationSettings.Substeps;
			solveJob.VisibleBodyIndices = m_Solver.Culler.VisibleBodyIndices.AsArray();
			solveJob.BodyDescriptorParticleRange = m_Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange;
			solveJob.BodyDescriptorConstraintsRange = m_Solver.BodyAllocator.BodyDescriptorSoA.ConstraintsRange;
			solveJob.BodyDescriptorConstraintBatchesRange = m_Solver.BodyAllocator.BodyDescriptorSoA.ConstraintBatchesRange;
			solveJob.BodyDescriptorSimplexConstraintsRange = m_Solver.BodyAllocator.BodyDescriptorSoA.SimplexConstraintsRange;
			solveJob.BodyDescriptorEnabledConstraints = m_Solver.BodyAllocator.BodyDescriptorSoA.EnabledConstraintTypeMask;
			solveJob.BodyDescriptorConstraintSettingsRange = m_Solver.BodyAllocator.BodyDescriptorSoA.ConstraintSettingsRange;
			solveJob.BodyDescriptorVerticesRange = m_Solver.BodyAllocator.BodyDescriptorSoA.VerticesRange;
			solveJob.ParticleBasePosition = m_Solver.BodyAllocator.ParticleSoA.BasePosition;
			solveJob.ParticlePrevPosition = m_Solver.BodyAllocator.ParticleSoA.PrevPosition;
			solveJob.ParticlePosition = m_Solver.BodyAllocator.ParticleSoA.Position;
			solveJob.ParticleVelocity = m_Solver.BodyAllocator.ParticleSoA.Velocity;
			solveJob.ParticleInvMass = m_Solver.BodyAllocator.ParticleSoA.InvMass;
			solveJob.ParticleRadius = m_Solver.BodyAllocator.ParticleSoA.Radius;
			solveJob.ConstraintsBatches = m_Solver.BodyAllocator.ConstraintBatchSoA.Array;
			solveJob.ConstraintSettings = m_Solver.BodyAllocator.ConstraintSettingsSoA.Array;
			solveJob.ConstraintIndices = m_Solver.BodyAllocator.ConstraintSoA.Indices;
			solveJob.ConstraintParameters0 = m_Solver.BodyAllocator.ConstraintSoA.Parameters0;
			solveJob.VertexRestNormal = m_Solver.BodyAllocator.VerticesSoA.RestNormal;
			SolveJob jobData = solveJob;
			m_Jobs.Handle = IJobParallelForExtensions.ScheduleByRef(ref jobData, Solver.Culler.VisibleBodyIndices.Length, 1, m_Jobs.Handle);
			if (Solver.Config.CollisionSettings.ColliderCollisionsEnabled || Solver.Config.CollisionSettings.ParticleCollisionsEnabled)
			{
				SolveContactsJob solveContactsJob = default(SolveContactsJob);
				solveContactsJob.StepTime = context.StepDelta;
				solveContactsJob.SubstepTime = context.SubstepTime;
				solveContactsJob.SubstepsToEnd = num;
				solveContactsJob.MaxDepenetration = Solver.Config.CollisionSettings.MaxDepenetration;
				solveContactsJob.Contacts = cpuBroadphaseGlobal.Contacts;
				solveContactsJob.ParticlePosition = m_Solver.BodyAllocator.ParticleSoA.Position;
				solveContactsJob.ParticlePrevPosition = m_Solver.BodyAllocator.ParticleSoA.PrevPosition;
				solveContactsJob.ParticleInvMass = m_Solver.BodyAllocator.ParticleSoA.InvMass;
				solveContactsJob.ParticleRadius = m_Solver.BodyAllocator.ParticleSoA.Radius;
				solveContactsJob.ParticleDeltas = m_Solver.BodyAllocator.ParticleSoA.JacobiPosDelta;
				solveContactsJob.ParticleCounts = m_Solver.BodyAllocator.ParticleSoA.JacobiPosCount;
				SolveContactsJob jobData2 = solveContactsJob;
				m_Jobs.Handle = IJobParallelForDeferExtensions.ScheduleByRef(ref jobData2, cpuBroadphaseGlobal.Contacts, 16, m_Jobs.Handle);
				ApplyContactsDeltasJob applyContactsDeltasJob = default(ApplyContactsDeltasJob);
				applyContactsDeltasJob.ParticlePosition = m_Solver.BodyAllocator.ParticleSoA.Position;
				applyContactsDeltasJob.ParticleDeltas = m_Solver.BodyAllocator.ParticleSoA.JacobiPosDelta;
				applyContactsDeltasJob.ParticleCounts = m_Solver.BodyAllocator.ParticleSoA.JacobiPosCount;
				ApplyContactsDeltasJob jobData3 = applyContactsDeltasJob;
				m_Jobs.Handle = IJobParallelForExtensions.ScheduleByRef(ref jobData3, m_Solver.BodyAllocator.ParticleSoA.Count, 16, m_Jobs.Handle);
			}
			UpdateVelocitiesJob updateVelocitiesJob = default(UpdateVelocitiesJob);
			updateVelocitiesJob.SubstepDt = context.SubstepTime;
			updateVelocitiesJob.SleepThreshold = Solver.Config.SimulationSettings.SleepThreshold;
			updateVelocitiesJob.VisibleBodyIndices = m_Solver.Culler.VisibleBodyIndices.AsArray();
			updateVelocitiesJob.BodyDescriptorParticleRange = Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange;
			updateVelocitiesJob.BodyDescriptorBodySimulationParameters = Solver.BodyAllocator.BodyDescriptorSoA.BodySimulationParameters;
			updateVelocitiesJob.ParticlePosition = m_Solver.BodyAllocator.ParticleSoA.Position;
			updateVelocitiesJob.ParticlePrevPosition = m_Solver.BodyAllocator.ParticleSoA.PrevPosition;
			updateVelocitiesJob.ParticleVelocity = m_Solver.BodyAllocator.ParticleSoA.Velocity;
			updateVelocitiesJob.ParticleInvMass = m_Solver.BodyAllocator.ParticleSoA.InvMass;
			UpdateVelocitiesJob jobData4 = updateVelocitiesJob;
			m_Jobs.Handle = IJobParallelForExtensions.ScheduleByRef(ref jobData4, Solver.Culler.VisibleBodyIndices.Length, 1, m_Jobs.Handle);
			num--;
		}
	}

	private AmbientWindParameters GetWind()
	{
		AmbientWindParameters result = default(AmbientWindParameters);
		result.Velocity = AmbientWind.Instance.Velocity;
		result.StrengthNoiseWeight = AmbientWind.Instance.StrengthNoiseWeight;
		result.StrengthNoiseContrast = AmbientWind.Instance.StrengthNoiseContrast;
		result.StrengthOctave0 = AmbientWind.Instance.PackedStrengthOctaves[0];
		result.StrengthOctave1 = AmbientWind.Instance.PackedStrengthOctaves[1];
		result.ShiftOctave0 = AmbientWind.Instance.PackedShiftOctaves[0];
		result.ShiftOctave1 = AmbientWind.Instance.PackedShiftOctaves[1];
		return result;
	}

	public void EndStep(in UpdateContext context)
	{
		UpdateVertices();
		UpdateBones();
		m_Jobs.FlushGroup(JobGroup.PostSolve);
		UpdateBodiesAabb();
		UpdateDeformableVertices();
		m_Jobs.FlushGroup(JobGroup.PostSolve);
		m_Jobs.Handle.Complete();
		foreach (AuthoringBase key in Solver.BodyAllocator.EntityAllocationMap.Keys)
		{
			key.EndStep(Solver);
		}
		foreach (MeshDeformer key2 in Solver.MeshDeformerAllocator.EntityAllocationMap.Keys)
		{
			key2.EndStep(Solver);
		}
		PushDataToGpu(context.Context);
	}

	private void UpdateVertices()
	{
		foreach (KeyValuePair<AuthoringBase, int> item in Solver.BodyAllocator.EntityAllocationMap)
		{
			int2 @int = Solver.BodyAllocator.BodyDescriptorSoA.VertexToParticleRange[item.Value];
			int2 particlesRange = Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange[item.Value];
			int2 trianglesRange = Solver.BodyAllocator.BodyDescriptorSoA.TrianglesRange[item.Value];
			int2 verticesRange = Solver.BodyAllocator.BodyDescriptorSoA.VerticesRange[item.Value];
			int2 vertexTrianglesMapRange = Solver.BodyAllocator.BodyDescriptorSoA.VertexTrianglesMapRange[item.Value];
			int2 vertexTrianglesMapRangesRange = Solver.BodyAllocator.BodyDescriptorSoA.VertexTrianglesMapRangesRange[item.Value];
			float4x4 bodyWorldToLocal = Solver.BodyAllocator.BodyDescriptorSoA.WorldToLocal[item.Value];
			if (@int.x > -1)
			{
				UpdateVerticesJob updateVerticesJob = default(UpdateVerticesJob);
				updateVerticesJob.ParticlesRange = particlesRange;
				updateVerticesJob.TrianglesRange = trianglesRange;
				updateVerticesJob.VerticesRange = verticesRange;
				updateVerticesJob.VertexTrianglesMapRange = vertexTrianglesMapRange;
				updateVerticesJob.VertexTrianglesMapRangesRange = vertexTrianglesMapRangesRange;
				updateVerticesJob.BodyWorldToLocal = bodyWorldToLocal;
				updateVerticesJob.ParticlePosition = Solver.BodyAllocator.ParticleSoA.Position;
				updateVerticesJob.Triangles = Solver.BodyAllocator.TrianglesSoA.Array;
				updateVerticesJob.VertexTrianglesMap = Solver.BodyAllocator.VertexTrianglesMapSoA.Array;
				updateVerticesJob.VertexTrianglesMapRanges = Solver.BodyAllocator.VertexTrianglesMapRangesSoA.Array;
				updateVerticesJob.VertexPosition = Solver.BodyAllocator.VerticesSoA.Position;
				updateVerticesJob.VertexNormal = Solver.BodyAllocator.VerticesSoA.Normal;
				UpdateVerticesJob jobData = updateVerticesJob;
				JobHandle handle = IJobParallelForExtensions.ScheduleByRef(ref jobData, verticesRange.y, 32, m_Jobs.Handle);
				m_Jobs.CombineWithGroup(JobGroup.PostSolve, handle);
			}
		}
	}

	private void UpdateBones()
	{
		foreach (KeyValuePair<AuthoringBase, int> item in Solver.BodyAllocator.EntityAllocationMap)
		{
			int2 bonesRange = Solver.BodyAllocator.BodyDescriptorSoA.BonesRange[item.Value];
			int2 particlesRange = Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange[item.Value];
			if (bonesRange.x > -1)
			{
				UpdateBonesJob updateBonesJob = default(UpdateBonesJob);
				updateBonesJob.BodyDescIndex = item.Value;
				updateBonesJob.BodyWorldToLocal = Solver.BodyAllocator.BodyDescriptorSoA.WorldToLocal;
				updateBonesJob.BonesRange = bonesRange;
				updateBonesJob.ParticlesRange = particlesRange;
				updateBonesJob.BaseParticlePosition = Solver.BodyAllocator.ParticleSoA.BasePosition;
				updateBonesJob.ParticlePosition = Solver.BodyAllocator.ParticleSoA.Position;
				updateBonesJob.Bindposes = Solver.BodyAllocator.BonesSoA.Bindpose;
				updateBonesJob.Boneposes = Solver.BodyAllocator.BonesSoA.Bonepose;
				updateBonesJob.ParentIndices = Solver.BodyAllocator.BonesSoA.ParentIndex;
				updateBonesJob.BoneToParticleMap = Solver.BodyAllocator.BonesSoA.ParticleIndex;
				updateBonesJob.SimulatedBindposes = Solver.BodyAllocator.BonesSoA.SimulatedBindpose;
				UpdateBonesJob jobData = updateBonesJob;
				JobHandle handle = IJobParallelForExtensions.ScheduleByRef(ref jobData, bonesRange.y, 16, m_Jobs.Handle);
				m_Jobs.CombineWithGroup(JobGroup.PostSolve, handle);
			}
		}
	}

	private void UpdateBodiesAabb()
	{
		UpdateBodyAabbJob updateBodyAabbJob = default(UpdateBodyAabbJob);
		updateBodyAabbJob.BodiesMap = Solver.BodyAllocator.IndicesMap;
		updateBodyAabbJob.BodyAabb = Solver.BodyAllocator.BodyDescriptorSoA.Aabb;
		updateBodyAabbJob.ParticleRanges = Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange;
		updateBodyAabbJob.ParticlePosition = Solver.BodyAllocator.ParticleSoA.Position;
		updateBodyAabbJob.ParticleRadius = Solver.BodyAllocator.ParticleSoA.Radius;
		UpdateBodyAabbJob jobData = updateBodyAabbJob;
		JobHandle handle = IJobParallelForExtensions.ScheduleByRef(ref jobData, Solver.BodyAllocator.EntityAllocationMap.Count, 16, m_Jobs.Handle);
		m_Jobs.CombineWithGroup(JobGroup.PostSolve, handle);
	}

	private void UpdateDeformableVertices()
	{
		DeformMeshJob deformMeshJob = default(DeformMeshJob);
		deformMeshJob.MeshDeformerIndicesMap = Solver.MeshDeformerAllocator.IndicesMap;
		deformMeshJob.MeshDeformerBindingsRange = Solver.MeshDeformerAllocator.DescriptorsSoA.BindingsRange;
		deformMeshJob.BindingsOffsets = Solver.MeshDeformerAllocator.BindingDescriptorsSoA.Offsets;
		deformMeshJob.BindingsBodyToDeformer = Solver.MeshDeformerAllocator.BindingDescriptorsSoA.BodyToDeformer;
		deformMeshJob.DeformableVertices = Solver.MeshDeformerAllocator.DeformableVerticesSoA.Array;
		deformMeshJob.SkinnedVertices = Solver.MeshDeformerAllocator.DeformableSkinnedVerticesSoA.Array;
		deformMeshJob.MeshBodyVertexPosition = Solver.BodyAllocator.VerticesSoA.Position;
		deformMeshJob.MeshBodyVertexNormal = Solver.BodyAllocator.VerticesSoA.Normal;
		DeformMeshJob jobData = deformMeshJob;
		JobHandle handle = IJobParallelForExtensions.ScheduleByRef(ref jobData, Solver.MeshDeformerAllocator.EntityAllocationMap.Count, 1, m_Jobs.Handle);
		m_Jobs.CombineWithGroup(JobGroup.PostSolve, handle);
	}

	private void PushDataToGpu(ScriptableRenderContext context)
	{
		m_VertexToParticleMapBuffer.Resize(Solver.BodyAllocator.VertexToParticleMapSoA.Capacity);
		m_VertexPositionBuffer.Resize(Solver.BodyAllocator.VerticesSoA.Capacity);
		m_VertexNormalBuffer.Resize(Solver.BodyAllocator.VerticesSoA.Capacity);
		m_BindposesBuffer.Resize(Solver.BodyAllocator.BonesSoA.Capacity);
		m_BoneIndicesMapBuffer.Resize(Solver.BodyAllocator.BoneIndicesMapSoA.Capacity);
		m_DeformableSkinnedVerticesBuffer.Resize(Solver.MeshDeformerAllocator.DeformableSkinnedVerticesSoA.Capacity);
		m_VertexToDeformableVertexMapBuffer.Resize(Solver.MeshDeformerAllocator.VertexToSkinnedVertexMapSoA.Capacity);
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		m_GpuUpdaters.Update(commandBuffer);
		commandBuffer.SetGlobalBuffer(ShaderPropertyId._XpbdVertexToParticleMapBuffer, m_VertexToParticleMapBuffer);
		commandBuffer.SetGlobalBuffer(ShaderPropertyId._XpbdBodyVertexPositionBuffer, m_VertexPositionBuffer);
		commandBuffer.SetGlobalBuffer(ShaderPropertyId._XpbdBodyVertexNormalBuffer, m_VertexNormalBuffer);
		commandBuffer.SetGlobalBuffer(ShaderPropertyId._XpbdBodyBoneSimulatedBindposeBuffer, m_BindposesBuffer);
		commandBuffer.SetGlobalBuffer(ShaderPropertyId._XpbdBoneIndicesMapBuffer, m_BoneIndicesMapBuffer);
		commandBuffer.SetGlobalBuffer(ShaderPropertyId._XpbdDeformableSkinnedVerticesBuffer, m_DeformableSkinnedVerticesBuffer);
		commandBuffer.SetGlobalBuffer(ShaderPropertyId._XpbdVertexToDeformableVertexMapBuffer, m_VertexToDeformableVertexMapBuffer);
		commandBuffer.SetGlobalFloat(ShaderPropertyId._XpbdEnabledGlobal, 1f);
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}

	public void UpdateConstraintSettings(AuthoringBase authoring)
	{
		Solver.BodyAllocator.BodyDescriptorSoA.EnabledConstraintTypeMask[authoring.IndexInSolver] = authoring.GetEnabledConstraintTypeMask();
		int2 @int = Solver.BodyAllocator.BodyDescriptorSoA.ConstraintSettingsRange[authoring.IndexInSolver];
		for (int i = 0; i < @int.y; i++)
		{
			Solver.BodyAllocator.ConstraintSettingsSoA[@int.x + i] = authoring.ConstraintSettings.GetPackedSettings((ConstraintType)i);
		}
	}

	public void UpdateBodySimulationParameters(AuthoringBase authoring)
	{
		Solver.BodyAllocator.BodyDescriptorSoA.BodySimulationParameters[authoring.IndexInSolver] = authoring.BodySimulationParameters.GetPackedParameters();
	}

	public void UpdateLayer(AuthoringBase authoring)
	{
		Solver.BodyAllocator.BodyDescriptorSoA.Layer[authoring.IndexInSolver] = authoring.Layer;
	}

	public void GetBodyAabb(AuthoringBase body, out Aabb bodyAabb)
	{
		bodyAabb = Solver.BodyAllocator.BodyDescriptorSoA.Aabb[body.IndexInSolver];
	}

	public MemoryStat GetMemoryStat()
	{
		MemoryStat memoryStat = GizmosImpl.GetMemoryStat();
		memoryStat.Cpu += m_ParticlesMap.Length * Marshal.SizeOf<int>();
		foreach (GraphicsBufferWrapper buffer in m_Buffers)
		{
			memoryStat.Gpu += buffer.GetSizeInBytes();
		}
		return memoryStat;
	}

	void ISolverImpl.BeginStep(in UpdateContext context)
	{
		BeginStep(in context);
	}

	void ISolverImpl.Step(in UpdateContext context)
	{
		Step(in context);
	}

	void ISolverImpl.EndStep(in UpdateContext updateContext)
	{
		EndStep(in updateContext);
	}
}
