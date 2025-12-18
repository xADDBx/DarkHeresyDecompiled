using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Utilities;

public static class TestTime
{
	public struct Override : IDisposable
	{
		private object m_Key;

		public Override([NotNull] object key, float time)
		{
			m_Key = key ?? throw new ArgumentNullException("key");
			AddOverride(key, time);
		}

		public void Dispose()
		{
			if (s_Overrides != null)
			{
				RemoveOverride(m_Key);
				m_Key = null;
			}
		}
	}

	private struct TimeOverride
	{
		public object Key;

		public float Time;
	}

	private static readonly List<TimeOverride> s_Overrides = new List<TimeOverride>();

	public static float Now
	{
		get
		{
			if (s_Overrides.Count <= 0)
			{
				return Time.time;
			}
			List<TimeOverride> list = s_Overrides;
			return list[list.Count - 1].Time;
		}
	}

	private static void AddOverride([NotNull] object key, float time)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		for (int i = 0; i < s_Overrides.Count; i++)
		{
			TimeOverride value = s_Overrides[i];
			if (value.Key == key)
			{
				value.Time = time;
				s_Overrides[i] = value;
				return;
			}
		}
		s_Overrides.Add(new TimeOverride
		{
			Key = key,
			Time = time
		});
	}

	private static void RemoveOverride([NotNull] object key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		for (int i = 0; i < s_Overrides.Count; i++)
		{
			if (s_Overrides[i].Key == key)
			{
				s_Overrides.RemoveAt(i);
				break;
			}
		}
	}
}
