using System;
using UnityEngine;

namespace Kingmaker.Visual.WeatherSystem;

[Obsolete("Global Effects")]
[CreateAssetMenu(menuName = "VFX Weather System/Thunder Distant Effect")]
public class WeatherThunderSettings : ScriptableObject
{
	public AnimationCurve AlphaOverIntensity = AnimationCurve.Linear(0f, 0f, 1f, 0.1f);

	public ParticleSystem ThunderPrefab;
}
