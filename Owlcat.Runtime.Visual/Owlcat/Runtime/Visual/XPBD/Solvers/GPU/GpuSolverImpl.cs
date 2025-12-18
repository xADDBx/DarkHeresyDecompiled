using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.GPU;
using Owlcat.Runtime.Visual.XPBD.Constraints;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.Debug;
using Owlcat.Runtime.Visual.XPBD.GPU.Replicators;
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

namespace Owlcat.Runtime.Visual.XPBD.Solvers.GPU;

public class GpuSolverImpl : ISolverImpl
{
	private enum JobGroup
	{
		BeginStep
	}

	internal class Shaders : ShaderResources
	{
		public ComputeShader SolveLocal;

		public ComputeKernel Solve32;

		public ComputeKernel Solve64;

		public ComputeKernel Solve128;

		public ComputeKernel Solve256;

		public ComputeKernel Solve512;

		public ComputeKernel Solve512Unlim;

		public ComputeShader SolveGlobal;

		public ComputeKernel SolveContacts;

		public ComputeKernel ClearContactDeltas;

		public ComputeKernel ApplyContactsDeltas;

		public ComputeShader SolveLocalAfterGlobal;

		public ComputeKernel UpdateVelocities32;

		public ComputeKernel UpdateVelocities64;

		public ComputeKernel UpdateVelocities128;

		public ComputeKernel UpdateVelocities256;

		public ComputeKernel UpdateVelocities512;

		public ComputeKernel UpdateVelocities512Unlim;

		public ComputeShader ParticleAttachments;

		public ComputeKernel UpdateAttachments;

		public ComputeKernel RestoreParticles;

		public ComputeShader Mesh;

		public ComputeKernel MeshPreSolve;

		public ComputeKernel MeshPostSolve;

		public ComputeShader SkinnedMesh;

		public ComputeKernel SkinnedMeshPreSolve;

		public ComputeShader SkinnedMeshRecalcNormals;

		public ComputeKernel SkinnedMeshRecalculateNormals;

		public ComputeShader Skeleton;

		public ComputeKernel SkeletonPostSolve;

		public ComputeShader MeshDeform;

		public ComputeKernel MeshDeformPostSolve;

		public ComputeShader BodyAabb;

		public ComputeKernel ComputeBodyAabb;

		public ComputeShader BodyTransform;

		public ComputeKernel TransformBody;

		public ComputeShader ApplyExternalForcesShader;

		public ComputeKernel ApplyExternalForcesKernel;

		public ComputeShader ReplicationShader;

		public ComputeKernel ReplicationStoreKernel;

		public ComputeKernel ReplicationRollKernel;

		public Shaders()
		{
			SolveLocal = LoadComputeShader("Solver/SolveLocal");
			Solve32 = new ComputeKernel(SolveLocal, "Solve32");
			Solve64 = new ComputeKernel(SolveLocal, "Solve64");
			Solve128 = new ComputeKernel(SolveLocal, "Solve128");
			Solve256 = new ComputeKernel(SolveLocal, "Solve256");
			Solve512 = new ComputeKernel(SolveLocal, "Solve512");
			Solve512Unlim = new ComputeKernel(SolveLocal, "Solve512Unlim");
			SolveGlobal = LoadComputeShader("Solver/SolveGlobal");
			SolveContacts = new ComputeKernel(SolveGlobal, "SolveContacts");
			ClearContactDeltas = new ComputeKernel(SolveGlobal, "ClearContactDeltas");
			ApplyContactsDeltas = new ComputeKernel(SolveGlobal, "ApplyContactsDeltas");
			SolveLocalAfterGlobal = LoadComputeShader("Solver/SolveLocalAfterGlobal");
			UpdateVelocities32 = new ComputeKernel(SolveLocalAfterGlobal, "UpdateVelocities32");
			UpdateVelocities64 = new ComputeKernel(SolveLocalAfterGlobal, "UpdateVelocities64");
			UpdateVelocities128 = new ComputeKernel(SolveLocalAfterGlobal, "UpdateVelocities128");
			UpdateVelocities256 = new ComputeKernel(SolveLocalAfterGlobal, "UpdateVelocities256");
			UpdateVelocities512 = new ComputeKernel(SolveLocalAfterGlobal, "UpdateVelocities512");
			UpdateVelocities512Unlim = new ComputeKernel(SolveLocalAfterGlobal, "UpdateVelocities512Unlim");
			ParticleAttachments = LoadComputeShader("ParticleAttachments");
			UpdateAttachments = new ComputeKernel(ParticleAttachments, "UpdateAttachments");
			RestoreParticles = new ComputeKernel(ParticleAttachments, "RestoreParticles");
			Mesh = LoadComputeShader("Mesh");
			MeshPreSolve = new ComputeKernel(Mesh, "PreSolve");
			MeshPostSolve = new ComputeKernel(Mesh, "PostSolve");
			SkinnedMesh = LoadComputeShader("SkinnedMesh");
			SkinnedMeshPreSolve = new ComputeKernel(SkinnedMesh, "PreSolve");
			SkinnedMeshRecalcNormals = LoadComputeShader("SkinnedMeshRecalcNormals");
			SkinnedMeshRecalculateNormals = new ComputeKernel(SkinnedMeshRecalcNormals, "RecalculateNormals");
			Skeleton = LoadComputeShader("Skeleton");
			SkeletonPostSolve = new ComputeKernel(Skeleton, "PostSolve");
			MeshDeform = LoadComputeShader("MeshDeform");
			MeshDeformPostSolve = new ComputeKernel(MeshDeform, "PostSolve");
			BodyAabb = LoadComputeShader("BodyAabb");
			ComputeBodyAabb = new ComputeKernel(BodyAabb, "ComputeBodyAabb");
			BodyTransform = LoadComputeShader("BodyTransform");
			TransformBody = new ComputeKernel(BodyTransform, "TransformBody");
			ApplyExternalForcesShader = LoadComputeShader("ApplyExternalForces");
			ApplyExternalForcesKernel = new ComputeKernel(ApplyExternalForcesShader, "ApplyExternalForces");
			ReplicationShader = LoadComputeShader("Replication");
			ReplicationStoreKernel = new ComputeKernel(ReplicationShader, "Store");
			ReplicationRollKernel = new ComputeKernel(ReplicationShader, "Roll");
		}
	}

	private GraphicsBuffer m_DummyBuffer;

	private List<ReplicatorBase> m_Replicators;

	private BodyReplicator m_BodyReplicator;

	private MeshDeformerReplicator m_MeshDeformerReplicator;

	private ParticleAttachmentReplicator m_ParticleAttachmentReplicator;

	private ColliderReplicator m_ColliderReplicator;

	private Solver m_Solver;

	private GpuGizmosImpl m_GizmosImpl;

	private Shaders m_Shaders;

	private bool m_WillTick;

	private float m_Dt;

	private float m_SubstepDt;

	private CommandBuffer m_Cmd;

	private CommandBuffer m_ReplicationCmd;

	private bool m_ReplicationCmdIsDirty;

	private NativeList<int> m_MeshBodiesToUpdate;

	private GraphicsBufferWrapper<int> m_MeshBodiesToUpdateBuffer;

	private Dictionary<SkinnedMeshBody, GraphicsBuffer> m_SkinnedBodiesToUpdate = new Dictionary<SkinnedMeshBody, GraphicsBuffer>();

	private HashSet<SkinnedMeshBody> m_SkinnedBodiesToRecalculateNormals = new HashSet<SkinnedMeshBody>();

	private SkinnedMeshConstantBuffer m_SkinnedMeshCB;

	private List<int> m_SkeletonBodiesToUpdate = new List<int>();

	private bool m_NeedUpdateGpuConstraintsSettings;

	private bool m_NeedUpdateBodySimulationParameters;

	private bool m_NeedUpdateBodyLayer;

	private GpuBroadphaseGlobal m_BroadphaseGlobal;

	private JobCombiner<JobGroup> m_Jobs;

	public Solver Solver => m_Solver;

	public IGizmosImpl GizmosImpl => m_GizmosImpl;

	public BodyReplicator BodyReplicator => m_BodyReplicator;

	public MeshDeformerReplicator MeshDeformerReplicator => m_MeshDeformerReplicator;

	public ParticleAttachmentReplicator ParticleAttachmentReplicator => m_ParticleAttachmentReplicator;

	public ColliderReplicator ColliderReplicator => m_ColliderReplicator;

	public GpuBroadphaseGlobal BroadphaseGlobal
	{
		get
		{
			if (m_BroadphaseGlobal == null)
			{
				m_BroadphaseGlobal = Solver.BroadphaseImpl as GpuBroadphaseGlobal;
			}
			return m_BroadphaseGlobal;
		}
	}

	public GpuSolverImpl(Solver solver)
	{
		m_Solver = solver;
		m_DummyBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, 4);
		m_DummyBuffer.name = "_XpbdDummyBuffer";
		m_Jobs = new JobCombiner<JobGroup>();
		m_MeshBodiesToUpdateBuffer = new GraphicsBufferWrapper<int>("_XpbdMeshBodiesToUpdateBuffer", 128);
		m_Shaders = new Shaders();
		m_ReplicationCmd = new CommandBuffer();
		m_ReplicationCmd.name = "Replication CommandBuffer";
		InitReplicators();
	}

	private void InitReplicators()
	{
		m_BodyReplicator = new BodyReplicator(m_Solver.BodyAllocator, m_Shaders);
		m_MeshDeformerReplicator = new MeshDeformerReplicator(m_Solver.MeshDeformerAllocator, m_Solver.BodyAllocator);
		m_ParticleAttachmentReplicator = new ParticleAttachmentReplicator(m_Solver.ParticleAttachmentAllocator);
		m_ColliderReplicator = new ColliderReplicator(m_Solver.ColliderWorld);
		m_Replicators = new List<ReplicatorBase> { m_BodyReplicator, m_MeshDeformerReplicator, m_ParticleAttachmentReplicator, m_ColliderReplicator };
	}

	public void Dispose()
	{
		m_DummyBuffer?.Dispose();
		m_GizmosImpl?.Dispose();
		m_ReplicationCmd?.Dispose();
		if (m_MeshBodiesToUpdate.IsCreated)
		{
			m_MeshBodiesToUpdate.Dispose();
		}
		m_MeshBodiesToUpdateBuffer?.Dispose();
		m_Shaders?.Dispose();
		DisposeReplicators();
	}

	private void DisposeReplicators()
	{
		foreach (ReplicatorBase replicator in m_Replicators)
		{
			replicator.Dispose();
		}
		m_Replicators.Clear();
	}

	public void BeginStep(in UpdateContext context)
	{
		m_WillTick = !Solver.BodyAllocator.IsEmpty;
		if (m_WillTick)
		{
			m_Dt = context.StepDelta;
			m_SubstepDt = context.SubstepTime;
			m_Jobs.Clear();
			UpdateBodyTransformsJob updateBodyTransformsJob = default(UpdateBodyTransformsJob);
			updateBodyTransformsJob.StepDt = m_Dt;
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
				updateMeshDeformerTransformsJob.MeshDeformerWorldToLocal = Solver.MeshDeformerAllocator.DescriptorsSoA.WorldToLocal;
				updateMeshDeformerTransformsJob.MeshDeformerLocalToWorld = Solver.MeshDeformerAllocator.DescriptorsSoA.LocalToWorld;
				updateMeshDeformerTransformsJob.MeshDeformerBindingsRange = Solver.MeshDeformerAllocator.DescriptorsSoA.BindingsRange;
				updateMeshDeformerTransformsJob.BindingsBodyToDeformer = Solver.MeshDeformerAllocator.BindingDescriptorsSoA.BodyToDeformer;
				updateMeshDeformerTransformsJob.BindingsMasterIndexInSimulation = Solver.MeshDeformerAllocator.BindingDescriptorsSoA.MasterIndexInSimulation;
				updateMeshDeformerTransformsJob.BodyLocalToWorld = Solver.BodyAllocator.BodyDescriptorSoA.LocalToWorld;
				UpdateMeshDeformerTransformsJob jobData2 = updateMeshDeformerTransformsJob;
				m_Jobs.Handle = IJobParallelForTransformExtensions.ScheduleReadOnlyByRef(ref jobData2, Solver.MeshDeformerAllocator.Transforms, 16, m_Jobs.Handle);
			}
			if (m_MeshBodiesToUpdate.IsCreated)
			{
				m_MeshBodiesToUpdate.Clear();
			}
			m_SkinnedBodiesToUpdate.Clear();
			m_SkinnedBodiesToRecalculateNormals.Clear();
			m_SkeletonBodiesToUpdate.Clear();
			foreach (AuthoringBase key in Solver.BodyAllocator.EntityAllocationMap.Keys)
			{
				key.BeginStep(Solver);
			}
			JobHandle handle = Solver.ParticleAttachmentAllocator.UpdateAttachmentTransforms(Solver.BodyAllocator, m_Jobs.Handle);
			m_Jobs.CombineWithGroup(JobGroup.BeginStep, handle);
			JobHandle handle2 = Solver.ColliderWorld.UpdateColliders(m_Jobs.Handle, Solver.Config.CollisionSettings.ColliderCCD);
			m_Jobs.CombineWithGroup(JobGroup.BeginStep, handle2);
			m_Jobs.FlushGroup(JobGroup.BeginStep);
			JobHandle.ScheduleBatchedJobs();
		}
		m_Jobs.Handle.Complete();
		m_Cmd = CommandBufferPool.Get();
		PreSolve(m_Cmd);
	}

	public void Step(in UpdateContext context)
	{
		SolveGlobal(m_Cmd);
	}

	private void SolveGlobal(CommandBuffer cmd)
	{
		cmd.BeginSample(ProfileId.SolveGlobal);
		BodyReplicator.BodyDescriptorSoA.PushToGpu(cmd);
		BodyReplicator.ParticlesSoA.PushToGpu(cmd);
		BodyReplicator.ConstraintSoA.PushToGpu(cmd);
		BodyReplicator.ConstraintBatchSoA.PushToGpu(cmd);
		BodyReplicator.ConstraintSettingsSoA.PushToGpu(cmd);
		BodyReplicator.VerticesSoA.PushToGpu(cmd);
		cmd.SetGlobalBuffer(BodyReplicator.BodyIndicesMapBuffer.NameId, BodyReplicator.BodyIndicesMapBuffer.Buffer);
		cmd.SetGlobalBuffer(BodyReplicator.BodyGroupsMapBuffer.NameId, BodyReplicator.BodyGroupsMapBuffer.Buffer);
		BroadphaseGlobal.ContactsBuffer.SetGlobal(cmd);
		BroadphaseGlobal.ContactsCounterBuffer.SetGlobal(cmd);
		cmd.SetGlobalFloat(ShaderPropertyId._XpbdBodyIndicesMapOffset, 0f);
		cmd.SetGlobalVector(ShaderPropertyId._XpbdGravity, (Vector3)Solver.Config.SimulationSettings.Gravity);
		cmd.SetGlobalFloat(ShaderPropertyId._XpbdSubstepDt, m_SubstepDt);
		cmd.SetGlobalFloat(ShaderPropertyId._XpbdSubsteps, Solver.Config.SimulationSettings.Substeps);
		cmd.SetGlobalFloat(ShaderPropertyId._XpbdDeltaTimeRcpSqr, math.rcp(m_SubstepDt * m_SubstepDt));
		cmd.SetGlobalFloat(ShaderPropertyId._XpbdSleepThreshold, Solver.Config.SimulationSettings.SleepThreshold);
		cmd.SetGlobalInt(ShaderPropertyId._XpbdColliderCollisionsEnabled, Solver.Config.CollisionSettings.ColliderCollisionsEnabled ? 1 : 0);
		cmd.SetGlobalInt(ShaderPropertyId._XpbdParticleCollisionsEnabled, Solver.Config.CollisionSettings.ParticleCollisionsEnabled ? 1 : 0);
		cmd.SetGlobalFloat(ShaderPropertyId._XpbdCollisionMaxDepenetration, Solver.Config.CollisionSettings.MaxDepenetration);
		cmd.SetGlobalFloat(ShaderPropertyId._XpbdDt, m_Dt);
		m_Shaders.ApplyExternalForcesKernel.Dispatch(cmd, Solver.BodyAllocator.EntityAllocationMap.Count, 1, 1);
		for (int i = 0; i < Solver.Config.SimulationSettings.Substeps; i++)
		{
			cmd.SetGlobalFloat(ShaderPropertyId._XpbdSubstep, i);
			foreach (KeyValuePair<BodyReplicator.BodyGroup, int2> bodyGroupRange in BodyReplicator.BodyGroupRanges)
			{
				if (bodyGroupRange.Value.y > 0)
				{
					ComputeKernel kernelSolveLocal = GetKernelSolveLocal(bodyGroupRange.Key);
					cmd.SetGlobalInt(ShaderPropertyId._XpbdBodyGroupsMapOffset, bodyGroupRange.Value.x);
					cmd.BeginSample(kernelSolveLocal.Name);
					kernelSolveLocal.Dispatch(cmd, bodyGroupRange.Value.y, 1, 1);
					cmd.EndSample(kernelSolveLocal.Name);
				}
			}
			if (Solver.Config.CollisionSettings.ColliderCollisionsEnabled || Solver.Config.CollisionSettings.ParticleCollisionsEnabled)
			{
				m_Shaders.SolveContacts.DispatchIndirect(cmd, BroadphaseGlobal.ContactsCounterIndirectBuffer, 0u);
				m_Shaders.ApplyContactsDeltas.Dispatch(cmd, XPBDMath.DivRoundUp(Solver.BodyAllocator.ParticleSoA.Capacity, m_Shaders.ApplyContactsDeltas.NumThreads.x), 1, 1);
			}
			foreach (KeyValuePair<BodyReplicator.BodyGroup, int2> bodyGroupRange2 in BodyReplicator.BodyGroupRanges)
			{
				if (bodyGroupRange2.Value.y > 0)
				{
					ComputeKernel kernelSolveLocalAfterGlobal = GetKernelSolveLocalAfterGlobal(bodyGroupRange2.Key);
					cmd.SetGlobalInt(ShaderPropertyId._XpbdBodyGroupsMapOffset, bodyGroupRange2.Value.x);
					cmd.BeginSample(kernelSolveLocalAfterGlobal.Name);
					kernelSolveLocalAfterGlobal.Dispatch(cmd, bodyGroupRange2.Value.y, 1, 1);
					cmd.EndSample(kernelSolveLocalAfterGlobal.Name);
				}
			}
		}
		cmd.EndSample(ProfileId.SolveGlobal);
	}

	private ComputeKernel GetKernelSolveLocal(BodyReplicator.BodyGroup key)
	{
		return key switch
		{
			BodyReplicator.BodyGroup.Simulate64 => m_Shaders.Solve64, 
			BodyReplicator.BodyGroup.Simulate128 => m_Shaders.Solve128, 
			BodyReplicator.BodyGroup.Simulate256 => m_Shaders.Solve256, 
			BodyReplicator.BodyGroup.Simulate512Unlim => m_Shaders.Solve512Unlim, 
			_ => m_Shaders.Solve256, 
		};
	}

	private ComputeKernel GetKernelSolveLocalAfterGlobal(BodyReplicator.BodyGroup key)
	{
		return key switch
		{
			BodyReplicator.BodyGroup.Simulate64 => m_Shaders.UpdateVelocities64, 
			BodyReplicator.BodyGroup.Simulate128 => m_Shaders.UpdateVelocities128, 
			BodyReplicator.BodyGroup.Simulate256 => m_Shaders.UpdateVelocities256, 
			BodyReplicator.BodyGroup.Simulate512Unlim => m_Shaders.UpdateVelocities512Unlim, 
			_ => m_Shaders.UpdateVelocities256, 
		};
	}

	public void EndStep(in UpdateContext updateContext)
	{
		PostSolve(m_Cmd);
		PushToGpu(m_Cmd);
		if (m_ReplicationCmdIsDirty)
		{
			updateContext.Context.ExecuteCommandBuffer(m_ReplicationCmd);
			m_ReplicationCmd.Clear();
		}
		updateContext.Context.ExecuteCommandBuffer(m_Cmd);
		CommandBufferPool.Release(m_Cmd);
		m_Cmd = null;
		foreach (AuthoringBase key in Solver.BodyAllocator.EntityAllocationMap.Keys)
		{
			key.EndStep(Solver);
		}
		foreach (MeshDeformer key2 in Solver.MeshDeformerAllocator.EntityAllocationMap.Keys)
		{
			key2.EndStep(Solver);
		}
	}

	public void UpdateSkin(SkinnedMeshBody body, GraphicsBuffer vertexBuffer, bool recalculateNormals)
	{
		m_SkinnedBodiesToUpdate.Add(body, vertexBuffer);
		if (recalculateNormals)
		{
			m_SkinnedBodiesToRecalculateNormals.Add(body);
		}
	}

	public void UpdateBoneTranforms(SkeletonBody body, TransformAccessArray bones)
	{
		int indexInSolver = body.IndexInSolver;
		m_SkeletonBodiesToUpdate.Add(indexInSolver);
		int2 bonesRange = Solver.BodyAllocator.BodyDescriptorSoA.BonesRange[indexInSolver];
		int2 particlesRange = Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange[indexInSolver];
		CopyBonesJob copyBonesJob = default(CopyBonesJob);
		copyBonesJob.BodyDescIndex = indexInSolver;
		copyBonesJob.BodyWorldToLocal = Solver.BodyAllocator.BodyDescriptorSoA.WorldToLocal;
		copyBonesJob.BonesRange = bonesRange;
		copyBonesJob.ParticlesRange = particlesRange;
		copyBonesJob.Boneposes = Solver.BodyAllocator.BonesSoA.Bonepose;
		copyBonesJob.ParticleBasePosition = Solver.BodyAllocator.ParticleSoA.BasePosition;
		copyBonesJob.BoneToParticleMap = Solver.BodyAllocator.BonesSoA.ParticleIndex;
		CopyBonesJob jobData = copyBonesJob;
		JobHandle handle = IJobParallelForTransformExtensions.ScheduleReadOnlyByRef(ref jobData, bones, 32, m_Jobs.Handle);
		m_Jobs.CombineWithGroup(JobGroup.BeginStep, handle);
	}

	public void UpdateConstraintSettings(AuthoringBase authoring)
	{
		int indexInSolver = authoring.IndexInSolver;
		Solver.BodyAllocator.BodyDescriptorSoA.EnabledConstraintTypeMask[indexInSolver] = authoring.GetEnabledConstraintTypeMask();
		int2 @int = Solver.BodyAllocator.BodyDescriptorSoA.ConstraintSettingsRange[indexInSolver];
		for (int i = 0; i < @int.y; i++)
		{
			Solver.BodyAllocator.ConstraintSettingsSoA[@int.x + i] = authoring.ConstraintSettings.GetPackedSettings((ConstraintType)i);
		}
		m_NeedUpdateGpuConstraintsSettings = true;
	}

	public void UpdateBodySimulationParameters(AuthoringBase authoring)
	{
		Solver.BodyAllocator.BodyDescriptorSoA.BodySimulationParameters[authoring.IndexInSolver] = authoring.BodySimulationParameters.GetPackedParameters();
		m_NeedUpdateBodySimulationParameters = true;
	}

	public void UpdateLayer(AuthoringBase authoring)
	{
		Solver.BodyAllocator.BodyDescriptorSoA.Layer[authoring.IndexInSolver] = authoring.Layer;
		m_NeedUpdateBodyLayer = true;
	}

	public void UpdateMeshBasePositions(MeshBody body)
	{
		int value = body.IndexInSolver;
		if (!m_MeshBodiesToUpdate.IsCreated)
		{
			m_MeshBodiesToUpdate = new NativeList<int>(128, Allocator.Persistent);
		}
		m_MeshBodiesToUpdate.Add(in value);
	}

	public void GetBodyAabb(AuthoringBase body, out Aabb bodyAabb)
	{
		bodyAabb = Solver.BodyAllocator.BodyDescriptorSoA.Aabb[body.IndexInSolver];
	}

	private void PreSolve(CommandBuffer cmd)
	{
		cmd.BeginSample(ProfileId.PreSolve);
		Replicate(m_ReplicationCmd);
		UpdateBodyVisibility(cmd);
		UpdateConstraintSettings(cmd);
		UpdateBodySimulationParameters(cmd);
		UpdateBodyLayer(cmd);
		UpdateBodyTransforms(cmd);
		MeshDeformersPreSolve(cmd);
		RestoreParticlesAfterParticleAttachmentDeactivation(cmd);
		UpdateAttachments(cmd);
		MeshPreSolve(cmd);
		SkinnedMeshPreSolve(cmd);
		SkeletonPreSolve(cmd);
		TransformBodies(cmd);
		Solver.BroadphaseImpl.SetCmd(cmd);
		Solver.BroadphaseImpl.CollisionDetection(m_Dt);
		ClearJacobiData(cmd);
		cmd.EndSample(ProfileId.PreSolve);
	}

	private void Replicate(CommandBuffer cmd)
	{
		m_ReplicationCmdIsDirty = false;
		foreach (ReplicatorBase replicator in m_Replicators)
		{
			if (!replicator.IsEmpty)
			{
				m_ReplicationCmdIsDirty |= replicator.Replicate(cmd);
			}
		}
	}

	private void UpdateBodyVisibility(CommandBuffer cmd)
	{
		m_BodyReplicator.UpdateBodyVisibility(cmd, Solver.Culler.VisibleBodyIndices);
	}

	private void UpdateConstraintSettings(CommandBuffer cmd)
	{
		if (m_NeedUpdateGpuConstraintsSettings)
		{
			m_NeedUpdateGpuConstraintsSettings = false;
			cmd.SetBufferData(BodyReplicator.BodyDescriptorSoA.EnabledConstraintTypeMask.Buffer, Solver.BodyAllocator.BodyDescriptorSoA.EnabledConstraintTypeMask);
			cmd.SetBufferData(BodyReplicator.ConstraintSettingsSoA.Buffer, Solver.BodyAllocator.ConstraintSettingsSoA.Array);
		}
	}

	public void UpdateBodySimulationParameters(CommandBuffer cmd)
	{
		if (m_NeedUpdateBodySimulationParameters)
		{
			m_NeedUpdateBodySimulationParameters = false;
			cmd.SetBufferData(BodyReplicator.BodyDescriptorSoA.BodySimulationParameters.Buffer, Solver.BodyAllocator.BodyDescriptorSoA.BodySimulationParameters);
		}
	}

	private void UpdateBodyLayer(CommandBuffer cmd)
	{
		if (m_NeedUpdateBodyLayer)
		{
			m_NeedUpdateBodyLayer = false;
			cmd.SetBufferData(BodyReplicator.BodyDescriptorSoA.Layer.Buffer, Solver.BodyAllocator.BodyDescriptorSoA.Layer);
		}
	}

	private void UpdateBodyTransforms(CommandBuffer cmd)
	{
		cmd.SetBufferData(BodyReplicator.BodyDescriptorSoA.WorldToLocal, Solver.BodyAllocator.BodyDescriptorSoA.WorldToLocal);
		cmd.SetBufferData(BodyReplicator.BodyDescriptorSoA.LocalToWorld, Solver.BodyAllocator.BodyDescriptorSoA.LocalToWorld);
		cmd.SetBufferData(BodyReplicator.BodyDescriptorSoA.PrevWorldToLocal, Solver.BodyAllocator.BodyDescriptorSoA.PrevWorldToLocal);
		cmd.SetBufferData(BodyReplicator.BodyDescriptorSoA.InertialForces, Solver.BodyAllocator.BodyDescriptorSoA.InertialForces);
	}

	private void MeshDeformersPreSolve(CommandBuffer cmd)
	{
		cmd.SetBufferData(MeshDeformerReplicator.MeshDeformerBindingDescriptorSoA.Offsets, Solver.MeshDeformerAllocator.BindingDescriptorsSoA.Offsets);
		cmd.SetBufferData(MeshDeformerReplicator.MeshDeformerBindingDescriptorSoA.BodyToDeformer, Solver.MeshDeformerAllocator.BindingDescriptorsSoA.BodyToDeformer);
	}

	private void SkeletonPreSolve(CommandBuffer cmd)
	{
		cmd.SetBufferData(BodyReplicator.BonesSoA.Bonepose, Solver.BodyAllocator.BonesSoA.Bonepose);
		for (int i = 0; i < m_SkeletonBodiesToUpdate.Count; i++)
		{
			int index = m_SkeletonBodiesToUpdate[i];
			int2 @int = Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange[index];
			cmd.SetBufferData(BodyReplicator.ParticlesSoA.BasePosition, Solver.BodyAllocator.ParticleSoA.BasePosition, @int.x, @int.x, @int.y);
		}
	}

	private void SkinnedMeshPreSolve(CommandBuffer cmd)
	{
		CopySkinnedVerticesAndNormals(cmd);
		RecalculateSkinnedNormals(cmd);
	}

	private unsafe void CopySkinnedVerticesAndNormals(CommandBuffer cmd)
	{
		if (m_SkinnedBodiesToUpdate.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < 26; i++)
		{
			cmd.SetGlobalBuffer(ShaderPropertyId._XpbdSkinnedVertexBuffers[i], m_DummyBuffer);
		}
		BodyReplicator.BodyDescriptorSoA.PushToGpu(cmd);
		BodyReplicator.ParticlesSoA.PushToGpu(cmd);
		BodyReplicator.VerticesSoA.PushToGpu(cmd);
		BodyReplicator.ParticleToVertexMapSoA.PushToGpu(cmd);
		int num = 0;
		foreach (KeyValuePair<SkinnedMeshBody, GraphicsBuffer> item in m_SkinnedBodiesToUpdate)
		{
			GraphicsBuffer value = item.Value;
			if (value != null)
			{
				m_SkinnedMeshCB._XpbdSkinnedBodyIndices[num * 4] = item.Key.IndexInSolver;
				m_SkinnedMeshCB._XpbdSkinnedBodyIndices[num * 4 + 1] = value.stride / 4;
				cmd.SetGlobalBuffer(ShaderPropertyId._XpbdSkinnedVertexBuffers[num], value);
				num++;
				if (num >= 26)
				{
					ConstantBuffer.PushGlobal(cmd, in m_SkinnedMeshCB, ShaderPropertyId.SkinnedMeshConstantBuffer);
					m_Shaders.SkinnedMeshPreSolve.Dispatch(cmd, 26, 1, 1);
					num = 0;
				}
			}
		}
		if (num > 0)
		{
			ConstantBuffer.PushGlobal(cmd, in m_SkinnedMeshCB, ShaderPropertyId.SkinnedMeshConstantBuffer);
			m_Shaders.SkinnedMeshPreSolve.Dispatch(cmd, math.min(26, num), 1, 1);
		}
	}

	private void RecalculateSkinnedNormals(CommandBuffer cmd)
	{
		if (m_SkinnedBodiesToRecalculateNormals.Count <= 0)
		{
			return;
		}
		int num = 0;
		float[] array = ArrayPool<float>.Shared.Rent(1024);
		foreach (SkinnedMeshBody skinnedBodiesToRecalculateNormal in m_SkinnedBodiesToRecalculateNormals)
		{
			array[num++] = skinnedBodiesToRecalculateNormal.IndexInSolver;
		}
		BodyReplicator.ParticlesSoA.PushToGpu(cmd);
		BodyReplicator.VerticesSoA.PushToGpu(cmd);
		BodyReplicator.TrianglesSoA.PushToGpu(cmd);
		BodyReplicator.VertexTrianglesMapSoA.PushToGpu(cmd);
		BodyReplicator.VertexTrianglesMapRangesSoA.PushToGpu(cmd);
		cmd.SetGlobalFloatArray(ShaderPropertyId._XpbdSkinnedBodyIndices, array);
		m_Shaders.SkinnedMeshRecalculateNormals.Dispatch(cmd, m_SkinnedBodiesToRecalculateNormals.Count, 1, 1);
		ArrayPool<float>.Shared.Return(array);
	}

	private void MeshPreSolve(CommandBuffer cmd)
	{
		if (m_MeshBodiesToUpdate.IsCreated && m_MeshBodiesToUpdate.Length > 0)
		{
			m_MeshBodiesToUpdateBuffer.Resize(m_MeshBodiesToUpdate.Capacity);
			cmd.SetBufferData(m_MeshBodiesToUpdateBuffer, m_MeshBodiesToUpdate.AsArray());
			cmd.SetGlobalBuffer(m_MeshBodiesToUpdateBuffer.NameId, m_MeshBodiesToUpdateBuffer);
			BodyReplicator.BodyDescriptorSoA.PushToGpu(cmd);
			BodyReplicator.ParticlesSoA.PushToGpu(cmd);
			BodyReplicator.VerticesSoA.PushToGpu(cmd);
			BodyReplicator.MeshLocalVerticesSoA.PushToGpu(cmd);
			m_Shaders.MeshPreSolve.Dispatch(cmd, m_MeshBodiesToUpdate.Length, 1, 1);
		}
	}

	private void RestoreParticlesAfterParticleAttachmentDeactivation(CommandBuffer cmd)
	{
		if (Solver.ParticleAttachmentAllocator.Deactivator.HasParticlesForRestore())
		{
			m_ParticleAttachmentReplicator.ParticleAttachmentDeactivatedParticleIndices.Resize(Solver.ParticleAttachmentAllocator.Deactivator.ParticleIndices.Length);
			m_ParticleAttachmentReplicator.ParticleAttachmentDeactivatedParticleInvMass.Resize(Solver.ParticleAttachmentAllocator.Deactivator.ParticleInvMass.Length);
			cmd.SetBufferData(m_ParticleAttachmentReplicator.ParticleAttachmentDeactivatedParticleIndices, Solver.ParticleAttachmentAllocator.Deactivator.ParticleIndices.AsArray());
			cmd.SetBufferData(m_ParticleAttachmentReplicator.ParticleAttachmentDeactivatedParticleInvMass, Solver.ParticleAttachmentAllocator.Deactivator.ParticleInvMass.AsArray());
			cmd.SetGlobalBuffer(m_ParticleAttachmentReplicator.ParticleAttachmentDeactivatedParticleIndices.NameId, m_ParticleAttachmentReplicator.ParticleAttachmentDeactivatedParticleIndices.Buffer);
			cmd.SetGlobalBuffer(m_ParticleAttachmentReplicator.ParticleAttachmentDeactivatedParticleInvMass.NameId, m_ParticleAttachmentReplicator.ParticleAttachmentDeactivatedParticleInvMass.Buffer);
			BodyReplicator.ParticlesSoA.PushToGpu(cmd);
			cmd.SetGlobalInt(ShaderPropertyId._XpbdDeactivatedParticlesCount, Solver.ParticleAttachmentAllocator.Deactivator.ParticleIndices.Length);
			m_Shaders.RestoreParticles.Dispatch(cmd, Solver.ParticleAttachmentAllocator.Deactivator.ParticleIndices.Length, 1, 1);
		}
	}

	private void UpdateAttachments(CommandBuffer cmd)
	{
		if (Solver.ParticleAttachmentAllocator.EntityAllocationMap.Count != 0)
		{
			cmd.SetGlobalBuffer(m_ParticleAttachmentReplicator.ParticleAttachmentMapBuffer.NameId, m_ParticleAttachmentReplicator.ParticleAttachmentMapBuffer.Buffer);
			cmd.SetBufferData(m_ParticleAttachmentReplicator.ParticleAttachmentDescriptorSoA.LocalToWorld, Solver.ParticleAttachmentAllocator.DescriptorSoA.LocalToWorld);
			cmd.SetBufferData(m_ParticleAttachmentReplicator.ParticleAttachmentDescriptorSoA.BodyParticlesRange, Solver.ParticleAttachmentAllocator.DescriptorSoA.BodyParticlesRange);
			m_ParticleAttachmentReplicator.ParticleAttachmentDescriptorSoA.PushToGpu(cmd);
			m_ParticleAttachmentReplicator.ParticleAttachmentDataSoA.PushToGpu(cmd);
			BodyReplicator.ParticlesSoA.PushToGpu(cmd);
			m_Shaders.UpdateAttachments.Dispatch(cmd, Solver.ParticleAttachmentAllocator.EntityAllocationMap.Count, 1, 1);
		}
	}

	private void TransformBodies(CommandBuffer cmd)
	{
		if (!Solver.BodyAllocator.IsEmpty)
		{
			cmd.SetGlobalBuffer(BodyReplicator.BodyIndicesMapBuffer.NameId, BodyReplicator.BodyIndicesMapBuffer.Buffer);
			BodyReplicator.BodyDescriptorSoA.PushToGpu(cmd);
			BodyReplicator.ParticlesSoA.PushToGpu(cmd);
			cmd.SetGlobalFloat(ShaderPropertyId._XpbdDt, m_Dt);
			m_Shaders.TransformBody.Dispatch(cmd, Solver.BodyAllocator.EntityAllocationMap.Count, 1, 1);
		}
	}

	private void ClearJacobiData(CommandBuffer cmd)
	{
		int capacity = Solver.BodyAllocator.ParticleSoA.Capacity;
		cmd.SetGlobalInt(ShaderPropertyId._XpbdParticlesCount, capacity);
		BodyReplicator.ParticlesSoA.PushToGpu(cmd);
		m_Shaders.ClearContactDeltas.Dispatch(cmd, RenderingUtils.DivRoundUp(capacity, m_Shaders.ClearContactDeltas.NumThreads.x), 1, 1);
	}

	private void PostSolve(CommandBuffer cmd)
	{
		cmd.BeginSample(ProfileId.PostSolve);
		ComputeBodyAabb(cmd);
		MeshPostSolve(cmd);
		SkeletonPostSolve(cmd);
		MeshDeformPostSolve(cmd);
		cmd.EndSample(ProfileId.PostSolve);
	}

	private void ComputeBodyAabb(CommandBuffer cmd)
	{
		if (Solver.BodyAllocator.EntityAllocationMap.Count > 0)
		{
			cmd.SetGlobalBuffer(BodyReplicator.BodyIndicesMapBuffer.NameId, BodyReplicator.BodyIndicesMapBuffer.Buffer);
			BodyReplicator.BodyDescriptorSoA.PushToGpu(cmd);
			BodyReplicator.ParticlesSoA.PushToGpu(cmd);
			m_Shaders.ComputeBodyAabb.Dispatch(cmd, Solver.BodyAllocator.EntityAllocationMap.Count, 1, 1);
			cmd.RequestAsyncReadback(BodyReplicator.BodyDescriptorSoA.Aabb.Buffer, OnAabbReadback);
		}
	}

	private void MeshPostSolve(CommandBuffer cmd)
	{
		if (BodyReplicator.MeshBodyCount > 0)
		{
			BodyReplicator.MeshBodyIndicesMapSoA.PushToGpu(cmd);
			BodyReplicator.VerticesSoA.PushToGpu(cmd);
			BodyReplicator.TrianglesSoA.PushToGpu(cmd);
			BodyReplicator.VertexTrianglesMapSoA.PushToGpu(cmd);
			BodyReplicator.VertexTrianglesMapRangesSoA.PushToGpu(cmd);
			m_Shaders.MeshPostSolve.Dispatch(cmd, BodyReplicator.MeshBodyCount, 1, 1);
		}
	}

	private void SkeletonPostSolve(CommandBuffer cmd)
	{
		if (BodyReplicator.SkeletonBodyCount > 0)
		{
			BodyReplicator.SkeletonBodyIndicesMapSoA.PushToGpu(cmd);
			BodyReplicator.BodyDescriptorSoA.PushToGpu(cmd);
			BodyReplicator.ParticlesSoA.PushToGpu(cmd);
			BodyReplicator.BonesSoA.PushToGpu(cmd);
			m_Shaders.SkeletonPostSolve.Dispatch(cmd, BodyReplicator.SkeletonBodyCount, 1, 1);
		}
	}

	private void MeshDeformPostSolve(CommandBuffer cmd)
	{
		if (!Solver.MeshDeformerAllocator.IsEmpty)
		{
			m_MeshDeformerReplicator.MeshDeformerIndicesBuffer.SetGlobal(cmd);
			m_MeshDeformerReplicator.GpuMeshDeformerDesctiptorSoA.PushToGpu(cmd);
			m_MeshDeformerReplicator.MeshDeformerBindingDescriptorSoA.PushToGpu(cmd);
			m_MeshDeformerReplicator.MeshDeformerDeformableVerticesSoA.PushToGpu(cmd);
			m_MeshDeformerReplicator.MeshDeformerDeformableSkinnedVerticesSoA.PushToGpu(cmd);
			BodyReplicator.VerticesSoA.PushToGpu(cmd);
			m_Shaders.MeshDeformPostSolve.Dispatch(cmd, Solver.MeshDeformerAllocator.EntityAllocationMap.Count, 1, 1);
		}
	}

	private void OnAabbReadback(AsyncGPUReadbackRequest request)
	{
		if (request.done && !request.hasError && Solver.BodyAllocator.BodyDescriptorSoA.Aabb.IsCreated)
		{
			NativeArray<Aabb> data = request.GetData<Aabb>();
			if (Solver.BodyAllocator.BodyDescriptorSoA.Aabb.Length == data.Length)
			{
				Solver.BodyAllocator.BodyDescriptorSoA.Aabb.CopyFrom(data);
			}
		}
	}

	private void PushToGpu(CommandBuffer cmd)
	{
		BodyReplicator.VerticesSoA.PushToGpu(cmd);
		BodyReplicator.VertexToParticleSoA.PushToGpu(cmd);
		BodyReplicator.BonesSoA.PushToGpu(cmd);
		BodyReplicator.BoneIndicesMapSoA.PushToGpu(cmd);
		m_MeshDeformerReplicator.MeshDeformerDeformableSkinnedVerticesSoA.PushToGpu(cmd);
		m_MeshDeformerReplicator.MeshDeformerVertexToSkinnedVertexMapSoA.PushToGpu(cmd);
		cmd.SetGlobalFloat(ShaderPropertyId._XpbdEnabledGlobal, 1f);
	}

	public MemoryStat GetMemoryStat()
	{
		MemoryStat memoryStat = m_GizmosImpl.GetMemoryStat();
		if (m_MeshBodiesToUpdate.IsCreated)
		{
			memoryStat.Cpu += m_MeshBodiesToUpdate.Length * Marshal.SizeOf<int>();
		}
		if (m_MeshBodiesToUpdateBuffer != null)
		{
			memoryStat.Gpu += m_MeshBodiesToUpdateBuffer.GetSizeInBytes();
		}
		foreach (ReplicatorBase replicator in m_Replicators)
		{
			MemoryStat memoryStat2 = replicator.GetMemoryStat();
			memoryStat.Cpu += memoryStat2.Cpu;
			memoryStat.Gpu += memoryStat2.Gpu;
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
