using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

public class DrawGizmosPass : ScriptableRenderPass
{
	private GizmoSubset m_GizmoSubset;

	private string m_Name;

	public override string Name => m_Name;

	public DrawGizmosPass(GizmoSubset gizmoSubset)
		: base((gizmoSubset == GizmoSubset.PreImageEffects) ? RenderPassEvent.AfterRenderingTransparents : ((RenderPassEvent)1010))
	{
		m_GizmoSubset = gizmoSubset;
		m_Name = string.Format("{0}.{1}", "DrawGizmosPass", m_GizmoSubset);
	}

	public override void RecordRenderGraph(ContextContainer frameData)
	{
	}

	private static void ExecturePass(DrawGizmosPassData passData, RasterGraphContext context)
	{
		context.cmd.DrawRendererList(passData.RendererListHdl);
	}
}
