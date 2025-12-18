using System;
using UnityEngine;

namespace Kingmaker.Visual.WeatherSystem;

[Obsolete("Global Effects")]
[CreateAssetMenu(menuName = "VFX Weather System/Lightning Bolt Effect")]
public class WeatherLightningBoltSettings : ScriptableObject
{
	public LayerMask RaycastMask;

	public GameObject LightningBoltPrefab;
}
