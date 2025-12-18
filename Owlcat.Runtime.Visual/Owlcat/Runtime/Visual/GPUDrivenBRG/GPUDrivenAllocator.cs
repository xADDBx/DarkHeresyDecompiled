using System;
using Owlcat.Runtime.Core.Allocators.OffsetAllocator;
using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public class GPUDrivenAllocator : IDisposable, IGPUDrivenMemoryProfilingSource
{
	public enum AllocationType
	{
		Invalid,
		Instance,
		Material,
		Custom
	}

	public struct DataAllocation
	{
		public static readonly DataAllocation Empty = new DataAllocation
		{
			Type = AllocationType.Invalid,
			Allocation = OffsetAllocator.Allocation.Empty
		};

		public AllocationType Type;

		public OffsetAllocator.Allocation Allocation;
	}

	private OffsetAllocator m_Allocator;

	public GPUDrivenAllocator(GPUDrivenBRGSettings settings)
	{
		uint size = (uint)Alignment.AlignUp(settings.MaxPersistentBufferCapacityInBytes, 256);
		uint maxAllocs = (uint)Alignment.AlignUp(settings.InitialInstanceCapacity + settings.InitialMaterialCapacity, 64);
		m_Allocator = new OffsetAllocator(size, maxAllocs);
	}

	public void Dispose()
	{
		m_Allocator.Dispose();
	}

	public void FillMemoryCounters(Counters.CounterCollection counters)
	{
		ProfilerCounterValue<int> instanceDataCPU = counters.InstanceDataCPU;
		instanceDataCPU.Value += m_Allocator.TotalUsedMemory;
	}

	private void EnsureSufficientMaxAllocs()
	{
		if (m_Allocator.NoFreeAllocations)
		{
			m_Allocator.GrowMaxAllocs(m_Allocator.MaxAllocs * 2);
		}
	}

	public DataAllocation AllocateInstanceData(int sizeInBytes)
	{
		EnsureSufficientMaxAllocs();
		OffsetAllocator.Allocation allocation = m_Allocator.Allocate((uint)sizeInBytes);
		DataAllocation result = default(DataAllocation);
		result.Type = AllocationType.Instance;
		result.Allocation = allocation;
		return result;
	}

	public DataAllocation AllocateMaterialData(int sizeInBytes)
	{
		EnsureSufficientMaxAllocs();
		OffsetAllocator.Allocation allocation = m_Allocator.Allocate((uint)sizeInBytes);
		DataAllocation result = default(DataAllocation);
		result.Type = AllocationType.Material;
		result.Allocation = allocation;
		return result;
	}

	public DataAllocation AllocateCustomData(int sizeInBytes)
	{
		EnsureSufficientMaxAllocs();
		OffsetAllocator.Allocation allocation = m_Allocator.Allocate((uint)sizeInBytes);
		DataAllocation result = default(DataAllocation);
		result.Type = AllocationType.Custom;
		result.Allocation = allocation;
		return result;
	}

	public void FreeMaterialData(in DataAllocation dataAllocation)
	{
		m_Allocator.Free(dataAllocation.Allocation);
	}

	public void FreeInstanceData(in DataAllocation dataAllocation)
	{
		m_Allocator.Free(dataAllocation.Allocation);
	}

	public void FreeCustomData(in DataAllocation dataAllocation)
	{
		m_Allocator.Free(dataAllocation.Allocation);
	}

	public unsafe static void ClearInstanceAllocations(NativeArray<DataAllocation> allocations)
	{
		UnsafeUtility.MemSet(allocations.GetUnsafePtr(), byte.MaxValue, allocations.Length * UnsafeUtility.SizeOf<DataAllocation>());
	}
}
