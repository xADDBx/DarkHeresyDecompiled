using System;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.GPUDrivenBRG.BakedLighting;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.SceneManagement;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Scenes;

public class GPUDrivenPersistentSceneData : IDisposable, IGPUDrivenMemoryProfilingSource
{
	private readonly GPUDrivenLightmapping m_Lightmapping;

	private NativeArray<GPUDrivenSceneData> m_SceneData;

	private GPUDrivenIndexAllocator m_SceneIndexAllocator;

	private NativeHashMap<GPUDrivenSceneHandle, GPUDrivenIndexAllocator.IndexAllocation> m_SceneIndices;

	public GPUDrivenPersistentSceneData(GPUDrivenBRGSettings settings, GPUDrivenLightmapping lightmapping)
	{
		CreateOrResizeScenes(settings.InitialScenesCapacity);
		m_SceneIndices = new NativeHashMap<GPUDrivenSceneHandle, GPUDrivenIndexAllocator.IndexAllocation>(settings.InitialScenesCapacity, Allocator.Persistent);
		m_Lightmapping = lightmapping;
		SceneManager.sceneLoaded += SceneManager_OnSceneLoaded;
		SceneManager.sceneUnloaded += SceneManaged_OnSceneUnloaded;
		Scene scene = SceneManager.GetActiveScene();
		if (scene.IsValid())
		{
			OnLoadedScene(GPUDrivenSceneHandle.FromScene(in scene));
		}
		for (int i = 0; i < SceneManager.loadedSceneCount; i++)
		{
			Scene scene2 = SceneManager.GetSceneAt(i);
			OnLoadedScene(GPUDrivenSceneHandle.FromScene(in scene2));
		}
	}

	public void Dispose()
	{
		if (m_SceneData.IsCreated)
		{
			m_SceneData.Dispose();
			m_SceneData = default(NativeArray<GPUDrivenSceneData>);
		}
		if (m_SceneIndices.IsCreated)
		{
			m_SceneIndices.Dispose();
			m_SceneIndices = default(NativeHashMap<GPUDrivenSceneHandle, GPUDrivenIndexAllocator.IndexAllocation>);
		}
		if (m_SceneIndexAllocator != null)
		{
			m_SceneIndexAllocator.Dispose();
			m_SceneIndexAllocator = null;
		}
		SceneManager.sceneLoaded -= SceneManager_OnSceneLoaded;
		SceneManager.sceneUnloaded -= SceneManaged_OnSceneUnloaded;
	}

	public void FillMemoryCounters(Counters.CounterCollection counters)
	{
		counters.CollectBufferSize(counters.MiscCPU, m_SceneData);
		counters.CollectBufferSize(counters.MiscCPU, m_SceneIndices);
		m_SceneIndexAllocator.FillMemoryCounter(counters, counters.MiscCPU);
		m_Lightmapping.FillMemoryCounters(counters);
	}

	private void CreateOrResizeScenes(int sceneCount)
	{
		if (m_SceneIndexAllocator == null)
		{
			m_SceneIndexAllocator = new GPUDrivenIndexAllocator(sceneCount);
		}
		else
		{
			m_SceneIndexAllocator.ForceGrow(sceneCount);
		}
		sceneCount = m_SceneIndexAllocator.Capacity;
		GPUDrivenRenderingUtils.CreateOrResizeNativeArray(ref m_SceneData, sceneCount, Allocator.Persistent);
	}

	private void SceneManager_OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		OnLoadedScene(GPUDrivenSceneHandle.FromScene(in scene));
	}

	private void SceneManaged_OnSceneUnloaded(Scene scene)
	{
		OnUnloadedScene(GPUDrivenSceneHandle.FromScene(in scene));
	}

	public bool TryGetSceneIndex(GPUDrivenSceneHandle sceneHandle, out GPUDrivenIndexAllocator.IndexAllocation sceneIndex)
	{
		return m_SceneIndices.TryGetValue(sceneHandle, out sceneIndex);
	}

	public void OnLoadedScene(GPUDrivenSceneHandle sceneHandle)
	{
		if (!m_SceneIndices.TryGetValue(sceneHandle, out var _))
		{
			GPUDrivenIndexAllocator.IndexAllocation item2 = m_SceneIndexAllocator.Allocate();
			if (item2.Index == -1)
			{
				CreateOrResizeScenes(m_SceneIndexAllocator.Capacity * 2);
				item2 = m_SceneIndexAllocator.Allocate();
			}
			m_SceneIndices.Add(sceneHandle, item2);
			LoadLightmaps(sceneHandle);
		}
	}

	private void LoadLightmaps(GPUDrivenSceneHandle sceneHandle)
	{
		if (!m_SceneIndices.TryGetValue(sceneHandle, out var item) || !GPUDrivenLightmapReference.TryGet(sceneHandle, out var lightmapReference) || !m_Lightmapping.AddLightmaps(sceneHandle, in lightmapReference.LightmapArrays, out var count))
		{
			return;
		}
		int num = 0;
		foreach (KVPair<GPUDrivenSceneHandle, GPUDrivenIndexAllocator.IndexAllocation> sceneIndex in m_SceneIndices)
		{
			GPUDrivenIndexAllocator.IndexAllocation value = sceneIndex.Value;
			if (value.Index != item.Index)
			{
				ref GPUDrivenSceneData reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_SceneData, value.Index);
				num = math.max(num, reference.LightmapIndexOffset + reference.LightmapCount);
			}
		}
		ref GPUDrivenSceneData reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in m_SceneData, item.Index);
		reference2.LightmapIndexOffset = num;
		reference2.LightmapCount = count;
	}

	private void OnUnloadedScene(GPUDrivenSceneHandle sceneHandle)
	{
		if (m_SceneIndices.TryGetValue(sceneHandle, out var item))
		{
			UnloadLightmaps(sceneHandle);
			UnsafeCollectionExtensions.ElementAsRef(in m_SceneData, item.Index) = default(GPUDrivenSceneData);
			m_SceneIndices.Remove(sceneHandle);
			m_SceneIndexAllocator.Free(item);
		}
	}

	private void UnloadLightmaps(GPUDrivenSceneHandle sceneHandle)
	{
		m_Lightmapping.RemoveLightmaps(sceneHandle);
		if (!m_SceneIndices.TryGetValue(sceneHandle, out var item))
		{
			return;
		}
		ref GPUDrivenSceneData reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_SceneData, item.Index);
		foreach (KVPair<GPUDrivenSceneHandle, GPUDrivenIndexAllocator.IndexAllocation> sceneIndex in m_SceneIndices)
		{
			GPUDrivenIndexAllocator.IndexAllocation value = sceneIndex.Value;
			if (value.Index != item.Index)
			{
				ref GPUDrivenSceneData reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in m_SceneData, value.Index);
				if (reference2.LightmapIndexOffset > reference.LightmapIndexOffset)
				{
					reference2.LightmapIndexOffset -= reference.LightmapCount;
				}
			}
		}
		reference.LightmapIndexOffset = 0;
		reference.LightmapCount = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NativeArray<GPUDrivenSceneData>.ReadOnly GetSceneDataReadonly()
	{
		return m_SceneData.AsReadOnly();
	}
}
