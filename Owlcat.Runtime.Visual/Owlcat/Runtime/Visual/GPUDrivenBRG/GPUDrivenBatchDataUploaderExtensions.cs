using System;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

internal static class GPUDrivenBatchDataUploaderExtensions
{
	private static class Profiling
	{
		public static readonly ProfilingSampler UploadBigSegments = new ProfilingSampler("Upload: Big Segments");
	}

	private static class JobUtils
	{
		private const int kBatchCountThreshold = 4;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RunJobInPlace<TJob>(in TJob job, int arrayLength, int batchSize) where TJob : struct, IJobParallelFor
		{
			if (arrayLength <= batchSize * 4)
			{
				IJobParallelForExtensions.Run(job, arrayLength);
			}
			else
			{
				IJobParallelForExtensions.Schedule(job, arrayLength, batchSize).Complete();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RunBatchJobInPlace<TJob>(in TJob job, int arrayLength, int batchSize) where TJob : struct, IJobParallelForBatch
		{
			if (arrayLength <= batchSize * 4)
			{
				job.RunBatch(arrayLength);
			}
			else
			{
				job.ScheduleBatch(arrayLength, batchSize).Complete();
			}
		}
	}

	[BurstCompile]
	private struct ApplyBufferDataSegmentUnitStride : IJobParallelFor
	{
		public const int kBatchSize = 32;

		public NativeArray<IndexSegment> Segments;

		public int UnitStride;

		public int CPUDataSize;

		public void Execute(int index)
		{
			ref IndexSegment reference = ref UnsafeCollectionExtensions.ElementAsRefUnchecked(in Segments, index);
			reference.From *= UnitStride;
			reference.ToExclusive *= UnitStride;
			reference.ToExclusive = math.min(reference.ToExclusive, CPUDataSize);
		}
	}

	[BurstCompile]
	private struct ComputeTotalSegmentByteSizeJob : IJobParallelForBatch
	{
		public const int kBatchSize = 32;

		[ReadOnly]
		public NativeArray<IndexSegment> Segments;

		[NativeDisableUnsafePtrRestriction]
		public UnsafeAtomicCounter32 TotalSmallSegmentByteSizeCounter;

		[NativeDisableUnsafePtrRestriction]
		public UnsafeAtomicCounter32 BigSegmentCounter;

		public int BigSegmentSize;

		public void Execute(int startIndex, int count)
		{
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < count; i++)
			{
				IndexSegment indexSegment = Segments[startIndex + i];
				int num3 = math.max(0, indexSegment.ToExclusive - indexSegment.From);
				bool flag = num3 >= BigSegmentSize;
				num += ((!flag) ? num3 : 0);
				num2 += (flag ? 1 : 0);
			}
			if (num > 0)
			{
				TotalSmallSegmentByteSizeCounter.Add(num);
			}
			if (num2 > 0)
			{
				BigSegmentCounter.Add(num2);
			}
		}
	}

	[BurstCompile]
	private struct ScheduleUploadSegmentsJob : IJobParallelForBatch
	{
		public const int kBatchSize = 128;

		[NativeDisableUnsafePtrRestriction]
		public unsafe byte* SourcePtr;

		[ReadOnly]
		public NativeArray<IndexSegment> Segments;

		[NativeDisableUnsafePtrRestriction]
		public UnsafeAtomicCounter32 StagingBufferCurrentOffsetAtomic;

		public NativeArray<byte> StagingBufferCPUDataBuffer;

		public NativeList<GPUDrivenBatchedDataUploader.GPUDrivenUploadSegment>.ParallelWriter CPUSegmentBuffer;

		public NativeList<IndexSegment>.ParallelWriter BigSegments;

		public int BigSegmentSize;

		public unsafe void Execute(int startIndex, int count)
		{
			NativeList<GPUDrivenBatchedDataUploader.GPUDrivenUploadSegment> list = new NativeList<GPUDrivenBatchedDataUploader.GPUDrivenUploadSegment>(count, Allocator.Temp);
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				IndexSegment indexSegment = Segments[startIndex + i];
				int num2 = math.max(0, indexSegment.ToExclusive - indexSegment.From);
				num += ((num2 < BigSegmentSize) ? num2 : 0);
			}
			int num3 = StagingBufferCurrentOffsetAtomic.Add(num);
			for (int j = 0; j < count; j++)
			{
				IndexSegment value = Segments[startIndex + j];
				if (value.IsValid())
				{
					int from = value.From;
					int num4 = value.ToExclusive - value.From;
					if (num4 >= BigSegmentSize)
					{
						BigSegments.AddNoResize(value);
						continue;
					}
					GPUDrivenBatchedDataUploader.CopyDataToStagingBuffer(StagingBufferCPUDataBuffer, SourcePtr, from, num4, num3);
					GPUDrivenBatchedDataUploader.GPUDrivenUploadSegment value2 = GPUDrivenBatchedDataUploader.CreateUploadSegment(num3, from, num4);
					list.Add(in value2);
					num3 += num4;
				}
			}
			if (list.Length > 0)
			{
				CPUSegmentBuffer.AddRangeNoResize(list);
			}
		}
	}

	private const int kBytesPerDispatchGroup = 4096;

	public unsafe static void SetBufferDataSegments<T>(this GPUDrivenBatchedDataUploader dataUploader, CommandBuffer cmd, GraphicsBuffer graphicsBuffer, ref NativeSparseSegmentList.SegmentMergeJobInfo mergeJobInfo, NativeArray<T> cpuData, GPUDrivenUploaderSegmentUnit gpuDrivenUploaderSegmentUnit) where T : unmanaged
	{
		if (mergeJobInfo.IsValid())
		{
			mergeJobInfo.JobHandle.Complete();
		}
		else if (!FrameDebugger.enabled)
		{
			return;
		}
		NativeArray<IndexSegment> nativeArray = (mergeJobInfo.Segments.IsCreated ? default(NativeArray<IndexSegment>) : new NativeArray<IndexSegment>(0, Allocator.TempJob));
		NativeArray<IndexSegment> segments = (nativeArray.IsCreated ? nativeArray : mergeJobInfo.Segments.AsArray());
		int length = segments.Length;
		if (length > 0 && gpuDrivenUploaderSegmentUnit != 0)
		{
			int num = UnsafeUtility.SizeOf<T>();
			ApplyBufferDataSegmentUnitStride job = default(ApplyBufferDataSegmentUnitStride);
			job.UnitStride = num;
			job.CPUDataSize = cpuData.Length * num;
			job.Segments = segments;
			JobUtils.RunJobInPlace(in job, length, 32);
		}
		int requiredByteCapacity = 0;
		int initialCapacity = 0;
		int bigSegmentSize = dataUploader.Mode switch
		{
			GPUUploadMode.Auto => 65536, 
			GPUUploadMode.ForceCompute => int.MaxValue, 
			GPUUploadMode.ForceSetBufferData => 0, 
			_ => throw new ArgumentOutOfRangeException("Mode", dataUploader.Mode, null), 
		};
		if (length > 0)
		{
			ComputeTotalSegmentByteSizeJob job2 = default(ComputeTotalSegmentByteSizeJob);
			job2.Segments = segments;
			job2.TotalSmallSegmentByteSizeCounter = new UnsafeAtomicCounter32(&requiredByteCapacity);
			job2.BigSegmentCounter = new UnsafeAtomicCounter32(&initialCapacity);
			job2.BigSegmentSize = bigSegmentSize;
			JobUtils.RunBatchJobInPlace(in job2, length, 32);
		}
		dataUploader.ResetCurrent();
		NativeList<IndexSegment> nativeList = new NativeList<IndexSegment>(initialCapacity, Allocator.TempJob);
		ref GPUDrivenBatchedDataUploader.StagingBuffer orAllocateCurrentStagingBuffer = ref dataUploader.GetOrAllocateCurrentStagingBuffer(requiredByteCapacity, length);
		if (length > 0)
		{
			fixed (int* ptr = &orAllocateCurrentStagingBuffer.CurrentOffset)
			{
				ScheduleUploadSegmentsJob job3 = default(ScheduleUploadSegmentsJob);
				job3.Segments = segments;
				job3.SourcePtr = (byte*)cpuData.GetUnsafeReadOnlyPtr();
				job3.CPUSegmentBuffer = orAllocateCurrentStagingBuffer.CPUSegmentsBuffer.AsParallelWriter();
				job3.StagingBufferCurrentOffsetAtomic = new UnsafeAtomicCounter32(ptr);
				job3.StagingBufferCPUDataBuffer = orAllocateCurrentStagingBuffer.CPUDataBuffer;
				job3.BigSegments = nativeList.AsParallelWriter();
				job3.BigSegmentSize = bigSegmentSize;
				JobUtils.RunBatchJobInPlace(in job3, length, 128);
			}
		}
		using (new ProfilingScope(cmd, Profiling.UploadBigSegments))
		{
			if (nativeList.Length > 0)
			{
				NativeArray<byte> data = cpuData.Reinterpret<byte>(sizeof(T));
				foreach (IndexSegment item in nativeList)
				{
					int count = item.ToExclusive - item.From;
					cmd.SetBufferData(graphicsBuffer, data, item.From, item.From, count);
				}
			}
			nativeList.Dispose();
		}
		dataUploader.Flush(cmd, graphicsBuffer);
		if (nativeArray.IsCreated)
		{
			nativeArray.Dispose();
		}
	}
}
