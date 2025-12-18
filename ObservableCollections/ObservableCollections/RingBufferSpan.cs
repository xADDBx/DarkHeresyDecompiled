using System;

namespace ObservableCollections;

public ref struct RingBufferSpan<T>
{
	public ref struct Enumerator
	{
		private ReadOnlySpan<T>.Enumerator firstEnumerator;

		private ReadOnlySpan<T>.Enumerator secondEnumerator;

		private bool useFirst;

		public T Current
		{
			get
			{
				if (useFirst)
				{
					return firstEnumerator.Current;
				}
				return secondEnumerator.Current;
			}
		}

		public Enumerator(RingBufferSpan<T> span)
		{
			firstEnumerator = span.First.GetEnumerator();
			secondEnumerator = span.Second.GetEnumerator();
			useFirst = true;
		}

		public bool MoveNext()
		{
			if (useFirst)
			{
				if (firstEnumerator.MoveNext())
				{
					return true;
				}
				useFirst = false;
			}
			if (secondEnumerator.MoveNext())
			{
				return true;
			}
			return false;
		}
	}

	public readonly ReadOnlySpan<T> First;

	public readonly ReadOnlySpan<T> Second;

	public readonly int Count;

	internal RingBufferSpan(ReadOnlySpan<T> first, ReadOnlySpan<T> second, int count)
	{
		First = first;
		Second = second;
		Count = count;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}
}
