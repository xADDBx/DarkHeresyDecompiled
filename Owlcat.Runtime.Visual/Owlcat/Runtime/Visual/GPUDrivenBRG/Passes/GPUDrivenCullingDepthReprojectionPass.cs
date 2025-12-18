using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;

public class GPUDrivenCullingDepthReprojectionPass : ScriptableRenderPass<GPUDrivenCullingDepthReprojectionPassData>
{
	private readonly GPUDrivenDepthReprojectionUtils m_DepthReprojectionUtils;

	public override string Name => "GPUDrivenCulling.DepthReprojectionPass";

	public GPUDrivenCullingDepthReprojectionPass(RenderPassEvent evt, GPUDrivenDepthReprojectionUtils depthReprojectionUtils)
		: base(evt)
	{
		m_DepthReprojectionUtils = depthReprojectionUtils;
	}

	protected override void Setup(RenderGraphBuilder builder, GPUDrivenCullingDepthReprojectionPassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		Matrix4x4 gPUProjectionMatrix = GL.GetGPUProjectionMatrix(waaaghCameraData.GetProjectionMatrixNoJitter(), renderIntoTexture: true);
		Matrix4x4 viewMatrix = waaaghCameraData.GetViewMatrix();
		Matrix4x4 gpuViewProjection = gPUProjectionMatrix * viewMatrix;
		data.ReprojectionParameters = m_DepthReprojectionUtils.Setup(frameData, gpuViewProjection, out data.Source);
		if (!data.ReprojectionParameters.Cull)
		{
			RenderGraph renderGraph = frameData.Get<WaaaghRenderingData>().RenderGraph;
			waaaghResourceData.PackedReprojectedDepth = renderGraph.CreateTexture(in data.ReprojectionParameters.PackedReprojectedDepthDesc);
			waaaghResourceData.DepthReprojectionParameters = data.ReprojectionParameters;
			data.PackedReprojectedDepth = builder.WriteTexture(in waaaghResourceData.PackedReprojectedDepth);
		}
	}

	protected override void Render(GPUDrivenCullingDepthReprojectionPassData data, RenderGraphContext context)
	{
		if (!data.ReprojectionParameters.Cull)
		{
			m_DepthReprojectionUtils.Reproject(in data.ReprojectionParameters, context.cmd, data.Source, data.PackedReprojectedDepth);
		}
	}
}
