using Owlcat.Runtime.Core.WindSystem;
using Owlcat.Runtime.Visual.Effects.GlobalEffects.Components;
using Owlcat.Runtime.Visual.Effects.GlobalEffects.Overrides;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.Controllers;

public class WindController : OverridableControllerBase<WindComponent, WindOverride>
{
	private GameObject m_WindGO;

	private AmbientWind m_AmbientWind;

	public WindController(WindComponent component)
		: base(component)
	{
	}

	public override void Initialize(GlobalEffectContext context)
	{
		m_WindGO = new GameObject("AmbientWind");
		m_WindGO.transform.SetParent(context.GlobalEffect.transform, worldPositionStays: false);
		m_AmbientWind = m_WindGO.AddComponent<AmbientWind>();
		m_AmbientWind.StrengthNoiseContrast = base.Component.StrengthNoiseContrast;
		m_AmbientWind.StrengthNoiseWeight = base.Component.StrengthNoiseWeight;
		m_AmbientWind.StrenghtOctave0 = base.Component.StrenghtOctave0;
		m_AmbientWind.StrengthOctave1 = base.Component.StrengthOctave1;
		m_AmbientWind.ShiftOctave0 = base.Component.ShiftOctave0;
		m_AmbientWind.ShiftOctave1 = base.Component.ShiftOctave1;
	}

	public override void CleanUp()
	{
		Object.DestroyImmediate(m_WindGO);
	}

	public override void Update(GlobalEffectContext context)
	{
		if (!(context.Camera != Camera.main))
		{
			m_AmbientWind.Intensity = base.VolumeOverride?.Intensity.value ?? 0f;
		}
	}
}
