using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.IndirectRendering;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.Debugging;

internal static class QuadOverdraw
{
	private sealed class PassData
	{
		public Camera Camera;

		public TextureHandle ColorTexture;

		public TextureHandle DepthTexture;

		public TextureHandle LockUav;

		public TextureHandle OverdrawUav;

		public RendererListHandle DepthRendererList;

		public RendererListHandle OpaqueRendererList;

		public RendererListHandle TransparentRendererList;

		public Material DebugOverdrawMaterial;

		public int DebugOverdrawPassQuadOverdrawBlit;

		public int OverdrawThreshold;

		public CameraType CameraType;

		public bool IsIndirectRenderingEnabled;

		public bool IsSceneViewInPrefabEditMode;

		public int IrsOverrideDebugMaterialPass;
	}

	private static readonly ShaderTagId[] IrsDepthOnlyShaderTags = new ShaderTagId[1] { ShaderConstants.LightModeTags.DepthOnly };

	private static readonly ProfilerMarker s_OpaqueDepthMarker = new ProfilerMarker("Opaque Depth");

	private static readonly ProfilerMarker s_OpaqueOverdrawMarker = new ProfilerMarker("Opaque Overdraw");

	private static readonly ProfilerMarker s_TransparentOverdrawMarker = new ProfilerMarker("Transparent Overdraw");

	public static void Record(in RendererRecordContext context, TextureHandle targetColor, TextureHandle targetDepth)
	{
		PassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("Draw Quad Overdraw Debug", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\Debugging\\QuadOverdraw.cs", 45);
		TextureDesc desc = new TextureDesc(Mathf.CeilToInt((float)context.CameraData.cameraTargetDescriptor.width / 2f), Mathf.CeilToInt((float)context.CameraData.cameraTargetDescriptor.height / 2f));
		desc.format = GraphicsFormat.R32_UInt;
		desc.clearBuffer = true;
		desc.enableRandomWrite = true;
		passData2.Camera = context.CameraData.camera;
		passData2.ColorTexture = targetColor;
		passData2.DepthTexture = targetDepth;
		passData2.LockUav = unsafeRenderGraphBuilder.CreateTransientTexture(in desc);
		passData2.OverdrawUav = unsafeRenderGraphBuilder.CreateTransientTexture(in desc);
		passData2.DepthRendererList = CreateDepthRendererList(in context);
		passData2.OpaqueRendererList = CreateOpaqueRendererList(in context);
		passData2.TransparentRendererList = CreateTransparentRendererList(in context);
		passData2.DebugOverdrawMaterial = context.DebugContext.MaterialLibrary.DebugOverdrawMaterial;
		passData2.DebugOverdrawPassQuadOverdrawBlit = context.DebugContext.MaterialLibrary.DebugOverdrawPassQuadOverdrawBlit;
		passData2.OverdrawThreshold = context.DebugContext.DebugData.RenderingDebug.OverdrawThreshold;
		passData2.CameraType = context.CameraData.cameraType;
		passData2.IsIndirectRenderingEnabled = context.CameraData.IrsData.Enabled;
		passData2.IsSceneViewInPrefabEditMode = context.CameraData.IsSceneViewInPrefabEditMode;
		passData2.IrsOverrideDebugMaterialPass = context.DebugContext.MaterialLibrary.DebugOverdrawPassQuadOverdrawOpaqueIrs;
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.UseRendererList(in passData2.DepthRendererList);
		unsafeRenderGraphBuilder.UseRendererList(in passData2.OpaqueRendererList);
		unsafeRenderGraphBuilder.UseRendererList(in passData2.TransparentRendererList);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData passData, UnsafeGraphContext context)
		{
			context.cmd.SetupCameraProperties(passData.Camera);
			context.cmd.BeginSample(s_OpaqueDepthMarker);
			context.cmd.SetRenderTarget(RenderTargetIdentifier.Invalid, passData.DepthTexture);
			context.cmd.DrawRendererList(passData.DepthRendererList);
			IndirectRenderingSystem.Instance.DrawPass(CommandBufferHelpers.GetNativeCommandBuffer(context.cmd), passData.CameraType, passData.IsIndirectRenderingEnabled, passData.IsSceneViewInPrefabEditMode, debugOverdraw: true, ShaderConstants.RenderQueueRanges.OpaqueAll, IrsDepthOnlyShaderTags);
			context.cmd.EndSample(s_OpaqueDepthMarker);
			context.cmd.SetRandomWriteTarget(1, passData.LockUav);
			context.cmd.SetRandomWriteTarget(2, passData.OverdrawUav);
			context.cmd.BeginSample(s_OpaqueOverdrawMarker);
			context.cmd.DrawRendererList(passData.OpaqueRendererList);
			IndirectRenderingSystem.Instance.DrawPass(CommandBufferHelpers.GetNativeCommandBuffer(context.cmd), passData.CameraType, passData.IsIndirectRenderingEnabled, passData.IsSceneViewInPrefabEditMode, debugOverdraw: true, ShaderConstants.RenderQueueRanges.OpaqueAll, IrsDepthOnlyShaderTags, passData.DebugOverdrawMaterial, passData.IrsOverrideDebugMaterialPass);
			context.cmd.EndSample(s_OpaqueOverdrawMarker);
			context.cmd.BeginSample(s_TransparentOverdrawMarker);
			context.cmd.DrawRendererList(passData.TransparentRendererList);
			context.cmd.EndSample(s_TransparentOverdrawMarker);
			context.cmd.ClearRandomWriteTargets();
			context.cmd.SetGlobalInt("_DebugOverdrawThreshold", passData.OverdrawThreshold);
			CommandBufferHelpers.GetNativeCommandBuffer(context.cmd).Blit(passData.OverdrawUav, passData.ColorTexture, passData.DebugOverdrawMaterial, passData.DebugOverdrawPassQuadOverdrawBlit);
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
		desc.drawSettings.overrideMaterial = context.DebugContext.MaterialLibrary.DebugOverdrawMaterial;
		desc.drawSettings.overrideMaterialPassIndex = context.DebugContext.MaterialLibrary.DebugOverdrawPassQuadOverdrawOpaque;
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
		desc.filteringSettings.batchLayerMask = BatchLayerMask.FromLayers(BatchLayerFlagBits.NonBrg | BatchLayerFlagBits.Default);
		desc.drawSettings.overrideMaterial = context.DebugContext.MaterialLibrary.DebugOverdrawMaterial;
		desc.drawSettings.overrideMaterialPassIndex = context.DebugContext.MaterialLibrary.DebugOverdrawPassQuadOverdrawTransparent;
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
}
