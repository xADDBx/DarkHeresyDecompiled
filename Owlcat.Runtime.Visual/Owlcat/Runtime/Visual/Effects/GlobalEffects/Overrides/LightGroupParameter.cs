using System;
using Owlcat.Runtime.Visual.Effects.GlobalEffects.Lighting;
using Owlcat.Runtime.Visual.Overrides;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.Overrides;

[Serializable]
public class LightGroupParameter : CompositeParameter
{
	[LightGroupType(LightGroupTypeAttribute.DrawMode.Mask)]
	public int LightGroupMask;

	public ColorParameter Color = new ColorParameter(new Color(1f, 1f, 1f, 1f));

	public MinFloatParameter Intensity = new MinFloatParameter(10f, 0f);

	public override object Clone()
	{
		LightGroupParameter obj = new LightGroupParameter
		{
			LightGroupMask = LightGroupMask
		};
		obj.Color = new ColorParameter(obj.Color.value);
		obj.Intensity = new MinFloatParameter(Intensity.value, Intensity.min);
		return obj;
	}

	public override int GetContentHash()
	{
		return LightGroupMask.GetHashCode();
	}

	public override void Interp(CompositeParameter to, float t)
	{
		LightGroupParameter arg = to as LightGroupParameter;
		LightGroup.Interp?.Invoke(arg, t);
	}

	public override void ResetToDefault()
	{
		LightGroup.ResetToDefault?.Invoke(LightGroupMask);
	}
}
