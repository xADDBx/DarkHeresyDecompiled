using System;
using UnityEngine;

namespace Kingmaker.Visual.WeatherSystem;

[Obsolete("Global Effects")]
[CreateAssetMenu(menuName = "VFX Weather System/Camera Shake Effect")]
public class WeatherCameraShakeSettings : ScriptableObject
{
	public CameraShakeFx CameraShakeFxPrefab;

	public AnimationCurve AmplitudeOverIntensity = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve FreqOverIntensity = AnimationCurve.Linear(0f, 0f, 1f, 1f);
}
