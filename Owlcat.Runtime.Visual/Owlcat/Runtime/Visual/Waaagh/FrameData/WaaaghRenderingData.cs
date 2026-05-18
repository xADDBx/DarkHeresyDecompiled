using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.VirtualTexture;
using Unity.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public class WaaaghRenderingData
{
	public RenderGraph RenderGraph;

	public CullingResults CullResults;

	public bool SupportsDynamicBatching;

	public PerObjectData PerObjectData;

	public TimeData TimeData;

	public ShaderTimeData ShaderTimeData;

	public NativeArray<VisibleLight> VisibleLights;

	public GPUDrivenBatchRendererGroup GPUDrivenBatchRendererGroup;

	public VirtualTextureManager VirtualTextureManager;

	public LightCookieManager LightCookieManager;

	public void Reset()
	{
		RenderGraph = null;
		CullResults = default(CullingResults);
		SupportsDynamicBatching = false;
		PerObjectData = PerObjectData.None;
		TimeData = default(TimeData);
		ShaderTimeData = default(ShaderTimeData);
		VisibleLights = default(NativeArray<VisibleLight>);
		GPUDrivenBatchRendererGroup = null;
		VirtualTextureManager = null;
	}
}
