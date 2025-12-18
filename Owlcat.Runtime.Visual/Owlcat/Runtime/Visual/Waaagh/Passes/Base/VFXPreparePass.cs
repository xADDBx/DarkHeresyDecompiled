using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

public class VFXPreparePass : ScriptableRenderPass<VFXPreparePassData>
{
	public override string Name => "VFXPreparePass";

	public VFXPreparePass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, VFXPreparePassData data, ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		data.Camera = waaaghCameraData.camera;
		data.CullingResults = waaaghRenderingData.CullResults;
	}

	protected override void Render(VFXPreparePassData data, RenderGraphContext context)
	{
		VFXManager.ProcessCameraCommand(data.Camera, context.cmd, default(VFXCameraXRSettings), data.CullingResults);
	}
}
