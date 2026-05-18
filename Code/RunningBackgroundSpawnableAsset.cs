using System;
using UnityEngine;

[Serializable]
public class RunningBackgroundSpawnableAsset
{
	[Tooltip("Prefab to scatter across terrain chunks")]
	public GameObject prefab;

	[Tooltip("Chance of spawning one instance per grid cell (0 = never, 1 = every cell)")]
	[Range(0f, 1f)]
	public float probability = 0.1f;

	[Tooltip("Size of the spawning grid cell in meters. Smaller = denser potential placement")]
	[Min(0.5f)]
	public float cellSize = 15f;

	[Tooltip("Minimum random uniform scale applied to the instance")]
	public float minScale = 0.8f;

	[Tooltip("Maximum random uniform scale applied to the instance")]
	public float maxScale = 1.2f;

	[Tooltip("Rotate the instance to match the terrain surface normal")]
	public bool alignToNormal;

	[Tooltip("Amount of random rotation around the X axis (0 = none, 1 = full ±180°)")]
	[Range(0f, 1f)]
	public float randomRotationX;

	[Tooltip("Amount of random rotation around the Y axis (0 = none, 1 = full ±180°)")]
	[Range(0f, 1f)]
	public float randomRotationY = 1f;

	[Tooltip("Amount of random rotation around the Z axis (0 = none, 1 = full ±180°)")]
	[Range(0f, 1f)]
	public float randomRotationZ;

	[Tooltip("Extra clearance from track center (added to trackHalfWidth). Prevents objects clipping into rails")]
	public float trackSafeDistance;

	[Tooltip("Vertical offset from terrain surface, in meters. Positive = raise, negative = sink")]
	public float verticalOffset;
}
