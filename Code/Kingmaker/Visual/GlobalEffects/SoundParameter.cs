using System;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Visual.Overrides;
using UnityEngine.Rendering;

namespace Kingmaker.Visual.GlobalEffects;

[Serializable]
public class SoundParameter : CompositeParameter
{
	[AkEventReference]
	public string AkSoundEvent;

	public ClampedFloatParameter GlobalEffectGainRTPC = new ClampedFloatParameter(0f, 0f, 100f);

	public ClampedFloatParameter GlobalEffectRTPC = new ClampedFloatParameter(0f, 0f, 100f);

	public override object Clone()
	{
		return new SoundParameter
		{
			AkSoundEvent = AkSoundEvent,
			GlobalEffectGainRTPC = new ClampedFloatParameter(GlobalEffectGainRTPC.value, GlobalEffectGainRTPC.min, GlobalEffectGainRTPC.max),
			GlobalEffectRTPC = new ClampedFloatParameter(GlobalEffectRTPC.value, GlobalEffectRTPC.min, GlobalEffectRTPC.max)
		};
	}

	public override int GetContentHash()
	{
		return AkSoundEvent.GetHashCode();
	}

	public override void Interp(CompositeParameter to, float t)
	{
		SoundParameter soundParameter = to as SoundParameter;
		GlobalEffectGainRTPC.Interp(GlobalEffectGainRTPC.value, soundParameter.GlobalEffectGainRTPC.value, t);
		GlobalEffectRTPC.Interp(GlobalEffectRTPC.value, soundParameter.GlobalEffectRTPC.value, t);
	}

	public override void ResetToDefault()
	{
		GlobalEffectGainRTPC.value = 0f;
		GlobalEffectRTPC.value = 0f;
	}
}
