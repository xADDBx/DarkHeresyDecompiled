using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Utilities;

internal static class ProfilingSamplerStorage<TProfileId> where TProfileId : struct, Enum
{
	private static readonly Dictionary<int, ProfilingSampler> m_DefaultProfilingSamplers;

	private static readonly Dictionary<TProfileId, string> m_NameOverrides;

	static ProfilingSamplerStorage()
	{
		m_DefaultProfilingSamplers = new Dictionary<int, ProfilingSampler>();
		m_NameOverrides = new Dictionary<TProfileId, string>();
	}

	private static ProfilingSampler GetDefaultProfilingSampler(string name)
	{
		return null;
	}

	public static ProfilingSampler Get(string name, TProfileId? profileId)
	{
		if (!profileId.HasValue)
		{
			return GetDefaultProfilingSampler(name);
		}
		if (m_NameOverrides.TryGetValue(profileId.Value, out var value))
		{
			return GetDefaultProfilingSampler(value);
		}
		return ProfilingSampler.Get(profileId.Value);
	}
}
