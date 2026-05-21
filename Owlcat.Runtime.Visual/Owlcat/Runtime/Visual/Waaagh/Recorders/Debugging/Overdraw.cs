using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.IndirectRendering;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.Debugging;

internal static class Overdraw
{
	private sealed class PassData
	{
		public Camera Camera;

		public TextureHandle ColorTexture;

		public TextureHandle DepthTexture;

		public TextureHandle HeatmapTexture;

		public RendererListHandle DepthRendererList;

		public RendererListHandle OpaqueRendererList;

		public RendererListHandle TransparentRendererList;

		public Material DebugOverdrawMaterial;

		public int DebugOverdrawPassOverdrawBlit;

		public int OverdrawThreshold;

		public CameraType CameraType;

		public bool IsIndirectRenderingEnabled;

		public bool IsSceneViewInPrefabEditMode;
	}

	private static readonly ShaderTagId[] IrsDepthOnlyShaderTags = new ShaderTagId[1] { ShaderConstants.LightModeTags.DepthOnly };

	private static readonly ShaderTagId[] IrsOpaqueShaderTags = new ShaderTagId[1] { ShaderConstants.LightModeTags.ForwardLit };

	private static readonly Color s_DebugColor = new Color(1f, 0f, 0f, 0f);

	private static readonly ProfilerMarker s_OpaqueDepthMarker = new ProfilerMarker("Opaque Depth");

	private static readonly ProfilerMarker s_OpaqueOverdrawMarker = new ProfilerMarker("Opaque Overdraw");

	private static readonly ProfilerMarker s_TransparentOverdrawMarker = new ProfilerMarker("Transparent Overdraw");

	public static void Record(in RendererRecordContext context, TextureHandle targetColor, TextureHandle targetDepth)
	{
		PassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("Draw Overdraw Debug", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\Debugging\\Overdraw.cs", 45);
		TextureDesc desc = new TextureDesc(context.CameraData.cameraTargetDescriptor.width, context.CameraData.cameraTargetDescriptor.height);
		desc.format = GraphicsFormat.R16_SFloat;
		desc.clearBuffer = true;
		passData2.Camera = context.CameraData.camera;
		passData2.ColorTexture = targetColor;
		passData2.DepthTexture = targetDepth;
		passData2.HeatmapTexture = unsafeRenderGraphBuilder.CreateTransientTexture(in desc);
		passData2.DepthRendererList = CreateDepthRendererList(in context);
		passData2.OpaqueRendererList = CreateOpaqueRendererList(in context);
		passData2.TransparentRendererList = CreateTransparentRendererList(in context);
		passData2.DebugOverdrawMaterial = context.DebugContext.MaterialLibrary.DebugOverdrawMaterial;
		passData2.DebugOverdrawPassOverdrawBlit = context.DebugContext.MaterialLibrary.DebugOverdrawPassOverdrawBlit;
		passData2.OverdrawThreshold = context.DebugContext.DebugData.RenderingDebug.OverdrawThreshold;
		passData2.CameraType = context.CameraData.cameraType;
		passData2.IsIndirectRenderingEnabled = context.CameraData.IrsData.Enabled;
		passData2.IsSceneViewInPrefabEditMode = context.CameraData.IsSceneViewInPrefabEditMode;
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.UseRendererList(in passData2.DepthRendererList);
		unsafeRenderGraphBuilder.UseRendererList(in passData2.OpaqueRendererList);
		unsafeRenderGraphBuilder.UseRendererList(in passData2.TransparentRendererList);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData passData, UnsafeGraphContext context)
		{
			context.cmd.SetupCameraProperties(passData.Camera);
			context.cmd.SetGlobalInt("_DebugForcedForwardPassColorEnabled", 1);
			context.cmd.SetGlobalColor("_DebugForcedForwardPassColor", s_DebugColor);
			context.cmd.BeginSample(s_OpaqueDepthMarker);
			context.cmd.SetRenderTarget(passData.HeatmapTexture, passData.DepthTexture);
			context.cmd.DrawRendererList(passData.DepthRendererList);
			IndirectRenderingSystem.Instance.DrawPass(CommandBufferHelpers.GetNativeCommandBuffer(context.cmd), passData.CameraType, passData.IsIndirectRenderingEnabled, passData.IsSceneViewInPrefabEditMode, debugOverdraw: true, ShaderConstants.RenderQueueRanges.OpaqueAll, IrsDepthOnlyShaderTags);
			context.cmd.EndSample(s_OpaqueDepthMarker);
			context.cmd.BeginSample(s_OpaqueOverdrawMarker);
			context.cmd.DrawRendererList(passData.OpaqueRendererList);
			IndirectRenderingSystem.Instance.DrawPass(CommandBufferHelpers.GetNativeCommandBuffer(context.cmd), passData.CameraType, passData.IsIndirectRenderingEnabled, passData.IsSceneViewInPrefabEditMode, debugOverdraw: true, ShaderConstants.RenderQueueRanges.OpaqueAll, IrsOpaqueShaderTags);
			context.cmd.EndSample(s_OpaqueOverdrawMarker);
			context.cmd.BeginSample(s_TransparentOverdrawMarker);
			context.cmd.DrawRendererList(passData.TransparentRendererList);
			context.cmd.EndSample(s_TransparentOverdrawMarker);
			context.cmd.SetGlobalInt("_DebugForcedForwardPassColorEnabled", 0);
			context.cmd.SetGlobalInt("_DebugOverdrawThreshold", passData.OverdrawThreshold);
			CommandBufferHelpers.GetNativeCommandBuffer(context.cmd).Blit(passData.HeatmapTexture, passData.ColorTexture, passData.DebugOverdrawMaterial, passData.DebugOverdrawPassOverdrawBlit);
		});
	}

	private static RendererListHandle CreateDepthRendererList(in RendererRecordContext context)
	{
		RendererListParams desc = GetBaseRendererListParams(in context);
		desc.drawSettings.SetShaderPassName(0, ShaderConstants.LightModeTags.DepthOnly);
		desc.filteringSettings.renderQueueRange = ShaderConstants.RenderQueueRanges.OpaqueAll;
		desc.filteringSettings.batchLayerMask = BatchLayerMask.FromLayers(BatchLayerFlagBits.NonBrg | BatchLayerFlagBits.Default);
		return context.RenderGraph.CreateRendererList(in desc);
	}

	private static RendererListHandle CreateOpaqueRendererList(in RendererRecordContext context)
	{
		RendererListParams desc = GetBaseRendererListParams(in context);
		for (int i = 0; i < ShaderConstants.LightModeTags.ForwardAll.Length; i++)
		{
			desc.drawSettings.SetShaderPassName(i, ShaderConstants.LightModeTags.ForwardAll[i]);
		}
		desc.filteringSettings.renderQueueRange = ShaderConstants.RenderQueueRanges.OpaqueAll;
		desc.filteringSettings.batchLayerMask = BatchLayerMask.FromLayers(BatchLayerFlagBits.NonBrg | BatchLayerFlagBits.Default);
		NativeArray<ShaderTagId> value = new NativeArray<ShaderTagId>(1, Allocator.Temp);
		value[0] = ShaderTagId.none;
		desc.tagValues = value;
		NativeArray<RenderStateBlock> value2 = new NativeArray<RenderStateBlock>(1, Allocator.Temp);
		value2[0] = GetOverrideRenderStateBlock(CompareFunction.Equal);
		desc.stateBlocks = value2;
		return context.RenderGraph.CreateRendererList(in desc);
	}

	private static RendererListHandle CreateTransparentRendererList(in RendererRecordContext context)
	{
		RendererListParams desc = GetBaseRendererListParams(in context);
		for (int i = 0; i < ShaderConstants.LightModeTags.ForwardAll.Length; i++)
		{
			desc.drawSettings.SetShaderPassName(i, ShaderConstants.LightModeTags.ForwardAll[i]);
		}
		desc.filteringSettings.renderQueueRange = ShaderConstants.RenderQueueRanges.Transparent;
		desc.filteringSettings.batchLayerMask = 3u;
		NativeArray<ShaderTagId> value = new NativeArray<ShaderTagId>(1, Allocator.Temp);
		value[0] = ShaderTagId.none;
		desc.tagValues = value;
		NativeArray<RenderStateBlock> value2 = new NativeArray<RenderStateBlock>(1, Allocator.Temp);
		value2[0] = GetOverrideRenderStateBlock(CompareFunction.LessEqual);
		desc.stateBlocks = value2;
		return context.RenderGraph.CreateRendererList(in desc);
	}

	private static RendererListParams GetBaseRendererListParams(in RendererRecordContext context)
	{
		SortingSettings sortingSettings = new SortingSettings(context.CameraData.camera);
		sortingSettings.criteria = SortingCriteria.OptimizeStateChanges;
		SortingSettings sortingSettings2 = sortingSettings;
		DrawingSettings drawingSettings = new DrawingSettings(ShaderTagId.none, sortingSettings2);
		drawingSettings.perObjectData = PerObjectData.None;
		drawingSettings.enableDynamicBatching = false;
		drawingSettings.enableInstancing = false;
		DrawingSettings drawSettings = drawingSettings;
		return new RendererListParams(filteringSettings: new FilteringSettings(RenderQueueRange.all), cullingResults: context.RenderingData.CullResults, drawSettings: drawSettings);
	}

	private static RenderStateBlock GetOverrideRenderStateBlock(CompareFunction depthCompare)
	{
		RenderStateBlock result = default(RenderStateBlock);
		result.depthState = new DepthState(writeEnabled: false, depthCompare);
		result.blendState = new BlendState(separateMRTBlend: false, alphaToMask: false)
		{
			blendState0 = new RenderTargetBlendState
			{
				writeMask = ColorWriteMask.All,
				colorBlendOperation = BlendOp.Add,
				alphaBlendOperation = BlendOp.Add,
				sourceColorBlendMode = BlendMode.One,
				sourceAlphaBlendMode = BlendMode.One,
				destinationColorBlendMode = BlendMode.One,
				destinationAlphaBlendMode = BlendMode.Zero
			}
		};
		result.stencilState = new StencilState(enabled: false);
		result.mask = RenderStateMask.Blend | RenderStateMask.Depth | RenderStateMask.Stencil;
		return result;
	}
}
