using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Owlcat.Runtime.Core.Collections;

public struct CustomParallelSortJob<T, TComparer> where T : unmanaged where TComparer : struct, IComparer<T>
{
	[BurstCompile(CompileSynchronously = true)]
	private struct SortPartJob : IJobParallelForBatch
	{
		public NativeArray<T> Src;

		public TComparer Cmp;

		public void Execute(int startIndex, int count)
		{
			Src.Slice(startIndex, count).Sort(Cmp);
		}
	}

	[BurstCompile(CompileSynchronously = true)]
	private struct MergePartJob : IJob
	{
		[ReadOnly]
		public NativeArray<T> Src;

		[WriteOnly]
		public NativeArray<T> Dst;

		public TComparer Cmp;

		public int Left;

		public int Mid;

		public int End;

		internal static void Merge<T1, TComparer1>(in NativeArray<T1> a, NativeArray<T1> b, TComparer1 cmp, int left, int mid, int end) where T1 : struct where TComparer1 : struct, IComparer<T1>
		{
			int num = left;
			int num2 = mid;
			int i;
			for (i = left; i < end; i++)
			{
				if (num >= mid)
				{
					break;
				}
				if (num2 >= end)
				{
					break;
				}
				if (cmp.Compare(a[num], a[num2]) <= 0)
				{
					b[i] = a[num++];
				}
				else
				{
					b[i] = a[num2++];
				}
			}
			if (num < mid)
			{
				NativeSlice<T1> slice = new NativeSlice<T1>(a, num, mid - num);
				new NativeSlice<T1>(b, i, end - i).CopyFrom(slice);
			}
			else if (num2 < end)
			{
				NativeSlice<T1> slice2 = new NativeSlice<T1>(a, num2, end - num2);
				new NativeSlice<T1>(b, i, end - i).CopyFrom(slice2);
			}
		}

		public void Execute()
		{
			Merge(in Src, Dst, Cmp, Left, Mid, End);
		}
	}

	[BurstCompile(CompileSynchronously = true)]
	private struct MergeSortJob : IJobParallelForBatch
	{
		[ReadOnly]
		public NativeArray<T> Src;

		[WriteOnly]
		public NativeArray<T> Dst;

		public TComparer Cmp;

		public void Execute(int startIndex, int count)
		{
			int num = startIndex + count;
			int mid = (startIndex + num) / 2;
			MergePartJob.Merge(in Src, Dst, Cmp, startIndex, mid, num);
		}
	}

	[BurstCompile(CompileSynchronously = true)]
	private struct CopyJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeSlice<T> Src;

		[WriteOnly]
		public NativeSlice<T> Dst;

		public void Execute(int i)
		{
			Dst[i] = Src[i];
		}
	}

	public NativeArray<T> Array;

	public TComparer Comparer;

	public JobHandle Schedule(JobHandle inputDeps = default(JobHandle))
	{
		int num = Mathf.ClosestPowerOfTwo((int)Mathf.Log(Array.Length, 2f));
		NativeArray<T> nativeArray = new NativeArray<T>(Array.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		SortPartJob jobData = default(SortPartJob);
		jobData.Src = Array;
		jobData.Cmp = Comparer;
		JobHandle jobHandle = jobData.ScheduleBatch(Array.Length, Array.Length / num, inputDeps);
		NativeArray<T> a2 = nativeArray;
		NativeArray<T> b2 = Array;
		int num2 = Array.Length / num * num;
		for (int num3 = num / 2; num3 > 0; num3 /= 2)
		{
			Swap(ref a2, ref b2);
			MergeSortJob jobData2 = default(MergeSortJob);
			jobData2.Src = a2;
			jobData2.Dst = b2;
			jobData2.Cmp = Comparer;
			jobHandle = jobData2.ScheduleBatch(num2, num2 / num3, jobHandle);
		}
		if (num2 < Array.Length)
		{
			Swap(ref a2, ref b2);
			if (a2 == nativeArray)
			{
				CopyJob jobData3 = default(CopyJob);
				jobData3.Src = Array.Slice(num2);
				jobData3.Dst = nativeArray.Slice(num2);
				jobHandle = IJobParallelForExtensions.Schedule(jobData3, Array.Length - num2, 64, jobHandle);
			}
			MergePartJob jobData4 = default(MergePartJob);
			jobData4.Src = a2;
			jobData4.Dst = b2;
			jobData4.Cmp = Comparer;
			jobData4.Left = 0;
			jobData4.Mid = num2;
			jobData4.End = Array.Length;
			jobHandle = jobData4.Schedule(jobHandle);
		}
		if (b2 == nativeArray)
		{
			CopyJob jobData3 = default(CopyJob);
			jobData3.Src = nativeArray;
			jobData3.Dst = Array;
			jobHandle = IJobParallelForExtensions.Schedule(jobData3, Array.Length, 64, jobHandle);
		}
		nativeArray.Dispose(jobHandle);
		return jobHandle;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void Swap(ref NativeArray<T> a, ref NativeArray<T> b)
		{
			NativeArray<T> nativeArray2 = b;
			NativeArray<T> nativeArray3 = a;
			a = nativeArray2;
			b = nativeArray3;
		}
	}
}
