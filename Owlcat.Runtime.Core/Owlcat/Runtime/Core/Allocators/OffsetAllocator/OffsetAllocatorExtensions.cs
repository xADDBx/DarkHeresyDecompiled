using System.Runtime.CompilerServices;

namespace Owlcat.Runtime.Core.Allocators.OffsetAllocator;

public static class OffsetAllocatorExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsValid(this in OffsetAllocator.Allocation allocation)
	{
		if (allocation.Offset != uint.MaxValue && allocation.Size != uint.MaxValue)
		{
			return allocation.Metadata != uint.MaxValue;
		}
		return false;
	}
}
