using System.Collections.Generic;

namespace OwlPack.Runtime;

internal class WriterPool<T> where T : IMemoryBufferWriter, new()
{
	private class Comparer : IComparer<T>
	{
		public int Compare(T x, T y)
		{
			return x.Capacity - y.Capacity;
		}
	}

	private SortedSet<T> m_Pool = new SortedSet<T>(new Comparer());

	public T Get()
	{
		if (m_Pool.Count == 0)
		{
			return new T();
		}
		T max = m_Pool.Max;
		m_Pool.Remove(max);
		max.Reset();
		return max;
	}

	public void Return(T t)
	{
		m_Pool.Add(t);
	}
}
