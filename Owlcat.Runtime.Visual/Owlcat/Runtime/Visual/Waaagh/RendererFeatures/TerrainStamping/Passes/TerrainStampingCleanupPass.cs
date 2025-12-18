using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping.Passes;

public class TerrainStampingCleanupPass : ScriptableRenderPass<PassDataBase>
{
	public override string Name => "TerrainStampingCleanupPass";

	public TerrainStampingCleanupPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, PassDataBase data, ContextContainer frameData)
	{
	}

	protected override void Render(PassDataBase data, RenderGraphContext context)
	{
		CoreUtils.SetKeyword(context.cmd, "_TERRAIN_STAMPING", state: false);
	}
}
