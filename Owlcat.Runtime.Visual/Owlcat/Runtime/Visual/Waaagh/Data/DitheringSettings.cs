using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Owlcat.Runtime.Visual.Waaagh.Data;

[Serializable]
public class DitheringSettings
{
	[SerializeField]
	[FormerlySerializedAs("m_GlobalType")]
	private DitheringTextureType m_TextureType = DitheringTextureType.BlueNoise;

	[SerializeField]
	private bool m_AnimateBlueNoiseWithTemporalAA;

	[SerializeField]
	[Range(0f, 2f)]
	private float m_JitterScale = 1f;

	[SerializeField]
	private bool m_EnableLODCrossFade;

	[SerializeField]
	private bool m_SupportAnimatedLODCrossFade = true;

	public DitheringTextureType TextureType => m_TextureType;

	public bool AnimateBlueNoiseWithTemporalAA => m_AnimateBlueNoiseWithTemporalAA;

	public float JitterScale => m_JitterScale;

	public bool EnableLODCrossFade => m_EnableLODCrossFade;

	public bool SupportAnimatedLODCrossFade => m_SupportAnimatedLODCrossFade;
}
