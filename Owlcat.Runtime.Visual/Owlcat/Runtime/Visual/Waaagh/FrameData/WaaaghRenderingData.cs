using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.VirtualTexture;
using Unity.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public class WaaaghRenderingData : ContextItem
{
	public RenderGraph RenderGraph;

	public CullingResults CullResults;

	public bool SupportsDynamicBatching;

	public PerObjectData PerObjectData;

	public TimeData TimeData;

	public NativeArray<VisibleLight> VisibleLights;

	public GPUDrivenBatchRendererGroup GPUDrivenBatchRendererGroup;

	public VirtualTextureManager VirtualTextureManager;

	public LightCookieManager LightCookieManager;

	public override void Reset()
	{
		RenderGraph = null;
		CullResults = default(CullingResults);
		SupportsDynamicBatching = false;
		PerObjectData = PerObjectData.None;
		TimeData = default(TimeData);
		VisibleLights = default(NativeArray<VisibleLight>);
		GPUDrivenBatchRendererGroup = null;
		VirtualTextureManager = null;
	}
}
