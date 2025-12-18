using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public class GPUDrivenCullingResourcesPool : IDisposable, IGPUDrivenMemoryProfilingSource
{
	public struct CullingResourceSet : IDisposable, IGPUDrivenMemoryProfilingSource
	{
		public int Index;

		public ResizableGraphicsBuffer VisibleIndices;

		public ResizableGraphicsBuffer IndirectArgs;

		public ResizableGraphicsBuffer CPUInstanceVisibilityMask;

		public void Dispose()
		{
			IndirectArgs.Dispose();
			VisibleIndices.Dispose();
			CPUInstanceVisibilityMask.Dispose();
		}

		public void FillMemoryCounters(Counters.CounterCollection counters)
		{
			counters.CollectBufferSize(counters.CullingBatchingGPU, VisibleIndices);
			counters.CollectBufferSize(counters.CullingBatchingGPU, IndirectArgs);
			counters.CollectBufferSize(counters.CullingBatchingGPU, CPUInstanceVisibilityMask);
		}
	}

	private const int kSizeMultiplier = 4;

	private readonly GPUDrivenCommandQueue m_CommandQueue;

	public List<CullingResourceSet> Sets { get; }

	public int UsedCount { get; private set; }

	public GPUDrivenCullingResourcesPool(GPUDrivenBRGSettings settings, GPUDrivenCommandQueue commandQueue)
	{
		m_CommandQueue = commandQueue;
		Sets = new List<CullingResourceSet>(settings.InitialCullingSplitsCapacity);
		int cpuInstanceVisibilityMaskCapacity = Alignment.AlignUp(settings.InitialInstanceCapacity, 32) / 32;
		for (int i = 0; i < settings.InitialCullingSplitsCapacity; i++)
		{
			CullingResourceSet item = CreateFreeResourceSet(settings.InitialInstanceCapacity, settings.InitialRendererGroupsCapacity, cpuInstanceVisibilityMaskCapacity, i);
			Sets.Add(item);
		}
	}

	public void Dispose()
	{
		foreach (CullingResourceSet set in Sets)
		{
			set.Dispose();
		}
		Sets.Clear();
		UsedCount = 0;
	}

	public void FillMemoryCounters(Counters.CounterCollection counters)
	{
		foreach (CullingResourceSet set in Sets)
		{
			set.FillMemoryCounters(counters);
		}
	}

	public void PreRender()
	{
		UsedCount = 0;
	}

	public CullingResourceSet GetOrAllocateResources(int visibleIndicesCapacity, int indirectArgsCapacity, int cpuInstanceVisibilityMaskCapacity)
	{
		int usedCount;
		if (UsedCount >= Sets.Count)
		{
			CullingResourceSet cullingResourceSet = CreateFreeResourceSet(visibleIndicesCapacity, indirectArgsCapacity, cpuInstanceVisibilityMaskCapacity, Sets.Count);
			Sets.Add(cullingResourceSet);
			usedCount = UsedCount + 1;
			UsedCount = usedCount;
			return cullingResourceSet;
		}
		CullingResourceSet cullingResourceSet2 = Sets[UsedCount];
		EnsureVisibleIndicesBufferCapacity(ref cullingResourceSet2.VisibleIndices, visibleIndicesCapacity);
		EnsureIndirectArgumentsBufferCapacity(ref cullingResourceSet2.IndirectArgs, indirectArgsCapacity);
		EnsureCPUInstanceVisibilityMaskBufferCapacity(ref cullingResourceSet2.CPUInstanceVisibilityMask, cpuInstanceVisibilityMaskCapacity);
		Sets[UsedCount] = cullingResourceSet2;
		usedCount = UsedCount + 1;
		UsedCount = usedCount;
		return cullingResourceSet2;
	}

	private CullingResourceSet CreateFreeResourceSet(int visibleIndicesCapacity, int indirectArgsCapacity, int cpuInstanceVisibilityMaskCapacity, int index)
	{
		CullingResourceSet result = default(CullingResourceSet);
		EnsureVisibleIndicesBufferCapacity(ref result.VisibleIndices, visibleIndicesCapacity);
		result.VisibleIndices.Name = string.Format("{0}_{1}_{2}", "CullingResources", "VisibleIndices", index);
		EnsureIndirectArgumentsBufferCapacity(ref result.IndirectArgs, indirectArgsCapacity);
		result.IndirectArgs.Name = string.Format("{0}_{1}_{2}", "CullingResources", "IndirectArgs", index);
		EnsureCPUInstanceVisibilityMaskBufferCapacity(ref result.CPUInstanceVisibilityMask, cpuInstanceVisibilityMaskCapacity);
		if (result.CPUInstanceVisibilityMask.IsCreated)
		{
			result.CPUInstanceVisibilityMask.Name = string.Format("{0}_{1}_{2}", "CullingResources", "CPUInstanceVisibilityMask", index);
		}
		result.Index = index;
		return result;
	}

	private void EnsureVisibleIndicesBufferCapacity(ref ResizableGraphicsBuffer visibleIndicesBuffer, int entitiesCount)
	{
		EnsureBufferCapacity(ref visibleIndicesBuffer, GraphicsBuffer.Target.Raw, entitiesCount, 4);
	}

	private void EnsureIndirectArgumentsBufferCapacity(ref ResizableGraphicsBuffer indirectArgumentsBuffer, int argumentCount)
	{
		EnsureBufferCapacity(ref indirectArgumentsBuffer, GraphicsBuffer.Target.IndirectArguments, argumentCount * 20 / 4, 4);
	}

	private void EnsureCPUInstanceVisibilityMaskBufferCapacity(ref ResizableGraphicsBuffer cpuInstanceVisibilityMaskBuffer, int maskCapacity)
	{
		if (maskCapacity != 0)
		{
			EnsureBufferCapacity(ref cpuInstanceVisibilityMaskBuffer, GraphicsBuffer.Target.Raw, maskCapacity, 4);
		}
	}

	public static (int count, int stride) ComputeVisibilityMaskBufferCountAndStride(int instanceCount)
	{
		return (count: Alignment.AlignUp(instanceCount, 32) / 32, stride: 4);
	}

	private void EnsureBufferCapacity(ref ResizableGraphicsBuffer buffer, GraphicsBuffer.Target target, int count, int stride)
	{
		int num = count * 4;
		if (!buffer.IsCreated)
		{
			buffer = new ResizableGraphicsBuffer(target, num, stride);
		}
		else if (num > buffer.Count)
		{
			num = math.max(num, buffer.Count * 2);
			buffer.Resize(num, m_CommandQueue);
		}
	}
}
