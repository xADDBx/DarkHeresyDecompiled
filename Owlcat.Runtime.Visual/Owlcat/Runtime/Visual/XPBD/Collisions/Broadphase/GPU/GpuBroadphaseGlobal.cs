using Owlcat.Runtime.Visual.XPBD.Shaders;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Owlcat.Runtime.Visual.XPBD.Solvers;
using Owlcat.Runtime.Visual.XPBD.Solvers.GPU;
using Owlcat.Runtime.Visual.XPBD.Stats;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.GPU;

public class GpuBroadphaseGlobal : IBroadphaseImpl
{
	private class Shaders : ShaderResources
	{
		public ComputeShader Broadphase;

		public ComputeKernel KernelBuildSimplexAabb;

		public ComputeKernel KernelOptimizeContacts;

		public ComputeKernel KernelGenerateColliderContactsGlobal;

		public ComputeKernel KernelGenerateParticleContactsGlobal;

		public Shaders()
		{
			Broadphase = LoadComputeShader("BroadphaseGlobal");
			KernelBuildSimplexAabb = new ComputeKernel(Broadphase, "BuildSimplexAabb");
			KernelOptimizeContacts = new ComputeKernel(Broadphase, "OptimizeContacts");
			KernelGenerateColliderContactsGlobal = new ComputeKernel(Broadphase, "GenerateColliderContactsGlobal");
			KernelGenerateParticleContactsGlobal = new ComputeKernel(Broadphase, "GenerateParticleContactsGlobal");
		}
	}

	private Shaders m_Shaders;

	private SpatialHashGrid m_ColliderGrid;

	private SpatialHashGrid m_ParticleGrid;

	private GpuSolverImpl m_SolverImpl;

	private CommandBuffer m_Cmd;

	private GraphicsBufferWrapper<Contact> m_ContactsBuffer;

	private GraphicsBufferWrapper<int> m_ContactsCounterBuffer;

	private GraphicsBufferWrapper<int> m_ContactsCounterIndirectBuffer;

	private int[] m_ResetContactsCounterData = new int[1];

	private int[] m_ResetContactCounterIndirectData = new int[3] { 0, 1, 1 };

	private bool m_IsReadbackFinished = true;

	private NativeArray<int> m_ContactsCounterCopy;

	public Solver Solver { get; private set; }

	public GraphicsBufferWrapper<Contact> ContactsBuffer => m_ContactsBuffer;

	public GraphicsBufferWrapper<int> ContactsCounterBuffer => m_ContactsCounterBuffer;

	public GraphicsBufferWrapper<int> ContactsCounterIndirectBuffer => m_ContactsCounterIndirectBuffer;

	public GpuBroadphaseGlobal(Solver solver)
	{
		Solver = solver;
		m_SolverImpl = solver.SolverImpl as GpuSolverImpl;
		m_ColliderGrid = new SpatialHashGrid();
		m_ParticleGrid = new SpatialHashGrid();
		m_Shaders = new Shaders();
		int size = 3;
		m_ContactsBuffer = new GraphicsBufferWrapper<Contact>("_XpbdContactsBuffer", solver.Config.CollisionSettings.MaxContactsCount);
		m_ContactsCounterBuffer = new GraphicsBufferWrapper<int>("_XpbdContactsCounterBuffer", 1);
		m_ContactsCounterIndirectBuffer = new GraphicsBufferWrapper<int>("_XpbdContactsCounterIndirectBuffer", size, GraphicsBuffer.Target.IndirectArguments);
		m_ContactsCounterCopy = new NativeArray<int>(1, Allocator.Persistent);
		m_IsReadbackFinished = true;
	}

	public void Dispose()
	{
		m_ColliderGrid?.Dispose();
		m_ParticleGrid?.Dispose();
		m_Shaders?.Dispose();
		m_ContactsBuffer?.Dispose();
		m_ContactsCounterBuffer?.Dispose();
		m_ContactsCounterIndirectBuffer?.Dispose();
		if (m_ContactsCounterCopy.IsCreated)
		{
			m_ContactsCounterCopy.Dispose();
		}
	}

	public void CollisionDetection(float dt)
	{
		m_Cmd.BeginSample(ProfileId.CollisionDetection);
		if (Solver.Config.CollisionSettings.ColliderCollisionsEnabled || Solver.Config.CollisionSettings.ParticleCollisionsEnabled)
		{
			Prepare();
		}
		if (Solver.Config.CollisionSettings.ColliderCollisionsEnabled)
		{
			CopyCollidersDataFromCpuToGpu();
			UpdateCollidersGrid();
		}
		if (Solver.Config.CollisionSettings.ColliderCollisionsEnabled || Solver.Config.CollisionSettings.ParticleCollisionsEnabled)
		{
			BuildSimplexAabb(dt);
		}
		if (Solver.Config.CollisionSettings.ParticleCollisionsEnabled)
		{
			UpdateSimplexGrid();
		}
		if (Solver.Config.CollisionSettings.ColliderCollisionsEnabled)
		{
			GenerateColliderContactsGlobal();
			m_ColliderGrid.DoReadbackMetrics(m_Cmd);
		}
		if (Solver.Config.CollisionSettings.ParticleCollisionsEnabled)
		{
			GenerateParticleContactsGlobal(dt);
			m_ParticleGrid.DoReadbackMetrics(m_Cmd);
		}
		DoReadback(m_Cmd);
		m_Cmd.EndSample(ProfileId.CollisionDetection);
	}

	private void DoReadback(CommandBuffer cmd)
	{
		if (m_IsReadbackFinished)
		{
			m_IsReadbackFinished = false;
			cmd.RequestAsyncReadback(m_ContactsCounterBuffer, OnReadbackFinished);
		}
	}

	private void OnReadbackFinished(AsyncGPUReadbackRequest request)
	{
		if (request.done)
		{
			if (m_ContactsCounterCopy.IsCreated)
			{
				m_ContactsCounterCopy.CopyFrom(request.GetData<int>());
			}
			m_IsReadbackFinished = true;
		}
	}

	private void Prepare()
	{
		m_Cmd.BeginSample("Prepare");
		m_Cmd.SetBufferData(m_ContactsCounterBuffer, m_ResetContactsCounterData);
		m_Cmd.SetBufferData(m_ContactsCounterIndirectBuffer, m_ResetContactCounterIndirectData);
		m_Cmd.EndSample("Prepare");
	}

	private void CopyCollidersDataFromCpuToGpu()
	{
		m_Cmd.BeginSample("CopyCollidersDataFromCpuToGpu");
		m_Cmd.SetBufferData(m_SolverImpl.ColliderReplicator.ColliderDescriptorSoA.Transform, Solver.ColliderWorld.ColliderDescriptorSoA.Transform);
		m_Cmd.SetBufferData(m_SolverImpl.ColliderReplicator.ColliderDescriptorSoA.PrevTransform, Solver.ColliderWorld.ColliderDescriptorSoA.PrevTransform);
		m_Cmd.SetBufferData(m_SolverImpl.ColliderReplicator.ColliderDescriptorSoA.Aabb, Solver.ColliderWorld.ColliderDescriptorSoA.Aabb);
		m_Cmd.SetBufferData(m_SolverImpl.ColliderReplicator.ColliderDescriptorSoA.Layer, Solver.ColliderWorld.ColliderDescriptorSoA.Layer);
		m_Cmd.SetBufferData(m_SolverImpl.ColliderReplicator.ColliderDescriptorSoA.Shape, Solver.ColliderWorld.ColliderDescriptorSoA.Shape);
		m_Cmd.EndSample("CopyCollidersDataFromCpuToGpu");
	}

	private void UpdateCollidersGrid()
	{
		m_Cmd.BeginSample("UpdateCollidersGrid");
		m_ColliderGrid.BuildCollidersGrid(m_Cmd, m_SolverImpl.ColliderReplicator.ColliderIndicesMapBuffer, m_SolverImpl.ColliderReplicator.ColliderDescriptorSoA.Aabb, m_SolverImpl.Solver.ColliderWorld.EntityAllocationMap.Count);
		m_Cmd.EndSample("UpdateCollidersGrid");
	}

	private void BuildSimplexAabb(float dt)
	{
		if (m_SolverImpl.Solver.Culler.VisibleBodyIndices.Length != 0)
		{
			m_Cmd.BeginSample("BuildSimplexAabb");
			m_Cmd.SetGlobalBuffer(m_SolverImpl.BodyReplicator.VisibleBodyIndices.NameId, m_SolverImpl.BodyReplicator.VisibleBodyIndices.Buffer);
			m_SolverImpl.BodyReplicator.BodyDescriptorSoA.PushToGpu(m_Cmd);
			m_SolverImpl.BodyReplicator.ConstraintSoA.PushToGpu(m_Cmd);
			m_SolverImpl.BodyReplicator.ParticlesSoA.PushToGpu(m_Cmd);
			m_Cmd.SetGlobalFloat(ShaderPropertyId._XpbdDt, dt);
			m_Cmd.SetGlobalFloat(ShaderPropertyId._XpbdContinousCollisionDetection, Solver.Config.CollisionSettings.ParticleCCD);
			m_Cmd.SetGlobalFloat(ShaderPropertyId._XpbdCollisionMargin, Solver.Config.CollisionSettings.CollisionMargin);
			m_Shaders.KernelBuildSimplexAabb.Dispatch(m_Cmd, m_SolverImpl.Solver.Culler.VisibleBodyIndices.Length, 1, 1);
			m_Cmd.EndSample("BuildSimplexAabb");
		}
	}

	private void UpdateSimplexGrid()
	{
		m_Cmd.BeginSample("UpdateSimplexGrid");
		m_ParticleGrid.BuildParticlesGrid(m_Cmd, Solver.BodyAllocator, Solver.Culler, m_SolverImpl.BodyReplicator);
		m_Cmd.EndSample("UpdateSimplexGrid");
	}

	private void GenerateColliderContactsGlobal()
	{
		if (m_SolverImpl.Solver.Culler.VisibleBodyIndices.Length != 0)
		{
			m_Cmd.BeginSample(ProfileId.GenerateColliderContactsGlobal);
			m_SolverImpl.BodyReplicator.VisibleBodyIndices.SetGlobal(m_Cmd);
			m_SolverImpl.BodyReplicator.BodyDescriptorSoA.PushToGpu(m_Cmd);
			m_SolverImpl.BodyReplicator.ConstraintSettingsSoA.PushToGpu(m_Cmd);
			m_SolverImpl.BodyReplicator.ConstraintSoA.PushToGpu(m_Cmd);
			m_SolverImpl.BodyReplicator.ParticlesSoA.PushToGpu(m_Cmd);
			m_SolverImpl.ColliderReplicator.ColliderDescriptorSoA.PushToGpu(m_Cmd);
			m_Cmd.SetGlobalBuffer(m_ColliderGrid.HashmapKeys.NameId, m_ColliderGrid.HashmapKeys.Buffer);
			m_Cmd.SetGlobalBuffer(m_ColliderGrid.HashmapValues.NameId, m_ColliderGrid.HashmapValues.Buffer);
			m_Cmd.SetGlobalBuffer(m_ColliderGrid.MetricsBuffer.NameId, m_ColliderGrid.MetricsBuffer.Buffer);
			m_ContactsBuffer.SetGlobal(m_Cmd);
			m_ContactsCounterBuffer.SetGlobal(m_Cmd);
			m_ContactsCounterIndirectBuffer.SetGlobal(m_Cmd);
			m_Cmd.SetGlobalInt(ShaderPropertyId._XpbdMaxContactsCount, Solver.Config.CollisionSettings.MaxContactsCount);
			m_Cmd.SetGlobalInt(ShaderPropertyId._XpbdHashmapSize, m_ColliderGrid.HashmapSize);
			m_Cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapMaxHashtableLookupIterations, 100);
			m_Cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapMetricsObjectSizeSum, 0);
			m_Cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapMetricsActiveContactsCount, 2);
			m_Cmd.SetGlobalFloat(ShaderPropertyId._XpbdSpatialHashmapSizeDescretizer, 100f);
			m_Cmd.SetGlobalFloat(ShaderPropertyId._XpbdSpatialHashmapSpacingScale, 2f);
			m_Cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapObjectsCount, m_ColliderGrid.ObjectsCount);
			m_Cmd.SetGlobalInt(ShaderPropertyId._XpbdGenerateContactsIterations, Solver.Config.CollisionSettings.OptimizationIterations);
			m_Cmd.SetGlobalFloat(ShaderPropertyId._XpbdGenerateContactsTolerance, Solver.Config.CollisionSettings.SurfaceCollisionTolerance);
			m_Shaders.KernelGenerateColliderContactsGlobal.Dispatch(m_Cmd, m_SolverImpl.Solver.Culler.VisibleBodyIndices.Length, 1, 1);
			m_Cmd.EndSample(ProfileId.GenerateColliderContactsGlobal);
			m_Cmd.BeginSample(ProfileId.OptimizeColliderContactsGlobal);
			m_Shaders.KernelOptimizeContacts.DispatchIndirect(m_Cmd, m_ContactsCounterIndirectBuffer, 0u);
			m_Cmd.EndSample(ProfileId.OptimizeColliderContactsGlobal);
		}
	}

	private void GenerateParticleContactsGlobal(float dt)
	{
		if (m_SolverImpl.Solver.Culler.VisibleBodyIndices.Length != 0)
		{
			m_Cmd.BeginSample("GenerateParticleContactsGlobal");
			m_SolverImpl.BodyReplicator.VisibleBodyIndices.SetGlobal(m_Cmd);
			m_SolverImpl.BodyReplicator.BodyDescriptorSoA.PushToGpu(m_Cmd);
			m_SolverImpl.BodyReplicator.ConstraintSettingsSoA.PushToGpu(m_Cmd);
			m_SolverImpl.BodyReplicator.ConstraintSoA.PushToGpu(m_Cmd);
			m_SolverImpl.BodyReplicator.ParticlesSoA.PushToGpu(m_Cmd);
			m_Cmd.SetGlobalBuffer(m_ParticleGrid.HashmapKeys.NameId, m_ParticleGrid.HashmapKeys.Buffer);
			m_Cmd.SetGlobalBuffer(m_ParticleGrid.HashmapValues.NameId, m_ParticleGrid.HashmapValues.Buffer);
			m_Cmd.SetGlobalBuffer(m_ParticleGrid.MetricsBuffer.NameId, m_ParticleGrid.MetricsBuffer.Buffer);
			m_ContactsBuffer.SetGlobal(m_Cmd);
			m_ContactsCounterBuffer.SetGlobal(m_Cmd);
			m_ContactsCounterIndirectBuffer.SetGlobal(m_Cmd);
			m_Cmd.SetGlobalInt(ShaderPropertyId._XpbdHashmapSize, m_ParticleGrid.HashmapSize);
			m_Cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapMaxHashtableLookupIterations, 100);
			m_Cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapMetricsObjectSizeSum, 0);
			m_Cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapMetricsActiveContactsCount, 2);
			m_Cmd.SetGlobalFloat(ShaderPropertyId._XpbdSpatialHashmapSizeDescretizer, 100f);
			m_Cmd.SetGlobalFloat(ShaderPropertyId._XpbdSpatialHashmapSpacingScale, 2f);
			m_Cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapObjectsCount, m_ParticleGrid.ObjectsCount);
			m_Cmd.SetGlobalInt(ShaderPropertyId._XpbdGenerateContactsIterations, Solver.Config.CollisionSettings.OptimizationIterations);
			m_Cmd.SetGlobalFloat(ShaderPropertyId._XpbdGenerateContactsTolerance, Solver.Config.CollisionSettings.SurfaceCollisionTolerance);
			m_Cmd.SetGlobalFloat(ShaderPropertyId._XpbdCollisionMargin, Solver.Config.CollisionSettings.CollisionMargin);
			m_Cmd.SetGlobalFloat(ShaderPropertyId._XpbdDt, dt);
			m_Shaders.KernelGenerateParticleContactsGlobal.Dispatch(m_Cmd, m_SolverImpl.Solver.Culler.VisibleBodyIndices.Length, 1, 1);
			m_Cmd.EndSample("GenerateParticleContactsGlobal");
		}
	}

	public BroadphaseStats GetStats()
	{
		BroadphaseStats result = default(BroadphaseStats);
		result.GridSpacing = m_ColliderGrid.GetSpacing();
		result.SpatialHashmapLoadFactor = m_ColliderGrid.GetLoadFactor();
		result.HashmapSize = m_ColliderGrid.GetHashmapSize();
		result.OccupiedCellsCount = m_ColliderGrid.GetOccupiedCellsCount();
		result.ObjectsSizeSum = m_ColliderGrid.GetObjectsSizeSum();
		result.ObjectsCount = m_ColliderGrid.ObjectsCount;
		result.ActiveColliderContactsCount = m_ColliderGrid.GetActiveContactsCount();
		result.SimplexGridSpacing = m_ParticleGrid.GetSpacing();
		result.SimplexSpatialHashmapLoadFactor = m_ParticleGrid.GetLoadFactor();
		result.SimplexHashmapSize = m_ParticleGrid.GetHashmapSize();
		result.SimplexOccupiedCellsCount = m_ParticleGrid.GetOccupiedCellsCount();
		result.ParticleSizeSum = m_ParticleGrid.GetObjectsSizeSum();
		result.SimplexCount = m_ParticleGrid.ObjectsCount;
		result.ActiveParticleContactsCount = m_ParticleGrid.GetActiveContactsCount();
		return result;
	}

	public void SetCmd(CommandBuffer cmd)
	{
		m_Cmd = cmd;
	}

	public MemoryStat GetMemoryStat()
	{
		MemoryStat result = m_ColliderGrid.GetMemoryStat() + m_ParticleGrid.GetMemoryStat();
		result.Gpu += m_ContactsBuffer.GetSizeInBytes();
		return result;
	}

	public int GetContactsCount()
	{
		return m_ContactsCounterCopy[0];
	}
}
