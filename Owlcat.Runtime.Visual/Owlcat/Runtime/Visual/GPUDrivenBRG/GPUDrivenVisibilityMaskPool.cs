using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public class GPUDrivenVisibilityMaskPool : IDisposable, IGPUDrivenMemoryProfilingSource
{
	private enum BufferResizeOptions
	{
		None,
		KeepContentsAndClear
	}

	public struct InstanceVisibilityMasks : IGPUDrivenMemoryProfilingSource
	{
		public int Index;

		public BatchPackedCullingViewID ViewID;

		public ResizableGraphicsBuffer MaskBuffer;

		public ResizableGraphicsBuffer PrevMaskBuffer;

		public int LastUsedFrameIndex;

		public void FillMemoryCounters(Counters.CounterCollection counters)
		{
			counters.CollectBufferSize(counters.CullingBatchingGPU, MaskBuffer);
			counters.CollectBufferSize(counters.CullingBatchingGPU, PrevMaskBuffer);
		}
	}

	private const int kFramesToRelease = 10;

	private readonly GPUDrivenBufferUtils m_BufferUtils;

	private readonly GPUDrivenCommandQueue m_CommandQueue;

	private int m_FrameIndex;

	public List<InstanceVisibilityMasks> VisibilityMasks { get; } = new List<InstanceVisibilityMasks>();


	public GPUDrivenVisibilityMaskPool(GPUDrivenBufferUtils bufferUtils, GPUDrivenCommandQueue commandQueue)
	{
		m_BufferUtils = bufferUtils;
		m_CommandQueue = commandQueue;
	}

	public void Dispose()
	{
		foreach (InstanceVisibilityMasks visibilityMask in VisibilityMasks)
		{
			visibilityMask.MaskBuffer.Dispose();
			visibilityMask.PrevMaskBuffer.Dispose();
		}
		VisibilityMasks.Clear();
	}

	public void FillMemoryCounters(Counters.CounterCollection counters)
	{
		foreach (InstanceVisibilityMasks visibilityMask in VisibilityMasks)
		{
			visibilityMask.FillMemoryCounters(counters);
		}
	}

	public bool UsedThisFrame(in InstanceVisibilityMasks masks)
	{
		return masks.LastUsedFrameIndex == m_FrameIndex;
	}

	public void PostRender()
	{
		for (int i = 0; i < VisibilityMasks.Count; i++)
		{
			InstanceVisibilityMasks value = VisibilityMasks[i];
			if (m_FrameIndex - value.LastUsedFrameIndex >= 10)
			{
				value.ViewID = default(BatchPackedCullingViewID);
				continue;
			}
			ref ResizableGraphicsBuffer maskBuffer = ref value.MaskBuffer;
			ref ResizableGraphicsBuffer prevMaskBuffer = ref value.PrevMaskBuffer;
			ResizableGraphicsBuffer prevMaskBuffer2 = value.PrevMaskBuffer;
			ResizableGraphicsBuffer maskBuffer2 = value.MaskBuffer;
			maskBuffer = prevMaskBuffer2;
			prevMaskBuffer = maskBuffer2;
			VisibilityMasks[i] = value;
		}
		m_FrameIndex++;
	}

	public int GetOrAllocateVisibilityMasks(BatchPackedCullingViewID viewID, int instanceCapacity)
	{
		(int index, bool clear) tuple = FindOrCreateVisibilityMask(viewID, instanceCapacity);
		int item = tuple.index;
		bool item2 = tuple.clear;
		InstanceVisibilityMasks value = VisibilityMasks[item];
		BufferResizeOptions bufferResizeOptions = ((!item2) ? BufferResizeOptions.KeepContentsAndClear : BufferResizeOptions.None);
		EnsureVisibilityMaskBufferCapacity(ref value.MaskBuffer, instanceCapacity, bufferResizeOptions);
		EnsureVisibilityMaskBufferCapacity(ref value.PrevMaskBuffer, instanceCapacity, bufferResizeOptions);
		if (item2)
		{
			m_BufferUtils.DispatchClearBuffer(m_CommandQueue.Cmd, value.MaskBuffer.InternalBuffer, 0);
			m_BufferUtils.DispatchClearBuffer(m_CommandQueue.Cmd, value.PrevMaskBuffer.InternalBuffer, 0);
		}
		VisibilityMasks[item] = value;
		return item;
	}

	private (int index, bool clear) FindOrCreateVisibilityMask(BatchPackedCullingViewID viewID, int instanceCapacity)
	{
		for (int i = 0; i < VisibilityMasks.Count; i++)
		{
			InstanceVisibilityMasks value = VisibilityMasks[i];
			if (value.ViewID == viewID)
			{
				value.LastUsedFrameIndex = m_FrameIndex;
				VisibilityMasks[i] = value;
				return (index: i, clear: false);
			}
		}
		for (int j = 0; j < VisibilityMasks.Count; j++)
		{
			InstanceVisibilityMasks value2 = VisibilityMasks[j];
			if (value2.ViewID.Equals(default(BatchPackedCullingViewID)))
			{
				value2.ViewID = viewID;
				value2.LastUsedFrameIndex = m_FrameIndex;
				VisibilityMasks[j] = value2;
				return (index: j, clear: true);
			}
		}
		int count = VisibilityMasks.Count;
		InstanceVisibilityMasks instanceVisibilityMasks = default(InstanceVisibilityMasks);
		instanceVisibilityMasks.ViewID = viewID;
		instanceVisibilityMasks.LastUsedFrameIndex = m_FrameIndex;
		instanceVisibilityMasks.Index = count;
		InstanceVisibilityMasks item = instanceVisibilityMasks;
		EnsureVisibilityMaskBufferCapacity(ref item.MaskBuffer, instanceCapacity, BufferResizeOptions.None);
		item.MaskBuffer.Name = string.Format("{0}_{1}0_{2}", "VisibilityMasks", "MaskBuffer", count);
		EnsureVisibilityMaskBufferCapacity(ref item.PrevMaskBuffer, instanceCapacity, BufferResizeOptions.None);
		item.PrevMaskBuffer.Name = string.Format("{0}_{1}1_{2}", "VisibilityMasks", "PrevMaskBuffer", count);
		VisibilityMasks.Add(item);
		return (index: count, clear: true);
	}

	private void EnsureVisibilityMaskBufferCapacity(ref ResizableGraphicsBuffer visibilityMask, int instanceCount, BufferResizeOptions bufferResizeOptions)
	{
		var (count, stride) = ComputeVisibilityMaskBufferCountAndStride(instanceCount);
		EnsureBufferCapacity(ref visibilityMask, GraphicsBuffer.Target.Raw, count, stride, bufferResizeOptions);
	}

	private static (int count, int stride) ComputeVisibilityMaskBufferCountAndStride(int instanceCount)
	{
		return (count: Alignment.AlignUp(instanceCount, 32) / 32, stride: 4);
	}

	private void EnsureBufferCapacity(ref ResizableGraphicsBuffer buffer, GraphicsBuffer.Target target, int count, int stride, BufferResizeOptions bufferResizeOptions = BufferResizeOptions.None)
	{
		int num = count;
		if (!buffer.IsCreated)
		{
			buffer = new ResizableGraphicsBuffer(target, num, stride);
		}
		else if (num > buffer.Count)
		{
			num = math.max(num, buffer.Count * 2);
			switch (bufferResizeOptions)
			{
			case BufferResizeOptions.None:
				buffer.Resize(num);
				break;
			case BufferResizeOptions.KeepContentsAndClear:
				buffer.ResizeKeepContentsAndClear(num, m_BufferUtils, m_CommandQueue, 0);
				break;
			}
		}
	}
}
