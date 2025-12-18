using System.Collections.Generic;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.Culling;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.GPU.Replicators;
using Owlcat.Runtime.Visual.XPBD.Shaders;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Owlcat.Runtime.Visual.XPBD.Stats;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.GPU;

public class SpatialHashGrid : IMemoryCounter
{
	public enum Metrics
	{
		ObjectSizeSum,
		OccupiedCellsCount,
		ActiveContactsCount,
		MetricsCount
	}

	private class Shaders : ShaderResources
	{
		public ComputeShader SpatialHashGridShader;

		public ComputeKernel KernelClear;

		public ComputeKernel KernelCalculateSpacing;

		public ComputeKernel KernelCalculateParticlesSpacing;

		public ComputeKernel KernelBuildGrid;

		public ComputeKernel KernelBuildParticlesGrid;

		public ComputeKernel KernelCalculateLoadFactor;

		public Shaders()
		{
			SpatialHashGridShader = LoadComputeShader("SpatialHashGrid");
			KernelClear = new ComputeKernel(SpatialHashGridShader, "Clear");
			KernelCalculateSpacing = new ComputeKernel(SpatialHashGridShader, "CalculateSpacing");
			KernelCalculateParticlesSpacing = new ComputeKernel(SpatialHashGridShader, "CalculateParticlesSpacing");
			KernelBuildGrid = new ComputeKernel(SpatialHashGridShader, "BuildGrid");
			KernelBuildParticlesGrid = new ComputeKernel(SpatialHashGridShader, "BuildParticlesGrid");
			KernelCalculateLoadFactor = new ComputeKernel(SpatialHashGridShader, "CalculateLoadFactor");
		}
	}

	public const float kSizeDescretizer = 100f;

	public const float kSpacingScale = 2f;

	public const int kMaxHashtableLookupIterations = 100;

	private Shaders m_Shaders;

	private GraphicsBufferWrapper<int> m_HashmapKeys;

	private GraphicsBufferWrapper<int> m_HashmapValues;

	private GraphicsBufferWrapper<int> m_MetricsBuffer;

	private int m_HashmapSize;

	private int m_ObjectsCount;

	private NativeArray<int> m_MetricsClear;

	private NativeArray<int> m_MetricsCopy;

	private bool m_IsReadbackFinished;

	public int HashmapSize => m_HashmapSize;

	public int ObjectsCount => m_ObjectsCount;

	public GraphicsBufferWrapper<int> HashmapKeys => m_HashmapKeys;

	public GraphicsBufferWrapper<int> HashmapValues => m_HashmapValues;

	public GraphicsBufferWrapper<int> MetricsBuffer => m_MetricsBuffer;

	public SpatialHashGrid()
	{
		m_Shaders = new Shaders();
		m_MetricsBuffer = new GraphicsBufferWrapper<int>("_XpbdSpatialHashGridMetricsBuffer", 3);
		m_MetricsClear = new NativeArray<int>(3, Allocator.Persistent);
		m_MetricsCopy = new NativeArray<int>(3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_IsReadbackFinished = true;
	}

	public void Dispose()
	{
		m_Shaders?.Dispose();
		m_HashmapKeys?.Dispose();
		m_HashmapValues?.Dispose();
		m_MetricsBuffer?.Dispose();
		if (m_MetricsClear.IsCreated)
		{
			m_MetricsClear.Dispose();
		}
		if (m_MetricsCopy.IsCreated)
		{
			m_MetricsCopy.Dispose();
		}
	}

	internal void BuildCollidersGrid(CommandBuffer cmd, GraphicsBufferWrapper<int> colliderIndicesMap, GraphicsBufferWrapper<Aabb> colliderAabb, int aabbCount)
	{
		m_ObjectsCount = aabbCount;
		ResizeBuffersInNeed();
		ClearHashmap(cmd);
		CalculateCollidersSpacing(cmd, colliderIndicesMap, colliderAabb);
		BuildCollidersGrid(cmd);
		CalculateLoadFactor(cmd);
	}

	internal void BuildParticlesGrid(CommandBuffer cmd, BodyAllocator bodyAllocator, Culler culler, BodyReplicator replicator)
	{
		m_ObjectsCount = CalculateActiveSimplexCount(bodyAllocator, culler);
		ResizeBuffersInNeed();
		ClearHashmap(cmd);
		CalculateParticlesSpacing(cmd, bodyAllocator, culler, replicator);
		BuildGridForParticles(cmd, bodyAllocator, culler, replicator);
		CalculateLoadFactor(cmd);
	}

	private int CalculateActiveSimplexCount(BodyAllocator bodyAllocator, Culler culler)
	{
		int num = 0;
		foreach (KeyValuePair<AuthoringBase, int> item in bodyAllocator.EntityAllocationMap)
		{
			int2 @int = bodyAllocator.BodyDescriptorSoA.SimplexConstraintsRange[item.Value];
			int2 int2 = bodyAllocator.BodyDescriptorSoA.ConstraintSettingsRange[item.Value];
			bool num2 = bodyAllocator.ConstraintSettingsSoA.Array[int2.x + 4].z > 0f;
			uint bitMask = bodyAllocator.BodyDescriptorSoA.EnabledConstraintTypeMask[item.Value];
			bool flag = culler.BodyVisibility[item.Value] != 0;
			int num3;
			if (num2 && @int.x > -1)
			{
				int bitIndex = 4;
				num3 = (XPBDMath.IsBitSetted(in bitMask, in bitIndex) ? 1 : 0);
			}
			else
			{
				num3 = 0;
			}
			if (((uint)num3 & (flag ? 1u : 0u)) != 0)
			{
				num += @int.y;
			}
		}
		return num;
	}

	public void DoReadbackMetrics(CommandBuffer cmd)
	{
		if (m_IsReadbackFinished)
		{
			m_IsReadbackFinished = false;
			cmd.RequestAsyncReadback(m_MetricsBuffer, OnMetricsReadbackFinished);
		}
	}

	private void ResizeBuffersInNeed()
	{
		int num = XPBDMath.NextPrimeNumber(4 * m_ObjectsCount);
		if (num > m_HashmapSize)
		{
			m_HashmapSize = num;
		}
		else if (GetLoadFactor() > 0.3f)
		{
			m_HashmapSize = XPBDMath.NextPrimeNumber(m_HashmapSize);
		}
		if (m_HashmapKeys == null)
		{
			m_HashmapKeys = new GraphicsBufferWrapper<int>("_XpbdSpatialHashmapKeysBuffer", 17);
		}
		if (m_HashmapValues == null)
		{
			m_HashmapValues = new GraphicsBufferWrapper<int>("_XpbdSpatialHashmapValuesBuffer", 17);
		}
		m_HashmapKeys.Resize(m_HashmapSize);
		m_HashmapValues.Resize(m_HashmapSize);
	}

	private void ClearHashmap(CommandBuffer cmd)
	{
		cmd.SetBufferData(m_MetricsBuffer, m_MetricsClear);
		cmd.SetGlobalBuffer(m_HashmapKeys.NameId, m_HashmapKeys.Buffer);
		cmd.SetGlobalBuffer(m_HashmapValues.NameId, m_HashmapValues.Buffer);
		cmd.SetGlobalInt(ShaderPropertyId._XpbdHashmapSize, m_HashmapSize);
		m_Shaders.KernelClear.Dispatch(cmd, XPBDMath.DivRoundUp(m_HashmapSize, m_Shaders.KernelClear.NumThreads.x), 1, 1);
	}

	private void CalculateCollidersSpacing(CommandBuffer cmd, GraphicsBufferWrapper<int> colliderIndicesMap, GraphicsBufferWrapper<Aabb> colliderAabb)
	{
		if (m_ObjectsCount != 0)
		{
			cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapMetricsObjectSizeSum, 0);
			cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapObjectsCount, m_ObjectsCount);
			cmd.SetGlobalFloat(ShaderPropertyId._XpbdSpatialHashmapSizeDescretizer, 100f);
			cmd.SetGlobalBuffer(m_MetricsBuffer.NameId, m_MetricsBuffer.Buffer);
			cmd.SetGlobalBuffer(colliderIndicesMap.NameId, colliderIndicesMap.Buffer);
			cmd.SetGlobalBuffer(colliderAabb.NameId, colliderAabb.Buffer);
			m_Shaders.KernelCalculateSpacing.Dispatch(cmd, XPBDMath.DivRoundUp(m_ObjectsCount, m_Shaders.KernelCalculateSpacing.NumThreads.x), 1, 1);
		}
	}

	private void CalculateParticlesSpacing(CommandBuffer cmd, BodyAllocator bodyAllocator, Culler culler, BodyReplicator replicator)
	{
		if (m_ObjectsCount != 0)
		{
			cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapMetricsObjectSizeSum, 0);
			cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapObjectsCount, m_ObjectsCount);
			cmd.SetGlobalFloat(ShaderPropertyId._XpbdSpatialHashmapSizeDescretizer, 100f);
			cmd.SetGlobalBuffer(m_MetricsBuffer.NameId, m_MetricsBuffer.Buffer);
			cmd.SetGlobalBuffer(replicator.VisibleBodyIndices.NameId, replicator.VisibleBodyIndices.Buffer);
			replicator.BodyDescriptorSoA.PushToGpu(cmd);
			replicator.ConstraintSoA.PushToGpu(cmd);
			replicator.ConstraintSettingsSoA.PushToGpu(cmd);
			m_Shaders.KernelCalculateParticlesSpacing.Dispatch(cmd, culler.VisibleBodyIndices.Length, 1, 1);
		}
	}

	private void BuildCollidersGrid(CommandBuffer cmd)
	{
		if (m_ObjectsCount != 0)
		{
			cmd.SetGlobalInt(ShaderPropertyId._XpbdHashmapSize, m_HashmapSize);
			cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapObjectsCount, m_ObjectsCount);
			cmd.SetGlobalFloat(ShaderPropertyId._XpbdSpatialHashmapSizeDescretizer, 100f);
			cmd.SetGlobalFloat(ShaderPropertyId._XpbdSpatialHashmapSpacingScale, 2f);
			cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapMetricsObjectSizeSum, 0);
			cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapMaxHashtableLookupIterations, 100);
			cmd.SetGlobalBuffer(m_MetricsBuffer.NameId, m_MetricsBuffer.Buffer);
			cmd.SetGlobalBuffer(m_HashmapKeys.NameId, m_HashmapKeys.Buffer);
			cmd.SetGlobalBuffer(m_HashmapValues.NameId, m_HashmapValues.Buffer);
			m_Shaders.KernelBuildGrid.Dispatch(cmd, XPBDMath.DivRoundUp(m_ObjectsCount, m_Shaders.KernelBuildGrid.NumThreads.x), 1, 1);
		}
	}

	private void BuildGridForParticles(CommandBuffer cmd, BodyAllocator bodyAllocator, Culler culler, BodyReplicator replicator)
	{
		if (m_ObjectsCount != 0)
		{
			cmd.SetGlobalInt(ShaderPropertyId._XpbdHashmapSize, m_HashmapSize);
			cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapObjectsCount, m_ObjectsCount);
			cmd.SetGlobalFloat(ShaderPropertyId._XpbdSpatialHashmapSizeDescretizer, 100f);
			cmd.SetGlobalFloat(ShaderPropertyId._XpbdSpatialHashmapSpacingScale, 2f);
			cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapMetricsObjectSizeSum, 0);
			cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapMaxHashtableLookupIterations, 100);
			cmd.SetGlobalBuffer(m_MetricsBuffer.NameId, m_MetricsBuffer.Buffer);
			cmd.SetGlobalBuffer(m_HashmapKeys.NameId, m_HashmapKeys.Buffer);
			cmd.SetGlobalBuffer(m_HashmapValues.NameId, m_HashmapValues.Buffer);
			cmd.SetGlobalBuffer(replicator.VisibleBodyIndices.NameId, replicator.VisibleBodyIndices.Buffer);
			m_Shaders.KernelBuildParticlesGrid.Dispatch(cmd, culler.VisibleBodyIndices.Length, 1, 1);
		}
	}

	private void CalculateLoadFactor(CommandBuffer cmd)
	{
		cmd.SetGlobalInt(ShaderPropertyId._XpbdHashmapSize, m_HashmapSize);
		cmd.SetGlobalInt(ShaderPropertyId._XpbdSpatialHashmapMetricsOccupiedCellsCount, 1);
		cmd.SetGlobalBuffer(m_MetricsBuffer.NameId, m_MetricsBuffer.Buffer);
		cmd.SetGlobalBuffer(m_HashmapKeys.NameId, m_HashmapKeys.Buffer);
		cmd.SetGlobalBuffer(m_HashmapValues.NameId, m_HashmapValues.Buffer);
		m_Shaders.KernelCalculateLoadFactor.Dispatch(cmd, XPBDMath.DivRoundUp(m_HashmapSize, m_Shaders.KernelCalculateLoadFactor.NumThreads.x), 1, 1);
	}

	private void OnMetricsReadbackFinished(AsyncGPUReadbackRequest request)
	{
		if (request.done)
		{
			if (m_MetricsCopy.IsCreated)
			{
				m_MetricsCopy.CopyFrom(request.GetData<int>());
			}
			m_IsReadbackFinished = true;
		}
	}

	internal int GetHashmapSize()
	{
		return m_HashmapSize;
	}

	internal float GetObjectsSizeSum()
	{
		if (IsValid())
		{
			return (float)m_MetricsCopy[0] / 100f;
		}
		return 0f;
	}

	internal int GetOccupiedCellsCount()
	{
		if (IsValid())
		{
			return m_MetricsCopy[1];
		}
		return 0;
	}

	internal float GetLoadFactor()
	{
		if (IsValid())
		{
			return (float)m_MetricsCopy[1] / (float)m_HashmapSize;
		}
		return 0f;
	}

	internal float GetSpacing()
	{
		if (IsValid())
		{
			return (float)m_MetricsCopy[0] / 100f / (float)m_ObjectsCount * 2f;
		}
		return 0f;
	}

	internal int GetActiveContactsCount()
	{
		if (IsValid())
		{
			return m_MetricsCopy[2];
		}
		return 0;
	}

	public MemoryStat GetMemoryStat()
	{
		MemoryStat result = default(MemoryStat);
		if (IsValid())
		{
			result.Cpu += m_MetricsClear.Length * Marshal.SizeOf<int>();
			result.Cpu += m_MetricsCopy.Length * Marshal.SizeOf<int>();
			result.Gpu += m_HashmapKeys.GetSizeInBytes();
			result.Gpu += m_HashmapValues.GetSizeInBytes();
			result.Gpu += m_MetricsBuffer.GetSizeInBytes();
		}
		return result;
	}

	public bool IsValid()
	{
		return m_HashmapKeys != null;
	}
}
