using System;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Utilities;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public struct GPUDrivenPropertyDataWriter
{
	private unsafe readonly byte* m_CPUData;

	private int m_Offset;

	private NativeSparseSegmentList m_DirtyDataSegmentList;

	private readonly int m_CPUDataSize;

	private readonly bool m_AutoMarkDirty;

	public unsafe GPUDrivenPropertyDataWriter(void* cpuData, int cpuDataSize, NativeSparseSegmentList dirtyDataSegmentList, bool autoMarkDirty)
	{
		m_CPUData = (byte*)cpuData;
		m_CPUDataSize = cpuDataSize;
		m_Offset = 0;
		m_DirtyDataSegmentList = dirtyDataSegmentList;
		m_AutoMarkDirty = autoMarkDirty;
	}

	public void WritePropertyData(in GPUDrivenAllocator.DataAllocation instanceAllocation, in GPUDrivenRenderer.PropertyData propertyData)
	{
		int propertyDataSize = propertyData.Type.GetPropertyDataSize();
		switch (propertyData.Type)
		{
		case GPUDrivenRenderer.PropertyDataType.Float:
			WritePropertyDataRaw(in instanceAllocation, in propertyData.Value.Float, propertyDataSize);
			break;
		case GPUDrivenRenderer.PropertyDataType.Int:
			WritePropertyDataRaw(in instanceAllocation, in propertyData.Value.Int, propertyDataSize);
			break;
		case GPUDrivenRenderer.PropertyDataType.Vector:
			WritePropertyDataRaw(in instanceAllocation, in propertyData.Value.Vector, propertyDataSize);
			break;
		case GPUDrivenRenderer.PropertyDataType.Color:
			WritePropertyDataRaw(in instanceAllocation, in propertyData.Value.Color, propertyDataSize);
			break;
		case GPUDrivenRenderer.PropertyDataType.Matrix:
			WritePropertyDataRaw(in instanceAllocation, in propertyData.Value.Matrix, propertyDataSize);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void SkipPropertyData(in GPUDrivenRenderer.PropertyData propertyData)
	{
		m_Offset += propertyData.Type.GetPropertyDataSize();
	}

	public void WritePropertyDataRaw<T>(in GPUDrivenAllocator.DataAllocation dataAllocation, in T data, int minDataSize = 0) where T : unmanaged
	{
		GetPropertyDataRawAndMove<T>(in dataAllocation, minDataSize) = data;
	}

	public unsafe ref T GetPropertyDataRawAndMove<T>(in GPUDrivenAllocator.DataAllocation dataAllocation, int minDataSize = 0) where T : unmanaged
	{
		int dataSize = math.max(minDataSize, UnsafeUtility.SizeOf<T>());
		return ref UnsafeUtility.AsRef<T>(GetDataPtrAndMove(in dataAllocation, dataSize));
	}

	public unsafe void WriteData<T>(in GPUDrivenAllocator.DataAllocation dataAllocation, ReadOnlySpan<T> sourceData) where T : unmanaged
	{
		int num = sourceData.Length * UnsafeUtility.SizeOf<T>();
		fixed (T* source = sourceData)
		{
			UnsafeUtility.MemCpy(GetDataPtrAndMove(in dataAllocation, num), source, num);
		}
	}

	private unsafe void* GetDataPtrAndMove(in GPUDrivenAllocator.DataAllocation dataAllocation, int dataSize)
	{
		long num = dataAllocation.TotalOffset() + m_Offset;
		void* result = m_CPUData + num;
		_ = num + dataSize;
		_ = m_CPUDataSize;
		_ = m_Offset + dataSize;
		_ = dataAllocation.Allocation.Size;
		m_Offset += dataSize;
		if (m_AutoMarkDirty)
		{
			m_DirtyDataSegmentList.AddItem((int)num, dataSize);
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void MarkWholeAllocationDirty(in GPUDrivenAllocator.DataAllocation dataAllocation)
	{
		m_DirtyDataSegmentList.AddItem((int)dataAllocation.TotalOffset(), (int)dataAllocation.Allocation.Size);
	}

	public void SkipPropertyDataRaw<T>(int minDataSize = 0) where T : unmanaged
	{
		m_Offset += math.max(minDataSize, UnsafeUtility.SizeOf<T>());
	}

	public void ResetOffset()
	{
		m_Offset = 0;
	}
}
