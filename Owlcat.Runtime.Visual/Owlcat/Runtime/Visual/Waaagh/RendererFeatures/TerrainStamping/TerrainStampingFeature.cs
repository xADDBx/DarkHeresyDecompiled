using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/Terrain Stamping Feature", fileName = "TerrainStampingFeature")]
public class TerrainStampingFeature : ScriptableRendererFeature
{
	public TerrainStampingManagerParameters ManagerParameters;

	private TerrainStampingBakeNormalsPass m_BakeNormalsPass;

	private TerrainStampingBrushPass m_BrushPass;

	private TerrainStampingChunkUpdatePass m_ChunkUpdatePass;

	private TerrainStampingCleanupPass m_CleanupPass;

	private TerrainStampingCustomDecalDrawer m_DecalDrawer;

	private RendererFeaturePipelineService m_ManagerService;

	private TerrainStampingSetupForRenderPass m_SetupForRenderPass;

	internal override void StartSetupJobs(ContextContainer frameData)
	{
		base.StartSetupJobs(frameData);
		if (m_DecalDrawer != null)
		{
			frameData.GetOrCreate<WaaaghDecalData>().DecalRenderers.Add(m_DecalDrawer);
		}
	}

	public override void Create()
	{
		if (m_ManagerService == null)
		{
			m_ManagerService = new RendererFeaturePipelineService(delegate(WaaaghPipeline pipeline)
			{
				TerrainStampingManager.Init(pipeline, ManagerParameters);
			}, delegate
			{
				TerrainStampingManager.Cleanup();
			}, () => TerrainStampingManager.IsInitialized);
		}
		m_ManagerService.OnCreate();
		m_DecalDrawer = new TerrainStampingCustomDecalDrawer(ManagerParameters);
		m_ChunkUpdatePass = new TerrainStampingChunkUpdatePass(ManagerParameters, RenderPassEvent.BeforeRendering);
		m_BrushPass = new TerrainStampingBrushPass(ManagerParameters, RenderPassEvent.BeforeRendering);
		m_SetupForRenderPass = new TerrainStampingSetupForRenderPass(ManagerParameters, RenderPassEvent.BeforeRendering);
		m_BakeNormalsPass = new TerrainStampingBakeNormalsPass(ManagerParameters, RenderPassEvent.BeforeRendering);
		m_CleanupPass = new TerrainStampingCleanupPass(RenderPassEvent.AfterRendering);
	}

	protected override void Dispose(bool disposing)
	{
		m_ManagerService.OnDispose();
		m_DecalDrawer.Dispose();
		base.Dispose(disposing);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ContextContainer frameData)
	{
		if (TerrainStampingManager.TryGetInstance(out var _))
		{
			renderer.EnqueuePass(m_ChunkUpdatePass);
			renderer.EnqueuePass(m_BrushPass);
			renderer.EnqueuePass(m_BakeNormalsPass);
			renderer.EnqueuePass(m_SetupForRenderPass);
			renderer.EnqueuePass(m_CleanupPass);
		}
	}
}
