using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.Debugging;

public static class GpuDrivenDebug
{
	private static class ShaderIDs
	{
		public static readonly int _GPUDrivenDebugFlags = Shader.PropertyToID("_GPUDrivenDebugFlags");

		public static readonly int _CullingStats = Shader.PropertyToID("_CullingStats");

		public static readonly int _OcclusionTestDebug = Shader.PropertyToID("_OcclusionTestDebug");

		public static readonly int _OcclusionTestDebugSize = Shader.PropertyToID("_OcclusionTestDebugSize");

		public static int _OcclusionTestCountRange = Shader.PropertyToID("_OcclusionTestCountRange");

		public static int _OcclusionTestOpacity = Shader.PropertyToID("_OcclusionTestOpacity");
	}

	private static class Profiling
	{
		public static readonly ProfilingSampler ClearOcclusionTestBuffer = new ProfilingSampler("Clear Occlusion Test Buffer");

		public static readonly ProfilingSampler ClearCullingStats = new ProfilingSampler("Clear Culling Stats");

		public static readonly ProfilingSampler CullingStatsReadback = new ProfilingSampler("Culling Stats Readback");
	}

	public class GpuDrivenPassDataBase
	{
		public GPUDrivenBatchRendererGroup BRG;

		public WaaaghDebugData DebugData;
	}

	public class GPUDrivenDebugShowOcclusionTestPassData
	{
		public TextureHandle CameraFinalTarget;

		public Material Material;

		public int Pass;
	}

	private const int kOcclusionTestDebugSize = 25;

	public static void Prepare(in RecordContext context)
	{
		if (context.GPUDrivenBatchRendererGroup.IsEnabledAndInitialized)
		{
			PreparePass(in context);
		}
	}

	public static void DrawOverlayAndFinish(in RecordContext context)
	{
		if (context.GPUDrivenBatchRendererGroup.IsEnabledAndInitialized)
		{
			if (context.GPUDrivenBatchRendererGroup.Settings.OcclusionCulling && context.DebugContext.DebugData.GPUDrivenBRGDebug.ShowOcclusionTest)
			{
				ShowOcclusionTestPass(in context);
			}
			FinishPass(in context);
		}
	}

	private static void PreparePass(in RecordContext context)
	{
		GpuDrivenPassDataBase passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<GpuDrivenPassDataBase>("DEBUG - BRG Prepare", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\Debugging\\GpuDrivenDebug.cs", 68);
		RenderGraph renderGraph = context.RenderGraph;
		passData.BRG = context.GPUDrivenBatchRendererGroup;
		GPUDrivenCullingPassSharedData sharedPassData = passData.BRG.SharedPassData;
		WaaaghDebugData waaaghDebugData = (passData.DebugData = context.DebugContext.DebugData);
		if (waaaghDebugData.GPUDrivenBRGDebug.CullingStats)
		{
			GPUDrivenCullingPassSharedData.UsedBuffers buffers = sharedPassData.Buffers;
			BufferDesc desc = new BufferDesc(6, 4, GraphicsBuffer.Target.Raw)
			{
				name = "CullingStats"
			};
			buffers.CullingStats = renderGraph.CreateBuffer(in desc);
			unsafeRenderGraphBuilder.UseBuffer(in sharedPassData.Buffers.CullingStats, AccessFlags.Write);
		}
		else
		{
			sharedPassData.Buffers.CullingStats = default(BufferHandle);
		}
		if (passData.BRG.Settings.OcclusionCulling && waaaghDebugData.GPUDrivenBRGDebug.ShowOcclusionTest)
		{
			GPUDrivenCullingPassSharedData.UsedBuffers buffers2 = sharedPassData.Buffers;
			BufferDesc desc = new BufferDesc(625, 4, GraphicsBuffer.Target.Raw)
			{
				name = "MainViewOcclusionTestDebug"
			};
			buffers2.MainViewOcclusionTestDebug = renderGraph.CreateBuffer(in desc);
			unsafeRenderGraphBuilder.UseBuffer(in sharedPassData.Buffers.MainViewOcclusionTestDebug, AccessFlags.Write);
		}
		else
		{
			sharedPassData.Buffers.MainViewOcclusionTestDebug = default(BufferHandle);
		}
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(GpuDrivenPassDataBase data, UnsafeGraphContext context)
		{
			CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
			GPUDrivenDebugFlags gPUDrivenDebugFlags = GPUDrivenDebugFlags.None;
			if (data.BRG.Settings.OcclusionCulling && data.DebugData.GPUDrivenBRGDebug.ShowOcclusionTest)
			{
				gPUDrivenDebugFlags |= GPUDrivenDebugFlags.ShowOcclusionTest;
			}
			context.cmd.SetGlobalInt(ShaderIDs._GPUDrivenDebugFlags, (int)gPUDrivenDebugFlags);
			BufferHandle cullingStats = data.BRG.SharedPassData.Buffers.CullingStats;
			if (cullingStats.IsValid())
			{
				using (new ProfilingScope(context.cmd, Profiling.ClearCullingStats))
				{
					data.BRG.BufferUtils.DispatchClearBuffer(nativeCommandBuffer, cullingStats, 0);
					context.cmd.SetGlobalBuffer(ShaderIDs._CullingStats, cullingStats);
				}
			}
			BufferHandle mainViewOcclusionTestDebug = data.BRG.SharedPassData.Buffers.MainViewOcclusionTestDebug;
			if (mainViewOcclusionTestDebug.IsValid())
			{
				using (new ProfilingScope(context.cmd, Profiling.ClearOcclusionTestBuffer))
				{
					data.BRG.BufferUtils.DispatchClearBuffer(nativeCommandBuffer, mainViewOcclusionTestDebug, 0);
					context.cmd.SetGlobalBuffer(ShaderIDs._OcclusionTestDebug, mainViewOcclusionTestDebug);
					context.cmd.SetGlobalVector(ShaderIDs._OcclusionTestDebugSize, new Vector4(25f, 25f));
				}
			}
		});
	}

	private static void FinishPass(in RecordContext context)
	{
		GpuDrivenPassDataBase passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<GpuDrivenPassDataBase>("DEBUG - BRG Finish Pass", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\Debugging\\GpuDrivenDebug.cs", 141);
		passData.BRG = context.GPUDrivenBatchRendererGroup;
		GPUDrivenCullingPassSharedData sharedPassData = passData.BRG.SharedPassData;
		if (sharedPassData.Buffers.CullingStats.IsValid())
		{
			unsafeRenderGraphBuilder.UseBuffer(in sharedPassData.Buffers.CullingStats);
		}
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(GpuDrivenPassDataBase data, UnsafeGraphContext context)
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
		});
	}

	private static void ShowOcclusionTestPass(in RecordContext context)
	{
		GPUDrivenDebugShowOcclusionTestPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<GPUDrivenDebugShowOcclusionTestPassData>("DEBUG - BRG Show Occlusion Test", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\Debugging\\GpuDrivenDebug.cs", 175);
		passData.Material = context.DebugContext.MaterialLibrary.GpuDrivenDebug;
		passData.Pass = context.DebugContext.MaterialLibrary.ShowOcclusionTestPass;
		passData.CameraFinalTarget = context.FrameResources.CameraStackTargets.Color;
		unsafeRenderGraphBuilder.UseTexture(in passData.CameraFinalTarget, AccessFlags.ReadWrite);
		unsafeRenderGraphBuilder.UseBuffer(in context.GPUDrivenBatchRendererGroup.SharedPassData.Buffers.MainViewOcclusionTestDebug);
		passData.Material.SetFloat(ShaderIDs._OcclusionTestCountRange, math.max(1, context.DebugContext.DebugData.GPUDrivenBRGDebug.OcclusionTestCountRange));
		passData.Material.SetFloat(ShaderIDs._OcclusionTestOpacity, math.saturate(context.DebugContext.DebugData.GPUDrivenBRGDebug.OcclusionTestOpacity));
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(GPUDrivenDebugShowOcclusionTestPassData data, UnsafeGraphContext context)
		{
			CommandBufferHelpers.GetNativeCommandBuffer(context.cmd).Blit(context.defaultResources.whiteTexture, data.CameraFinalTarget, data.Material, data.Pass);
		});
	}
}
