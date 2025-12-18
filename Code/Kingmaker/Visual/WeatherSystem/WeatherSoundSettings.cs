using System;
using UnityEngine;

namespace Kingmaker.Visual.WeatherSystem;

[Obsolete("Global Effects")]
[CreateAssetMenu(menuName = "VFX Weather System/Sound Effect")]
public class WeatherSoundSettings : ScriptableObject
{
	[SerializeField]
	private WeatherSoundType m_SoundType;
}
