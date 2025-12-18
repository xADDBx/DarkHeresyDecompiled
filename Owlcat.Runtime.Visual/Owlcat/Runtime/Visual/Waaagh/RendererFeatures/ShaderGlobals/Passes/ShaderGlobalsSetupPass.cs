using Owlcat.Runtime.Visual.ShaderGlobals;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ShaderGlobals.Passes;

public class ShaderGlobalsSetupPass : ScriptableRenderPass<ShaderGlobalsSetupPassData>
{
	private readonly ShaderGlobalsState m_State;

	public override string Name => "ShaderGlobalsSetupPass";

	public ShaderGlobalsSetupPass(RenderPassEvent evt, ShaderGlobalsState state)
		: base(evt)
	{
		m_State = state;
	}

	protected override void Setup(RenderGraphBuilder builder, ShaderGlobalsSetupPassData data, ContextContainer frameData)
	{
		data.State = m_State;
	}

	protected override void Render(ShaderGlobalsSetupPassData data, RenderGraphContext context)
	{
		data.State.UploadToGPU(context.cmd);
	}
}
