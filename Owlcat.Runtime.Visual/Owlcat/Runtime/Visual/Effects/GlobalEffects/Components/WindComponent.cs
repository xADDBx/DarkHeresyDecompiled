using Owlcat.Runtime.Core.WindSystem;
using Owlcat.Runtime.Visual.Effects.GlobalEffects.Controllers;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.Components;

[GlobalEffectComponentMenu("Wind")]
public class WindComponent : ComponentBase
{
	[Range(0f, 1f)]
	public float StrengthNoiseWeight = 0.5f;

	[Range(1f, 10f)]
	public float StrengthNoiseContrast = 1f;

	public AmbientWind.NoiseOctave StrenghtOctave0 = new AmbientWind.NoiseOctave();

	public AmbientWind.NoiseOctave StrengthOctave1 = new AmbientWind.NoiseOctave();

	public AmbientWind.NoiseOctave ShiftOctave0 = new AmbientWind.NoiseOctave();

	public AmbientWind.NoiseOctave ShiftOctave1 = new AmbientWind.NoiseOctave();

	public override ControllerBase CreateController()
	{
		return new WindController(this);
	}
}
