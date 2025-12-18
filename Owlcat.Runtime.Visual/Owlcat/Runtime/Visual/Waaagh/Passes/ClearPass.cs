using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class ClearPass : ScriptableRenderPass<ClearPassData>
{
	public override string Name => "ClearPass";

	public ClearPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, ClearPassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghCameraData cameraData = frameData.Get<WaaaghCameraData>();
		TextureHandle input = waaaghResourceData.CameraColorBuffer;
		data.CameraColorBuffer = builder.WriteTexture(in input);
		input = waaaghResourceData.CameraDepthBuffer;
		data.CameraDepthBuffer = builder.WriteTexture(in input);
		data.CameraDepthCopy = builder.WriteTexture(in waaaghResourceData.CameraDepthCopyRT);
		data.DepthCopyClearColor = (SystemInfo.usesReversedZBuffer ? new Color(1f, 1f, 1f, 1f) : new Color(0f, 0f, 0f, 0f));
		SetupClearData(data, cameraData);
	}

	private void SetupClearData(ClearPassData data, WaaaghCameraData cameraData)
	{
		if (cameraData.renderType != 0)
		{
			data.ClearFlags = (cameraData.clearDepth ? RTClearFlags.DepthStencil : RTClearFlags.Stencil);
			data.ClearColor = Color.clear;
			return;
		}
		switch (cameraData.camera.clearFlags)
		{
		case CameraClearFlags.Skybox:
			data.ClearFlags = RTClearFlags.All;
			data.ClearColor = Color.clear;
			break;
		case CameraClearFlags.Color:
			data.ClearFlags = RTClearFlags.All;
			data.ClearColor = cameraData.backgroundColor;
			data.ClearColor.a = 0f;
			break;
		case CameraClearFlags.Depth:
			data.ClearFlags = RTClearFlags.DepthStencil;
			data.ClearColor = Color.clear;
			break;
		default:
			data.ClearFlags = RTClearFlags.None;
			data.ClearColor = Color.clear;
			break;
		}
	}

	protected override void Render(ClearPassData data, RenderGraphContext context)
	{
		if (data.ClearFlags != 0)
		{
			context.cmd.SetRenderTarget(data.CameraColorBuffer, data.CameraDepthBuffer);
			context.cmd.ClearRenderTarget(data.ClearFlags, data.ClearColor);
		}
		context.cmd.SetRenderTarget(data.CameraDepthCopy);
		context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, data.DepthCopyClearColor);
	}
}
