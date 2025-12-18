using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Core.Math;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Utilities;

public struct NativeSparseSegmentList : IDisposable
{
	public struct ParallelWriter
	{
		[NativeDisableContainerSafetyRestriction]
		private NativeList<IndexSegment>.ParallelWriter m_Segments;

		private readonly int m_SegmentSizeAlignment;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddItemNoResize(int offset, int size)
		{
			if (size != 0)
			{
				if (m_SegmentSizeAlignment > 0)
				{
					size = Alignment.AlignUp(size, m_SegmentSizeAlignment);
				}
				IndexSegment indexSegment = default(IndexSegment);
				indexSegment.From = offset;
				indexSegment.ToExclusive = offset + size;
				IndexSegment value = indexSegment;
				m_Segments.AddNoResize(value);
			}
		}

		internal ParallelWriter(NativeList<IndexSegment>.ParallelWriter segments, int segmentSizeAlignment)
		{
			m_Segments = segments;
			m_SegmentSizeAlignment = segmentSizeAlignment;
		}
	}

	public struct SegmentMergeJobInfo : IDisposable
	{
		public NativeList<IndexSegment> Segments;

		public int SegmentsCapacity;

		public JobHandle JobHandle;

		public void Dispose()
		{
			if (Segments.IsCreated)
			{
				Segments.Dispose();
			}
		}

		public bool IsValid()
		{
			return Segments.IsCreated;
		}
	}

	[BurstCompile]
	private struct MergeJob : IJob
	{
		[ReadOnly]
		public NativeArray<IndexSegment> SortedSegments;

		public NativeList<IndexSegment> MergedSegments;

		public void Execute()
		{
			foreach (IndexSegment sortedSegment in SortedSegments)
			{
				if (MergedSegments.Length == 0)
				{
					MergedSegments.AddNoResize(sortedSegment);
					continue;
				}
				ref IndexSegment reference = ref UnsafeCollectionExtensions.ElementAsRef(in MergedSegments, MergedSegments.Length - 1);
				if (IntersectOrAdjacentOrdered(reference, sortedSegment))
				{
					reference = MergeSegments(reference, sortedSegment);
				}
				else
				{
					MergedSegments.AddNoResize(sortedSegment);
				}
			}
		}
	}

	private NativeList<IndexSegment> m_Segments;

	private readonly int m_SegmentSizeAlignment;

	public int TotalMemoryUsed => m_Segments.Capacity * UnsafeUtility.SizeOf<IndexSegment>();

	public int Count => m_Segments.Length;

	public NativeSparseSegmentList(Allocator allocator, int segmentSizeAlignment = 0)
	{
		m_Segments = new NativeList<IndexSegment>(allocator);
		m_SegmentSizeAlignment = segmentSizeAlignment;
	}

	public void Dispose()
	{
		if (m_Segments.IsCreated)
		{
			m_Segments.Dispose();
			m_Segments = default(NativeList<IndexSegment>);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AddItem(int offset, int size)
	{
		if (size != 0)
		{
			if (m_SegmentSizeAlignment > 0)
			{
				size = Alignment.AlignUp(size, m_SegmentSizeAlignment);
			}
			IndexSegment indexSegment = default(IndexSegment);
			indexSegment.From = offset;
			indexSegment.ToExclusive = offset + size;
			IndexSegment value = indexSegment;
			m_Segments.Add(in value);
		}
	}

	public void Clear()
	{
		m_Segments.Clear();
	}

	public ParallelWriter AsParallelWriter()
	{
		return new ParallelWriter(m_Segments.AsParallelWriter(), m_SegmentSizeAlignment);
	}

	[MustUseReturnValue]
	public SegmentMergeJobInfo Merge(Allocator allocator)
	{
		if (m_Segments.Length == 0)
		{
			return default(SegmentMergeJobInfo);
		}
		JobHandle dependsOn = m_Segments.SortJob().Schedule();
		int num = math.max(1, m_Segments.Length);
		NativeList<IndexSegment> nativeList = new NativeList<IndexSegment>(num, allocator);
		MergeJob jobData = default(MergeJob);
		jobData.SortedSegments = m_Segments.AsArray();
		jobData.MergedSegments = nativeList;
		dependsOn = jobData.Schedule(dependsOn);
		SegmentMergeJobInfo result = default(SegmentMergeJobInfo);
		result.Segments = nativeList;
		result.JobHandle = dependsOn;
		result.SegmentsCapacity = num;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	private static bool IntersectOrAdjacentOrdered(IndexSegment segment1, IndexSegment segment2)
	{
		if (segment1.ToExclusive >= segment2.From)
		{
			return segment2.ToExclusive >= segment1.From;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	private static IndexSegment MergeSegments(IndexSegment segment1, IndexSegment segment2)
	{
		IndexSegment result = default(IndexSegment);
		result.From = math.min(segment1.From, segment2.From);
		result.ToExclusive = math.max(segment1.ToExclusive, segment2.ToExclusive);
		return result;
	}

	public void EnsureCapacity(int capacity)
	{
		if (m_Segments.Capacity < capacity)
		{
			int capacity2 = math.max(m_Segments.Capacity * 2, capacity);
			m_Segments.SetCapacity(capacity2);
		}
	}
}
