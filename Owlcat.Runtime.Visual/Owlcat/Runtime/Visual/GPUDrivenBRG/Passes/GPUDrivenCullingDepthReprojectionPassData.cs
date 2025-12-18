using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;

public class GPUDrivenCullingDepthReprojectionPassData : PassDataBase
{
	public TextureHandle PackedReprojectedDepth;

	public GPUDrivenDepthReprojectionUtils.ReprojectionParameters ReprojectionParameters;

	public TextureHandle Source;
}
