using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.XPBDGizmos.Passes;
using Owlcat.Runtime.Visual.XPBD;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.XPBDGizmos;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/XPBD Gizmos Feature", fileName = "XPBDGizmosFeature")]
public class XPBDGizmosFeature : ScriptableRendererFeature
{
	private XPBDGizmosFeatureResources m_Resources;

	private XPBDGizmosGBufferPass m_GBufferPass;

	private XPBDGizmosAfterRenderPass m_AfterRenderPass;

	private Material m_GizmosMaterial;

	public Material GizmosMaterial => m_GizmosMaterial;

	public override void AddRenderPasses(ScriptableRenderer renderer, ContextContainer frameData)
	{
		if (Application.isPlaying && WaaaghPipeline.Asset.DebugData.XPBDDebug.GizmosEnabled && !Owlcat.Runtime.Visual.XPBD.XPBD.IsEmpty && Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl.IsValid)
		{
			renderer.EnqueuePass(m_GBufferPass);
			renderer.EnqueuePass(m_AfterRenderPass);
		}
	}

	public override void Create()
	{
		m_Resources = GraphicsSettings.GetRenderPipelineSettings<XPBDGizmosFeatureResources>();
		m_GizmosMaterial = CoreUtils.CreateEngineMaterial(m_Resources.GizmosShader);
		m_GBufferPass = new XPBDGizmosGBufferPass(RenderPassEvent.BeforeRenderingGbuffer, WaaaghPipeline.Asset.DebugData, this);
		m_AfterRenderPass = new XPBDGizmosAfterRenderPass(RenderPassEvent.AfterRendering, WaaaghPipeline.Asset.DebugData, this);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		CoreUtils.Destroy(m_GizmosMaterial);
	}
}
