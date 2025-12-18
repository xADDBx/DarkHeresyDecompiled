using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class CapturePass : ScriptableRenderPass<CapturePassData>
{
	public override string Name => "CapturePass";

	public CapturePass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, CapturePassData data, ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		data.CaptureActions = waaaghCameraData.captureActions;
		TextureHandle input = waaaghResourceData.CameraColorBuffer;
		data.CameraColorBuffer = builder.ReadWriteTexture(in input);
	}

	protected override void Render(CapturePassData data, RenderGraphContext context)
	{
		data.CaptureActions.Reset();
		while (data.CaptureActions.MoveNext())
		{
			data.CaptureActions.Current(data.CameraColorBuffer, context.cmd);
		}
	}
}
