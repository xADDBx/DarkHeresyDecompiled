using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public abstract class DrawMultiRendererListPass<T> : ScriptableRenderPass where T : DrawMultiRendererListPassData, new()
{
	private readonly List<DrawMultiRendererListPassData.RendererListData> m_RendererLists = new List<DrawMultiRendererListPassData.RendererListData>();

	private BaseRenderFunc<T, RenderGraphContext> m_RenderFunc;

	protected DrawMultiRendererListPass(RenderPassEvent evt)
		: base(evt)
	{
		m_RenderFunc = Render;
	}

	public override void ConfigureRendererLists(ScriptableRenderContext context, ContextContainer frameData)
	{
		m_RendererLists.Clear();
		GetOrCreateRendererLists(context, frameData, m_RendererLists);
		foreach (DrawMultiRendererListPassData.RendererListData rendererList in m_RendererLists)
		{
			DrawMultiRendererListPassData.RendererListData current = rendererList;
			DependsOn(in current.List);
		}
	}

	public override void RecordRenderGraph(ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		frameData.Get<WaaaghCameraData>();
		T passData;
		using RenderGraphBuilder builder = waaaghRenderingData.RenderGraph.AddRenderPass<T>(Name, out passData, ProfilingSamplerStorage<WaaaghProfileId>.Get(Name, ProfileId), ".\\Library\\PackageCache\\com.owlcat.visual@01c4fcbc474f\\Runtime\\Waaagh\\Passes\\DrawRendererListPass.cs", 108);
		passData.RendererLists.Clear();
		foreach (DrawMultiRendererListPassData.RendererListData rendererList in m_RendererLists)
		{
			passData.RendererLists.Add(rendererList);
		}
		builder.AllowRendererListCulling(value: true);
		Setup(builder, passData, frameData);
		builder.SetRenderFunc(m_RenderFunc);
	}

	protected abstract void GetOrCreateRendererLists(ScriptableRenderContext context, ContextContainer frameData, List<DrawMultiRendererListPassData.RendererListData> rendererLists);

	protected abstract void Setup(RenderGraphBuilder builder, T data, ContextContainer frameData);

	protected abstract void Render(T data, RenderGraphContext context);
}
