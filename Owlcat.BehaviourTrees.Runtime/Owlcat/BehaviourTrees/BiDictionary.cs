using System.Collections.Generic;

namespace Owlcat.BehaviourTrees;

public class BiDictionary<TKey, TValue>
{
	private readonly Dictionary<TKey, TValue> m_Forward = new Dictionary<TKey, TValue>();

	private readonly Dictionary<TValue, TKey> m_Backward = new Dictionary<TValue, TKey>();

	public IEnumerable<TKey> Keys => m_Forward.Keys;

	public IEnumerable<TValue> Values => m_Backward.Keys;

	public int Count => m_Forward.Count;

	public void Add(TKey key, TValue value)
	{
		m_Forward.Add(key, value);
		m_Backward.Add(value, key);
	}

	public TValue GetValue(TKey key)
	{
		return m_Forward[key];
	}

	public TKey GetKey(TValue value)
	{
		return m_Backward[value];
	}

	public bool ContainsKey(TKey key)
	{
		return m_Forward.ContainsKey(key);
	}

	public bool ContainsValue(TValue value)
	{
		return m_Backward.ContainsKey(value);
	}

	public bool TryGetKey(TValue value, out TKey key)
	{
		return m_Backward.TryGetValue(value, out key);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		return m_Forward.TryGetValue(key, out value);
	}

	public void RemoveKey(TKey key)
	{
		TValue key2 = m_Forward[key];
		m_Forward.Remove(key);
		m_Backward.Remove(key2);
	}

	public void RemoveValue(TValue value)
	{
		TKey key = m_Backward[value];
		m_Backward.Remove(value);
		m_Forward.Remove(key);
	}

	public void Clear()
	{
		m_Forward.Clear();
		m_Backward.Clear();
	}
}
