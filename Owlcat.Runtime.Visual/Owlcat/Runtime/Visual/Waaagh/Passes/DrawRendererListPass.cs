using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public abstract class DrawRendererListPass<T> : ScriptableRenderPass where T : DrawRendererListPassData, new()
{
	private RendererList m_RendererList;

	private RendererListParams m_RendererListParams;

	private BaseRenderFunc<T, RenderGraphContext> m_RenderFunc;

	protected DrawRendererListPass(RenderPassEvent evt)
		: base(evt)
	{
		m_RenderFunc = Render;
	}

	public override void ConfigureRendererLists(ScriptableRenderContext context, ContextContainer frameData)
	{
		GetOrCreateRendererList(context, frameData, out m_RendererList, out m_RendererListParams);
		DependsOn(in m_RendererList);
	}

	public override void RecordRenderGraph(ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		frameData.Get<WaaaghCameraData>();
		T passData;
		using RenderGraphBuilder builder = waaaghRenderingData.RenderGraph.AddRenderPass<T>(Name, out passData, ProfilingSamplerStorage<WaaaghProfileId>.Get(Name, ProfileId), ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\Passes\\DrawRendererListPass.cs", 39);
		passData.RendererList = m_RendererList;
		passData.RendererListParams = m_RendererListParams;
		builder.AllowRendererListCulling(value: true);
		Setup(builder, passData, frameData);
		builder.SetRenderFunc(m_RenderFunc);
	}

	protected abstract void GetOrCreateRendererList(ScriptableRenderContext context, ContextContainer frameData, out RendererList rendererList, out RendererListParams rendererListParams);

	protected abstract void Setup(RenderGraphBuilder builder, T data, ContextContainer frameData);

	protected abstract void Render(T data, RenderGraphContext context);
}
