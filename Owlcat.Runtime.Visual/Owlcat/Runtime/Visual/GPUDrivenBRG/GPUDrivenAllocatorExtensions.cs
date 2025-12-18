using System.Runtime.CompilerServices;
using Owlcat.Runtime.Core.Allocators.OffsetAllocator;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public static class GPUDrivenAllocatorExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint TotalOffset(this in GPUDrivenAllocator.DataAllocation dataAllocation)
	{
		return dataAllocation.Allocation.Offset;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsValid(this in GPUDrivenAllocator.DataAllocation dataAllocation)
	{
		if (dataAllocation.Type != 0)
		{
			return dataAllocation.Allocation.IsValid();
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint TotalOffsetOrDefault(this in GPUDrivenAllocator.DataAllocation dataAllocation)
	{
		if (!dataAllocation.IsValid())
		{
			return 0u;
		}
		return dataAllocation.TotalOffset();
	}
}
