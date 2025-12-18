using System;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;
using Unity.Collections;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public class GPUDrivenResourcePool<TKey, TManagedResource, TUnmanagedResource> : IDisposable, IGPUDrivenResourcePool<TKey>, IGPUDrivenMemoryProfilingSource where TKey : unmanaged, IEquatable<TKey> where TManagedResource : IGPUDrivenMemoryProfilingSource, new() where TUnmanagedResource : unmanaged
{
	private readonly GPUDrivenIndexAllocator m_IndexAllocator;

	private NativeHashMap<TKey, GPUDrivenIndexAllocator.IndexAllocation> m_IndexAllocations;

	private TManagedResource[] m_ManagedResources;

	private NativeArray<TUnmanagedResource> m_UnmanagedResources;

	public int Count => m_IndexAllocations.Count;

	public GPUDrivenResourcePool(int initialCapacity)
	{
		m_ManagedResources = new TManagedResource[initialCapacity];
		m_UnmanagedResources = new NativeArray<TUnmanagedResource>(initialCapacity, Allocator.Persistent);
		m_IndexAllocator = new GPUDrivenIndexAllocator(initialCapacity, autoGrow: true);
		m_IndexAllocations = new NativeHashMap<TKey, GPUDrivenIndexAllocator.IndexAllocation>(initialCapacity, Allocator.Persistent);
	}

	public void Dispose()
	{
		m_IndexAllocator.Dispose();
		if (m_IndexAllocations.IsCreated)
		{
			m_IndexAllocations.Dispose();
			m_IndexAllocations = default(NativeHashMap<TKey, GPUDrivenIndexAllocator.IndexAllocation>);
		}
		if (m_UnmanagedResources.IsCreated)
		{
			m_UnmanagedResources.Dispose();
			m_UnmanagedResources = default(NativeArray<TUnmanagedResource>);
		}
	}

	public GPUDrivenResourcePoolEnumerator<TKey> GetEnumerator()
	{
		return new GPUDrivenResourcePoolEnumerator<TKey>(m_IndexAllocations);
	}

	public void FillMemoryCounters(Counters.CounterCollection counters)
	{
		counters.CollectBufferSize(counters.ResourceDataCPU, m_UnmanagedResources);
		counters.CollectBufferSize(counters.ResourceDataCPU, m_IndexAllocations);
		TManagedResource[] managedResources = m_ManagedResources;
		for (int i = 0; i < managedResources.Length; i++)
		{
			TManagedResource val = managedResources[i];
			val.FillMemoryCounters(counters);
		}
		m_IndexAllocator.FillMemoryCounter(counters, counters.ResourceDataCPU);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref TManagedResource GetManagedResource(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		return ref m_ManagedResources[indexAllocation.Index];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref TUnmanagedResource GetUnmanagedResource(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		return ref UnsafeCollectionExtensions.ElementAsRef(in m_UnmanagedResources, indexAllocation.Index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref readonly TUnmanagedResource ReadUnmanagedResource(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		return ref UnsafeCollectionExtensions.ElementAsRefReadonly(in m_UnmanagedResources, indexAllocation.Index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NativeArray<TUnmanagedResource>.ReadOnly GetInnerUnmanagedPool()
	{
		return m_UnmanagedResources.AsReadOnly();
	}

	public bool TryGetAllocation(in TKey key, out GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		return m_IndexAllocations.TryGetValue(key, out indexAllocation);
	}

	public GPUDrivenIndexAllocator.IndexAllocation GetOrAllocate(in TKey key, out bool isNew)
	{
		if (m_IndexAllocations.TryGetValue(key, out var item))
		{
			isNew = false;
			return item;
		}
		GPUDrivenIndexAllocator.IndexAllocation indexAllocation = m_IndexAllocator.Allocate();
		if (m_ManagedResources.Length < m_IndexAllocator.Capacity)
		{
			Array.Resize(ref m_ManagedResources, m_IndexAllocator.Capacity);
			ArrayExtensions.ResizeArray(ref m_UnmanagedResources, m_IndexAllocator.Capacity);
		}
		m_ManagedResources[indexAllocation.Index] = new TManagedResource();
		m_UnmanagedResources[indexAllocation.Index] = new TUnmanagedResource();
		m_IndexAllocations[key] = indexAllocation;
		isNew = true;
		return indexAllocation;
	}

	public void Free(in TKey key)
	{
		if (m_IndexAllocations.TryGetValue(key, out var item))
		{
			m_ManagedResources[item.Index] = default(TManagedResource);
			m_UnmanagedResources[item.Index] = default(TUnmanagedResource);
			m_IndexAllocator.Free(item);
			m_IndexAllocations.Remove(key);
		}
	}
}
