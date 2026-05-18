using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.PostProcess;

public struct FrameState
{
	public bool EnabledForCamera;

	public ColorGradingMode ColorGradingMode;

	public int LutSize;

	public bool UseFastSRGBLinearConversion;

	public bool SupportScreenSpaceLensFlare;

	public bool SupportDataDrivenLensFlare;

	public RenderTextureDescriptor Descriptor;

	public bool HasFinalPass;
}
