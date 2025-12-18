using System.Collections.Generic;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.Collisions;
using Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase;
using Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU;
using Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.GPU;
using Owlcat.Runtime.Visual.XPBD.Culling;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.ParticleAttachments;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Owlcat.Runtime.Visual.XPBD.Solvers.CPU;
using Owlcat.Runtime.Visual.XPBD.Solvers.GPU;
using Owlcat.Runtime.Visual.XPBD.Stats;
using UnityEngine;
using UnityEngine.Jobs;

namespace Owlcat.Runtime.Visual.XPBD.Solvers;

public class Solver
{
	private XPBDConfig m_Config;

	private ISolverImpl m_SolverImpl;

	private IBroadphaseImpl m_BroadphaseImpl;

	private Culler m_Culler;

	private float m_LastDt;

	private List<EntityAllocatorBase> m_Allocators = new List<EntityAllocatorBase>();

	private BodyAllocator m_BodyAllocator;

	private ParticleAttachmentAllocator m_ParticleAttachmentAllocator;

	private ColliderWorld m_ColliderWorld;

	private MeshDeformerAllocator m_MeshDeformerAllocator;

	public XPBDConfig Config => m_Config;

	public ISolverImpl SolverImpl => m_SolverImpl;

	public IBroadphaseImpl BroadphaseImpl => m_BroadphaseImpl;

	public BodyAllocator BodyAllocator => m_BodyAllocator;

	public ParticleAttachmentAllocator ParticleAttachmentAllocator => m_ParticleAttachmentAllocator;

	public ColliderWorld ColliderWorld => m_ColliderWorld;

	public MeshDeformerAllocator MeshDeformerAllocator => m_MeshDeformerAllocator;

	public float LastDt => m_LastDt;

	public Culler Culler => m_Culler;

	public Solver(XPBDConfig config)
	{
		m_Config = config;
		m_BodyAllocator = new BodyAllocator();
		m_ParticleAttachmentAllocator = new ParticleAttachmentAllocator();
		m_ColliderWorld = new ColliderWorld();
		m_MeshDeformerAllocator = new MeshDeformerAllocator();
		m_Allocators.Add(m_BodyAllocator);
		m_Allocators.Add(m_ParticleAttachmentAllocator);
		m_Allocators.Add(m_ColliderWorld);
		m_Allocators.Add(m_MeshDeformerAllocator);
		switch (config.SimulationSettings.Backend)
		{
		case Backend.CPU:
			m_SolverImpl = new CpuSolverImpl(this);
			m_BroadphaseImpl = new CpuBroadphaseGlobal(this);
			break;
		case Backend.GPU:
			m_SolverImpl = new GpuSolverImpl(this);
			m_BroadphaseImpl = new GpuBroadphaseGlobal(this);
			break;
		}
		m_Culler = new Culler(this);
	}

	public void Dispose()
	{
		m_SolverImpl.Dispose();
		m_BroadphaseImpl.Dispose();
		foreach (EntityAllocatorBase allocator in m_Allocators)
		{
			allocator.Dispose();
		}
		m_Culler?.Dispose();
	}

	internal void RegisterAuthoring(AuthoringBase authoring)
	{
		m_BodyAllocator.RegisterEntity(authoring);
	}

	internal void UnregisterAuthoring(AuthoringBase authoring)
	{
		m_BodyAllocator.UnregisterEntity(authoring);
	}

	internal void SyncAuthoringEnabledState(AuthoringBase authoringBase)
	{
		if (authoringBase.IsActiveInSimulation)
		{
			m_BodyAllocator.SyncAuthoringEnabledState(authoringBase);
			authoringBase.AfterEnabledStateSync(this);
		}
	}

	internal void ResetAuthoring(AuthoringBase authoring)
	{
		UnregisterAuthoring(authoring);
		RegisterAuthoring(authoring);
	}

	internal void RegisterParticleAttachment(ParticleAttachment particleAttachment)
	{
		m_ParticleAttachmentAllocator.RegisterEntity(particleAttachment);
	}

	internal void UnregisterParticleAttachment(ParticleAttachment particleAttachment)
	{
		m_ParticleAttachmentAllocator.UnregisterEntity(particleAttachment);
	}

	internal void RegisterCollider(XpbdCollider xpbdCollider)
	{
		m_ColliderWorld.RegisterEntity(xpbdCollider);
	}

	internal void UnregisterCollider(XpbdCollider xpbdCollider)
	{
		m_ColliderWorld.UnregisterEntity(xpbdCollider);
	}

	internal void RegisterMeshDeformer(MeshDeformer meshDeformer)
	{
		m_MeshDeformerAllocator.RegisterEntity(meshDeformer);
	}

	internal void UnregisterMeshDeformer(MeshDeformer meshDeformer)
	{
		m_MeshDeformerAllocator.UnregisterEntity(meshDeformer);
	}

	internal void PrepareFrame()
	{
		foreach (EntityAllocatorBase allocator in m_Allocators)
		{
			allocator.Build();
		}
		foreach (AuthoringBase key in BodyAllocator.EntityAllocationMap.Keys)
		{
			key.PrepareFrame(this);
		}
	}

	internal void BeginStep(in UpdateContext updateContext)
	{
		m_Culler.Cull();
		m_LastDt = updateContext.StepDelta;
		m_SolverImpl.BeginStep(in updateContext);
	}

	internal void Step(in UpdateContext updateContext)
	{
		m_SolverImpl.Step(in updateContext);
	}

	internal void EndStep(in UpdateContext updateContext)
	{
		m_SolverImpl.EndStep(in updateContext);
		m_ParticleAttachmentAllocator.EndStep();
	}

	internal void UpdateSkin(SkinnedMeshBody body, GraphicsBuffer vertexBuffer, bool recalculateNormals)
	{
		m_SolverImpl.UpdateSkin(body, vertexBuffer, recalculateNormals);
	}

	internal void UpdateBoneTranforms(SkeletonBody body, TransformAccessArray bones)
	{
		m_SolverImpl.UpdateBoneTranforms(body, bones);
	}

	internal void UpdateConstraintSettings(AuthoringBase authoring)
	{
		m_SolverImpl.UpdateConstraintSettings(authoring);
	}

	internal void UpdateMeshBasePositions(MeshBody body)
	{
		m_SolverImpl.UpdateMeshBasePositions(body);
	}

	internal void UpdateBodySimulationParameters(AuthoringBase authoring)
	{
		m_SolverImpl.UpdateBodySimulationParameters(authoring);
	}

	internal void UpdateLayer(AuthoringBase authoring)
	{
		m_SolverImpl.UpdateLayer(authoring);
	}

	internal void GetBodyAabb(AuthoringBase body, out Aabb bodyAabb)
	{
		m_SolverImpl.GetBodyAabb(body, out bodyAabb);
	}

	internal bool GetVisibility(AuthoringBase authoringBase)
	{
		return m_Culler.GetVisibility(BodyAllocator.EntityAllocationMap[authoringBase]);
	}

	internal MemoryStats GetMemoryStats()
	{
		MemoryStats result = default(MemoryStats);
		result.BodyAllocator = BodyAllocator.GetMemoryStat();
		result.ColliderAllocator = ColliderWorld.GetMemoryStat();
		result.ParticleAttachmentAllocator = ParticleAttachmentAllocator.GetMemoryStat();
		result.DeformerAllocator = MeshDeformerAllocator.GetMemoryStat();
		result.SolverImpl = m_SolverImpl.GetMemoryStat();
		result.BroadphaseHashGrid = m_BroadphaseImpl.GetMemoryStat();
		return result;
	}
}
