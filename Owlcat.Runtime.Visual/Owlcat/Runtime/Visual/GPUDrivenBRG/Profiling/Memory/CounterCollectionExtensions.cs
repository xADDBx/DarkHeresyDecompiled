using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;

public static class CounterCollectionExtensions
{
	public static void CollectBufferSize<T>(this Counters.CounterCollection _, ProfilerCounterValue<int> counter, NativeArray<T> buffer) where T : unmanaged
	{
		if (buffer.IsCreated)
		{
			counter.Value += buffer.Length * UnsafeUtility.SizeOf<T>();
		}
	}

	public static void CollectBufferSize<T>(this Counters.CounterCollection _, ProfilerCounterValue<int> counter, NativeList<T> buffer) where T : unmanaged
	{
		if (buffer.IsCreated)
		{
			counter.Value += buffer.Capacity * UnsafeUtility.SizeOf<T>();
		}
	}

	public static void CollectBufferSize<T>(this Counters.CounterCollection _, ProfilerCounterValue<int> counter, UnsafeList<T> buffer) where T : unmanaged
	{
		if (buffer.IsCreated)
		{
			counter.Value += buffer.Capacity * UnsafeUtility.SizeOf<T>();
		}
	}

	public static void CollectBufferSize<T>(this Counters.CounterCollection _, ProfilerCounterValue<int> counter, List<T> buffer) where T : unmanaged
	{
		if (buffer != null)
		{
			counter.Value += buffer.Capacity * UnsafeUtility.SizeOf<T>();
		}
	}

	public static void CollectBufferSize<TKey, TValue>(this Counters.CounterCollection _, ProfilerCounterValue<int> counter, NativeHashMap<TKey, TValue> buffer) where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
	{
		if (buffer.IsCreated)
		{
			counter.Value += buffer.Capacity * (UnsafeUtility.SizeOf<TKey>() + UnsafeUtility.SizeOf<TValue>());
		}
	}

	public static void CollectBufferSize<TKey, TValue>(this Counters.CounterCollection _, ProfilerCounterValue<int> counter, Dictionary<TKey, List<TValue>> buffer) where TKey : class where TValue : struct
	{
		counter.Value += buffer.Count * UnsafeUtility.SizeOf<IntPtr>();
		foreach (List<TValue> value in buffer.Values)
		{
			counter.Value += value.Capacity * UnsafeUtility.SizeOf<TValue>();
		}
	}

	public static void CollectBufferSize<TKey, TValue>(this Counters.CounterCollection _, ProfilerCounterValue<int> counter, Dictionary<TKey, TValue> buffer) where TKey : struct where TValue : struct
	{
		counter.Value += buffer.Count * (UnsafeUtility.SizeOf<TKey>() + UnsafeUtility.SizeOf<TValue>());
	}

	public static void CollectBufferSize(this Counters.CounterCollection _, ProfilerCounterValue<int> counter, ResizableGraphicsBuffer buffer)
	{
		if (buffer.IsCreated)
		{
			counter.Value += buffer.Count * buffer.Stride;
		}
	}
}
