using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OwlPack.Runtime;

public class ReferenceResolver
{
	private class ByRefComparer : IEqualityComparer<object>
	{
		public new bool Equals(object x, object y)
		{
			return x == y;
		}

		public int GetHashCode(object obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	private Dictionary<object, uint> m_ObjectToId = new Dictionary<object, uint>(10000, new ByRefComparer());

	private Dictionary<uint, object> m_IdToObject = new Dictionary<uint, object>(10000);

	private uint m_LastId;

	public (uint id, bool isRef) GetOrRegister(object obj)
	{
		if (m_ObjectToId.TryGetValue(obj, out var value))
		{
			return (id: value, isRef: true);
		}
		value = m_LastId++;
		m_ObjectToId.Add(obj, value);
		m_IdToObject.Add(value, obj);
		return (id: value, isRef: false);
	}

	public void Register(uint id, object obj)
	{
		if (m_IdToObject.TryGetValue(id, out var _))
		{
			throw new InvalidOperationException($"Trying to register object of type {obj.GetType().FullName} with duplicate id {id}");
		}
		m_IdToObject.Add(id, obj);
	}

	public T Resolve<T>(uint id)
	{
		return (T)m_IdToObject[id];
	}
}
