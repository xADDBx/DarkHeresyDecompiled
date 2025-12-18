using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public abstract class ScriptableRenderPass
{
	private ProfilingSampler m_ProfilingSampler;

	private ProfilingSampler m_RecordProfilingSampler;

	private List<RendererList> m_RendererLists = new List<RendererList>();

	public RenderPassEvent RenderPassEvent { get; set; }

	public abstract string Name { get; }

	public ProfilingSampler ProfilingSampler => m_ProfilingSampler;

	public ProfilingSampler RecordProfilingSampler => m_RecordProfilingSampler;

	internal List<RendererList> UsedRendererLists => m_RendererLists;

	private protected virtual WaaaghProfileId? ProfileId => null;

	public ScriptableRenderPass(RenderPassEvent evt)
	{
		RenderPassEvent = evt;
		string name = Name;
		if (string.IsNullOrEmpty(name))
		{
			name = GetType().Name;
		}
		m_ProfilingSampler = new ProfilingSampler(name);
		m_RecordProfilingSampler = new ProfilingSampler("Record " + name);
	}

	public void DependsOn(in RendererList rendererList)
	{
		m_RendererLists.Add(rendererList);
	}

	public virtual void ConfigureRendererLists(ScriptableRenderContext context, ContextContainer frameData)
	{
	}

	public virtual bool HasAnyCustomDependencyThatPreventsPassCulling(ScriptableRenderContext context, ContextContainer frameData)
	{
		return false;
	}

	public virtual bool AreRendererListsEmpty(ScriptableRenderContext context)
	{
		int count = m_RendererLists.Count;
		for (int i = 0; i < count; i++)
		{
			_ = m_RendererLists[i];
			if (context.QueryRendererListStatus(m_RendererLists[i]) == RendererListStatus.kRendererListPopulated)
			{
				return false;
			}
		}
		if (m_RendererLists.Count > 0)
		{
			return true;
		}
		return false;
	}

	public abstract void RecordRenderGraph(ContextContainer frameData);

	public static bool operator <(ScriptableRenderPass lhs, ScriptableRenderPass rhs)
	{
		return lhs.RenderPassEvent < rhs.RenderPassEvent;
	}

	public static bool operator >(ScriptableRenderPass lhs, ScriptableRenderPass rhs)
	{
		return lhs.RenderPassEvent > rhs.RenderPassEvent;
	}

	public void ClearRendererLists()
	{
		m_RendererLists.Clear();
	}
}
public abstract class ScriptableRenderPass<T> : ScriptableRenderPass where T : PassDataBase, new()
{
	private BaseRenderFunc<T, RenderGraphContext> m_RenderFunc;

	public ScriptableRenderPass(RenderPassEvent evt)
		: base(evt)
	{
		m_RenderFunc = Render;
	}

	public sealed override void RecordRenderGraph(ContextContainer frameData)
	{
		T passData;
		using RenderGraphBuilder builder = frameData.Get<WaaaghRenderingData>().RenderGraph.AddRenderPass<T>(Name, out passData, ProfilingSamplerStorage<WaaaghProfileId>.Get(Name, ProfileId), ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Passes\\ScriptableRenderPass.cs", 132);
		Setup(builder, passData, frameData);
		builder.SetRenderFunc(m_RenderFunc);
	}

	protected abstract void Setup(RenderGraphBuilder builder, T data, ContextContainer frameData);

	protected abstract void Render(T data, RenderGraphContext context);
}
