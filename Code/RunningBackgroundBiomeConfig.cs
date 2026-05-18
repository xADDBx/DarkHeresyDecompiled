using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "BiomeConfig", menuName = "Techart/RunningBackgroundBiomeConfig")]
public class RunningBackgroundBiomeConfig : ScriptableObject
{
	[Header("Terrain Visual")]
	[Tooltip("Material applied to generated terrain mesh. Should support vertex color blending (RGBA channels)")]
	public Material terrainMaterial;

	[Header("Vertex Paint — Base (R)")]
	[Tooltip("Base layer weight stored in vertex color R channel. Fills where other layers are absent")]
	[Range(0f, 3f)]
	public float baseWeight = 1f;

	[Header("Vertex Paint — Slopes (G)")]
	[Tooltip("Slope angle at which the layer appears (0 = flat, 1 = vertical)")]
	[Range(0f, 1f)]
	public float slopeThreshold = 0.3f;

	[Tooltip("Width of the soft transition around the threshold")]
	[Range(0.01f, 1f)]
	public float slopeFalloff = 0.2f;

	[Tooltip("Overall slope layer strength")]
	[Range(0f, 3f)]
	public float slopeStrength = 1.5f;

	[Header("Vertex Paint — Pattern Breakup (B)")]
	[Tooltip("Overall pattern layer strength")]
	[Range(0f, 3f)]
	public float patternStrength = 1f;

	[Tooltip("Noise frequency — smaller = larger blobs")]
	public float patternScale = 0.1f;

	[Tooltip("Noise offset for variation")]
	public Vector2 patternOffset = new Vector2(500f, 500f);

	[Tooltip("Higher = more area covered by the pattern")]
	[Range(0f, 1f)]
	public float patternBrightness = 0.5f;

	[Tooltip("Width of soft edge transition (smaller = sharper)")]
	[Range(0.01f, 1f)]
	public float patternFalloff = 0.3f;

	[Header("Vertex Paint — Embankment (A)")]
	[Tooltip("Deposit layer strength near the track embankment")]
	[Range(0f, 3f)]
	public float embankmentStrength = 1.5f;

	[Header("Asset Spawning")]
	[Tooltip("List of prefabs scattered across terrain chunks using grid-based random placement")]
	public RunningBackgroundSpawnableAsset[] spawnableAssets;

	[Header("Global Effects")]
	[Tooltip("Persistent VFX/prefabs attached to the RunningBackground transform (e.g. dust, fog, sparks)")]
	public RunningBackgroundBiomeEffect[] effects;
}
