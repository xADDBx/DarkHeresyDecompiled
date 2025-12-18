using System;
using Owlcat.Runtime.Core.Allocators.Guillotiere;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.Atlas.Jobs;

[BurstCompile]
public struct EntryAllocationJob : IJob
{
	[NativeDisableUnsafePtrRestriction]
	public unsafe NativeAtlasAllocator* Allocator;

	public NativeList<EntryToAllocate> DataToAllocate;

	public bool AllowGrowing;

	internal GrowStrategy GrowStrategy;

	public unsafe void Execute()
	{
		Span<EntryToAllocate> span = DataToAllocate.AsArray().AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref EntryToAllocate reference = ref span[i];
			if (AllowGrowing)
			{
				Allocation allocation = Allocator->Allocate(reference.Rect.zw);
				while (allocation.IsEmpty())
				{
					Grow();
					allocation = Allocator->Allocate(reference.Rect.zw);
				}
				reference.AllocId = allocation.Id;
				reference.IndexInAllocator = allocation.NodeIndex;
				reference.Rect = allocation.Rect.Value;
			}
			else
			{
				Allocation allocation2 = Allocator->Allocate(reference.Rect.zw);
				reference.AllocId = allocation2.Id;
				reference.IndexInAllocator = allocation2.NodeIndex;
				reference.Rect = allocation2.Rect.Value;
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
