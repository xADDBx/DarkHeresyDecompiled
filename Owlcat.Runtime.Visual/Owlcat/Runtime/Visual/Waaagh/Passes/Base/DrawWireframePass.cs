using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

public class DrawWireframePass : ScriptableRenderPass
{
	public override string Name => "DrawWireframePass";

	public DrawWireframePass(RenderPassEvent evt)
		: base(evt)
	{
	}

	public override void RecordRenderGraph(ContextContainer frameData)
	{
	}

	private static void ExecutePass(DrawWireframePassData passData, RasterGraphContext context)
	{
		context.cmd.DrawRendererList(passData.RendererListHdl);
	}
}
