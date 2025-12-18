using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Allocators.Guillotiere;

[BurstCompile]
public struct AllocationJob : IJob
{
	[NativeDisableUnsafePtrRestriction]
	public unsafe NativeAtlasAllocator* Allocator;

	public NativeList<int2> RectanglesToAlloc;

	[WriteOnly]
	public NativeList<Allocation> Allocations;

	public bool AllowGrowing;

	internal GrowStrategy GrowStrategy;

	public unsafe void Execute()
	{
		Span<int2> span = RectanglesToAlloc.AsArray().AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref int2 reference = ref span[i];
			if (AllowGrowing)
			{
				Allocation value = Allocator->Allocate(reference);
				while (value.IsEmpty())
				{
					Grow();
					value = Allocator->Allocate(reference);
				}
				Allocations.Add(in value);
			}
			else
			{
				ref NativeList<Allocation> allocations = ref Allocations;
				Allocation value2 = Allocator->Allocate(reference);
				allocations.Add(in value2);
			}
		}
	}

	private void Grow()
	{
		switch (GrowStrategy)
		{
		case GrowStrategy.SmallestDimension:
			GrowSmallestDimension();
			break;
		case GrowStrategy.Horizontally:
			GrowHorizontally();
			break;
		case GrowStrategy.Vertically:
			GrowVertically();
			break;
		default:
			GrowSmallestDimension();
			break;
		}
	}

	private unsafe void GrowVertically()
	{
		Allocator->Grow(new int2(Allocator->Width, Allocator->Height * 2));
	}

	private unsafe void GrowHorizontally()
	{
		Allocator->Grow(new int2(Allocator->Width * 2, Allocator->Height));
	}

	private unsafe readonly void GrowSmallestDimension()
	{
		if (Allocator->Width <= Allocator->Height)
		{
			Allocator->Grow(new int2(Allocator->Width * 2, Allocator->Height));
		}
		else
		{
			Allocator->Grow(new int2(Allocator->Width, Allocator->Height * 2));
		}
	}
}
