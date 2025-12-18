using System;
using System.Collections;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Utility.DotNetExtensions;

namespace Owlcat.Fmw.Blueprints;

public class BpRefList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable where T : SimpleBlueprint
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly IList<BpRef<T>?>? m_List;

		private int m_Index;

		public T Current
		{
			get
			{
				IList<BpRef<T>?>? list = m_List;
				if (list == null)
				{
					return null;
				}
				BpRef<T>? bpRef = list.Get(m_Index);
				if ((object)bpRef == null)
				{
					return null;
				}
				return bpRef.MaybeBlueprint;
			}
		}

		object IEnumerator.Current => Current;

		public Enumerator(IList<BpRef<T>?>? list)
		{
			m_List = list;
			m_Index = -1;
		}

		public bool MoveNext()
		{
			if (m_List != null)
			{
				return ++m_Index < m_List.Count;
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

	private readonly List<BpRef<T>?>? m_List;

	public int Count => m_List?.Count ?? 0;

	public bool IsReadOnly => false;

	public T this[int index]
	{
		get
		{
			List<BpRef<T>?>? list = m_List;
			if (list == null)
			{
				return null;
			}
			BpRef<T>? bpRef = list[index];
			if ((object)bpRef == null)
			{
				return null;
			}
			return bpRef.MaybeBlueprint;
		}
		set
		{
			if (m_List != null)
			{
				m_List[index] = ((value != null) ? new BpRef<T>(value) : null);
			}
		}
	}

	public BpRefList(List<BpRef<T>?>? list)
	{
		m_List = list;
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		if (m_List == null)
		{
			yield break;
		}
		for (int i = 0; i < m_List.Count; i++)
		{
			BpRef<T>? bpRef = m_List[i];
			T val = (((object)bpRef != null) ? bpRef.MaybeBlueprint : null);
			if (val != null)
			{
				yield return val;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (m_List == null)
		{
			yield break;
		}
		for (int i = 0; i < m_List.Count; i++)
		{
			BpRef<T>? bpRef = m_List[i];
			T val = (((object)bpRef != null) ? bpRef.MaybeBlueprint : null);
			if (val != null)
			{
				yield return val;
			}
		}
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(m_List);
	}

	public void Add(T item)
	{
		m_List?.Add(new BpRef<T>(item));
	}

	public void Clear()
	{
		m_List?.Clear();
	}

	public bool Contains(T item)
	{
		T item = item;
		return m_List.Contains((BpRef<T> r) => r != null && r.Is(item));
	}

	public void CopyTo(T?[] array, int arrayIndex)
	{
		if (m_List != null)
		{
			for (int i = 0; i < m_List.Count; i++)
			{
				int num = arrayIndex + i;
				BpRef<T>? bpRef = m_List[i];
				array[num] = (((object)bpRef != null) ? bpRef.MaybeBlueprint : null);
			}
		}
	}

	public bool Remove(T item)
	{
		T item = item;
		List<BpRef<T>?>? list = m_List;
		if (list == null)
		{
			return false;
		}
		return list.RemoveAll((BpRef<T>? r) => r != null && r.Is(item)) > 0;
	}

	public int IndexOf(T item)
	{
		T item = item;
		return m_List?.FindIndex((BpRef<T>? r) => r != null && r.Is(item)) ?? (-1);
	}

	public void Insert(int index, T item)
	{
		m_List?.Insert(index, new BpRef<T>(item));
	}

	public void RemoveAt(int index)
	{
		m_List?.RemoveAt(index);
	}

	public void RemoveAll(Predicate<T> pred)
	{
		Predicate<T> pred = pred;
		m_List?.RemoveAll(delegate(BpRef<T>? r)
		{
			T val = (((object)r != null) ? r.MaybeBlueprint : null);
			return val != null && pred(val);
		});
	}

	public static implicit operator List<BpRef<T>?>?(BpRefList<T> proxy)
	{
		return proxy.m_List;
	}

	public static implicit operator BpRefList<T>(List<BpRef<T>?>? list)
	{
		return new BpRefList<T>(list);
	}
}
