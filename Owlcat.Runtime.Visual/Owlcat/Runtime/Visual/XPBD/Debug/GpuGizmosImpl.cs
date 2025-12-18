using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.GPU;
using Owlcat.Runtime.Visual.XPBD.Constraints;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Owlcat.Runtime.Visual.XPBD.Solvers.GPU;
using Owlcat.Runtime.Visual.XPBD.Stats;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Debug;

public class GpuGizmosImpl : IGizmosImpl
{
	private class Shaders : ShaderResources
	{
		public ComputeShader XPBDDebug;

		public ComputeKernel TransformMeshVerticesToWorld;

		public ComputeKernel TransformDeformedVerticesToWorld;

		public Shaders()
		{
			XPBDDebug = LoadComputeShader("XPBDDebug");
			TransformMeshVerticesToWorld = new ComputeKernel(XPBDDebug, "TransformMeshVerticesToWorld");
			TransformDeformedVerticesToWorld = new ComputeKernel(XPBDDebug, "TransformDeformedVerticesToWorld");
		}
	}

	private uint[] m_IndirectArgs = new uint[5] { 0u, 1u, 0u, 0u, 0u };

	private GpuSolverImpl m_SolverImpl;

	private GpuBroadphaseGlobal m_Broadphase;

	private List<GraphicsBufferWrapper> m_Buffers;

	private Shaders m_Shaders;

	private GraphicsBufferWrapper<int> m_BodyVisibilityBuffer;

	private GraphicsBufferWrapper<int> m_ParticlesMapBuffer;

	private GraphicsBufferWrapper<int2> m_DistanceConstraintsMapBuffer;

	private GraphicsBufferWrapper<int2> m_BendConstraintsMapBuffer;

	private GraphicsBufferWrapper<int2> m_AngularConstraintsMapBuffer;

	private GraphicsBufferWrapper<GizmoMeshVertex> m_MeshVerticesBuffer;

	private GraphicsBufferWrapper<GizmoDeformedVertex> m_DeformableGizmosVerticesBuffer;

	private GraphicsBufferWrapper<int> m_SimplexMapBuffer;

	private GraphicsBufferWrapper<uint> m_ColliderContactsIndirectArgsBuffer;

	private GraphicsBufferWrapper<uint> m_SimplexContactsIndirectArgsBuffer;

	public bool IsValid { get; private set; }

	public GraphicsBufferWrapper BodiesMapBuffer => m_SolverImpl.BodyReplicator.BodyIndicesMapBuffer;

	public GraphicsBufferWrapper BodyVisibilityBuffer => m_BodyVisibilityBuffer;

	public GraphicsBufferWrapper BodyAabbBuffer => m_SolverImpl.BodyReplicator.BodyDescriptorSoA.Aabb;

	public GraphicsBufferWrapper ParticlesMapBuffer => m_ParticlesMapBuffer;

	public GraphicsBufferWrapper ParticlePositionBuffer => m_SolverImpl.BodyReplicator.ParticlesSoA.Position;

	public GraphicsBufferWrapper ParticleVelocityBuffer => m_SolverImpl.BodyReplicator.ParticlesSoA.Velocity;

	public GraphicsBufferWrapper ParticleRadiusBuffer => m_SolverImpl.BodyReplicator.ParticlesSoA.Radius;

	public GraphicsBufferWrapper DistanceConstraintsMapBuffer => m_DistanceConstraintsMapBuffer;

	public GraphicsBufferWrapper ConstraintsIndicesBuffer => m_SolverImpl.BodyReplicator.ConstraintSoA.Indices;

	public GraphicsBufferWrapper BendConstraintsMapBuffer => m_BendConstraintsMapBuffer;

	public GraphicsBufferWrapper AngularConstraintsMapBuffer => m_AngularConstraintsMapBuffer;

	public GraphicsBufferWrapper ConstraintsParameters0Buffer => m_SolverImpl.BodyReplicator.ConstraintSoA.Parameters0;

	public GraphicsBufferWrapper ConstraintsParameters1Buffer => m_SolverImpl.BodyReplicator.ConstraintSoA.Parameters1;

	public GraphicsBufferWrapper MeshVerticesBuffer => m_MeshVerticesBuffer;

	public GraphicsBufferWrapper DeformableGizmosVerticesBuffer => m_DeformableGizmosVerticesBuffer;

	public GraphicsBufferWrapper CollidersMapBuffer => m_SolverImpl.ColliderReplicator.ColliderIndicesMapBuffer;

	public GraphicsBufferWrapper ColliderAabbBuffer => m_SolverImpl.ColliderReplicator.ColliderDescriptorSoA.Aabb;

	public GraphicsBufferWrapper SimplexMapBuffer => m_SimplexMapBuffer;

	public GraphicsBufferWrapper ContactsBuffer
	{
		get
		{
			if (m_Broadphase == null)
			{
				m_Broadphase = m_SolverImpl.Solver.BroadphaseImpl as GpuBroadphaseGlobal;
			}
			return m_Broadphase.ContactsBuffer;
		}
	}

	public GraphicsBufferWrapper ColliderContactsIndirectArgsBuffer => m_ColliderContactsIndirectArgsBuffer;

	public GraphicsBufferWrapper SimplexContactsIndirectArgsBuffer => m_SimplexContactsIndirectArgsBuffer;

	public GpuGizmosImpl(GpuSolverImpl solverImpl)
	{
		m_SolverImpl = solverImpl;
		m_Broadphase = m_SolverImpl.Solver.BroadphaseImpl as GpuBroadphaseGlobal;
		m_Shaders = new Shaders();
		m_BodyVisibilityBuffer = new GraphicsBufferWrapper<int>("_XpbdBodyVisibility", 16);
		m_ParticlesMapBuffer = new GraphicsBufferWrapper<int>("_XpbdParticlesMap", 128);
		m_DistanceConstraintsMapBuffer = new GraphicsBufferWrapper<int2>("_XpbdConstraintsMap", 128);
		m_BendConstraintsMapBuffer = new GraphicsBufferWrapper<int2>("_XpbdConstraintsMap", 128);
		m_AngularConstraintsMapBuffer = new GraphicsBufferWrapper<int2>("_XpbdConstraintsMap", 128);
		m_MeshVerticesBuffer = new GraphicsBufferWrapper<GizmoMeshVertex>("_XpbdGizmoMeshVerticesBuffer", 128, GraphicsBuffer.Target.Append);
		m_DeformableGizmosVerticesBuffer = new GraphicsBufferWrapper<GizmoDeformedVertex>("_XpbdGizmoDeformedVerticesBuffer", 128, GraphicsBuffer.Target.Append);
		m_SimplexMapBuffer = new GraphicsBufferWrapper<int>("_XpbdSimplexMap", 128);
		m_ColliderContactsIndirectArgsBuffer = new GraphicsBufferWrapper<uint>("_XpbdContactsIndirectArgsBuffer", 5, GraphicsBuffer.Target.IndirectArguments);
		m_SimplexContactsIndirectArgsBuffer = new GraphicsBufferWrapper<uint>("_XpbdSimplexContactsIndirectArgsBuffer", 5, GraphicsBuffer.Target.IndirectArguments);
		m_Buffers = new List<GraphicsBufferWrapper> { m_BodyVisibilityBuffer, m_ParticlesMapBuffer, m_DistanceConstraintsMapBuffer, m_BendConstraintsMapBuffer, m_AngularConstraintsMapBuffer, m_MeshVerticesBuffer, m_DeformableGizmosVerticesBuffer, m_SimplexMapBuffer, m_ColliderContactsIndirectArgsBuffer, m_SimplexContactsIndirectArgsBuffer };
		BodyAllocator bodyAllocator = m_SolverImpl.Solver.BodyAllocator;
		bodyAllocator.AfterAlloc = (Action)Delegate.Combine(bodyAllocator.AfterAlloc, new Action(OnAfterBodyAlloc));
	}

	public void Dispose()
	{
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

	private void InitBodyMaps()
	{
		InitBodyDescriptorMaps();
		InitParticlesMap();
		InitConstraintsMap(m_DistanceConstraintsMapBuffer, ConstraintType.Distance);
		InitConstraintsMap(m_BendConstraintsMapBuffer, ConstraintType.Bend);
		InitConstraintsMap(m_AngularConstraintsMapBuffer, ConstraintType.Angular);
		InitSimplicesMap();
	}

	private void InitBodyDescriptorMaps()
	{
		m_BodyVisibilityBuffer.Resize(m_SolverImpl.Solver.Culler.BodyVisibility.Length);
	}

	private void InitParticlesMap()
	{
		NativeArray<int> data = new NativeArray<int>(m_SolverImpl.Solver.BodyAllocator.ParticleSoA.Count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		int num = 0;
		foreach (KeyValuePair<AuthoringBase, int> item in m_SolverImpl.Solver.BodyAllocator.EntityAllocationMap)
		{
			int2 @int = m_SolverImpl.Solver.BodyAllocator.BodyDescriptorSoA.ParticlesRange[item.Value];
			for (int i = 0; i < @int.y; i++)
			{
				data[num++] = @int.x + i;
			}
		}
		m_ParticlesMapBuffer.Resize(data.Length);
		m_ParticlesMapBuffer.SetData(data);
		data.Dispose();
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

	public void PushDataToGpu(CommandBuffer cmd)
	{
		XPBDDebug debugSettings = m_SolverImpl.Solver.Config.DebugSettings;
		if (!debugSettings.GizmosEnabled)
		{
			return;
		}
		cmd.BeginSample("XPBD Gizmos");
		if (debugSettings.DrawVisibleBodyAabbs)
		{
			if (m_BodyVisibilityBuffer.Buffer.count != m_SolverImpl.Solver.Culler.BodyVisibility.Length)
			{
				m_BodyVisibilityBuffer.Resize(m_SolverImpl.Solver.Culler.BodyVisibility.Length);
			}
			cmd.SetBufferData(m_BodyVisibilityBuffer.Buffer, m_SolverImpl.Solver.Culler.BodyVisibility);
		}
		if ((debugSettings.DrawColliderContacts || debugSettings.DrawContactNormals || debugSettings.DrawSimplexContacts) && m_Broadphase != null)
		{
			m_IndirectArgs[0] = XPBDMeshUtils.CubeMeshWithUvAndNormals.GetIndexCount(0);
			m_IndirectArgs[1] = (uint)m_Broadphase.GetContactsCount();
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
		if ((debugSettings.DrawNormals || debugSettings.DrawRestNormals) && m_SolverImpl.Solver.BodyAllocator.VerticesSoA.Count > 0)
		{
			UpdateGizmoVertices(cmd);
		}
		if (debugSettings.DrawDeformedVertices && m_SolverImpl.Solver.MeshDeformerAllocator.DeformableSkinnedVerticesSoA.Count > 0)
		{
			UpdateGizmoDeformedVertices(cmd);
		}
		cmd.EndSample("XPBD Gizmos");
		IsValid = true;
	}

	private void UpdateGizmoVertices(CommandBuffer cmd)
	{
		m_MeshVerticesBuffer.Resize(m_SolverImpl.Solver.BodyAllocator.VerticesSoA.Count);
		cmd.SetBufferCounterValue(m_MeshVerticesBuffer.Buffer, 0u);
		m_SolverImpl.BodyReplicator.BodyIndicesMapBuffer.SetGlobal(cmd);
		m_SolverImpl.BodyReplicator.BodyDescriptorSoA.PushToGpu(cmd);
		m_SolverImpl.BodyReplicator.VerticesSoA.PushToGpu(cmd);
		m_MeshVerticesBuffer.SetGlobal(cmd);
		m_Shaders.TransformMeshVerticesToWorld.Dispatch(cmd, m_SolverImpl.Solver.BodyAllocator.EntityAllocationMap.Count, 1, 1);
	}

	private void UpdateGizmoDeformedVertices(CommandBuffer cmd)
	{
		m_DeformableGizmosVerticesBuffer.Resize(m_SolverImpl.Solver.MeshDeformerAllocator.DeformableSkinnedVerticesSoA.Count);
		cmd.SetBufferCounterValue(m_DeformableGizmosVerticesBuffer.Buffer, 0u);
		cmd.SetBufferData(m_SolverImpl.MeshDeformerReplicator.GpuMeshDeformerDesctiptorSoA.LocalToWorld, m_SolverImpl.Solver.MeshDeformerAllocator.DescriptorsSoA.LocalToWorld);
		m_SolverImpl.MeshDeformerReplicator.MeshDeformerIndicesBuffer.SetGlobal(cmd);
		m_SolverImpl.MeshDeformerReplicator.GpuMeshDeformerDesctiptorSoA.PushToGpu(cmd);
		m_SolverImpl.MeshDeformerReplicator.MeshDeformerDeformableSkinnedVerticesSoA.PushToGpu(cmd);
		m_DeformableGizmosVerticesBuffer.SetGlobal(cmd);
		m_Shaders.TransformDeformedVerticesToWorld.Dispatch(cmd, m_SolverImpl.Solver.MeshDeformerAllocator.EntityAllocationMap.Count, 1, 1);
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
