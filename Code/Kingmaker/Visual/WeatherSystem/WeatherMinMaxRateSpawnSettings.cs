using System;
using UnityEngine;

namespace Kingmaker.Visual.WeatherSystem;

[Obsolete("Global Effects")]
public abstract class WeatherMinMaxRateSpawnSettings : ScriptableObject
{
	public AnimationCurve EmissionRateMinOverIntensity = AnimationCurve.Linear(0f, 0f, 1f, 0.1f);

	public AnimationCurve EmissionRateMaxOverIntensity = AnimationCurve.Linear(0f, 0f, 1f, 0.1f);
}
