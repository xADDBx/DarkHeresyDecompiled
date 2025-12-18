using System;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public struct GPUDrivenBRGComponentMapping : IDisposable, IGPUDrivenMemoryProfilingSource
{
	private NativeHashMap<int, int> m_GPUDrivenRendererToMeshRendererInstanceID;

	private NativeHashMap<int, int> m_MeshRendererInstanceIDToGPUDrivenRenderer;

	private NativeHashMap<int, int> m_MeshFilterInstanceIDToMeshRenderer;

	private NativeHashMap<int, int> m_MeshRendererInstanceIDToMeshFilter;

	public GPUDrivenBRGComponentMapping(int initialCapacity, AllocatorManager.AllocatorHandle allocator)
	{
		m_GPUDrivenRendererToMeshRendererInstanceID = new NativeHashMap<int, int>(initialCapacity, allocator);
		m_MeshRendererInstanceIDToGPUDrivenRenderer = new NativeHashMap<int, int>(initialCapacity, allocator);
		m_MeshFilterInstanceIDToMeshRenderer = new NativeHashMap<int, int>(initialCapacity, allocator);
		m_MeshRendererInstanceIDToMeshFilter = new NativeHashMap<int, int>(initialCapacity, allocator);
	}

	public void AddGPUDrivenRendererMapping(GPUDrivenInstanceID meshRendererInstanceID, GPUDrivenInstanceID gpuDrivenRendererInstanceID)
	{
		int num = ExtractUnityObjectInstanceID(meshRendererInstanceID);
		int num2 = ExtractUnityObjectInstanceID(gpuDrivenRendererInstanceID);
		m_MeshRendererInstanceIDToGPUDrivenRenderer[num] = num2;
		m_GPUDrivenRendererToMeshRendererInstanceID[num2] = num;
	}

	private static int ExtractUnityObjectInstanceID(GPUDrivenInstanceID instanceID)
	{
		if (instanceID.Type == GPUDrivenInstanceID.InstanceIDType.UnityObject)
		{
			return instanceID.RawInstanceID;
		}
		return 0;
	}

	public void AddMeshFilterMapping(GPUDrivenInstanceID meshRendererInstanceID, GPUDrivenInstanceID meshFilterInstanceID)
	{
		int num = ExtractUnityObjectInstanceID(meshRendererInstanceID);
		int num2 = ExtractUnityObjectInstanceID(meshFilterInstanceID);
		m_MeshRendererInstanceIDToMeshFilter[num] = num2;
		m_MeshFilterInstanceIDToMeshRenderer[num2] = num;
	}

	public void RemoveAllMappings(GPUDrivenInstanceID meshRendererInstanceID)
	{
		RemoveGPUDrivenRendererMapping(meshRendererInstanceID);
		RemoveMeshFilterMapping(meshRendererInstanceID);
	}

	public void RemoveGPUDrivenRendererMapping(GPUDrivenInstanceID meshRendererInstanceID)
	{
		int key = ExtractUnityObjectInstanceID(meshRendererInstanceID);
		if (m_MeshRendererInstanceIDToGPUDrivenRenderer.TryGetValue(key, out var item))
		{
			m_MeshRendererInstanceIDToGPUDrivenRenderer.Remove(key);
			m_GPUDrivenRendererToMeshRendererInstanceID.Remove(item);
		}
	}

	private void RemoveMeshFilterMapping(GPUDrivenInstanceID meshRendererInstanceID)
	{
		int key = ExtractUnityObjectInstanceID(meshRendererInstanceID);
		if (m_MeshRendererInstanceIDToMeshFilter.TryGetValue(key, out var item))
		{
			m_MeshRendererInstanceIDToMeshFilter.Remove(key);
			m_MeshFilterInstanceIDToMeshRenderer.Remove(item);
		}
	}

	public bool TryMapGPUDrivenRendererInstanceIDToMeshRenderer(GPUDrivenInstanceID gpuDrivenRendererInstanceID, out GPUDrivenInstanceID rendererInstanceID)
	{
		int key = ExtractUnityObjectInstanceID(gpuDrivenRendererInstanceID);
		if (m_GPUDrivenRendererToMeshRendererInstanceID.TryGetValue(key, out var item))
		{
			rendererInstanceID = GPUDrivenInstanceID.UnityObject(item);
			return true;
		}
		rendererInstanceID = GPUDrivenInstanceID.Invalid();
		return false;
	}

	public bool TryMapMeshFilterInstanceIDToMeshRenderer(GPUDrivenInstanceID meshFilterInstanceID, out GPUDrivenInstanceID rendererInstanceID)
	{
		int key = ExtractUnityObjectInstanceID(meshFilterInstanceID);
		if (m_MeshFilterInstanceIDToMeshRenderer.TryGetValue(key, out var item))
		{
			rendererInstanceID = GPUDrivenInstanceID.UnityObject(item);
			return true;
		}
		rendererInstanceID = GPUDrivenInstanceID.Invalid();
		return false;
	}

	public void Dispose()
	{
		if (m_GPUDrivenRendererToMeshRendererInstanceID.IsCreated)
		{
			m_GPUDrivenRendererToMeshRendererInstanceID.Dispose();
			m_GPUDrivenRendererToMeshRendererInstanceID = default(NativeHashMap<int, int>);
		}
		if (m_MeshRendererInstanceIDToGPUDrivenRenderer.IsCreated)
		{
			m_MeshRendererInstanceIDToGPUDrivenRenderer.Dispose();
			m_MeshRendererInstanceIDToGPUDrivenRenderer = default(NativeHashMap<int, int>);
		}
		if (m_MeshFilterInstanceIDToMeshRenderer.IsCreated)
		{
			m_MeshFilterInstanceIDToMeshRenderer.Dispose();
			m_MeshFilterInstanceIDToMeshRenderer = default(NativeHashMap<int, int>);
		}
		if (m_MeshRendererInstanceIDToMeshFilter.IsCreated)
		{
			m_MeshRendererInstanceIDToMeshFilter.Dispose();
			m_MeshRendererInstanceIDToMeshFilter = default(NativeHashMap<int, int>);
		}
	}

	public void FillMemoryCounters(Counters.CounterCollection counters)
	{
		counters.CollectBufferSize(counters.InstanceDataCPU, m_GPUDrivenRendererToMeshRendererInstanceID);
		counters.CollectBufferSize(counters.InstanceDataCPU, m_MeshRendererInstanceIDToGPUDrivenRenderer);
		counters.CollectBufferSize(counters.InstanceDataCPU, m_MeshFilterInstanceIDToMeshRenderer);
		counters.CollectBufferSize(counters.InstanceDataCPU, m_MeshRendererInstanceIDToMeshFilter);
	}
}
