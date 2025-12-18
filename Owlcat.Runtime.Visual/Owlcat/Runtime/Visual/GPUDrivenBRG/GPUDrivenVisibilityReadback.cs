using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public class GPUDrivenVisibilityReadback : IDisposable, IGPUDrivenMemoryProfilingSource
{
	private struct BufferData : IDisposable
	{
		public NativeArray<int> VisibilityMask;

		public Action<AsyncGPUReadbackRequest> ReadbackAction;

		public int LastUsageFrameIndex;

		public void Dispose()
		{
			VisibilityMask.Dispose();
		}
	}

	private const int kFramesToRelease = 10;

	private readonly Dictionary<int, BufferData> m_Buffers = new Dictionary<int, BufferData>();

	private int m_FrameIndex;

	public void Dispose()
	{
		foreach (BufferData value in m_Buffers.Values)
		{
			value.Dispose();
		}
		m_Buffers.Clear();
	}

	public void FillMemoryCounters(Counters.CounterCollection counters)
	{
		foreach (BufferData value in m_Buffers.Values)
		{
			counters.CollectBufferSize(counters.CullingBatchingCPU, value.VisibilityMask);
		}
	}

	[MustUseReturnValue]
	public Action<AsyncGPUReadbackRequest> GetReadbackAction(int cameraInstanceID)
	{
		return m_Buffers[cameraInstanceID].ReadbackAction;
	}

	public bool TryGetVisibilityMask(int cameraInstanceID, out NativeArray<int> visibilityMask)
	{
		if (m_Buffers.TryGetValue(cameraInstanceID, out var value) && value.VisibilityMask.IsCreated)
		{
			visibilityMask = value.VisibilityMask;
			return true;
		}
		visibilityMask = default(NativeArray<int>);
		return false;
	}

	public unsafe void OnCameraUsed(int cameraInstanceID, int maskCapacity)
	{
		BufferData valueOrDefault = m_Buffers.GetValueOrDefault(cameraInstanceID);
		int num = (valueOrDefault.VisibilityMask.IsCreated ? valueOrDefault.VisibilityMask.Length : 0);
		if (num != maskCapacity)
		{
			ArrayExtensions.ResizeArray(ref valueOrDefault.VisibilityMask, maskCapacity);
			UnsafeUtility.MemSet((byte*)valueOrDefault.VisibilityMask.GetUnsafePtr() + (nint)num * (nint)4, byte.MaxValue, valueOrDefault.VisibilityMask.Length - num);
		}
		valueOrDefault.LastUsageFrameIndex = m_FrameIndex;
		ref Action<AsyncGPUReadbackRequest> readbackAction = ref valueOrDefault.ReadbackAction;
		if (readbackAction == null)
		{
			readbackAction = AllocateReadbackAction(cameraInstanceID, this);
		}
		m_Buffers[cameraInstanceID] = valueOrDefault;
		unsafe static Action<AsyncGPUReadbackRequest> AllocateReadbackAction(int cameraInstanceID, GPUDrivenVisibilityReadback visibilityReadback)
		{
			return delegate(AsyncGPUReadbackRequest request)
			{
				if (visibilityReadback.m_Buffers.TryGetValue(cameraInstanceID, out var value) && value.VisibilityMask.IsCreated)
				{
					NativeArray<int> data = request.GetData<int>();
					int num2 = math.min(data.Length, value.VisibilityMask.Length);
					if (num2 > 0)
					{
						UnsafeUtility.MemCpy(value.VisibilityMask.GetUnsafePtr(), data.GetUnsafeReadOnlyPtr(), num2 * 4);
					}
				}
			};
		}
	}

	public void PostRender()
	{
		NativeList<int> nativeList = new NativeList<int>(m_Buffers.Count, Allocator.Temp);
		foreach (KeyValuePair<int, BufferData> buffer in m_Buffers)
		{
			if (buffer.Value.LastUsageFrameIndex < m_FrameIndex - 10)
			{
				int value = buffer.Key;
				nativeList.Add(in value);
				buffer.Value.Dispose();
			}
		}
		foreach (int item in nativeList)
		{
			m_Buffers.Remove(item);
		}
		m_FrameIndex++;
	}
}
