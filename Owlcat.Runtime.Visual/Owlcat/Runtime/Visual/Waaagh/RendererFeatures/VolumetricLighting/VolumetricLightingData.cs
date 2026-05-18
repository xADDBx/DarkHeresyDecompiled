using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting;

internal sealed class VolumetricLightingData
{
	public VolumetricFog VolumetricFog;

	public TileSize TileSize;

	public int FogTilesBufferCount;

	public TextureHandle ScatterTexture;

	public BufferHandle FogTilesBuffer;

	public BufferHandle VisibleVolumesBoundsBuffer;

	public BufferHandle VisibleVolumesDataBuffer;

	public BufferHandle ZBinsBuffer;
}
