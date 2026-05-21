using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.PostProcess;

[Serializable]
public class PostProcessSettings
{
	public const int kMinLutSize = 32;

	public const int kMaxLutSize = 65;

	[SerializeField]
	private int m_ColorGradingLutSize = 32;

	[SerializeField]
	private ColorGradingMode m_ColorGradingMode;

	[HideInInspector]
	[SerializeField]
	private bool m_AllowPostProcessAlphaOutput;

	[SerializeField]
	private bool m_UseFastSRGBLinearConversion;

	[SerializeField]
	private bool m_SupportDataDrivenLensFlare = true;

	[SerializeField]
	private bool m_SupportScreenSpaceLensFlare = true;

	public int ColorGradingLutSize
	{
		get
		{
			return m_ColorGradingLutSize;
		}
		set
		{
			m_ColorGradingLutSize = Mathf.Clamp(value, 32, 65);
		}
	}

	public ColorGradingMode ColorGradingMode
	{
		get
		{
			return m_ColorGradingMode;
		}
		set
		{
			m_ColorGradingMode = value;
		}
	}

	public bool AllowPostProcessAlphaOutput => m_AllowPostProcessAlphaOutput;

	public bool UseFastSRGBLinearConversion => m_UseFastSRGBLinearConversion;

	public bool SupportScreenSpaceLensFlare => m_SupportScreenSpaceLensFlare;

	public bool SupportDataDrivenLensFlare => m_SupportDataDrivenLensFlare;
}
