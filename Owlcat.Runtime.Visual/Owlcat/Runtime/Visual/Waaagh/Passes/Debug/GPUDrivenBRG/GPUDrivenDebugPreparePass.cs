using Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug.GPUDrivenBRG;

public class GPUDrivenDebugPreparePass : ScriptableRenderPass<GPUDrivenDebugPreparePassData>
{
	private static class ShaderIDs
	{
		public static readonly int _GPUDrivenDebugFlags = Shader.PropertyToID("_GPUDrivenDebugFlags");

		public static readonly int _CullingStats = Shader.PropertyToID("_CullingStats");

		public static readonly int _OcclusionTestDebug = Shader.PropertyToID("_OcclusionTestDebug");

		public static readonly int _OcclusionTestDebugSize = Shader.PropertyToID("_OcclusionTestDebugSize");
	}

	private static class Profiling
	{
		public static readonly ProfilingSampler ClearOcclusionTestBuffer = new ProfilingSampler("Clear Occlusion Test Buffer");

		public static readonly ProfilingSampler ClearCullingStats = new ProfilingSampler("Clear Culling Stats");
	}

	private const int kOcclusionTestDebugSize = 25;

	private readonly WaaaghDebugData m_DebugData;

	public override string Name => "GPUDrivenDebug.PreparePass";

	public GPUDrivenDebugPreparePass(RenderPassEvent evt, WaaaghDebugData debugData)
		: base(evt)
	{
		m_DebugData = debugData;
	}

	protected override void Setup(RenderGraphBuilder builder, GPUDrivenDebugPreparePassData data, ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		RenderGraph renderGraph = waaaghRenderingData.RenderGraph;
		data.BRG = waaaghRenderingData.GPUDrivenBatchRendererGroup;
		GPUDrivenCullingPassSharedData sharedPassData = data.BRG.SharedPassData;
		if (m_DebugData.GPUDrivenBRGDebug.CullingStats)
		{
			GPUDrivenCullingPassSharedData.UsedBuffers buffers = sharedPassData.Buffers;
			BufferDesc desc = new BufferDesc(6, 4, GraphicsBuffer.Target.Raw)
			{
				name = "CullingStats"
			};
			buffers.CullingStats = renderGraph.CreateBuffer(in desc);
			builder.WriteBuffer(in sharedPassData.Buffers.CullingStats);
		}
		else
		{
			sharedPassData.Buffers.CullingStats = default(BufferHandle);
		}
		if (data.BRG.Settings.OcclusionCulling && m_DebugData.GPUDrivenBRGDebug.ShowOcclusionTest)
		{
			GPUDrivenCullingPassSharedData.UsedBuffers buffers2 = sharedPassData.Buffers;
			BufferDesc desc = new BufferDesc(625, 4, GraphicsBuffer.Target.Raw)
			{
				name = "MainViewOcclusionTestDebug"
			};
			BufferHandle input = renderGraph.CreateBuffer(in desc);
			buffers2.MainViewOcclusionTestDebug = builder.WriteBuffer(in input);
		}
		else
		{
			sharedPassData.Buffers.MainViewOcclusionTestDebug = default(BufferHandle);
		}
		builder.AllowPassCulling(value: false);
	}

	protected override void Render(GPUDrivenDebugPreparePassData data, RenderGraphContext context)
	{
		GPUDrivenDebugFlags gPUDrivenDebugFlags = GPUDrivenDebugFlags.None;
		if (data.BRG.Settings.OcclusionCulling && m_DebugData.GPUDrivenBRGDebug.ShowOcclusionTest)
		{
			gPUDrivenDebugFlags |= GPUDrivenDebugFlags.ShowOcclusionTest;
		}
		context.cmd.SetGlobalInt(ShaderIDs._GPUDrivenDebugFlags, (int)gPUDrivenDebugFlags);
		BufferHandle cullingStats = data.BRG.SharedPassData.Buffers.CullingStats;
		if (cullingStats.IsValid())
		{
			using (new ProfilingScope(context.cmd, Profiling.ClearCullingStats))
			{
				data.BRG.BufferUtils.DispatchClearBuffer(context.cmd, cullingStats, 0);
				context.cmd.SetGlobalBuffer(ShaderIDs._CullingStats, cullingStats);
			}
		}
		BufferHandle mainViewOcclusionTestDebug = data.BRG.SharedPassData.Buffers.MainViewOcclusionTestDebug;
		if (mainViewOcclusionTestDebug.IsValid())
		{
			using (new ProfilingScope(context.cmd, Profiling.ClearOcclusionTestBuffer))
			{
				data.BRG.BufferUtils.DispatchClearBuffer(context.cmd, mainViewOcclusionTestDebug, 0);
				context.cmd.SetGlobalBuffer(ShaderIDs._OcclusionTestDebug, mainViewOcclusionTestDebug);
				context.cmd.SetGlobalVector(ShaderIDs._OcclusionTestDebugSize, new Vector4(25f, 25f));
			}
		}
	}
}
