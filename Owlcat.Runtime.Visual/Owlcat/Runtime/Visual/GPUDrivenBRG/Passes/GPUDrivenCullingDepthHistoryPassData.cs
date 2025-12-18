using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;

public class GPUDrivenCullingDepthHistoryPassData : PassDataBase
{
	public bool Cull;

	public TextureHandle Destination;

	public BufferHandle GlobalAtomicCounterBuffer;

	public int HistoryMipLevel;

	public DepthPyramidGenerationUtils.Context PyramidContext;

	public TextureHandle PyramidUAV;

	public TextureHandle Source;
}
