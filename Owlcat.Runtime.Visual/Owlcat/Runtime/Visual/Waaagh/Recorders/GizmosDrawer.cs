using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class GizmosDrawer
{
	private class DrawGizmosData
	{
		public RendererListHandle RendererList;
	}

	public static void DrawGizmosPreImageEffects(in RecordContext context)
	{
		DrawGizmos(in context, "DrawGizmos.PreImageEffects", GizmoSubset.PreImageEffects);
	}

	public static void DrawGizmosPostImageEffects(in RecordContext context)
	{
		DrawGizmos(in context, "DrawGizmos.PostImageEffects", GizmoSubset.PostImageEffects);
	}

	private static void DrawGizmos(in RecordContext context, string passName, GizmoSubset gizmoSubset)
	{
	}
}
