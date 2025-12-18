using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class SetupReflectionProbesPass : ScriptableRenderPass<SetupReflectionProbesPassData>
{
	private readonly WaaaghReflectionProbes m_ReflectionProbes;

	public override string Name => "SetupReflectionProbesPass";

	public SetupReflectionProbesPass(RenderPassEvent evt, WaaaghReflectionProbes reflectionProbes)
		: base(evt)
	{
		m_ReflectionProbes = reflectionProbes;
	}

	protected override void Setup(RenderGraphBuilder builder, SetupReflectionProbesPassData data, ContextContainer frameData)
	{
		data.RenderingData = frameData.Get<WaaaghRenderingData>();
	}

	protected override void Render(SetupReflectionProbesPassData data, RenderGraphContext context)
	{
		m_ReflectionProbes.UpdateGpuData(context.cmd, ref data.RenderingData.CullResults);
	}
}
