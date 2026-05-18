using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "RunningBackgroundSettings", menuName = "Techart/RunningBackgroundSettings")]
public class RunningBackgroundSettings : ScriptableObject
{
	[Header("Chunk Dimensions")]
	[Tooltip("Length of each terrain chunk along the Z axis (movement direction), in meters")]
	[Min(1f)]
	public float chunkLength = 100f;

	[Tooltip("Width of each terrain chunk along the X axis (perpendicular to track), in meters")]
	[Min(1f)]
	public float chunkWidth = 80f;

	[Header("Mesh Resolution")]
	[Tooltip("Number of vertices along the X axis per chunk. Higher = more detailed terrain cross-section")]
	[Min(2f)]
	public int resolutionX = 20;

	[Tooltip("Number of vertices along the Z axis per chunk. Higher = more detailed terrain along movement")]
	[Min(2f)]
	public int resolutionZ = 25;

	[Header("Terrain Height")]
	[Tooltip("Base ground level in world units. Track and embankment are offset from this")]
	public float baseHeight;

	[Tooltip("Maximum terrain height deviation from base level (peak-to-valley half-range)")]
	public float heightVariation = 5f;

	[Header("Perlin Noise")]
	[Tooltip("Noise sampling frequency. Smaller values produce larger, smoother hills")]
	public float noiseScale = 0.02f;

	[Tooltip("Number of noise layers blended together. More octaves add finer detail")]
	[Range(1f, 8f)]
	public int noiseOctaves = 3;

	[Tooltip("Amplitude decay per octave. Lower values make fine detail subtler")]
	[Range(0f, 1f)]
	public float noisePersistence = 0.5f;

	[Tooltip("Frequency multiplier per octave. Higher values add more high-frequency detail")]
	public float noiseLacunarity = 2f;

	[Tooltip("Offset to vary terrain pattern between biomes")]
	public Vector2 noiseSeed = new Vector2(1000f, 1000f);

	[Header("Track Embankment")]
	[Tooltip("Half-width of flat area along track center. 0 = disabled.")]
	public float trackHalfWidth = 4f;

	[Tooltip("Width of the smooth transition zone between embankment slope and noisy terrain")]
	public float trackBlendWidth = 3f;

	[Tooltip("Height of the embankment above baseHeight")]
	public float embankmentHeight = 3f;

	[Tooltip("Width of the slope from embankment top to ground level")]
	public float embankmentSlopeWidth = 8f;

	[Header("UV Settings")]
	[Tooltip("World-space divisor for UV0 tiling")]
	[Min(0.01f)]
	public float textureWorldScale = 10f;

	[Header("Track")]
	[Tooltip("Prefab placed repeatedly along the track center line (e.g. rail segment)")]
	public GameObject trackPrefab;

	[Tooltip("Distance between consecutive track segment instances, in meters")]
	[Min(0.5f)]
	public float trackSegmentLength = 10f;

	[Header("Camera Shake — Common")]
	[Tooltip("Master toggle for all camera shake layers (ambient, periodic, impulse)")]
	public bool enableCameraShake;

	[Tooltip("Amplitude multiplier by normalized speed (0..1)")]
	public AnimationCurve shakeBySpeed = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[Tooltip("Speed at which shakeBySpeed curve reaches 1.0")]
	public float shakeMaxSpeed = 150f;

	[Header("Camera Shake — Ambient (constant subtle vibration)")]
	[Tooltip("Maximum pixel offset of the constant background vibration layer")]
	public float ambientAmplitude = 0.02f;

	[Tooltip("Oscillation rate of the ambient vibration (cycles per second)")]
	public float ambientFrequency = 12f;

	[Header("Camera Shake — Periodic (impulse loop)")]
	[Tooltip("Maximum pixel offset of the periodic shake impulse")]
	[FormerlySerializedAs("shakeAmplitude")]
	public float periodicAmplitude = 0.15f;

	[Tooltip("Oscillation rate of the periodic shake (cycles per second)")]
	[FormerlySerializedAs("shakeFrequency")]
	public float periodicFrequency = 6f;

	[Tooltip("Duration of one full periodic shake cycle, in seconds")]
	[FormerlySerializedAs("shakeCycleDuration")]
	[Min(0.1f)]
	public float periodicCycleDuration = 2f;

	[FormerlySerializedAs("shakeCycleEnvelope")]
	[Tooltip("Amplitude envelope over one cycle (0..1 time → multiplier)")]
	public AnimationCurve periodicCycleEnvelope = AnimationCurve.Constant(0f, 1f, 1f);

	public float SampleHeight(float worldX, float worldZ)
	{
		float num = 1f;
		float num2 = 1f;
		float num3 = 0f;
		float num4 = 0f;
		for (int i = 0; i < noiseOctaves; i++)
		{
			float x = (worldX + noiseSeed.x) * noiseScale * num2;
			float y = (worldZ + noiseSeed.y) * noiseScale * num2;
			num3 += Mathf.PerlinNoise(x, y) * num;
			num4 += num;
			num *= noisePersistence;
			num2 *= noiseLacunarity;
		}
		num3 /= num4;
		return baseHeight + (num3 - 0.5f) * 2f * heightVariation;
	}

	public float SampleHeightWithTrack(float localX, float worldX, float worldZ)
	{
		float num = SampleHeight(worldX, worldZ);
		if (trackHalfWidth <= 0f)
		{
			return num;
		}
		float num2 = Mathf.Abs(localX);
		float num3 = baseHeight + embankmentHeight;
		if (num2 <= trackHalfWidth)
		{
			return num3;
		}
		float num4 = trackHalfWidth + embankmentSlopeWidth;
		if (num2 <= num4)
		{
			float num5 = (num2 - trackHalfWidth) / embankmentSlopeWidth;
			num5 = num5 * num5 * (3f - 2f * num5);
			return Mathf.Lerp(num3, baseHeight, num5);
		}
		float num6 = num4 + trackBlendWidth;
		if (num2 <= num6)
		{
			float t = (num2 - num4) / trackBlendWidth;
			return Mathf.Lerp(baseHeight, num, t);
		}
		return num;
	}

	public Vector3 SampleNormal(float worldX, float worldZ)
	{
		float num = SampleHeight(worldX - 0.1f, worldZ);
		float num2 = SampleHeight(worldX + 0.1f, worldZ);
		float num3 = SampleHeight(worldX, worldZ - 0.1f);
		float num4 = SampleHeight(worldX, worldZ + 0.1f);
		return new Vector3(num - num2, 0.2f, num3 - num4).normalized;
	}
}
