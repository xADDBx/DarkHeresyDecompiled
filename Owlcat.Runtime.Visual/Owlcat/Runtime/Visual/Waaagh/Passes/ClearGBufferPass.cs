using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public sealed class ClearGBufferPass : ScriptableRenderPass<ClearGBufferPassData>
{
	public override string Name => "ClearGBufferPass";

	public ClearGBufferPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, ClearGBufferPassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		TextureHandle input = waaaghResourceData.CameraDepthBuffer;
		builder.UseDepthBuffer(in input, DepthAccess.Write);
		data.CameraAlbedoRT = builder.UseColorBuffer(in waaaghResourceData.CameraAlbedoRT, 0);
		data.CameraSpecularRT = builder.UseColorBuffer(in waaaghResourceData.CameraSpecularRT, 1);
		data.CameraNormalsRT = builder.UseColorBuffer(in waaaghResourceData.CameraNormalsRT, 2);
		data.CameraTranslucencyRT = builder.UseColorBuffer(in waaaghResourceData.CameraTranslucencyRT, 3);
		data.CameraBakedGIRT = builder.UseColorBuffer(in waaaghResourceData.CameraBakedGIRT, 4);
		data.CameraShadowmaskRT = builder.UseColorBuffer(in waaaghResourceData.CameraShadowmaskRT, 5);
		data.ClearFlags = (waaaghCameraData.IsLightingEnabled ? RTClearFlags.All : RTClearFlags.DepthStencil);
	}

	protected override void Render(ClearGBufferPassData data, RenderGraphContext context)
	{
		if (data.ClearFlags != 0)
		{
			context.cmd.ClearRenderTarget(data.ClearFlags, Color.clear, 1f, 128u);
		}
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraAlbedoRT, data.CameraAlbedoRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraSpecularRT, data.CameraSpecularRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraNormalsRT, data.CameraNormalsRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraTranslucencyRT, data.CameraTranslucencyRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraBakedGIRT, data.CameraBakedGIRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraShadowmaskRT, data.CameraShadowmaskRT);
	}
}
