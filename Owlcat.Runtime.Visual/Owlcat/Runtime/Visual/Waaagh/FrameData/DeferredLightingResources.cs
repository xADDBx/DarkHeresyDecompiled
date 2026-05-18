using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public struct DeferredLightingResources
{
	public BufferHandle LightDataConstantBuffer;

	public BufferHandle LightVolumeDataConstantBuffer;

	public BufferHandle LightTilesBuffer;

	public BufferHandle FeatureTilesBuffer;

	public BufferHandle FeatureTilesListsBuffer;

	public BufferHandle FeatureIndirectArgsBuffer;
}
