using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VertexPaint.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VertexPaint;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/Vertex Paint", fileName = "VertexPaintFeature")]
public class VertexPaintFeature : ScriptableRendererFeature
{
	public VertexPaintManagerParameters ManagerParameters;

	private RendererFeaturePipelineService m_ManagerService;

	private VertexPaintPass m_Pass;

	public override void Create()
	{
		m_Pass = new VertexPaintPass(RenderPassEvent.BeforeRendering);
		if (m_ManagerService == null)
		{
			m_ManagerService = new RendererFeaturePipelineService(delegate(WaaaghPipeline pipeline)
			{
				VertexPaintManager.Init(pipeline, ManagerParameters);
			}, delegate
			{
				VertexPaintManager.Cleanup();
			}, () => VertexPaintManager.IsInitialized);
		}
		m_ManagerService.OnCreate();
	}

	protected override void Dispose(bool disposing)
	{
		m_ManagerService.OnDispose();
		base.Dispose(disposing);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ContextContainer frameData)
	{
		renderer.EnqueuePass(m_Pass);
	}
}
