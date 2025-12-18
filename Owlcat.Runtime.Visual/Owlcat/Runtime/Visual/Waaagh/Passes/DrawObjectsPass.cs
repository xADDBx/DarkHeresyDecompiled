using System;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.IndirectRendering;
using Owlcat.Runtime.Visual.VirtualTexture;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawObjectsPass : DrawRendererListPass<DrawObjectsPassData>
{
	public enum RendererListType
	{
		OpaqueDistortionForward,
		Transparent,
		Overlay
	}

	private RendererListType m_RendererListType;

	private string m_Name;

	public override string Name => m_Name;

	private protected sealed override WaaaghProfileId? ProfileId { get; }

	public DrawObjectsPass(RenderPassEvent evt, RendererListType rendererListType)
		: base(evt)
	{
		m_RendererListType = rendererListType;
		m_Name = $"DrawObjects.{m_RendererListType}";
		ProfileId = rendererListType switch
		{
			RendererListType.OpaqueDistortionForward => WaaaghProfileId.DrawObjects_OpaqueDistortionForward, 
			RendererListType.Transparent => WaaaghProfileId.DrawObjects_Transparent, 
			RendererListType.Overlay => WaaaghProfileId.DrawObjects_Overlay, 
			_ => throw new ArgumentOutOfRangeException("rendererListType", rendererListType, null), 
		};
	}

	protected override void GetOrCreateRendererList(ScriptableRenderContext context, ContextContainer frameData, out RendererList rendererList, out RendererListParams rendererListParams)
	{
		WaaaghRendererListData waaaghRendererListData = frameData.Get<WaaaghRendererListData>();
		switch (m_RendererListType)
		{
		case RendererListType.OpaqueDistortionForward:
			rendererList = waaaghRendererListData.OpaqueDistortionForward.List;
			rendererListParams = waaaghRendererListData.OpaqueDistortionForward.ListParams;
			break;
		case RendererListType.Transparent:
			rendererList = waaaghRendererListData.Transparent.List;
			rendererListParams = waaaghRendererListData.Transparent.ListParams;
			break;
		case RendererListType.Overlay:
			rendererList = waaaghRendererListData.Overlay.List;
			rendererListParams = waaaghRendererListData.Overlay.ListParams;
			break;
		default:
			rendererList = waaaghRendererListData.Transparent.List;
			rendererListParams = waaaghRendererListData.Transparent.ListParams;
			break;
		}
	}

	protected override void Setup(RenderGraphBuilder builder, DrawObjectsPassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		TextureHandle input = waaaghResourceData.CameraColorBuffer;
		builder.UseColorBuffer(in input, 0);
		input = waaaghResourceData.CameraDepthBuffer;
		builder.UseDepthBuffer(in input, DepthAccess.Read);
		data.NeedsVTFeedback = m_RendererListType == RendererListType.Transparent;
		if (data.NeedsVTFeedback)
		{
			input = waaaghResourceData.VTFeedbackRT;
			data.VTFeedbackRT = builder.WriteTexture(in input);
		}
		if (m_RendererListType == RendererListType.OpaqueDistortionForward)
		{
			data.CameraNormalsRT = builder.ReadTexture(in waaaghResourceData.CameraNormalsRT);
			data.CameraBakedGIRT = builder.ReadTexture(in waaaghResourceData.CameraBakedGIRT);
			data.CameraShadowmaskRT = builder.ReadTexture(in waaaghResourceData.CameraShadowmaskRT);
			data.CameraDepthCopyRT = builder.ReadTexture(in waaaghResourceData.CameraDepthCopyRT);
		}
		if (m_RendererListType == RendererListType.Transparent)
		{
			GPUDrivenBatchRendererGroup gPUDrivenBatchRendererGroup = waaaghRenderingData.GPUDrivenBatchRendererGroup;
			if (gPUDrivenBatchRendererGroup.IsEnabledAndInitialized)
			{
				BufferHandle input2 = gPUDrivenBatchRendererGroup.SharedPassData.Buffers.ForwardReflectionProbeIndices;
				if (input2.IsValid())
				{
					builder.ReadBuffer(in input2);
				}
			}
		}
		if (m_RendererListType != RendererListType.Overlay && waaaghResourceData.Shadowmap.IsValid())
		{
			input = waaaghResourceData.Shadowmap;
			builder.ReadTexture(in input);
		}
		data.CameraType = waaaghCameraData.cameraType;
		data.IsIndirectRenderingEnabled = waaaghCameraData.IrsData.Enabled;
		data.IsSceneViewInPrefabEditMode = waaaghCameraData.IsSceneViewInPrefabEditMode;
		if (m_RendererListType == RendererListType.Transparent)
		{
			builder.AllowRendererListCulling(!waaaghCameraData.IrsData.IrsHasTransparents);
		}
		else if (m_RendererListType == RendererListType.OpaqueDistortionForward)
		{
			builder.AllowRendererListCulling(!waaaghCameraData.IrsData.IrsHasOpaqueDistortions);
		}
		else
		{
			builder.AllowRendererListCulling(value: true);
		}
	}

	protected override void Render(DrawObjectsPassData data, RenderGraphContext context)
	{
		if (data.NeedsVTFeedback)
		{
			VirtualTextureManager.SetFeedbackBufferRandomWriteTarget(context.cmd, data.VTFeedbackRT);
		}
		context.cmd.DrawRendererList(data.RendererList);
		IndirectRenderingSystem.Instance.DrawPass(context.cmd, data.CameraType, data.IsIndirectRenderingEnabled, data.IsSceneViewInPrefabEditMode, data.RendererListParams, debugOverdraw: false);
		if (data.NeedsVTFeedback)
		{
			context.cmd.ClearRandomWriteTargets();
		}
	}
}
