using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ObservableCollections.Internal;

internal struct CloneCollection<T> : IDisposable
{
	private class EnumerableCollection : ICollection<T>, IEnumerable<T>, IEnumerable
	{
		private readonly T[] array;

		private readonly int count;

		public int Count => count;

		public bool IsReadOnly => true;

		public EnumerableCollection(T[]? array, int count)
		{
			if (array == null)
			{
				this.array = Array.Empty<T>();
				this.count = 0;
			}
			else
			{
				this.array = array;
				this.count = count;
			}
		}

		public void Add(T item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(T item)
		{
			throw new NotSupportedException();
		}

		public void CopyTo(T[] dest, int destIndex)
		{
			Array.Copy(array, 0, dest, destIndex, count);
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < count; i++)
			{
				yield return array[i];
			}
		}

		public bool Remove(T item)
		{
			throw new NotSupportedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private T[]? array;

	private int length;

	public ReadOnlySpan<T> Span => array.AsSpan(0, length);

	public IEnumerable<T> AsEnumerable()
	{
		return new EnumerableCollection(array, length);
	}

	public CloneCollection(T item)
	{
		array = ArrayPool<T>.Shared.Rent(1);
		length = 1;
		array[0] = item;
	}

	public CloneCollection(IEnumerable<T> source)
	{
		if (source.TryGetNonEnumeratedCount(out var count))
		{
			T[] array = ArrayPool<T>.Shared.Rent(count);
			if (source is ICollection<T> collection)
			{
				collection.CopyTo(array, 0);
			}
			else
			{
				int num = 0;
				foreach (T item in source)
				{
					array[num++] = item;
				}
			}
			this.array = array;
			length = count;
			return;
		}
		T[] array2 = ArrayPool<T>.Shared.Rent(16);
		int index = 0;
		foreach (T item2 in source)
		{
			TryEnsureCapacity(ref array2, index);
			array2[index++] = item2;
		}
		this.array = array2;
		length = index;
	}

	public CloneCollection(ReadOnlySpan<T> source)
	{
		T[] array = ArrayPool<T>.Shared.Rent(source.Length);
		source.CopyTo(array);
		this.array = array;
		length = source.Length;
	}

	private static void TryEnsureCapacity(ref T[] array, int index)
	{
		if (array.Length == index)
		{
			T[] array2 = ArrayPool<T>.Shared.Rent(index * 2);
			Array.Copy(array, array2, index);
			ArrayPool<T>.Shared.Return(array, RuntimeHelpersEx.IsReferenceOrContainsReferences<T>());
			array = array2;
		}
	}

	public void Dispose()
	{
		if (array != null)
		{
			ArrayPool<T>.Shared.Return(array, RuntimeHelpersEx.IsReferenceOrContainsReferences<T>());
			array = null;
		}
	}
}
