using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTexture;

[Serializable]
public class FeedbackMipBiasSettings
{
	public const float kMaxMipBias = 10f;

	[Min(1f)]
	public int MemoryLength = 10;

	[Range(0f, 10f)]
	public float BiasIncrement = 0.1f;

	[Range(0f, 10f)]
	public float BiasDecrement = 0.1f;

	[Range(0f, 1f)]
	public float UndersubscribedThreshold = 0.5f;

	[Range(0f, 1f)]
	public float OversubscribedThreshold = 0.75f;
}
