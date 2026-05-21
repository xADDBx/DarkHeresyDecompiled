using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping.Passes;

internal static class CleanupPass
{
	private static class Profiling
	{
		public static readonly ProfilingSampler TerrainStampingCleanup = new ProfilingSampler("TerrainStampingCleanup");
	}

	public static void Record(in RecordContext context)
	{
		object passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<object>("Terrain Stamping Cleanup", out passData2, Profiling.TerrainStampingCleanup, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\TerrainStamping\\Passes\\CleanupPass.cs", 15);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(object passData, UnsafeGraphContext context)
		{
			CoreUtils.SetKeyword(context.cmd, "_TERRAIN_STAMPING", state: false);
		});
	}
}
