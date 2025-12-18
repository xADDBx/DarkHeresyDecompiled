using System.Collections.Generic;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU.Jobs;
using Owlcat.Runtime.Visual.XPBD.Solvers;
using Owlcat.Runtime.Visual.XPBD.Solvers.CPU;
using Owlcat.Runtime.Visual.XPBD.Solvers.CPU.Jobs;
using Owlcat.Runtime.Visual.XPBD.Stats;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU;

public class CpuBroadphaseGlobal : IBroadphaseImpl
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct ContactComparer : IComparer<Contact>
	{
		public int Compare(Contact x, Contact y)
		{
			return x.ParticleIndicesA[0].CompareTo(y.ParticleIndicesA[0]);
		}
	}

	private SpatialHashGrid m_CollidersGrid;

	private SpatialHashGrid m_ParticlesGrid;

	private CpuSolverImpl m_SolverImpl;

	private NativeList<Contact> m_Contacts;

	private AtomicCounter m_ContactsCounter;

	public Solver Solver { get; private set; }

	public NativeList<Contact> Contacts => m_Contacts;

	public CpuBroadphaseGlobal(Solver solver)
	{
		Solver = solver;
		m_SolverImpl = (CpuSolverImpl)Solver.SolverImpl;
		m_CollidersGrid = new SpatialHashGrid();
		m_ParticlesGrid = new SpatialHashGrid();
		m_Contacts = new NativeList<Contact>(solver.Config.CollisionSettings.MaxContactsCount, Allocator.Persistent);
		m_ContactsCounter = AtomicCounter.Create();
	}

	public void Dispose()
	{
		m_CollidersGrid?.Dispose();
		m_ParticlesGrid?.Dispose();
		if (m_Contacts.IsCreated)
		{
			m_Contacts.Dispose();
		}
		m_ContactsCounter.Dispose();
	}

	public void CollisionDetection(float dt)
	{
		Prepare();
		if (Solver.Config.CollisionSettings.ColliderCollisionsEnabled)
		{
			UpdateCollidersGrid();
		}
		if (Solver.Config.CollisionSettings.ColliderCollisionsEnabled || Solver.Config.CollisionSettings.ParticleCollisionsEnabled)
		{
			BuildSimplexAabbs(dt);
		}
		if (Solver.Config.CollisionSettings.ParticleCollisionsEnabled)
		{
			UpdateSimplexGrid();
		}
		if (Solver.Config.CollisionSettings.ColliderCollisionsEnabled)
		{
			GenerateColliderContactsGlobal();
		}
		if (Solver.Config.CollisionSettings.ParticleCollisionsEnabled)
		{
			GenerateParticleContactsGlobal(dt);
		}
	}

	private void Prepare()
	{
		m_Contacts.Clear();
		m_ContactsCounter.Reset();
		if (m_Contacts.Capacity != Solver.Config.CollisionSettings.MaxContactsCount)
		{
			m_Contacts.Dispose();
			m_Contacts = new NativeList<Contact>(Solver.Config.CollisionSettings.MaxContactsCount, Allocator.Persistent);
		}
	}

	private void UpdateCollidersGrid()
	{
		m_SolverImpl.m_Jobs.Handle = m_CollidersGrid.BuildCollidersGrid(m_SolverImpl.m_Jobs.Handle, m_SolverImpl.Solver.ColliderWorld.IndicesMap, m_SolverImpl.Solver.ColliderWorld.ColliderDescriptorSoA.Aabb, m_SolverImpl.Solver.ColliderWorld.EntityAllocationMap.Count);
	}

	private void BuildSimplexAabbs(float dt)
	{
		foreach (KeyValuePair<AuthoringBase, int> item in Solver.BodyAllocator.EntityAllocationMap)
		{
			int2 simplexConstraintsRange = Solver.BodyAllocator.BodyDescriptorSoA.SimplexConstraintsRange[item.Value];
			int2 constraintsRange = Solver.BodyAllocator.BodyDescriptorSoA.ConstraintsRange[item.Value];
			int2 particlesRange = Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange[item.Value];
			int bodyLayer = Solver.BodyAllocator.BodyDescriptorSoA.Layer[item.Value];
			bool flag = Solver.Culler.BodyVisibility[item.Value] != 0;
			float particleCCD = Solver.Config.CollisionSettings.ParticleCCD;
			if (simplexConstraintsRange.x > -1 && flag)
			{
				BuildSimplexAabbJob buildSimplexAabbJob = default(BuildSimplexAabbJob);
				buildSimplexAabbJob.Dt = dt;
				buildSimplexAabbJob.CollisionMargin = Solver.Config.CollisionSettings.CollisionMargin;
				buildSimplexAabbJob.ContinousCollisionDetection = particleCCD;
				buildSimplexAabbJob.BodyLayer = bodyLayer;
				buildSimplexAabbJob.SimplexConstraintsRange = simplexConstraintsRange;
				buildSimplexAabbJob.ConstraintsRange = constraintsRange;
				buildSimplexAabbJob.ParticlesRange = particlesRange;
				buildSimplexAabbJob.SimplexIndices = Solver.BodyAllocator.ConstraintSoA.Indices;
				buildSimplexAabbJob.SimplexParameters0 = Solver.BodyAllocator.ConstraintSoA.Parameters0;
				buildSimplexAabbJob.SimplexParameters1 = Solver.BodyAllocator.ConstraintSoA.Parameters1;
				buildSimplexAabbJob.ParticlePosition = Solver.BodyAllocator.ParticleSoA.Position;
				buildSimplexAabbJob.ParticleVelocity = Solver.BodyAllocator.ParticleSoA.Velocity;
				buildSimplexAabbJob.ParticleRadius = Solver.BodyAllocator.ParticleSoA.Radius;
				BuildSimplexAabbJob jobData = buildSimplexAabbJob;
				JobHandle handle = IJobParallelForExtensions.ScheduleByRef(ref jobData, simplexConstraintsRange.y, 16, m_SolverImpl.m_Jobs.Handle);
				m_SolverImpl.m_Jobs.CombineWithGroup(JobGroup.BuildSimplexAabb, handle);
			}
		}
		m_SolverImpl.m_Jobs.FlushGroup(JobGroup.BuildSimplexAabb);
	}

	private void UpdateSimplexGrid()
	{
		m_SolverImpl.m_Jobs.Handle = m_ParticlesGrid.BuildParticlesGrid(m_SolverImpl.m_Jobs.Handle, Solver.BodyAllocator, Solver.Culler);
	}

	private void GenerateColliderContactsGlobal()
	{
		foreach (KeyValuePair<AuthoringBase, int> item in Solver.BodyAllocator.EntityAllocationMap)
		{
			int2 simplexConstraintsRange = Solver.BodyAllocator.BodyDescriptorSoA.SimplexConstraintsRange[item.Value];
			int2 constraintsRange = Solver.BodyAllocator.BodyDescriptorSoA.ConstraintsRange[item.Value];
			uint bitMask = Solver.BodyAllocator.BodyDescriptorSoA.EnabledConstraintTypeMask[item.Value];
			int2 constraintSettingsRange = Solver.BodyAllocator.BodyDescriptorSoA.ConstraintSettingsRange[item.Value];
			int2 particlesRange = Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange[item.Value];
			bool flag = Solver.Culler.BodyVisibility[item.Value] != 0;
			int num;
			if (simplexConstraintsRange.x > -1)
			{
				int bitIndex = 4;
				num = (XPBDMath.IsBitSetted(in bitMask, in bitIndex) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
			if (((uint)num & (flag ? 1u : 0u)) != 0)
			{
				GenerateColliderContactsGlobalJob generateColliderContactsGlobalJob = default(GenerateColliderContactsGlobalJob);
				generateColliderContactsGlobalJob.ConstraintsRange = constraintsRange;
				generateColliderContactsGlobalJob.SimplexConstraintsRange = simplexConstraintsRange;
				generateColliderContactsGlobalJob.MaxContactsCount = Solver.Config.CollisionSettings.MaxContactsCount;
				generateColliderContactsGlobalJob.ConstraintSettingsRange = constraintSettingsRange;
				generateColliderContactsGlobalJob.ParticlesRange = particlesRange;
				generateColliderContactsGlobalJob.OptimizationIterations = Solver.Config.CollisionSettings.OptimizationIterations;
				generateColliderContactsGlobalJob.OptimizationTolerance = Solver.Config.CollisionSettings.SurfaceCollisionTolerance;
				generateColliderContactsGlobalJob.ConstraintSettings = Solver.BodyAllocator.ConstraintSettingsSoA.Array;
				generateColliderContactsGlobalJob.SimplexIndices = Solver.BodyAllocator.ConstraintSoA.Indices;
				generateColliderContactsGlobalJob.SimplexParameters0 = Solver.BodyAllocator.ConstraintSoA.Parameters0;
				generateColliderContactsGlobalJob.SimplexParameters1 = Solver.BodyAllocator.ConstraintSoA.Parameters1;
				generateColliderContactsGlobalJob.ColliderAabb = Solver.ColliderWorld.ColliderDescriptorSoA.Aabb;
				generateColliderContactsGlobalJob.ColliderShape = Solver.ColliderWorld.ColliderDescriptorSoA.Shape;
				generateColliderContactsGlobalJob.ColliderTransform = Solver.ColliderWorld.ColliderDescriptorSoA.Transform;
				generateColliderContactsGlobalJob.ColliderPrevTransform = Solver.ColliderWorld.ColliderDescriptorSoA.PrevTransform;
				generateColliderContactsGlobalJob.ColliderLayer = Solver.ColliderWorld.ColliderDescriptorSoA.Layer;
				generateColliderContactsGlobalJob.ParticlePosition = Solver.BodyAllocator.ParticleSoA.Position;
				generateColliderContactsGlobalJob.ParticleRadius = Solver.BodyAllocator.ParticleSoA.Radius;
				generateColliderContactsGlobalJob.ParticleInvMass = Solver.BodyAllocator.ParticleSoA.InvMass;
				generateColliderContactsGlobalJob.Contacts = m_Contacts.AsParallelWriter();
				generateColliderContactsGlobalJob.ContactsCounter = m_ContactsCounter;
				generateColliderContactsGlobalJob.ActiveContactsCount = m_CollidersGrid.ActiveContactsCount;
				generateColliderContactsGlobalJob.ObjectSizeSum = m_CollidersGrid.ObjectSizeSum;
				generateColliderContactsGlobalJob.ObjectsCount = m_CollidersGrid.ObjectsCount;
				generateColliderContactsGlobalJob.HashmapSize = m_CollidersGrid.HashmapSize;
				generateColliderContactsGlobalJob.HashmapKeys = m_CollidersGrid.HashmapKeys;
				generateColliderContactsGlobalJob.HashmapValues = m_CollidersGrid.HashmapValues;
				GenerateColliderContactsGlobalJob jobData = generateColliderContactsGlobalJob;
				JobHandle handle = IJobParallelForExtensions.ScheduleByRef(ref jobData, simplexConstraintsRange.y, 1, m_SolverImpl.m_Jobs.Handle);
				m_SolverImpl.m_Jobs.CombineWithGroup(JobGroup.GenerateContacts, handle);
			}
		}
		m_SolverImpl.m_Jobs.FlushGroup(JobGroup.GenerateContacts);
	}

	private void GenerateParticleContactsGlobal(float dt)
	{
		foreach (KeyValuePair<AuthoringBase, int> item in Solver.BodyAllocator.EntityAllocationMap)
		{
			int2 simplexConstraintsRange = Solver.BodyAllocator.BodyDescriptorSoA.SimplexConstraintsRange[item.Value];
			int2 constraintsRange = Solver.BodyAllocator.BodyDescriptorSoA.ConstraintsRange[item.Value];
			int2 constraintSettingsRange = Solver.BodyAllocator.BodyDescriptorSoA.ConstraintSettingsRange[item.Value];
			uint bitMask = Solver.BodyAllocator.BodyDescriptorSoA.EnabledConstraintTypeMask[item.Value];
			ParticleCollisionMode particleCollisionMode = (ParticleCollisionMode)Solver.BodyAllocator.ConstraintSettingsSoA[constraintSettingsRange.x + 4].z;
			bool flag = Solver.Culler.BodyVisibility[item.Value] != 0;
			int num;
			if (simplexConstraintsRange.x > -1)
			{
				int bitIndex = 4;
				if (XPBDMath.IsBitSetted(in bitMask, in bitIndex))
				{
					num = ((particleCollisionMode != ParticleCollisionMode.Disabled) ? 1 : 0);
					goto IL_0117;
				}
			}
			num = 0;
			goto IL_0117;
			IL_0117:
			if (((uint)num & (flag ? 1u : 0u)) != 0)
			{
				GenerateParticleContactsGlobalJob generateParticleContactsGlobalJob = default(GenerateParticleContactsGlobalJob);
				generateParticleContactsGlobalJob.SimplexConstraintsRange = simplexConstraintsRange;
				generateParticleContactsGlobalJob.ConstraintsRange = constraintsRange;
				generateParticleContactsGlobalJob.ConstraintSettingsRange = constraintSettingsRange;
				generateParticleContactsGlobalJob.Dt = dt;
				generateParticleContactsGlobalJob.CollisionMargin = Solver.Config.CollisionSettings.CollisionMargin;
				generateParticleContactsGlobalJob.OptimizationIterations = Solver.Config.CollisionSettings.OptimizationIterations;
				generateParticleContactsGlobalJob.OptimizationTolerance = Solver.Config.CollisionSettings.SurfaceCollisionTolerance;
				generateParticleContactsGlobalJob.MaxContactsCount = Solver.Config.CollisionSettings.MaxContactsCount;
				generateParticleContactsGlobalJob.ConstraintSettings = Solver.BodyAllocator.ConstraintSettingsSoA.Array;
				generateParticleContactsGlobalJob.SimplexIndices = Solver.BodyAllocator.ConstraintSoA.Indices;
				generateParticleContactsGlobalJob.SimplexParameters0 = Solver.BodyAllocator.ConstraintSoA.Parameters0;
				generateParticleContactsGlobalJob.SimplexParameters1 = Solver.BodyAllocator.ConstraintSoA.Parameters1;
				generateParticleContactsGlobalJob.ParticlePosition = Solver.BodyAllocator.ParticleSoA.Position;
				generateParticleContactsGlobalJob.ParticleBasePosition = Solver.BodyAllocator.ParticleSoA.BasePosition;
				generateParticleContactsGlobalJob.ParticleInvMass = Solver.BodyAllocator.ParticleSoA.InvMass;
				generateParticleContactsGlobalJob.ParticleRadius = Solver.BodyAllocator.ParticleSoA.Radius;
				generateParticleContactsGlobalJob.ParticleVelocity = Solver.BodyAllocator.ParticleSoA.Velocity;
				generateParticleContactsGlobalJob.ActiveContactsCount = m_ParticlesGrid.ActiveContactsCount;
				generateParticleContactsGlobalJob.HashmapSize = m_ParticlesGrid.HashmapSize;
				generateParticleContactsGlobalJob.ObjectsCount = m_ParticlesGrid.ObjectsCount;
				generateParticleContactsGlobalJob.ObjectSizeSum = m_ParticlesGrid.ObjectSizeSum;
				generateParticleContactsGlobalJob.HashmapKeys = m_ParticlesGrid.HashmapKeys;
				generateParticleContactsGlobalJob.HashmapValues = m_ParticlesGrid.HashmapValues;
				generateParticleContactsGlobalJob.Contacts = m_Contacts.AsParallelWriter();
				generateParticleContactsGlobalJob.ContactsCounter = m_ContactsCounter;
				GenerateParticleContactsGlobalJob jobData = generateParticleContactsGlobalJob;
				JobHandle handle = IJobParallelForExtensions.ScheduleByRef(ref jobData, simplexConstraintsRange.y, 16, m_SolverImpl.m_Jobs.Handle);
				m_SolverImpl.m_Jobs.CombineWithGroup(JobGroup.GenerateContacts, handle);
			}
		}
		m_SolverImpl.m_Jobs.FlushGroup(JobGroup.GenerateContacts);
	}

	public BroadphaseStats GetStats()
	{
		BroadphaseStats result = default(BroadphaseStats);
		result.GridSpacing = m_CollidersGrid.GetSpacing();
		result.SpatialHashmapLoadFactor = m_CollidersGrid.GetLoadFactor();
		result.HashmapSize = m_CollidersGrid.GetHashmapSize();
		result.OccupiedCellsCount = m_CollidersGrid.GetOccupiedCellsCount();
		result.ObjectsSizeSum = m_CollidersGrid.GetObjectsSizeSum();
		result.ObjectsCount = m_CollidersGrid.ObjectsCount;
		result.ActiveColliderContactsCount = m_CollidersGrid.GetActiveContactsCount();
		result.SimplexGridSpacing = m_ParticlesGrid.GetSpacing();
		result.SimplexSpatialHashmapLoadFactor = m_ParticlesGrid.GetLoadFactor();
		result.SimplexHashmapSize = m_ParticlesGrid.GetHashmapSize();
		result.SimplexOccupiedCellsCount = m_ParticlesGrid.GetOccupiedCellsCount();
		result.ParticleSizeSum = m_ParticlesGrid.GetObjectsSizeSum();
		result.SimplexCount = m_ParticlesGrid.ObjectsCount;
		result.ActiveParticleContactsCount = m_ParticlesGrid.GetActiveContactsCount();
		return result;
	}

	public void SetCmd(CommandBuffer cmd)
	{
	}

	public MemoryStat GetMemoryStat()
	{
		MemoryStat result = m_CollidersGrid.GetMemoryStat() + m_ParticlesGrid.GetMemoryStat();
		result.Cpu += m_Contacts.Capacity * UnsafeUtility.SizeOf<Contact>();
		return result;
	}

	public int GetContactsCount()
	{
		return m_ContactsCounter.Count;
	}
}
