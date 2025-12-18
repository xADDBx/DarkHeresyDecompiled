using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.Overrides;

[VolumeComponentMenu("Global Effects/Wind")]
public class WindOverride : VolumeComponent
{
	public FloatParameter Intensity = new FloatParameter(0f);
}
