using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Allocators.Guillotiere;

public class AtlasAllocator : IDisposable
{
	private NativeAtlasAllocator m_Allocator;

	public NativeList<Node> Nodes => m_Allocator.Nodes;

	public int Width => m_Allocator.Width;

	public int Height => m_Allocator.Height;

	public AtlasAllocator(int2 size, in AllocatorOptions options)
	{
		m_Allocator = new NativeAtlasAllocator(size, in options);
	}

	public AtlasAllocator(int2 size)
		: this(size, in AllocatorOptions.DefaultOptions)
	{
	}

	public void Dispose()
	{
		m_Allocator.Dispose();
	}

	public Allocation Allocate(int2 requestedSize)
	{
		return m_Allocator.Allocate(requestedSize);
	}

	public void Deallocate(uint allocId)
	{
		m_Allocator.Deallocate(allocId);
	}

	public void Grow(int2 newSize)
	{
		m_Allocator.Grow(newSize);
	}

	public void Clear()
	{
		m_Allocator.Clear();
	}

	public void Reset(in int2 size, in AllocatorOptions options)
	{
		m_Allocator.Reset(in size, in options);
	}

	public unsafe JobHandle ScheduleAllocationJob(NativeList<int2> rectanglesToAllocate, NativeList<Allocation> allocations, bool allowGrowing, GrowStrategy growStrategy, JobHandle dependency)
	{
		AllocationJob allocationJob = default(AllocationJob);
		allocationJob.Allocator = GetAllocatorPtr();
		allocationJob.RectanglesToAlloc = rectanglesToAllocate;
		allocationJob.Allocations = allocations;
		allocationJob.AllowGrowing = allowGrowing;
		allocationJob.GrowStrategy = growStrategy;
		AllocationJob jobData = allocationJob;
		return IJobExtensions.ScheduleByRef(ref jobData, dependency);
	}

	public float Occupancy()
	{
		return m_Allocator.Occupancy();
	}

	public unsafe ref Node GetNode(int nodeIndex)
	{
		if (nodeIndex < 0 || nodeIndex >= m_Allocator.Nodes.Length)
		{
			throw new ArgumentOutOfRangeException("nodeIndex");
		}
		return ref UnsafeUtility.ArrayElementAsRef<Node>(m_Allocator.Nodes.GetUnsafePtr(), nodeIndex);
	}

	public unsafe NativeAtlasAllocator* GetAllocatorPtr()
	{
		return (NativeAtlasAllocator*)UnsafeUtility.AddressOf(ref m_Allocator);
	}
}
