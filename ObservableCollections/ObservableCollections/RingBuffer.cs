using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ObservableCollections;

public sealed class RingBuffer<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>
{
	private T[] buffer;

	private int head;

	private int count;

	private int mask;

	public T this[int index]
	{
		get
		{
			int num = (head + index) & mask;
			return buffer[num];
		}
		set
		{
			int num = (head + index) & mask;
			buffer[num] = value;
		}
	}

	public int Count => count;

	public bool IsReadOnly => false;

	public RingBuffer()
	{
		buffer = new T[8];
		head = 0;
		count = 0;
		mask = buffer.Length - 1;
	}

	public RingBuffer(int capacity)
	{
		buffer = new T[CalculateCapacity(capacity)];
		head = 0;
		count = 0;
		mask = buffer.Length - 1;
	}

	public RingBuffer(IEnumerable<T> collection)
	{
		int size;
		T[] array = (collection.TryGetNonEnumeratedCount(out size) ? new T[CalculateCapacity(size)] : new T[8]);
		int num = 0;
		foreach (T item in collection)
		{
			if (num == array.Length)
			{
				Array.Resize(ref array, num * 2);
			}
			array[num++] = item;
		}
		buffer = array;
		head = 0;
		count = num;
		mask = buffer.Length - 1;
	}

	private static int CalculateCapacity(int size)
	{
		size--;
		size |= size >> 1;
		size |= size >> 2;
		size |= size >> 4;
		size |= size >> 8;
		size |= size >> 16;
		size++;
		if (size < 8)
		{
			size = 8;
		}
		return size;
	}

	public void AddLast(T item)
	{
		if (count == buffer.Length)
		{
			EnsureCapacity();
		}
		int num = (head + count) & mask;
		buffer[num] = item;
		count++;
	}

	public void AddFirst(T item)
	{
		if (count == buffer.Length)
		{
			EnsureCapacity();
		}
		head = (head - 1) & mask;
		buffer[head] = item;
		count++;
	}

	public T RemoveLast()
	{
		if (count == 0)
		{
			ThrowForEmpty();
		}
		int num = (head + count - 1) & mask;
		T result = buffer[num];
		buffer[num] = default(T);
		count--;
		return result;
	}

	public T RemoveFirst()
	{
		if (count == 0)
		{
			ThrowForEmpty();
		}
		int num = head & mask;
		T result = buffer[num];
		buffer[num] = default(T);
		head++;
		count--;
		return result;
	}

	private void EnsureCapacity()
	{
		T[] array = new T[buffer.Length * 2];
		int num = head & mask;
		buffer.AsSpan(num).CopyTo(array);
		if (num != 0)
		{
			buffer.AsSpan(0, num).CopyTo(array.AsSpan(buffer.Length - num));
		}
		head = 0;
		buffer = array;
		mask = array.Length - 1;
	}

	void ICollection<T>.Add(T item)
	{
		AddLast(item);
	}

	public void Clear()
	{
		Array.Clear(buffer, 0, buffer.Length);
		head = 0;
		count = 0;
	}

	public RingBufferSpan<T> GetSpan()
	{
		if (count == 0)
		{
			return new RingBufferSpan<T>(Array.Empty<T>(), Array.Empty<T>(), 0);
		}
		int num = head & mask;
		int num2 = (head + count) & mask;
		if (num2 > num)
		{
			Span<T> span = buffer.AsSpan(num, count);
			return new RingBufferSpan<T>(second: Array.Empty<T>().AsSpan(), first: span, count: count);
		}
		Span<T> span2 = buffer.AsSpan(num, buffer.Length - num);
		return new RingBufferSpan<T>(second: buffer.AsSpan(0, num2), first: span2, count: count);
	}

	public IEnumerator<T> GetEnumerator()
	{
		if (count == 0)
		{
			yield break;
		}
		int num = head & mask;
		int end = (head + count) & mask;
		if (end > num)
		{
			for (int i = num; i < end; i++)
			{
				yield return buffer[i];
			}
			yield break;
		}
		for (int i = num; i < buffer.Length; i++)
		{
			yield return buffer[i];
		}
		for (int i = 0; i < end; i++)
		{
			yield return buffer[i];
		}
	}

	public IEnumerable<T> Reverse()
	{
		if (count == 0)
		{
			yield break;
		}
		int start = head & mask;
		int num = (head + count) & mask;
		if (num > start)
		{
			for (int i = num - 1; i >= start; i--)
			{
				yield return buffer[i];
			}
			yield break;
		}
		for (int i = num - 1; i >= 0; i--)
		{
			yield return buffer[i];
		}
		for (int i = buffer.Length - 1; i >= start; i--)
		{
			yield return buffer[i];
		}
	}

	public bool Contains(T item)
	{
		return IndexOf(item) != -1;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		RingBufferSpan<T> span = GetSpan();
		Span<T> destination = array.AsSpan(arrayIndex);
		span.First.CopyTo(destination);
		span.Second.CopyTo(destination.Slice(span.First.Length));
	}

	public int IndexOf(T item)
	{
		int num = 0;
		RingBufferSpan<T>.Enumerator enumerator = GetSpan().GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			if (EqualityComparer<T>.Default.Equals(item, current))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public T[] ToArray()
	{
		T[] array = new T[count];
		int num = 0;
		RingBufferSpan<T>.Enumerator enumerator = GetSpan().GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			array[num++] = current;
		}
		return array;
	}

	public int BinarySearch(T item)
	{
		return BinarySearch(item, Comparer<T>.Default);
	}

	public int BinarySearch(T item, IComparer<T> comparer)
	{
		int num = 0;
		int num2 = count - 1;
		while (num <= num2)
		{
			int num3 = num2 + num >>> 1;
			int num4 = comparer.Compare(this[num3], item);
			if (num4 == 0)
			{
				return num3;
			}
			if (num4 < 0)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		return ~num;
	}

	void IList<T>.Insert(int index, T item)
	{
		throw new NotSupportedException();
	}

	bool ICollection<T>.Remove(T item)
	{
		throw new NotSupportedException();
	}

	void IList<T>.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<T>)this).GetEnumerator();
	}

	[DoesNotReturn]
	private static void ThrowForEmpty()
	{
		throw new InvalidOperationException("RingBuffer is empty.");
	}
}
