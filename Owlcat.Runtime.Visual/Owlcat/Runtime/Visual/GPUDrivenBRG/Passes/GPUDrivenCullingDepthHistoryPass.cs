using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.History;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;

public class GPUDrivenCullingDepthHistoryPass : ScriptableRenderPass<GPUDrivenCullingDepthHistoryPassData>
{
	private readonly DepthPyramidGenerationUtils m_PyramidGenerationUtils;

	private readonly DepthPyramidGenerationUtils.Resources m_Resources = new DepthPyramidGenerationUtils.Resources();

	public override string Name => "GPUDrivenCulling.DepthHistoryPass";

	public GPUDrivenCullingDepthHistoryPass(RenderPassEvent evt, DepthPyramidGenerationUtils pyramidGenerationUtils)
		: base(evt)
	{
		m_PyramidGenerationUtils = pyramidGenerationUtils;
	}

	protected override void Setup(RenderGraphBuilder builder, GPUDrivenCullingDepthHistoryPassData data, ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		CullingDepthHistory cullingDepthHistory = waaaghCameraData.CullingDepthHistory;
		if (cullingDepthHistory == null)
		{
			data.Cull = true;
			return;
		}
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		int2 viewportSize = new int2(waaaghCameraData.scaledWidth, waaaghCameraData.scaledHeight);
		int lodCount = cullingDepthHistory.MipLevel + 1;
		data.PyramidContext = m_PyramidGenerationUtils.Setup(viewportSize, m_Resources, useMax: true, lodCount);
		ref DepthPyramidGenerationUtils.Context pyramidContext = ref data.PyramidContext;
		data.Cull = false;
		TextureHandle input = waaaghResourceData.CameraDepthBuffer;
		data.Source = builder.ReadTexture(in input);
		data.Destination = waaaghRenderingData.RenderGraph.ImportBackbuffer(cullingDepthHistory.GetTexture());
		data.PyramidUAV = builder.CreateTransientTexture(in pyramidContext.PyramidDesc);
		data.HistoryMipLevel = cullingDepthHistory.MipLevel;
		if (pyramidContext.GlobalAtomicCounterDesc.count > 0)
		{
			data.GlobalAtomicCounterBuffer = builder.CreateTransientBuffer(in pyramidContext.GlobalAtomicCounterDesc);
		}
		cullingDepthHistory.UpdateVersion();
	}

	protected override void Render(GPUDrivenCullingDepthHistoryPassData data, RenderGraphContext context)
	{
		if (!data.Cull)
		{
			ref DepthPyramidGenerationUtils.Context pyramidContext = ref data.PyramidContext;
			m_PyramidGenerationUtils.Render(context, in pyramidContext, data.Source, data.PyramidUAV, data.GlobalAtomicCounterBuffer);
			Vector4 vector = pyramidContext.Resources.PyramidMipRects[data.HistoryMipLevel];
			int srcX = (int)vector.x;
			int srcY = (int)vector.y;
			int srcWidth = (int)vector.z;
			int srcHeight = (int)vector.w;
			context.cmd.CopyTexture(data.PyramidUAV, 0, 0, srcX, srcY, srcWidth, srcHeight, data.Destination, 0, 0, 0, 0);
		}
	}
}
