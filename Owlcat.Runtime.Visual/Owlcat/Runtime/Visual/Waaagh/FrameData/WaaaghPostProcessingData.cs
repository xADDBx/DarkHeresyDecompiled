using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public class WaaaghPostProcessingData : ContextItem
{
	public bool isEnabled;

	public ColorGradingMode gradingMode;

	public int lutSize;

	public bool useFastSRGBLinearConversion;

	public bool supportScreenSpaceLensFlare;

	public bool supportDataDrivenLensFlare;

	public TextureHandle StencilMaskTexture;

	public override void Reset()
	{
		isEnabled = false;
		gradingMode = ColorGradingMode.LowDynamicRange;
		lutSize = 0;
		useFastSRGBLinearConversion = false;
		supportScreenSpaceLensFlare = false;
		supportDataDrivenLensFlare = false;
		StencilMaskTexture = TextureHandle.nullHandle;
	}
}
