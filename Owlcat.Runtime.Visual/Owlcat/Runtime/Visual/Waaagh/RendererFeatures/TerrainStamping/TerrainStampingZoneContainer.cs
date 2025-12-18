using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping;

internal static class TerrainStampingZoneContainer
{
	private static readonly List<OwlcatTerrainStampingZone> s_Zones = new List<OwlcatTerrainStampingZone>();

	public static event Action Changed;

	public static void Get(List<OwlcatTerrainStampingZone> zones)
	{
		List<int> value;
		using (ListPool<int>.Get(out value))
		{
			for (int i = 0; i < s_Zones.Count; i++)
			{
				OwlcatTerrainStampingZone owlcatTerrainStampingZone = s_Zones[i];
				if (owlcatTerrainStampingZone != null)
				{
					zones.Add(owlcatTerrainStampingZone);
				}
				else
				{
					value.Add(i);
				}
			}
			for (int num = value.Count - 1; num >= 0; num--)
			{
				s_Zones.RemoveAt(value[num]);
			}
		}
	}

	public static void Add(OwlcatTerrainStampingZone zone)
	{
		s_Zones.Add(zone);
		TerrainStampingZoneContainer.Changed?.Invoke();
	}

	public static void Remove(OwlcatTerrainStampingZone zone)
	{
		s_Zones.Remove(zone);
		TerrainStampingZoneContainer.Changed?.Invoke();
	}
}
