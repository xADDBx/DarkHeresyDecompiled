using Owlcat.Runtime.Visual.ShaderGlobals;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ShaderGlobals.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ShaderGlobals;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/Shader Globals")]
public sealed class ShaderGlobalsFeature : ScriptableRendererFeature
{
	private ShaderGlobalsSetupPass m_SetupPass;

	public override void AddRenderPasses(ScriptableRenderer renderer, ContextContainer frameData)
	{
		ShaderGlobalsCommon.EnsureConfig();
		renderer.EnqueuePass(m_SetupPass);
	}

	public override void Create()
	{
		m_SetupPass = new ShaderGlobalsSetupPass(RenderPassEvent.BeforeRendering, GetState());
	}

	private ShaderGlobalsState GetState()
	{
		return ShaderGlobalsState.Instance;
	}

	protected override void Dispose(bool disposing)
	{
	}
}
