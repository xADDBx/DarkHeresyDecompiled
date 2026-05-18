using System;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public sealed class DeferredReflectionProbeBatcher : IDisposable
{
	private struct BatchKey : IEquatable<BatchKey>
	{
		public bool BoxProjection;

		public bool Instanced;

		public readonly bool Equals(BatchKey other)
		{
			if (BoxProjection == other.BoxProjection)
			{
				return Instanced == other.Instanced;
			}
			return false;
		}

		[BurstDiscard]
		public override bool Equals(object obj)
		{
			if (obj is BatchKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (BoxProjection.GetHashCode() * 397) ^ Instanced.GetHashCode();
		}
	}

	private struct BatchInfo
	{
		public BatchKey Key;

		public int InstanceCount;

		public int VisibleProbeOffset;

		public int MatrixOffset;

		public int InstanceDataOffset;
	}

	private static class Keywords
	{
		public const string USE_BOX_PROJECTION = "USE_BOX_PROJECTION";
	}

	private static class ShaderIDs
	{
		public static readonly int _ProbeData = Shader.PropertyToID("_ProbeData");
	}

	private const int kMaxInstanceCountPerBatch = 1023;

	private readonly DeferredReflectionsBatchingMode m_BatchingMode;

	private readonly Vector4[] m_InstanceProbeDataArray = new Vector4[1023];

	private readonly Matrix4x4[] m_MatricesArray = new Matrix4x4[1023];

	private readonly MaterialPropertyBlock m_PropertyBlock = new MaterialPropertyBlock();

	private readonly WaaaghReflectionProbes m_ReflectionProbes;

	private NativeList<BatchInfo> m_Batches = new NativeList<BatchInfo>(Allocator.Persistent);

	private NativeList<float4> m_InstanceProbeData = new NativeList<float4>(Allocator.Persistent);

	private NativeList<Matrix4x4> m_Matrices = new NativeList<Matrix4x4>(Allocator.Persistent);

	public DeferredReflectionProbeBatcher(WaaaghReflectionProbes reflectionProbes, WaaaghRendererData settings)
	{
		m_ReflectionProbes = reflectionProbes;
		m_BatchingMode = settings.DeferredReflectionsBatchingMode;
	}

	public void Dispose()
	{
		m_Batches.Dispose();
		m_Matrices.Dispose();
		m_InstanceProbeData.Dispose();
	}

	public void Batch(NativeArray<VisibleReflectionProbe> visibleReflectionProbes)
	{
		for (int i = 0; i < visibleReflectionProbes.Length; i++)
		{
			VisibleReflectionProbe visibleProbe = visibleReflectionProbes[i];
			ReflectionProbe reflectionProbe = visibleProbe.reflectionProbe;
			if (!(reflectionProbe == null))
			{
				int dataIndex;
				bool flag = m_ReflectionProbes.TryMapVisibleProbeToGpuDataIndex(i, out dataIndex) && m_BatchingMode != DeferredReflectionsBatchingMode.Off;
				BatchKey batchKey = default(BatchKey);
				batchKey.Instanced = flag;
				batchKey.BoxProjection = visibleProbe.isBoxProjection;
				BatchKey batchKey2 = batchKey;
				Matrix4x4 value = CreateMatrix(in visibleProbe, reflectionProbe);
				if (flag && m_Batches.Length > 0 && CanBeBatched(in UnsafeCollectionExtensions.ElementAsRef(in m_Batches, m_Batches.Length - 1), batchKey2))
				{
					UnsafeCollectionExtensions.ElementAsRef(in m_Batches, m_Batches.Length - 1).InstanceCount++;
				}
				else
				{
					BatchInfo batchInfo = default(BatchInfo);
					batchInfo.Key = batchKey2;
					batchInfo.InstanceCount = 1;
					batchInfo.MatrixOffset = m_Matrices.Length;
					batchInfo.VisibleProbeOffset = i;
					batchInfo.InstanceDataOffset = m_InstanceProbeData.Length;
					BatchInfo value2 = batchInfo;
					m_Batches.Add(in value2);
				}
				ref NativeList<float4> instanceProbeData = ref m_InstanceProbeData;
				float4 value3 = math.float4(dataIndex, reflectionProbe.blendDistance, 0f, 0f);
				instanceProbeData.Add(in value3);
				m_Matrices.Add(in value);
			}
		}
	}

	public unsafe void Render(NativeArray<VisibleReflectionProbe> visibleReflectionProbes, CommandBuffer cmd, Material material, int passIndex)
	{
		Mesh cubeMesh = RenderingUtils.CubeMesh;
		for (int i = 0; i < m_Batches.Length; i++)
		{
			ref BatchInfo reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_Batches, i);
			if (reference.Key.Instanced && (reference.InstanceCount > 1 || m_BatchingMode == DeferredReflectionsBatchingMode.Forced))
			{
				fixed (Matrix4x4* destination = m_MatricesArray)
				{
					int num = reference.InstanceCount * UnsafeUtility.SizeOf<Matrix4x4>();
					UnsafeUtility.MemCpy(destination, m_Matrices.GetUnsafePtr() + reference.MatrixOffset, num);
				}
				fixed (Vector4* destination2 = m_InstanceProbeDataArray)
				{
					int num2 = reference.InstanceCount * UnsafeUtility.SizeOf<Vector4>();
					UnsafeUtility.MemCpy(destination2, m_InstanceProbeData.GetUnsafePtr() + reference.InstanceDataOffset, num2);
				}
				m_PropertyBlock.Clear();
				m_PropertyBlock.SetVectorArray(ShaderIDs._ProbeData, m_InstanceProbeDataArray);
				SetupKeywords(cmd, in reference);
				cmd.DrawMeshInstanced(cubeMesh, 0, material, passIndex, m_MatricesArray, reference.InstanceCount, m_PropertyBlock);
				continue;
			}
			ref VisibleReflectionProbe reference2 = ref UnsafeCollectionExtensions.ElementAsRefReadonly(in visibleReflectionProbes, reference.VisibleProbeOffset);
			VisibleReflectionProbe visibleReflectionProbe = reference2;
			if (!(visibleReflectionProbe.reflectionProbe == null))
			{
				visibleReflectionProbe = reference2;
				Vector4 value = visibleReflectionProbe.reflectionProbe.transform.position;
				visibleReflectionProbe = reference2;
				value.w = visibleReflectionProbe.blendDistance;
				int custom_SpecCube = ShaderPropertyId.custom_SpecCube0;
				visibleReflectionProbe = reference2;
				cmd.SetGlobalTexture(custom_SpecCube, visibleReflectionProbe.texture);
				int custom_SpecCube0_HDR = ShaderPropertyId.custom_SpecCube0_HDR;
				visibleReflectionProbe = reference2;
				cmd.SetGlobalVector(custom_SpecCube0_HDR, visibleReflectionProbe.hdrData);
				cmd.SetGlobalVector(ShaderPropertyId.custom_SpecCube0_ProbePosition, value);
				int custom_SpecCube0_BoxMin = ShaderPropertyId.custom_SpecCube0_BoxMin;
				visibleReflectionProbe = reference2;
				cmd.SetGlobalVector(custom_SpecCube0_BoxMin, visibleReflectionProbe.bounds.min);
				int custom_SpecCube0_BoxMax = ShaderPropertyId.custom_SpecCube0_BoxMax;
				visibleReflectionProbe = reference2;
				cmd.SetGlobalVector(custom_SpecCube0_BoxMax, visibleReflectionProbe.bounds.max);
				Matrix4x4 matrix = m_Matrices[reference.MatrixOffset];
				SetupKeywords(cmd, in reference);
				cmd.DrawMesh(cubeMesh, matrix, material, 0, passIndex);
			}
		}
		static void SetupKeywords(CommandBuffer cmd, in BatchInfo batchInfo)
		{
			CoreUtils.SetKeyword(cmd, "USE_BOX_PROJECTION", batchInfo.Key.BoxProjection);
		}
	}

	[MustUseReturnValue]
	private static bool CanBeBatched(in BatchInfo existingBatchInfo, BatchKey newBatchKey)
	{
		if (newBatchKey.Instanced && existingBatchInfo.InstanceCount < 1023)
		{
			return existingBatchInfo.Key.Equals(newBatchKey);
		}
		return false;
	}

	[MustUseReturnValue]
	private static Matrix4x4 CreateMatrix(in VisibleReflectionProbe visibleProbe, ReflectionProbe probe)
	{
		return Matrix4x4.TRS(visibleProbe.center + probe.transform.position, Quaternion.identity, visibleProbe.bounds.size);
	}

	public void Clear()
	{
		m_Batches.Clear();
		m_Matrices.Clear();
		m_InstanceProbeData.Clear();
	}
}
