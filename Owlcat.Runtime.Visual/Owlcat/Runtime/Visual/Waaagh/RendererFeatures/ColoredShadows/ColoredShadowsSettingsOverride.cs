using System.Collections.Generic;
using JetBrains.Annotations;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ColoredShadows;

public static class ColoredShadowsSettingsOverride
{
	private struct Override
	{
		public object Key;

		public ColoredShadowsSettings Settings;
	}

	private static readonly List<Override> s_Overrides = new List<Override>();

	public static ColoredShadowsSettings Resolve(ColoredShadowsSettings fallback)
	{
		if (s_Overrides.Count <= 0)
		{
			return fallback;
		}
		List<Override> list = s_Overrides;
		return list[list.Count - 1].Settings;
	}

	public static void Add([NotNull] object key, [NotNull] ColoredShadowsSettings settings)
	{
		for (int i = 0; i < s_Overrides.Count; i++)
		{
			Override value = s_Overrides[i];
			if (value.Key == key)
			{
				value.Settings = settings;
				s_Overrides[i] = value;
				return;
			}
		}
		s_Overrides.Add(new Override
		{
			Key = key,
			Settings = settings
		});
	}

	public static bool Remove([NotNull] object key)
	{
		for (int i = 0; i < s_Overrides.Count; i++)
		{
			if (s_Overrides[i].Key == key)
			{
				s_Overrides.RemoveAt(i);
				break;
			}
		}
		return false;
	}
}
