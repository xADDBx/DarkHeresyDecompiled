using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ColoredShadows.Passes;

public class ColoredShadowsCleanupPass : ScriptableRenderPass<PassDataBase>
{
	public override string Name => "ColoredShadowsCleanupPass";

	public ColoredShadowsCleanupPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, PassDataBase data, ContextContainer frameData)
	{
	}

	protected override void Render(PassDataBase data, RenderGraphContext context)
	{
		CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.COLORED_SHADOWS, state: false);
	}
}
