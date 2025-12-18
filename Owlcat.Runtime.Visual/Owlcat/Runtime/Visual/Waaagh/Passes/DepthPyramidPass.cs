using Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DepthPyramidPass : ScriptableRenderPass<DepthPyramidPassData>
{
	private static class ShaderConstantsId
	{
		public static readonly int _DepthPyramidSamplingRatio = Shader.PropertyToID("_DepthPyramidSamplingRatio");

		public static readonly int _DepthPyramidMipRects = Shader.PropertyToID("_DepthPyramidMipRects");

		public static readonly int _DepthPyramidLodCount = Shader.PropertyToID("_DepthPyramidLodCount");
	}

	private readonly GPUDrivenDepthReprojectionUtils m_DepthReprojectionUtils;

	private readonly DepthPyramidGenerationUtils m_GenerationUtils;

	private readonly DepthPyramidGenerationUtils.Resources m_Resources = new DepthPyramidGenerationUtils.Resources();

	private readonly bool m_UnpackReprojectedDepth;

	private readonly bool m_UseMax;

	public override string Name { get; }

	private protected override WaaaghProfileId? ProfileId { get; }

	public DepthPyramidPass(RenderPassEvent evt, DepthPyramidGenerationUtils generationUtils, GPUDrivenDepthReprojectionUtils depthReprojectionUtils, string name = "DepthPyramidPass", WaaaghProfileId? profileId = null, bool useMax = false, bool unpackReprojectedDepth = false)
		: base(evt)
	{
		m_DepthReprojectionUtils = depthReprojectionUtils;
		m_GenerationUtils = generationUtils;
		Name = name;
		ProfileId = profileId;
		m_UseMax = useMax;
		m_UnpackReprojectedDepth = unpackReprojectedDepth;
	}

	protected override void Setup(RenderGraphBuilder builder, DepthPyramidPassData data, ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		RenderGraph renderGraph = frameData.Get<WaaaghRenderingData>().RenderGraph;
		int2 viewportSize = new int2(waaaghCameraData.scaledWidth, waaaghCameraData.scaledHeight);
		data.Context = m_GenerationUtils.Setup(viewportSize, m_Resources, m_UseMax);
		if (data.Context.GlobalAtomicCounterDesc.count > 0)
		{
			data.GlobalAtomicCounterBuffer = builder.CreateTransientBuffer(in data.Context.GlobalAtomicCounterDesc);
		}
		waaaghResourceData.CameraDepthPyramidRT = renderGraph.CreateTexture(in data.Context.PyramidDesc);
		data.DepthPyramidUAV = builder.ReadWriteTexture(in waaaghResourceData.CameraDepthPyramidRT);
		TextureHandle input = waaaghResourceData.CameraDepthBuffer;
		data.CameraDepthBuffer = builder.ReadTexture(in input);
		if (m_UnpackReprojectedDepth && waaaghResourceData.PackedReprojectedDepth.IsValid())
		{
			data.PackedReprojectedDepth = builder.ReadTexture(in waaaghResourceData.PackedReprojectedDepth);
			data.DepthReprojectionParameters = waaaghResourceData.DepthReprojectionParameters;
		}
		else
		{
			data.PackedReprojectedDepth = TextureHandle.nullHandle;
			data.DepthReprojectionParameters = default(GPUDrivenDepthReprojectionUtils.ReprojectionParameters);
		}
	}

	protected override void Render(DepthPyramidPassData data, RenderGraphContext context)
	{
		if (!data.PackedReprojectedDepth.IsValid())
		{
			m_GenerationUtils.Render(context, in data.Context, data.CameraDepthBuffer, data.DepthPyramidUAV, data.GlobalAtomicCounterBuffer);
		}
		else
		{
			int historyMipLevel = data.DepthReprojectionParameters.HistoryMipLevel;
			int toMipExclusive = historyMipLevel + 1;
			m_GenerationUtils.Render(context, in data.Context, data.CameraDepthBuffer, data.DepthPyramidUAV, data.GlobalAtomicCounterBuffer, 1, toMipExclusive);
			Vector4 vector = data.Context.Resources.PyramidMipRects[historyMipLevel];
			Rect destinationViewport = new Rect(vector.x, vector.y, vector.z, vector.w);
			m_DepthReprojectionUtils.UnpackReprojectedDepth(in data.DepthReprojectionParameters, context.cmd, data.PackedReprojectedDepth, data.DepthPyramidUAV, destinationViewport);
			int fromMip = historyMipLevel + 1;
			m_GenerationUtils.Render(context, in data.Context, data.CameraDepthBuffer, data.DepthPyramidUAV, data.GlobalAtomicCounterBuffer, fromMip);
		}
		DepthPyramidGenerationUtils.Resources resources = data.Context.Resources;
		context.cmd.SetGlobalVector(ShaderConstantsId._DepthPyramidSamplingRatio, resources.PyramidSamplingRatio);
		context.cmd.SetGlobalVectorArray(ShaderConstantsId._DepthPyramidMipRects, resources.PyramidMipRects);
		context.cmd.SetGlobalInt(ShaderConstantsId._DepthPyramidLodCount, resources.PyramidLodCount);
	}
}
