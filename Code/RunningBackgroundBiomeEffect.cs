using System;
using UnityEngine;

[Serializable]
public class RunningBackgroundBiomeEffect
{
	[Tooltip("VFX or ambient prefab instantiated as a child of RunningBackground")]
	public GameObject prefab;

	[Tooltip("Local position offset from RunningBackground transform")]
	public Vector3 offset;

	[Tooltip("Minimum speed (km/h) for this effect to be active. 0 = always active")]
	public float minSpeed;
}
