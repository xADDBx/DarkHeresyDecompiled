using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.Bodies;
using Owlcat.Runtime.Visual.XPBD.Constraints;
using Owlcat.Runtime.Visual.XPBD.Particles;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Owlcat.Runtime.Visual.XPBD.Solvers.GPU;
using Owlcat.Runtime.Visual.XPBD.Stats;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.GPU.Replicators;

public class BodyReplicator : ReplicatorBase<BodyAllocator>
{
	private class DynamicData : IDisposable, IMemoryCounter
	{
		private BodyReplicator m_Replicator;

		private ComputeShader m_ReplicationShader;

		private ComputeKernel m_StoreKernel;

		private ComputeKernel m_RollKernel;

		private GraphicsBufferWrapper<float3> m_ParticleBasePosition;

		private GraphicsBufferWrapper<float3> m_ParticlePrevPosition;

		private GraphicsBufferWrapper<float3> m_ParticlePosition;

		private GraphicsBufferWrapper<float3> m_ParticleVelocity;

		private GraphicsBufferWrapper<int2> m_ParticleRanges;

		private GraphicsBufferWrapper<int> m_BodyIndicesForReplication;

		private NativeList<int> m_BodyIndices;

		private NativeList<int2> m_StoredBodyParticleRanges;

		private List<AuthoringBase> m_StoredBodies = new List<AuthoringBase>();

		private bool m_HasData;

		public DynamicData(BodyReplicator replicator, GpuSolverImpl.Shaders shaders)
		{
			m_Replicator = replicator;
			m_ReplicationShader = shaders.ReplicationShader;
			m_StoreKernel = shaders.ReplicationStoreKernel;
			m_RollKernel = shaders.ReplicationRollKernel;
			m_ParticleBasePosition = new GraphicsBufferWrapper<float3>("_XpbdRepParticleBasePositionBuffer", 128);
			m_ParticlePrevPosition = new GraphicsBufferWrapper<float3>("_XpbdRepParticlePrevPositionBuffer", 128);
			m_ParticlePosition = new GraphicsBufferWrapper<float3>("_XpbdRepParticlePositionBuffer", 128);
			m_ParticleVelocity = new GraphicsBufferWrapper<float3>("_XpbdRepParticleVelocityBuffer", 128);
			m_ParticleRanges = new GraphicsBufferWrapper<int2>("_XpbdRepParticleRangesBuffer", 128);
			m_BodyIndicesForReplication = new GraphicsBufferWrapper<int>("_XpbdBodyIndicesForReplicationBuffer", 128);
			m_BodyIndices = new NativeList<int>(128, AllocatorManager.Persistent);
			m_StoredBodyParticleRanges = new NativeList<int2>(128, AllocatorManager.Persistent);
		}

		public void Dispose()
		{
			m_ParticleBasePosition?.Dispose();
			m_ParticlePrevPosition?.Dispose();
			m_ParticlePosition?.Dispose();
			m_ParticleVelocity?.Dispose();
			m_ParticleRanges?.Dispose();
			m_BodyIndicesForReplication?.Dispose();
			if (m_BodyIndices.IsCreated)
			{
				m_BodyIndices.Dispose();
			}
			if (m_StoredBodyParticleRanges.IsCreated)
			{
				m_StoredBodyParticleRanges.Dispose();
			}
		}

		internal void StoreIndices()
		{
			if (m_BodyIndices.Length > 0)
			{
				m_BodyIndices.Clear();
			}
			if (m_StoredBodies.Count > 0)
			{
				m_StoredBodies.Clear();
			}
			if (m_StoredBodyParticleRanges.Length > 0)
			{
				m_StoredBodyParticleRanges.Clear();
			}
			IEnumerable<AuthoringBase> enumerable = m_Replicator.Allocator.EntityAllocationMap.Keys.Except(m_Replicator.Allocator.AddedEntities);
			if (m_Replicator.Allocator.EntityAllocationMap.Count <= 0)
			{
				return;
			}
			int num = 0;
			using (IEnumerator<AuthoringBase> enumerator = enumerable.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					AuthoringBase current = enumerator.Current;
					int value = current.IndexInSolver;
					m_StoredBodies.Add(current);
					m_BodyIndices.Add(in value);
					int2 @int = m_Replicator.Allocator.BodyDescriptorSoA.ParticlesRange[value];
					ref NativeList<int2> storedBodyParticleRanges = ref m_StoredBodyParticleRanges;
					int2 value2 = new int2(num, @int.y);
					storedBodyParticleRanges.Add(in value2);
					num += @int.y;
				}
			}
			if (m_ParticleBasePosition.Buffer.count < num)
			{
				m_ParticleBasePosition.Resize(num);
				m_ParticlePrevPosition.Resize(num);
				m_ParticlePosition.Resize(num);
				m_ParticleVelocity.Resize(num);
			}
			if (m_BodyIndicesForReplication.Buffer.count < m_BodyIndices.Length)
			{
				m_BodyIndicesForReplication.Resize(m_BodyIndices.Length);
			}
			if (m_ParticleRanges.Buffer.count < m_StoredBodyParticleRanges.Length)
			{
				m_ParticleRanges.Resize(m_StoredBodyParticleRanges.Length);
			}
			m_HasData = true;
		}

		public void Store(CommandBuffer cmd)
		{
			if (m_HasData)
			{
				UnityEngine.Debug.Log("Store");
				cmd.SetBufferData(m_BodyIndicesForReplication.Buffer, m_BodyIndices.AsArray(), 0, 0, m_BodyIndices.Length);
				cmd.SetBufferData(m_ParticleRanges.Buffer, m_StoredBodyParticleRanges.AsArray(), 0, 0, m_StoredBodyParticleRanges.Length);
				m_Replicator.ParticlesSoA.PushToGpu(cmd);
				m_Replicator.BodyDescriptorSoA.PushToGpu(cmd);
				SetGlobal(cmd);
				cmd.DispatchCompute(m_ReplicationShader, m_StoreKernel.Index, m_BodyIndices.Length, 1, 1);
			}
		}

		public void Roll(CommandBuffer cmd)
		{
			if (m_HasData)
			{
				UnityEngine.Debug.Log("Roll");
				for (int i = 0; i < m_StoredBodies.Count; i++)
				{
					AuthoringBase key = m_StoredBodies[i];
					_ = m_BodyIndices[i];
					m_BodyIndices[i] = m_Replicator.Allocator.EntityAllocationMap[key];
				}
				cmd.SetBufferData(m_BodyIndicesForReplication, m_BodyIndices.AsArray(), 0, 0, m_BodyIndices.Length);
				m_BodyIndicesForReplication.SetGlobal(cmd);
				cmd.DispatchCompute(m_ReplicationShader, m_RollKernel.Index, m_BodyIndices.Length, 1, 1);
				m_HasData = false;
			}
		}

		private void SetGlobal(CommandBuffer cmd)
		{
			m_ParticleBasePosition.SetGlobal(cmd);
			m_ParticlePrevPosition.SetGlobal(cmd);
			m_ParticlePosition.SetGlobal(cmd);
			m_ParticleVelocity.SetGlobal(cmd);
			m_ParticleRanges.SetGlobal(cmd);
			m_BodyIndicesForReplication.SetGlobal(cmd);
		}

		public MemoryStat GetMemoryStat()
		{
			MemoryStat result = default(MemoryStat);
			result.Cpu += m_BodyIndices.Capacity * UnsafeUtility.SizeOf<int>();
			result.Cpu += m_StoredBodyParticleRanges.Capacity * UnsafeUtility.SizeOf<int2>();
			result.Gpu += m_ParticleBasePosition.GetSizeInBytes();
			result.Gpu += m_ParticlePosition.GetSizeInBytes();
			result.Gpu += m_ParticlePrevPosition.GetSizeInBytes();
			result.Gpu += m_ParticleVelocity.GetSizeInBytes();
			result.Gpu += m_BodyIndicesForReplication.GetSizeInBytes();
			result.Gpu += m_ParticleRanges.GetSizeInBytes();
			return result;
		}
	}

	public enum BodyGroup
	{
		Simulate64 = 0x40,
		Simulate128 = 0x80,
		Simulate256 = 0x100,
		Simulate512Unlim = 0x200
	}

	private Dictionary<BodyGroup, int2> m_BodyGroupsParticleCount = new Dictionary<BodyGroup, int2>
	{
		{
			BodyGroup.Simulate64,
			new int2(0, 0)
		},
		{
			BodyGroup.Simulate128,
			new int2(1, 192)
		},
		{
			BodyGroup.Simulate256,
			new int2(193, 512)
		},
		{
			BodyGroup.Simulate512Unlim,
			new int2(513, int.MaxValue)
		}
	};

	private Dictionary<BodyGroup, List<int>> m_BodyGroupsMap = new Dictionary<BodyGroup, List<int>>
	{
		{
			BodyGroup.Simulate64,
			new List<int>()
		},
		{
			BodyGroup.Simulate128,
			new List<int>()
		},
		{
			BodyGroup.Simulate256,
			new List<int>()
		},
		{
			BodyGroup.Simulate512Unlim,
			new List<int>()
		}
	};

	private Dictionary<BodyGroup, int2> m_BodyGroupRanges = new Dictionary<BodyGroup, int2>
	{
		{
			BodyGroup.Simulate64,
			0
		},
		{
			BodyGroup.Simulate128,
			0
		},
		{
			BodyGroup.Simulate256,
			0
		},
		{
			BodyGroup.Simulate512Unlim,
			0
		}
	};

	private GraphicsBufferWrapper<int> m_BodyGroupsMapBuffer;

	private GraphicsBufferWrapper<int> m_BodyIndicesMapBuffer;

	private GraphicsBufferWrapper<int> m_VisibleBodyIndices;

	private GpuBodyDescriptorSoA m_BodyDescriptorSoA;

	private GpuParticleSoA m_ParticleSoA;

	private GpuConstraintSoA m_ConstraintSoA;

	private GpuSingleArraySoA<int3> m_ConstraintBatchSoA;

	private GpuSingleArraySoA<float4> m_ConstraintSettingsSoA;

	private int m_MeshBodyCount;

	private GpuSingleArraySoA<int> m_MeshBodyIndicesMapSoA;

	private GpuSingleArraySoA<int> m_VertexToParticleMapSoA;

	private GpuSingleArraySoA<int> m_TrianglesSoA;

	private GpuSingleArraySoA<int> m_VertexTrianglesMapSoA;

	private GpuSingleArraySoA<int2> m_VertexTrianglesMapRangesSoA;

	private GpuBodyVertexSoA m_VerticesSoA;

	private GpuMeshVertexSoA m_MeshLocalVerticesSoA;

	private GpuSingleArraySoA<int> m_ParticleToVertexMapSoA;

	private int m_SkeletonBodyCount;

	private GpuSingleArraySoA<int> m_SkeletonBodyIndicesMapSoA;

	private GpuBodyBoneSoA m_BonesSoA;

	private GpuSingleArraySoA<int> m_BoneIndicesMapSoA;

	private GpuSingleArraySoA<int2> m_BoneIndicesMapRangesSoA;

	private HashSet<AuthoringBase> m_AddedBodies = new HashSet<AuthoringBase>();

	private bool m_NeedRebuildIndices;

	private bool m_WillGrow;

	public Dictionary<BodyGroup, int2> BodyGroupRanges => m_BodyGroupRanges;

	public GraphicsBufferWrapper<int> BodyGroupsMapBuffer => m_BodyGroupsMapBuffer;

	public GraphicsBufferWrapper<int> BodyIndicesMapBuffer => m_BodyIndicesMapBuffer;

	public GraphicsBufferWrapper<int> VisibleBodyIndices => m_VisibleBodyIndices;

	public GpuBodyDescriptorSoA BodyDescriptorSoA => m_BodyDescriptorSoA;

	public GpuParticleSoA ParticlesSoA => m_ParticleSoA;

	public GpuConstraintSoA ConstraintSoA => m_ConstraintSoA;

	public GpuSingleArraySoA<int3> ConstraintBatchSoA => m_ConstraintBatchSoA;

	public GpuSingleArraySoA<float4> ConstraintSettingsSoA => m_ConstraintSettingsSoA;

	public int MeshBodyCount => m_MeshBodyCount;

	public GpuSingleArraySoA<int> MeshBodyIndicesMapSoA => m_MeshBodyIndicesMapSoA;

	public GpuBodyVertexSoA VerticesSoA => m_VerticesSoA;

	public GpuSingleArraySoA<int> VertexToParticleSoA => m_VertexToParticleMapSoA;

	public GpuSingleArraySoA<int> TrianglesSoA => m_TrianglesSoA;

	public GpuSingleArraySoA<int> VertexTrianglesMapSoA => m_VertexTrianglesMapSoA;

	public GpuSingleArraySoA<int2> VertexTrianglesMapRangesSoA => m_VertexTrianglesMapRangesSoA;

	public GpuMeshVertexSoA MeshLocalVerticesSoA => m_MeshLocalVerticesSoA;

	public GpuSingleArraySoA<int> ParticleToVertexMapSoA => m_ParticleToVertexMapSoA;

	public int SkeletonBodyCount => m_SkeletonBodyCount;

	public GpuSingleArraySoA<int> SkeletonBodyIndicesMapSoA => m_SkeletonBodyIndicesMapSoA;

	public GpuBodyBoneSoA BonesSoA => m_BonesSoA;

	public GpuSingleArraySoA<int> BoneIndicesMapSoA => m_BoneIndicesMapSoA;

	public GpuSingleArraySoA<int2> BoneIndicesMapRangesSoA => m_BoneIndicesMapRangesSoA;

	public override bool IsEmpty => base.Allocator.IsEmpty;

	internal BodyReplicator(BodyAllocator allocator, GpuSolverImpl.Shaders shaders)
		: base(allocator)
	{
		m_BodyGroupsMapBuffer = new GraphicsBufferWrapper<int>("_XpbdBodyGroupsMapBuffer", 128);
		m_BodyIndicesMapBuffer = new GraphicsBufferWrapper<int>("_XpbdBodyIndicesMapBuffer", 128);
		m_VisibleBodyIndices = new GraphicsBufferWrapper<int>("_XpbdVisibleBodyIndicesBuffer", 128);
		m_BodyDescriptorSoA = CreateSoA(base.Allocator.BodyDescriptorSoA.Capacity, (int size) => new GpuBodyDescriptorSoA(size));
		m_ParticleSoA = CreateSoA(base.Allocator.ParticleSoA.Capacity, (int size) => new GpuParticleSoA(size));
		m_ConstraintSoA = CreateSoA(base.Allocator.ConstraintSoA.Capacity, (int size) => new GpuConstraintSoA(size));
		m_ConstraintBatchSoA = CreateSingleArraySoA<int3>("_XpbdConstraintBatchBuffer", base.Allocator.ConstraintBatchSoA.Capacity);
		m_ConstraintSettingsSoA = CreateSingleArraySoA<float4>("_XpbdConstraintSettingsBuffer", base.Allocator.ConstraintSettingsSoA.Capacity);
		m_MeshBodyIndicesMapSoA = CreateSingleArraySoA<int>("_XpbdMeshBodyIndicesMapBuffer", 128);
		m_VertexToParticleMapSoA = CreateSingleArraySoA<int>("_XpbdVertexToParticleMapBuffer", base.Allocator.VertexToParticleMapSoA.Capacity);
		m_TrianglesSoA = CreateSingleArraySoA<int>("_XpbdTrianglesBuffer", base.Allocator.TrianglesSoA.Capacity);
		m_VertexTrianglesMapSoA = CreateSingleArraySoA<int>("_XpbdVertexTrianglesMapBuffer", base.Allocator.VertexTrianglesMapSoA.Capacity);
		m_VertexTrianglesMapRangesSoA = CreateSingleArraySoA<int2>("_XpbdVertexTrianglesMapRangesBuffer", base.Allocator.VertexTrianglesMapRangesSoA.Capacity);
		m_VerticesSoA = CreateSoA(base.Allocator.VerticesSoA.Capacity, (int size) => new GpuBodyVertexSoA(size));
		m_MeshLocalVerticesSoA = CreateSoA(base.Allocator.MeshLocalVerticesSoA.Capacity, (int size) => new GpuMeshVertexSoA(size));
		m_ParticleToVertexMapSoA = CreateSingleArraySoA<int>("_XpbdParticleToVertexMapBuffer", base.Allocator.ParticleToVertexMapSoA.Capacity);
		m_SkeletonBodyIndicesMapSoA = CreateSingleArraySoA<int>("_XpbdSkeletonBodyIndicesMapBuffer", 128);
		m_BonesSoA = CreateSoA(base.Allocator.BonesSoA.Capacity, (int size) => new GpuBodyBoneSoA(size));
		m_BoneIndicesMapSoA = CreateSingleArraySoA<int>("_XpbdBoneIndicesMapBuffer", base.Allocator.BoneIndicesMapSoA.Capacity);
		m_BoneIndicesMapRangesSoA = CreateSingleArraySoA<int2>("_XpbdBoneIndicesMapRangesBuffer", base.Allocator.BoneIndicesMapRangesSoA.Capacity);
		BodyAllocator allocator2 = base.Allocator;
		allocator2.BeforeGrow = (Action)Delegate.Combine(allocator2.BeforeGrow, new Action(OnBeforeGrow));
	}

	public override void Dispose()
	{
		base.Dispose();
		BodyAllocator allocator = base.Allocator;
		allocator.BeforeGrow = (Action)Delegate.Remove(allocator.BeforeGrow, new Action(OnBeforeGrow));
		m_BodyIndicesMapBuffer?.Dispose();
		m_BodyGroupsMapBuffer?.Dispose();
		m_VisibleBodyIndices?.Dispose();
	}

	public override bool Replicate(CommandBuffer cmd)
	{
		bool result = false;
		if (m_WillGrow)
		{
			FullCopyFromCPUToGPU(cmd);
			result = true;
			m_WillGrow = false;
		}
		else
		{
			CopyAddedBodiesFromCPUToGPU(cmd);
			if (m_AddedBodies.Count > 0)
			{
				result = true;
			}
		}
		if (m_NeedRebuildIndices)
		{
			UpdateIndicesMap(cmd);
			CreateMeshBodyIndicesMap(cmd);
			CreateSkeletonBodyIndicesMap(cmd);
			m_NeedRebuildIndices = false;
			result = true;
		}
		m_AddedBodies.Clear();
		return result;
	}

	private void CopyAddedBodiesFromCPUToGPU(CommandBuffer cmd)
	{
		if (m_AddedBodies.Count <= 0)
		{
			return;
		}
		foreach (AuthoringBase addedBody in m_AddedBodies)
		{
			int num = base.Allocator.EntityAllocationMap[addedBody];
			BodyDescriptor bodyDescriptor = base.Allocator.BodyDescriptorSoA[num];
			m_BodyDescriptorSoA.SetData(cmd, base.Allocator.BodyDescriptorSoA, num, 1);
			m_ParticleSoA.SetData(cmd, base.Allocator.ParticleSoA, bodyDescriptor.ParticlesRange.x, bodyDescriptor.ParticlesRange.y);
			m_ConstraintSoA.SetData(cmd, base.Allocator.ConstraintSoA, bodyDescriptor.ConstraintsRange.x, bodyDescriptor.ConstraintsRange.y);
			m_ConstraintBatchSoA.SetData(cmd, base.Allocator.ConstraintBatchSoA, bodyDescriptor.ConstraintBatchesRange.x, bodyDescriptor.ConstraintBatchesRange.y);
			m_ConstraintSettingsSoA.SetData(cmd, base.Allocator.ConstraintSettingsSoA, bodyDescriptor.ConstraintSettingsRange.x, bodyDescriptor.ConstraintSettingsRange.y);
			if (bodyDescriptor.HasVertices)
			{
				m_VertexToParticleMapSoA.SetData(cmd, base.Allocator.VertexToParticleMapSoA, bodyDescriptor.VertexToParticleRange.x, bodyDescriptor.VertexToParticleRange.y);
				m_TrianglesSoA.SetData(cmd, base.Allocator.TrianglesSoA, bodyDescriptor.TrianglesRange.x, bodyDescriptor.TrianglesRange.y);
				m_VertexTrianglesMapSoA.SetData(cmd, base.Allocator.VertexTrianglesMapSoA, bodyDescriptor.VertexTrianglesMapRange.x, bodyDescriptor.VertexTrianglesMapRange.y);
				m_VertexTrianglesMapRangesSoA.SetData(cmd, base.Allocator.VertexTrianglesMapRangesSoA, bodyDescriptor.VertexTrianglesMapRangesRange.x, bodyDescriptor.VertexTrianglesMapRangesRange.y);
				m_VerticesSoA.SetData(cmd, base.Allocator.VerticesSoA, bodyDescriptor.VerticesRange.x, bodyDescriptor.VerticesRange.y);
				if (bodyDescriptor.HasSkinnedVertices)
				{
					m_ParticleToVertexMapSoA.SetData(cmd, base.Allocator.ParticleToVertexMapSoA, bodyDescriptor.ParticleToVertexRange.x, bodyDescriptor.ParticleToVertexRange.y);
				}
				else
				{
					m_MeshLocalVerticesSoA.SetData(cmd, base.Allocator.MeshLocalVerticesSoA, bodyDescriptor.MeshLocalVerticesRange.x, bodyDescriptor.MeshLocalVerticesRange.y);
				}
			}
			if (bodyDescriptor.HasBones)
			{
				m_BonesSoA.SetData(cmd, base.Allocator.BonesSoA, bodyDescriptor.BonesRange.x, bodyDescriptor.BonesRange.y);
				m_BoneIndicesMapSoA.SetData(cmd, base.Allocator.BoneIndicesMapSoA, bodyDescriptor.BoneIndicesMapRange.x, bodyDescriptor.BoneIndicesMapRange.y);
				m_BoneIndicesMapRangesSoA.SetData(cmd, base.Allocator.BoneIndicesMapRangesSoA, bodyDescriptor.BoneIndicesMapRangesRange.x, bodyDescriptor.BoneIndicesMapRangesRange.y);
			}
		}
	}

	private void FullCopyFromCPUToGPU(CommandBuffer cmd)
	{
		cmd.SetBufferData(m_BodyIndicesMapBuffer.Buffer, base.Allocator.IndicesMap);
		m_BodyDescriptorSoA.SetData(cmd, base.Allocator.BodyDescriptorSoA);
		m_ParticleSoA.SetData(cmd, base.Allocator.ParticleSoA);
		m_ConstraintSoA.SetData(cmd, base.Allocator.ConstraintSoA);
		m_ConstraintBatchSoA.SetData(cmd, base.Allocator.ConstraintBatchSoA);
		m_ConstraintSettingsSoA.SetData(cmd, base.Allocator.ConstraintSettingsSoA);
		m_VertexToParticleMapSoA.SetData(cmd, base.Allocator.VertexToParticleMapSoA);
		m_TrianglesSoA.SetData(cmd, base.Allocator.TrianglesSoA);
		m_VertexTrianglesMapSoA.SetData(cmd, base.Allocator.VertexTrianglesMapSoA);
		m_VertexTrianglesMapRangesSoA.SetData(cmd, base.Allocator.VertexTrianglesMapRangesSoA);
		m_VerticesSoA.SetData(cmd, base.Allocator.VerticesSoA);
		m_MeshLocalVerticesSoA.SetData(cmd, base.Allocator.MeshLocalVerticesSoA);
		m_ParticleToVertexMapSoA.SetData(cmd, base.Allocator.ParticleToVertexMapSoA);
		m_BonesSoA.SetData(cmd, base.Allocator.BonesSoA);
		m_BoneIndicesMapSoA.SetData(cmd, base.Allocator.BoneIndicesMapSoA);
		m_BoneIndicesMapRangesSoA.SetData(cmd, base.Allocator.BoneIndicesMapRangesSoA);
	}

	private void CreateMeshBodyIndicesMap(CommandBuffer cmd)
	{
		NativeList<int> nativeList = new NativeList<int>(AllocatorManager.TempJob);
		foreach (KeyValuePair<AuthoringBase, int> item in base.Allocator.EntityAllocationMap)
		{
			if (base.Allocator.BodyDescriptorSoA[item.Value].HasVertices)
			{
				int value = item.Value;
				nativeList.Add(in value);
			}
		}
		m_MeshBodyCount = nativeList.Length;
		if (nativeList.Length > 0)
		{
			if (m_MeshBodyIndicesMapSoA.Capacity < nativeList.Length)
			{
				m_MeshBodyIndicesMapSoA.Resize(nativeList.Length);
			}
			cmd.SetBufferData(m_MeshBodyIndicesMapSoA.Buffer, nativeList.AsArray());
		}
		nativeList.Dispose();
	}

	private void CreateSkeletonBodyIndicesMap(CommandBuffer cmd)
	{
		NativeList<int> nativeList = new NativeList<int>(AllocatorManager.TempJob);
		foreach (KeyValuePair<AuthoringBase, int> item in base.Allocator.EntityAllocationMap)
		{
			if (base.Allocator.BodyDescriptorSoA[item.Value].HasBones)
			{
				int value = item.Value;
				nativeList.Add(in value);
			}
		}
		m_SkeletonBodyCount = nativeList.Length;
		if (nativeList.Length > 0)
		{
			if (m_SkeletonBodyIndicesMapSoA.Capacity < nativeList.Length)
			{
				m_SkeletonBodyIndicesMapSoA.Resize(nativeList.Length);
			}
			cmd.SetBufferData(m_SkeletonBodyIndicesMapSoA.Buffer, nativeList.AsArray());
		}
		nativeList.Dispose();
	}

	private void UpdateIndicesMap(CommandBuffer cmd)
	{
		if (m_BodyIndicesMapBuffer.Buffer.count < base.Allocator.IndicesMap.Length)
		{
			m_BodyIndicesMapBuffer.Resize(base.Allocator.IndicesMap.Length);
		}
		cmd.SetBufferData(m_BodyIndicesMapBuffer.Buffer, base.Allocator.IndicesMap);
	}

	public void UpdateBodyVisibility(CommandBuffer cmd, NativeList<int> visibleBodyIndices)
	{
		if (m_VisibleBodyIndices.Buffer.count < visibleBodyIndices.Length)
		{
			m_VisibleBodyIndices.Resize(visibleBodyIndices.Length);
		}
		if (visibleBodyIndices.Length > 0)
		{
			cmd.SetBufferData(m_VisibleBodyIndices, visibleBodyIndices.AsArray());
		}
		foreach (KeyValuePair<BodyGroup, List<int>> item in m_BodyGroupsMap)
		{
			item.Value.Clear();
		}
		for (int i = 0; i < visibleBodyIndices.Length; i++)
		{
			int num = visibleBodyIndices[i];
			int2 @int = base.Allocator.BodyDescriptorSoA.ParticlesRange[num];
			foreach (KeyValuePair<BodyGroup, int2> item2 in m_BodyGroupsParticleCount)
			{
				if (@int.y >= item2.Value.x && @int.y <= item2.Value.y)
				{
					m_BodyGroupsMap[item2.Key].Add(num);
					break;
				}
			}
		}
		if (m_BodyGroupsMapBuffer.Buffer.count < base.Allocator.IndicesMap.Length)
		{
			m_BodyGroupsMapBuffer.Resize(base.Allocator.IndicesMap.Length);
		}
		int num2 = 0;
		foreach (KeyValuePair<BodyGroup, List<int>> item3 in m_BodyGroupsMap)
		{
			m_BodyGroupRanges[item3.Key] = new int2(num2, item3.Value.Count);
			if (item3.Value.Count > 0)
			{
				cmd.SetBufferData(m_BodyGroupsMapBuffer.Buffer, item3.Value, 0, num2, item3.Value.Count);
			}
			num2 += item3.Value.Count;
		}
	}

	private void OnBeforeGrow()
	{
		m_WillGrow = true;
	}

	protected override void OnAfterAlloc()
	{
		if (m_WillGrow)
		{
			ResizeAll();
		}
		m_AddedBodies.Clear();
		foreach (AuthoringBase addedEntity in base.Allocator.AddedEntities)
		{
			m_AddedBodies.Add(addedEntity);
		}
		m_NeedRebuildIndices = base.Allocator.AddedEntities.Count > 0 || base.Allocator.RemovedEntities.Count > 0;
	}

	private void ResizeAll()
	{
		m_BodyGroupsMapBuffer.Resize(base.Allocator.IndicesMap.Length);
		m_BodyIndicesMapBuffer.Resize(base.Allocator.IndicesMap.Length);
		m_BodyDescriptorSoA.Resize(base.Allocator.BodyDescriptorSoA.Capacity);
		m_ParticleSoA.Resize(base.Allocator.ParticleSoA.Capacity);
		m_ConstraintSoA.Resize(base.Allocator.ConstraintSoA.Capacity);
		m_ConstraintBatchSoA.Resize(base.Allocator.ConstraintBatchSoA.Capacity);
		m_ConstraintSettingsSoA.Resize(base.Allocator.ConstraintSettingsSoA.Capacity);
		m_VertexToParticleMapSoA.Resize(base.Allocator.VertexToParticleMapSoA.Capacity);
		m_TrianglesSoA.Resize(base.Allocator.TrianglesSoA.Capacity);
		m_VertexTrianglesMapSoA.Resize(base.Allocator.VertexTrianglesMapSoA.Capacity);
		m_VertexTrianglesMapRangesSoA.Resize(base.Allocator.VertexTrianglesMapRangesSoA.Capacity);
		m_VerticesSoA.Resize(base.Allocator.VerticesSoA.Capacity);
		m_MeshLocalVerticesSoA.Resize(base.Allocator.MeshLocalVerticesSoA.Capacity);
		m_ParticleToVertexMapSoA.Resize(base.Allocator.ParticleToVertexMapSoA.Capacity);
		m_BonesSoA.Resize(base.Allocator.BonesSoA.Capacity);
		m_BoneIndicesMapSoA.Resize(base.Allocator.BoneIndicesMapSoA.Capacity);
		m_BoneIndicesMapRangesSoA.Resize(base.Allocator.BoneIndicesMapRangesSoA.Capacity);
	}

	public override MemoryStat GetMemoryStat()
	{
		MemoryStat memoryStat = base.GetMemoryStat();
		memoryStat.Gpu += m_BodyGroupsMapBuffer.GetSizeInBytes();
		memoryStat.Gpu += m_BodyIndicesMapBuffer.GetSizeInBytes();
		memoryStat.Gpu += m_VisibleBodyIndices.GetSizeInBytes();
		return memoryStat;
	}
}
