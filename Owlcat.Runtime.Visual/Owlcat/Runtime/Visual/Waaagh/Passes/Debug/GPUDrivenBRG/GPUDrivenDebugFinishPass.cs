using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug.GPUDrivenBRG;

public class GPUDrivenDebugFinishPass : ScriptableRenderPass<GPUDrivenDebugFinishPassData>
{
	private static class Profiling
	{
		public static readonly ProfilingSampler CullingStatsReadback = new ProfilingSampler("Culling Stats Readback");
	}

	public override string Name => "GPUDrivenDebug.FinishPass";

	public GPUDrivenDebugFinishPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, GPUDrivenDebugFinishPassData data, ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		data.BRG = waaaghRenderingData.GPUDrivenBatchRendererGroup;
		GPUDrivenCullingPassSharedData sharedPassData = data.BRG.SharedPassData;
		if (sharedPassData.Buffers.CullingStats.IsValid())
		{
			builder.ReadBuffer(in sharedPassData.Buffers.CullingStats);
		}
	}

	protected override void Render(GPUDrivenDebugFinishPassData data, RenderGraphContext context)
	{
		using (new ProfilingScope(context.cmd, Profiling.CullingStatsReadback))
		{
			GPUDrivenBatchRendererGroup bRG = data.BRG;
			BufferHandle cullingStats = bRG.SharedPassData.Buffers.CullingStats;
			if (cullingStats.IsValid())
			{
				context.cmd.RequestAsyncReadback(cullingStats, bRG.SharedPassData.ReadbackCullingStats);
			}
		}
	}
}
