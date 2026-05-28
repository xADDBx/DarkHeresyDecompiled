using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Owlcat.Editor.Visual.UserData;

public class SharedUserData
{
	private readonly List<object> m_UserData;

	public SharedUserData(string userData)
	{
		m_UserData = JsonConvert.DeserializeObject<object[]>(userData)?.ToList() ?? new List<object>();
		for (int num = m_UserData.Count - 1; num >= 0; num--)
		{
			if (m_UserData[num] == null)
			{
				m_UserData.RemoveAt(num);
			}
		}
	}

	public bool IsEmpty()
	{
		return m_UserData != null;
	}

	public bool TryGet<T>(out T value) where T : class, IUserData, new()
	{
		string key = new T().Key;
		for (int num = m_UserData.Count - 1; num >= 0; num--)
		{
			object obj = m_UserData[num];
			if (obj is T val)
			{
				value = val;
				return true;
			}
			if (obj is JObject jObject && jObject.TryGetValue("Key", out JToken value2) && value2.ToString() == key)
			{
				T val2 = jObject.ToObject<T>();
				if (val2 != null)
				{
					m_UserData[num] = val2;
					value = val2;
					return true;
				}
				m_UserData.RemoveAt(num);
				value = null;
				return false;
			}
		}
		value = null;
		return false;
	}

	public T GetOrAdd<T>() where T : class, IUserData, new()
	{
		if (!TryGet<T>(out var value))
		{
			value = new T();
			m_UserData.Add(value);
			return value;
		}
		return value;
	}

	public void Remove<T>() where T : IUserData
	{
		for (int num = m_UserData.Count - 1; num >= 0; num--)
		{
			if (m_UserData[num] is T)
			{
				m_UserData.RemoveAt(num);
			}
		}
	}

	public override string ToString()
	{
		using StringWriter stringWriter = new StringWriter();
		new JsonSerializer().Serialize(stringWriter, m_UserData.ToArray());
		return stringWriter.ToString();
	}
}
