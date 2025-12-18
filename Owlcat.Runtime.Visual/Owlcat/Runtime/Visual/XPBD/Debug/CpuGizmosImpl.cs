using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.Collisions;
using Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU;
using Owlcat.Runtime.Visual.XPBD.Constraints;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.Debug.Jobs;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Owlcat.Runtime.Visual.XPBD.Solvers.CPU;
using Owlcat.Runtime.Visual.XPBD.Stats;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Debug;

public class CpuGizmosImpl : IGizmosImpl
{
	private uint[] m_IndirectArgs = new uint[5] { 0u, 1u, 0u, 0u, 0u };

	private CpuSolverImpl m_SolverImpl;

	private NativeList<GizmoMeshVertex> m_GizmoVertices;

	private NativeList<GizmoDeformedVertex> m_GizmoDeformedVertices;

	private List<GraphicsBufferWrapper> m_Buffers;

	private GraphicsBufferWrapper<int> m_BodiesMapBuffer;

	private GraphicsBufferWrapper<int> m_BodyVisibilityBuffer;

	private GraphicsBufferWrapper<Aabb> m_BodyAabbBuffer;

	private GraphicsBufferWrapper<int> m_ParticlesMap;

	private GraphicsBufferWrapper<float3> m_ParticlesPositionBuffer;

	private GraphicsBufferWrapper<float3> m_ParticlesVelocityBuffer;

	private GraphicsBufferWrapper<float> m_ParticlesRadiusBuffer;

	private GraphicsBufferWrapper<int4> m_ConstraintIndicesBuffer;

	private GraphicsBufferWrapper<int2> m_DistanceConstraintsMapBuffer;

	private GraphicsBufferWrapper<int2> m_BendConstraintsMapBuffer;

	private GraphicsBufferWrapper<int2> m_AngularConstraintsMapBuffer;

	private GraphicsBufferWrapper<float4> m_ConstraintsParameters0Buffer;

	private GraphicsBufferWrapper<float4> m_ConstraintsParameters1Buffer;

	private GraphicsBufferWrapper<GizmoMeshVertex> m_MeshVerticesBuffer;

	private GraphicsBufferWrapper<GizmoDeformedVertex> m_DeformableGizmosVerticesBuffer;

	private GraphicsBufferWrapper<int> m_CollidersMapBuffer;

	private GraphicsBufferWrapper<Aabb> m_ColliderAabbBuffer;

	private GraphicsBufferWrapper<int> m_SimplexMapBuffer;

	private GraphicsBufferWrapper<Contact> m_ContactsBuffer;

	private GraphicsBufferWrapper<uint> m_ColliderContactsIndirectArgsBuffer;

	private GraphicsBufferWrapper<uint> m_SimplexContactsIndirectArgsBuffer;

	public bool IsValid { get; private set; }

	public GraphicsBufferWrapper BodiesMapBuffer => m_BodiesMapBuffer;

	public GraphicsBufferWrapper BodyVisibilityBuffer => m_BodyVisibilityBuffer;

	public GraphicsBufferWrapper BodyAabbBuffer => m_BodyAabbBuffer;

	public GraphicsBufferWrapper ParticlesMapBuffer => m_ParticlesMap;

	public GraphicsBufferWrapper ParticlePositionBuffer => m_ParticlesPositionBuffer;

	public GraphicsBufferWrapper ParticleVelocityBuffer => m_ParticlesVelocityBuffer;

	public GraphicsBufferWrapper ParticleRadiusBuffer => m_ParticlesRadiusBuffer;

	public GraphicsBufferWrapper DistanceConstraintsMapBuffer => m_DistanceConstraintsMapBuffer;

	public GraphicsBufferWrapper ConstraintsIndicesBuffer => m_ConstraintIndicesBuffer;

	public GraphicsBufferWrapper BendConstraintsMapBuffer => m_BendConstraintsMapBuffer;

	public GraphicsBufferWrapper AngularConstraintsMapBuffer => m_AngularConstraintsMapBuffer;

	public GraphicsBufferWrapper ConstraintsParameters0Buffer => m_ConstraintsParameters0Buffer;

	public GraphicsBufferWrapper ConstraintsParameters1Buffer => m_ConstraintsParameters1Buffer;

	public GraphicsBufferWrapper MeshVerticesBuffer => m_MeshVerticesBuffer;

	public GraphicsBufferWrapper DeformableGizmosVerticesBuffer => m_DeformableGizmosVerticesBuffer;

	public GraphicsBufferWrapper CollidersMapBuffer => m_CollidersMapBuffer;

	public GraphicsBufferWrapper ColliderAabbBuffer => m_ColliderAabbBuffer;

	public GraphicsBufferWrapper SimplexMapBuffer => m_SimplexMapBuffer;

	public GraphicsBufferWrapper ContactsBuffer => m_ContactsBuffer;

	public GraphicsBufferWrapper ColliderContactsIndirectArgsBuffer => m_ColliderContactsIndirectArgsBuffer;

	public GraphicsBufferWrapper SimplexContactsIndirectArgsBuffer => m_SimplexContactsIndirectArgsBuffer;

	public CpuBroadphaseGlobal CpuBroadphaseGlobal => m_SolverImpl.Solver.BroadphaseImpl as CpuBroadphaseGlobal;

	public CpuGizmosImpl(CpuSolverImpl solverImpl)
	{
		m_SolverImpl = solverImpl;
		m_BodiesMapBuffer = new GraphicsBufferWrapper<int>("_XpbdBodiesMap", 128);
		m_BodyVisibilityBuffer = new GraphicsBufferWrapper<int>("_XpbdBodyVisibilityBuffer", 128);
		m_BodyAabbBuffer = new GraphicsBufferWrapper<Aabb>("_XpbdBodyAabbBuffer", 128);
		m_ParticlesMap = new GraphicsBufferWrapper<int>("_XpbdParticlesMap", 128);
		m_ParticlesPositionBuffer = new GraphicsBufferWrapper<float3>("_XpbdParticlePositionBuffer", 128);
		m_ParticlesVelocityBuffer = new GraphicsBufferWrapper<float3>("_XpbdParticleVelocityBuffer", 128);
		m_ParticlesRadiusBuffer = new GraphicsBufferWrapper<float>("_XpbdParticleRadiusBuffer", 128);
		m_ConstraintIndicesBuffer = new GraphicsBufferWrapper<int4>("_XpbdConstraintIndicesBuffer", 128);
		m_DistanceConstraintsMapBuffer = new GraphicsBufferWrapper<int2>("_XpbdConstraintsMap", 128);
		m_BendConstraintsMapBuffer = new GraphicsBufferWrapper<int2>("_XpbdConstraintsMap", 128);
		m_AngularConstraintsMapBuffer = new GraphicsBufferWrapper<int2>("_XpbdConstraintsMap", 128);
		m_ConstraintsParameters0Buffer = new GraphicsBufferWrapper<float4>("_XpbdConstraintParameters0Buffer", 128);
		m_ConstraintsParameters1Buffer = new GraphicsBufferWrapper<float4>("_XpbdConstraintParameters1Buffer", 128);
		m_MeshVerticesBuffer = new GraphicsBufferWrapper<GizmoMeshVertex>("_XpbdGizmoMeshVerticesBuffer", 128);
		m_DeformableGizmosVerticesBuffer = new GraphicsBufferWrapper<GizmoDeformedVertex>("_XpbdGizmoDeformedVerticesBuffer", 128);
		m_CollidersMapBuffer = new GraphicsBufferWrapper<int>("_XpbdColliderIndicesMapBuffer", 128);
		m_ColliderAabbBuffer = new GraphicsBufferWrapper<Aabb>("_XpbdColliderDescriptorAabbBuffer", 128);
		m_SimplexMapBuffer = new GraphicsBufferWrapper<int>("_XpbdSimplexMap", 128);
		m_ContactsBuffer = new GraphicsBufferWrapper<Contact>("_XpbdContactsBuffer", 128);
		m_ColliderContactsIndirectArgsBuffer = new GraphicsBufferWrapper<uint>("_XpbdContactsIndirectArgsBuffer", 5, GraphicsBuffer.Target.IndirectArguments);
		m_SimplexContactsIndirectArgsBuffer = new GraphicsBufferWrapper<uint>("_XpbdSimplexContactsIndirectArgsBuffer", 5, GraphicsBuffer.Target.IndirectArguments);
		m_Buffers = new List<GraphicsBufferWrapper>
		{
			m_BodiesMapBuffer, m_BodyVisibilityBuffer, m_BodyAabbBuffer, m_ParticlesMap, m_ParticlesPositionBuffer, m_ParticlesVelocityBuffer, m_ParticlesRadiusBuffer, m_ConstraintIndicesBuffer, m_DistanceConstraintsMapBuffer, m_BendConstraintsMapBuffer,
			m_AngularConstraintsMapBuffer, m_ConstraintsParameters0Buffer, m_ConstraintsParameters1Buffer, m_MeshVerticesBuffer, m_DeformableGizmosVerticesBuffer, m_CollidersMapBuffer, m_ColliderAabbBuffer, m_SimplexMapBuffer, m_ContactsBuffer, m_ColliderContactsIndirectArgsBuffer,
			m_SimplexContactsIndirectArgsBuffer
		};
		BodyAllocator bodyAllocator = m_SolverImpl.Solver.BodyAllocator;
		bodyAllocator.AfterAlloc = (Action)Delegate.Combine(bodyAllocator.AfterAlloc, new Action(OnAfterBodyAlloc));
		ColliderWorld colliderWorld = m_SolverImpl.Solver.ColliderWorld;
		colliderWorld.AfterAlloc = (Action)Delegate.Combine(colliderWorld.AfterAlloc, new Action(OnAfterColliderAlloc));
	}

	public void Dispose()
	{
		if (m_GizmoVertices.IsCreated)
		{
			m_GizmoVertices.Dispose();
		}
		if (m_GizmoDeformedVertices.IsCreated)
		{
			m_GizmoDeformedVertices.Dispose();
		}
		foreach (GraphicsBufferWrapper buffer in m_Buffers)
		{
			buffer.Dispose();
		}
		BodyAllocator bodyAllocator = m_SolverImpl.Solver.BodyAllocator;
		bodyAllocator.AfterAlloc = (Action)Delegate.Remove(bodyAllocator.AfterAlloc, new Action(OnAfterBodyAlloc));
	}

	private void OnAfterBodyAlloc()
	{
		InitBodyMaps();
	}

	private void OnAfterColliderAlloc()
	{
		InitColliderMaps();
	}

	private void InitBodyMaps()
	{
		InitBodyDescriptorMaps();
		InitParticleMaps();
		InitConstraintIndices();
		InitConstraintParameters();
		InitConstraintsMap(m_DistanceConstraintsMapBuffer, ConstraintType.Distance);
		InitConstraintsMap(m_BendConstraintsMapBuffer, ConstraintType.Bend);
		InitConstraintsMap(m_AngularConstraintsMapBuffer, ConstraintType.Angular);
		InitSimplicesMap();
	}

	private void InitBodyDescriptorMaps()
	{
		m_BodiesMapBuffer.Resize(m_SolverImpl.Solver.BodyAllocator.IndicesMap.Length);
		m_BodiesMapBuffer.SetData(m_SolverImpl.Solver.BodyAllocator.IndicesMap);
		m_BodyVisibilityBuffer.Resize(m_SolverImpl.Solver.Culler.BodyVisibility.Length);
		m_BodyAabbBuffer.Resize(m_SolverImpl.Solver.BodyAllocator.BodyDescriptorSoA.Aabb.Length);
	}

	private void InitParticleMaps()
	{
		m_ParticlesMap.Resize(m_SolverImpl.ParticlesMap.Length);
		m_ParticlesMap.SetData(m_SolverImpl.ParticlesMap);
		m_ParticlesPositionBuffer.Resize(m_SolverImpl.Solver.BodyAllocator.ParticleSoA.Position.Length);
		m_ParticlesVelocityBuffer.Resize(m_SolverImpl.Solver.BodyAllocator.ParticleSoA.Velocity.Length);
		m_ParticlesRadiusBuffer.Resize(m_SolverImpl.Solver.BodyAllocator.ParticleSoA.Radius.Length);
	}

	private void InitConstraintIndices()
	{
		m_ConstraintIndicesBuffer.Resize(m_SolverImpl.Solver.BodyAllocator.ConstraintSoA.Capacity);
		m_ConstraintIndicesBuffer.SetData(m_SolverImpl.Solver.BodyAllocator.ConstraintSoA.Indices);
	}

	private void InitConstraintParameters()
	{
		m_ConstraintsParameters0Buffer.Resize(m_SolverImpl.Solver.BodyAllocator.ConstraintSoA.Capacity);
		m_ConstraintsParameters0Buffer.SetData(m_SolverImpl.Solver.BodyAllocator.ConstraintSoA.Parameters0);
		m_ConstraintsParameters1Buffer.Resize(m_SolverImpl.Solver.BodyAllocator.ConstraintSoA.Capacity);
		m_ConstraintsParameters1Buffer.SetData(m_SolverImpl.Solver.BodyAllocator.ConstraintSoA.Parameters1);
	}

	private void InitConstraintsMap(GraphicsBufferWrapper<int2> buffer, ConstraintType constraintType)
	{
		NativeList<int2> nativeList = new NativeList<int2>(128, Allocator.Temp);
		foreach (KeyValuePair<AuthoringBase, int> item in m_SolverImpl.Solver.BodyAllocator.EntityAllocationMap)
		{
			int2 @int = m_SolverImpl.Solver.BodyAllocator.BodyDescriptorSoA.ConstraintBatchesRange[item.Value];
			int2 int2 = m_SolverImpl.Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange[item.Value];
			for (int i = 0; i < @int.y; i++)
			{
				int3 int3 = m_SolverImpl.Solver.BodyAllocator.ConstraintBatchSoA[@int.x + i];
				if (constraintType == (ConstraintType)int3.z)
				{
					for (int j = 0; j < int3.y; j++)
					{
						int2 value = new int2(int3.x + j, int2.x);
						nativeList.Add(in value);
					}
				}
			}
		}
		buffer.Resize(nativeList.Length);
		buffer.SetData(nativeList.AsArray());
		nativeList.Dispose();
	}

	private void InitSimplicesMap()
	{
		NativeList<int> nativeList = new NativeList<int>(128, Allocator.Temp);
		foreach (KeyValuePair<AuthoringBase, int> item in m_SolverImpl.Solver.BodyAllocator.EntityAllocationMap)
		{
			int2 @int = m_SolverImpl.Solver.BodyAllocator.BodyDescriptorSoA.SimplexConstraintsRange[item.Value];
			int2 int2 = m_SolverImpl.Solver.BodyAllocator.BodyDescriptorSoA.ConstraintsRange[item.Value];
			if (@int.x > -1)
			{
				for (int i = 0; i < @int.y; i++)
				{
					int value = @int.x + int2.x + i;
					nativeList.Add(in value);
				}
			}
		}
		m_SimplexMapBuffer.Resize(nativeList.Length);
		m_SimplexMapBuffer.SetData(nativeList.AsArray());
		nativeList.Dispose();
	}

	private void InitColliderMaps()
	{
		m_CollidersMapBuffer.Resize(m_SolverImpl.Solver.ColliderWorld.IndicesMap.Length);
		m_CollidersMapBuffer.SetData(m_SolverImpl.Solver.ColliderWorld.IndicesMap);
		m_ColliderAabbBuffer.Resize(m_SolverImpl.Solver.ColliderWorld.ColliderDescriptorSoA.Capacity);
		m_ColliderAabbBuffer.SetData(m_SolverImpl.Solver.ColliderWorld.ColliderDescriptorSoA.Aabb);
	}

	public void PushDataToGpu(CommandBuffer cmd)
	{
		XPBDDebug debugSettings = m_SolverImpl.Solver.Config.DebugSettings;
		if (!debugSettings.GizmosEnabled)
		{
			return;
		}
		if (debugSettings.DrawContactNormals || debugSettings.DrawConstraints != 0 || debugSettings.DrawParticles != 0 || debugSettings.DrawInertialForces || debugSettings.DrawSimplexContacts)
		{
			cmd.SetBufferData(m_ParticlesPositionBuffer.Buffer, m_SolverImpl.Solver.BodyAllocator.ParticleSoA.Position);
			cmd.SetBufferData(m_ParticlesRadiusBuffer.Buffer, m_SolverImpl.Solver.BodyAllocator.ParticleSoA.Radius);
		}
		if (debugSettings.DrawVelocities || debugSettings.DrawInertialForces)
		{
			cmd.SetBufferData(m_ParticlesVelocityBuffer.Buffer, m_SolverImpl.Solver.BodyAllocator.ParticleSoA.Velocity);
		}
		if (debugSettings.DrawColliderContacts || debugSettings.DrawSimplexAabb)
		{
			cmd.SetBufferData(m_ConstraintsParameters0Buffer.Buffer, m_SolverImpl.Solver.BodyAllocator.ConstraintSoA.Parameters0);
			cmd.SetBufferData(m_ConstraintsParameters1Buffer.Buffer, m_SolverImpl.Solver.BodyAllocator.ConstraintSoA.Parameters1);
		}
		if ((debugSettings.DrawNormals || debugSettings.DrawRestNormals) && m_SolverImpl.Solver.BodyAllocator.VerticesSoA.Count > 0)
		{
			UpdateGizmoVertices();
			m_MeshVerticesBuffer.Resize(m_GizmoVertices.Length);
			cmd.SetBufferData(m_MeshVerticesBuffer.Buffer, m_GizmoVertices.AsArray());
		}
		if (debugSettings.DrawDeformedVertices && m_SolverImpl.Solver.MeshDeformerAllocator.DeformableSkinnedVerticesSoA.Count > 0)
		{
			UpdateGizmoDeformedVertices();
			m_DeformableGizmosVerticesBuffer.Resize(m_GizmoDeformedVertices.Length);
			cmd.SetBufferData(m_DeformableGizmosVerticesBuffer.Buffer, m_GizmoDeformedVertices.AsArray());
		}
		if (debugSettings.DrawColliderContacts || debugSettings.DrawColliderAabb)
		{
			cmd.SetBufferData(m_ColliderAabbBuffer.Buffer, m_SolverImpl.Solver.ColliderWorld.ColliderDescriptorSoA.Aabb);
		}
		if (debugSettings.DrawColliderContacts || debugSettings.DrawContactNormals || debugSettings.DrawSimplexContacts)
		{
			CpuBroadphaseGlobal cpuBroadphaseGlobal = CpuBroadphaseGlobal;
			if (cpuBroadphaseGlobal != null)
			{
				if (m_ContactsBuffer.Buffer.count != m_SolverImpl.Solver.Config.CollisionSettings.MaxContactsCount)
				{
					m_ContactsBuffer.Resize(m_SolverImpl.Solver.Config.CollisionSettings.MaxContactsCount);
				}
				NativeArray<Contact> data = cpuBroadphaseGlobal.Contacts.AsArray();
				cmd.SetBufferData(m_ContactsBuffer.Buffer, data, 0, 0, data.Length);
				m_IndirectArgs[0] = XPBDMeshUtils.CubeMeshWithUvAndNormals.GetIndexCount(0);
				m_IndirectArgs[1] = (uint)cpuBroadphaseGlobal.Contacts.Length;
				m_IndirectArgs[2] = XPBDMeshUtils.CubeMeshWithUvAndNormals.GetIndexStart(0);
				m_IndirectArgs[3] = XPBDMeshUtils.CubeMeshWithUvAndNormals.GetBaseVertex(0);
				m_IndirectArgs[4] = 0u;
				cmd.SetBufferData(m_ColliderContactsIndirectArgsBuffer, m_IndirectArgs);
				m_IndirectArgs[0] = RenderingUtils.QuadMesh.GetIndexCount(0);
				m_IndirectArgs[2] = RenderingUtils.QuadMesh.GetIndexStart(0);
				m_IndirectArgs[3] = RenderingUtils.QuadMesh.GetBaseVertex(0);
				m_IndirectArgs[4] = 0u;
				cmd.SetBufferData(m_SimplexContactsIndirectArgsBuffer, m_IndirectArgs);
			}
		}
		if (debugSettings.DrawVisibleBodyAabbs)
		{
			cmd.SetBufferData(m_BodyAabbBuffer.Buffer, m_SolverImpl.Solver.BodyAllocator.BodyDescriptorSoA.Aabb);
			if (m_BodyVisibilityBuffer.Buffer.count != m_SolverImpl.Solver.Culler.BodyVisibility.Length)
			{
				m_BodyVisibilityBuffer.Resize(m_SolverImpl.Solver.Culler.BodyVisibility.Length);
			}
			cmd.SetBufferData(m_BodyVisibilityBuffer.Buffer, m_SolverImpl.Solver.Culler.BodyVisibility);
		}
		IsValid = true;
	}

	private void UpdateGizmoVertices()
	{
		if (!m_GizmoVertices.IsCreated || m_GizmoVertices.Length < m_SolverImpl.Solver.BodyAllocator.VerticesSoA.Count)
		{
			if (m_GizmoVertices.IsCreated)
			{
				m_GizmoVertices.Dispose();
			}
			m_GizmoVertices = new NativeList<GizmoMeshVertex>(m_SolverImpl.Solver.BodyAllocator.VerticesSoA.Count, Allocator.Persistent);
		}
		m_GizmoVertices.Clear();
		GizmosTransformVerticesToWorldJob gizmosTransformVerticesToWorldJob = default(GizmosTransformVerticesToWorldJob);
		gizmosTransformVerticesToWorldJob.BodyIndicesMap = m_SolverImpl.Solver.BodyAllocator.IndicesMap;
		gizmosTransformVerticesToWorldJob.VerticesRange = m_SolverImpl.Solver.BodyAllocator.BodyDescriptorSoA.VerticesRange;
		gizmosTransformVerticesToWorldJob.BodyLocalToWorld = m_SolverImpl.Solver.BodyAllocator.BodyDescriptorSoA.LocalToWorld;
		gizmosTransformVerticesToWorldJob.VertexPosition = m_SolverImpl.Solver.BodyAllocator.VerticesSoA.Position;
		gizmosTransformVerticesToWorldJob.VertexNormal = m_SolverImpl.Solver.BodyAllocator.VerticesSoA.Normal;
		gizmosTransformVerticesToWorldJob.VertexRestNormal = m_SolverImpl.Solver.BodyAllocator.VerticesSoA.RestNormal;
		gizmosTransformVerticesToWorldJob.GizmoVertices = m_GizmoVertices.AsParallelWriter();
		GizmosTransformVerticesToWorldJob jobData = gizmosTransformVerticesToWorldJob;
		IJobParallelForExtensions.ScheduleByRef(ref jobData, m_SolverImpl.Solver.BodyAllocator.EntityAllocationMap.Count, 1).Complete();
	}

	private void UpdateGizmoDeformedVertices()
	{
		if (!m_GizmoDeformedVertices.IsCreated || m_GizmoDeformedVertices.Length < m_SolverImpl.Solver.MeshDeformerAllocator.DeformableSkinnedVerticesSoA.Count)
		{
			if (m_GizmoDeformedVertices.IsCreated)
			{
				m_GizmoDeformedVertices.Dispose();
			}
			m_GizmoDeformedVertices = new NativeList<GizmoDeformedVertex>(m_SolverImpl.Solver.MeshDeformerAllocator.DeformableSkinnedVerticesSoA.Count, Allocator.Persistent);
		}
		m_GizmoDeformedVertices.Clear();
		GizmosTransformDeformedVerticesToWorldJob gizmosTransformDeformedVerticesToWorldJob = default(GizmosTransformDeformedVerticesToWorldJob);
		gizmosTransformDeformedVerticesToWorldJob.DeformerIndicesMap = m_SolverImpl.Solver.MeshDeformerAllocator.IndicesMap;
		gizmosTransformDeformedVerticesToWorldJob.VerticesRange = m_SolverImpl.Solver.MeshDeformerAllocator.DescriptorsSoA.SkinnedVerticesRange;
		gizmosTransformDeformedVerticesToWorldJob.DeformerLocalToWorld = m_SolverImpl.Solver.MeshDeformerAllocator.DescriptorsSoA.LocalToWorld;
		gizmosTransformDeformedVerticesToWorldJob.DeformedSkinnedVertices = m_SolverImpl.Solver.MeshDeformerAllocator.DeformableSkinnedVerticesSoA.Array;
		gizmosTransformDeformedVerticesToWorldJob.GizmoDeformedVertices = m_GizmoDeformedVertices.AsParallelWriter();
		GizmosTransformDeformedVerticesToWorldJob jobData = gizmosTransformDeformedVerticesToWorldJob;
		IJobParallelForExtensions.ScheduleByRef(ref jobData, m_SolverImpl.Solver.MeshDeformerAllocator.EntityAllocationMap.Count, 1).Complete();
	}

	public MemoryStat GetMemoryStat()
	{
		MemoryStat result = default(MemoryStat);
		result.Cpu = 0;
		foreach (GraphicsBufferWrapper buffer in m_Buffers)
		{
			result.Gpu += buffer.GetSizeInBytes();
		}
		return result;
	}
}
