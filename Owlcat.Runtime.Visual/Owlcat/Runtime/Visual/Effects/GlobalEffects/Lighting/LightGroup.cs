using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Effects.GlobalEffects.Overrides;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.Lighting;

public class LightGroup : MonoBehaviour
{
	private struct LightProperties
	{
		public Color Color;

		public float Intensity;
	}

	private Dictionary<Light, LightProperties> m_DefaultProperties;

	public static Action<int> ResetToDefault;

	public static Action<LightGroupParameter, float> Interp;

	[LightGroupType(LightGroupTypeAttribute.DrawMode.Flag)]
	public int Type;

	public List<Light> Lights;

	private void OnEnable()
	{
		ResetToDefault = (Action<int>)Delegate.Combine(ResetToDefault, new Action<int>(OnResetToDefault));
		Interp = (Action<LightGroupParameter, float>)Delegate.Combine(Interp, new Action<LightGroupParameter, float>(OnInterp));
		SaveDefaults();
	}

	private void SaveDefaults()
	{
		if (m_DefaultProperties != null)
		{
			return;
		}
		m_DefaultProperties = new Dictionary<Light, LightProperties>();
		foreach (Light light in Lights)
		{
			m_DefaultProperties.Add(light, new LightProperties
			{
				Color = light.color,
				Intensity = light.intensity
			});
		}
	}

	private void OnDisable()
	{
		ResetToDefault = (Action<int>)Delegate.Remove(ResetToDefault, new Action<int>(OnResetToDefault));
		Interp = (Action<LightGroupParameter, float>)Delegate.Remove(Interp, new Action<LightGroupParameter, float>(OnInterp));
		OnResetToDefault(1 << Type);
	}

	private void OnResetToDefault(int typeMask)
	{
		if (!DoesMaskContainsType(typeMask))
		{
			return;
		}
		foreach (Light light in Lights)
		{
			if (light != null)
			{
				ResetLight(light);
			}
		}
	}

	private bool DoesMaskContainsType(int typeMask)
	{
		return (typeMask & (1 << Type)) != 0;
	}

	private void ResetLight(Light light)
	{
		LightProperties lightProperties = m_DefaultProperties[light];
		light.color = lightProperties.Color;
		light.intensity = lightProperties.Intensity;
	}

	private void OnInterp(LightGroupParameter o, float t)
	{
		if (!DoesMaskContainsType(o.LightGroupMask))
		{
			return;
		}
		foreach (Light light in Lights)
		{
			InterpolateLight(light, o, t);
		}
	}

	private void InterpolateLight(Light light, LightGroupParameter o, float t)
	{
		if (o.Color.overrideState)
		{
			light.color = Color.Lerp(light.color, o.Color.value, t);
		}
		if (o.Intensity.overrideState)
		{
			light.intensity = math.lerp(light.intensity, o.Intensity.value, t);
		}
	}
}
