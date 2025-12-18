using System;
using System.Collections;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Utility.DotNetExtensions;

namespace Owlcat.Fmw.Blueprints;

public readonly struct BpRefArray<T> : IEnumerable<T>, IEnumerable where T : SimpleBlueprint
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly BpRef<T>[]? m_Array;

		private int m_Index;

		public T Current
		{
			get
			{
				BpRef<T>[]? array = m_Array;
				if (array == null)
				{
					return null;
				}
				BpRef<T> bpRef = array.Get(m_Index);
				if ((object)bpRef == null)
				{
					return null;
				}
				return bpRef.MaybeBlueprint;
			}
		}

		object IEnumerator.Current => Current;

		public Enumerator(BpRef<T>[]? array)
		{
			m_Array = array;
			m_Index = -1;
		}

		public bool MoveNext()
		{
			if (m_Array != null)
			{
				return ++m_Index < m_Array.Length;
			}
			return false;
		}

		public void Reset()
		{
			m_Index = -1;
		}

		public void Dispose()
		{
		}
	}

	private readonly BpRef<T>[]? m_Array;

	public T? this[int i]
	{
		get
		{
			BpRef<T>[]? array = m_Array;
			if (array == null)
			{
				return null;
			}
			return array[i].MaybeBlueprint;
		}
	}

	public int Length
	{
		get
		{
			BpRef<T>[]? array = m_Array;
			if (array == null)
			{
				return 0;
			}
			return array.Length;
		}
	}

	public BpRefArray(BpRef<T>[]? array)
	{
		m_Array = array;
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		if (m_Array == null)
		{
			yield break;
		}
		BpRef<T>[] array = m_Array;
		for (int i = 0; i < array.Length; i++)
		{
			T maybeBlueprint = array[i].MaybeBlueprint;
			if (maybeBlueprint != null)
			{
				yield return maybeBlueprint;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (m_Array != null)
		{
			BpRef<T>[] array = m_Array;
			foreach (BpRef<T> bpRef in array)
			{
				yield return bpRef.MaybeBlueprint;
			}
		}
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(m_Array);
	}

	public bool Contains(SimpleBlueprint bp)
	{
		SimpleBlueprint bp = bp;
		return m_Array.Contains((BpRef<T> r) => r.Is(bp));
	}

	public bool Contains(BpRef<T> r1)
	{
		BpRef<T> r1 = r1;
		return m_Array.Contains((BpRef<T> r) => r.Guid == r1.Guid);
	}

	public int IndexOf(T bp)
	{
		if (m_Array == null)
		{
			return -1;
		}
		for (int i = 0; i < m_Array.Length; i++)
		{
			if (m_Array[i].Is(bp))
			{
				return i;
			}
		}
		return -1;
	}

	public static implicit operator BpRef<T>[]?(BpRefArray<T> proxy)
	{
		return proxy.m_Array;
	}

	public static implicit operator BpRefArray<T>(BpRef<T>[] array)
	{
		return new BpRefArray<T>(array);
	}
}
