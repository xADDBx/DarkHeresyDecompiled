using System;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.Components;

[Serializable]
public class VfxEffect
{
	public GameObject VfxPrefab;

	public VfxPositionSpace PositionSpace;

	public float3 Position = new float3(0f, 0f, 0f);

	public PositionType PositionType;

	public string BoundsProperty = "Bounds";

	public float DistanceFromCamera = 10f;

	public bool UseSnapMap;

	public string SnapMapProperty = "VfxGlobalSnapMap";

	public string SnapMapBoundsProperty = "SnapMapBounds";
}
