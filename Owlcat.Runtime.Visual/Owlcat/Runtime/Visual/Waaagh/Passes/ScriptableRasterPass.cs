using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public abstract class ScriptableRasterPass<T> : ScriptableRenderPass where T : PassDataBase, new()
{
	private BaseRenderFunc<T, RasterGraphContext> m_RenderFunc;

	protected ScriptableRasterPass(RenderPassEvent evt)
		: base(evt)
	{
		m_RenderFunc = Render;
	}

	public sealed override void RecordRenderGraph(ContextContainer frameData)
	{
		T passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = frameData.Get<WaaaghRenderingData>().RenderGraph.AddRasterRenderPass<T>(Name, out passData, ProfilingSamplerStorage<WaaaghProfileId>.Get(Name, ProfileId), ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\ScriptableRenderPass.cs", 157);
		Setup(rasterRenderGraphBuilder, passData, frameData);
		rasterRenderGraphBuilder.SetRenderFunc(m_RenderFunc);
	}

	protected abstract void Setup(IRasterRenderGraphBuilder builder, T passData, ContextContainer frameData);

	protected abstract void Render(T data, RasterGraphContext context);
}
