using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.PostProcess;

public class ColorGradingLutPassData : PassDataBase
{
	public WaaaghCameraData CameraData;

	public WaaaghPostProcessingData PostProcessingData;

	public Material LutBuilderLdr;

	public Material LutBuilderHdr;

	public bool AllowColorGradingACESHDR;

	public TextureHandle LutTarget;
}
