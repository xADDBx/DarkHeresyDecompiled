using Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DepthPyramidPassData : PassDataBase
{
	public TextureHandle CameraDepthBuffer;

	public DepthPyramidGenerationUtils.Context Context;

	public TextureHandle DepthPyramidUAV;

	public GPUDrivenDepthReprojectionUtils.ReprojectionParameters DepthReprojectionParameters;

	public BufferHandle GlobalAtomicCounterBuffer;

	public TextureHandle PackedReprojectedDepth;
}
