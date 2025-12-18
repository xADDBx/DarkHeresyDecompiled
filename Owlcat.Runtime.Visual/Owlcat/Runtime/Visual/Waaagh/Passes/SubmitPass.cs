using Owlcat.Runtime.Visual.IndirectRendering;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class SubmitPass : ScriptableRenderPass<SubmitPassData>
{
	private int m_LastFrameId;

	public override string Name => "SubmitPass";

	public SubmitPass(RenderPassEvent evt)
		: base(evt)
	{
		m_LastFrameId = -1;
	}

	protected override void Setup(RenderGraphBuilder builder, SubmitPassData data, ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		data.NeedSubmit = false;
		if (m_LastFrameId != waaaghRenderingData.TimeData.FrameId)
		{
			data.NeedSubmit = true;
			m_LastFrameId = waaaghRenderingData.TimeData.FrameId;
		}
	}

	protected override void Render(SubmitPassData data, RenderGraphContext context)
	{
		if (data.NeedSubmit)
		{
			IndirectRenderingSystem.Instance.Submit();
		}
	}
}
