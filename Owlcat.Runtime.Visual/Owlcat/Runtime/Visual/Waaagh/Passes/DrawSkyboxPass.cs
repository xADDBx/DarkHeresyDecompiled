using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawSkyboxPass : ScriptableRenderPass<DrawSkyboxPassData>
{
	public override string Name => "DrawSkyboxPass";

	private protected override WaaaghProfileId? ProfileId => WaaaghProfileId.DrawSkyboxPass;

	public DrawSkyboxPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, DrawSkyboxPassData data, ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		TextureHandle input = waaaghResourceData.CameraColorBuffer;
		data.ColorOutput = builder.WriteTexture(in input);
		input = waaaghResourceData.CameraDepthBuffer;
		data.DepthOutput = builder.WriteTexture(in input);
		data.Camera = waaaghCameraData.camera;
	}

	protected override void Render(DrawSkyboxPassData data, RenderGraphContext context)
	{
		context.cmd.SetRenderTarget(data.ColorOutput, data.DepthOutput);
		RendererList rendererList = context.renderContext.CreateSkyboxRendererList(data.Camera);
		context.cmd.DrawRendererList(rendererList);
	}
}
