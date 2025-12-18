using UnityEngine;

namespace Owlcat.Runtime.Visual.FogOfWar;

public class FogOfWarSettingsOverride
{
	public Color Color;

	public float RevealerOuterRadius;

	public FogOfWarSettingsOverride(FogOfWarSettings settings)
	{
		Color = settings.Color;
		RevealerOuterRadius = settings.RevealerOutterRadius;
	}
}
