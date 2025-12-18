using Owlcat.Runtime.Visual.IndirectRendering;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.IndirectRendering;

public class CullingPass : ScriptableRenderPass<CullingPassData>
{
	public override string Name => "CullingPass";

	public CullingPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, CullingPassData data, ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		data.Camera = waaaghCameraData.camera;
	}

	protected override void Render(CullingPassData data, RenderGraphContext context)
	{
		IndirectRenderingSystem.Instance.Cull(context.renderContext, data.Camera);
	}
}
