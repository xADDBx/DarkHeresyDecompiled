using System;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Shadows;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class ShadowsDrawer
{
	private static class PropertyId
	{
		public static readonly int _OutputNormalizedViewport = Shader.PropertyToID("_OutputNormalizedViewport");

		public static readonly int _InputDepthTex = Shader.PropertyToID("_InputDepthTex");
	}

	private static class ProfilerMarkers
	{
		public static readonly ProfilerMarker s_ShadowCacheAtlasRenderMarker = new ProfilerMarker("Shadow Cache Atlas (Draw)");

		public static readonly ProfilerMarker s_ShadowCacheAtlasRenderMarkerFrameDebugger = new ProfilerMarker("Shadow Cache Atlas - ALWAYS DRAW (because FrameDebugger)");

		public static readonly ProfilerMarker s_ShadowAtlasCopyMarker = new ProfilerMarker("Shadow Atlas (Copy From Cache)");

		public static readonly ProfilerMarker s_ShadowAtlasRenderMarker = new ProfilerMarker("Shadow Atlas (Draw)");
	}

	private class ShadowCasterPassData
	{
		public TextureHandle CachedShadowMapAtlasTexture;

		public TextureHandle ShadowMapAtlasTexture;

		public NativeList<ShadowRenderRequest> RenderRequestsForCache;

		public NativeList<ShadowRenderRequest> RenderRequests;

		public NativeList<ShadowCacheCopyRequest> ShadowCacheCopyRequests;

		public ShadowQuality ShadowQuality;

		public ShadowManager ShadowManager;

		public bool CacheEnabled;

		public Material CopyShadowsMaterial;
	}

	public static bool ShouldDraw(in RecordContext context)
	{
		bool num = context.ShadowData.ShadowQuality == ShadowQuality.Disable;
		bool flag = Mathf.Approximately(context.CameraData.maxShadowDistance, 0f);
		if (!num)
		{
			return !flag;
		}
		return false;
	}

	public unsafe static void FillGlobalShaderVariables(ref WaaaghShaderVariablesGlobal g, WaaaghShadowData shadowData, WaaaghCameraData cameraData)
	{
		float num = (float)shadowData.AtlasSize;
		g._ShadowAtlasSize = new float4(1f / num, 1f / num, num, num);
		g._GlobalShadowsEnabled = ((cameraData.maxShadowDistance > 0f) ? 1f : 0f);
		g._ShadowReceiverNormalBias = shadowData.ReceiverNormalBias;
		g._DirectionalCascadesCount = shadowData.DirectionalLightCascades?.Count ?? 0;
		GetScaleAndBiasForLinearDistanceFade(cameraData.maxShadowDistance, out var scale, out var bias);
		g._ShadowFadeDistanceScaleAndBias = new float2(scale, bias);
		Vector4[] faceVectors = shadowData.ShadowManager.FaceVectors;
		fixed (float* ptr = g._FaceVectors)
		{
			for (int i = 0; i < 4; i++)
			{
				ptr[i * 4] = faceVectors[i].x;
				ptr[i * 4 + 1] = faceVectors[i].y;
				ptr[i * 4 + 2] = faceVectors[i].z;
			}
		}
	}

	public static void ShadowCasterPass(in RecordContext context)
	{
		ShadowCasterPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<ShadowCasterPassData>("ShadowCasterPass", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\ShadowsDrawer.cs", 79);
		passData.ShadowManager = context.ShadowData.ShadowManager;
		passData.CopyShadowsMaterial = context.MaterialLibrary.CopyCachedShadowsMaterial;
		passData.ShadowMapAtlasTexture = context.FrameResources.Shadows.Shadowmap;
		unsafeRenderGraphBuilder.UseTexture(in passData.ShadowMapAtlasTexture, AccessFlags.Write);
		WaaaghShadowData shadowData = context.ShadowData;
		ShadowManager shadowManager = shadowData.ShadowManager;
		passData.RenderRequestsForCache = shadowManager.RenderRequestsForCache;
		passData.RenderRequests = shadowManager.RenderRequests;
		passData.ShadowCacheCopyRequests = shadowManager.ShadowCacheCopyRequests;
		passData.ShadowQuality = shadowData.ShadowQuality;
		passData.ShadowManager = shadowData.ShadowManager;
		passData.CacheEnabled = shadowData.StaticShadowsCacheEnabled;
		if (passData.CacheEnabled)
		{
			passData.CachedShadowMapAtlasTexture = context.FrameResources.Shadows.CachedShadowmap;
			unsafeRenderGraphBuilder.UseTexture(in passData.CachedShadowMapAtlasTexture, AccessFlags.ReadWrite);
			passData.CopyShadowsMaterial = context.MaterialLibrary.CopyCachedShadowsMaterial;
		}
		unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in passData.ShadowMapAtlasTexture, GlobalTextureShaderPropertyId._ShadowmapRT);
		unsafeRenderGraphBuilder.SetRenderFunc<ShadowCasterPassData>(ExecuteShadowCasterPass);
	}

	private static void ExecuteShadowCasterPass(ShadowCasterPassData data, UnsafeGraphContext context)
	{
		BindShadowData(data, context);
		NativeArray<ShadowRenderRequest> nativeArray;
		if (data.CacheEnabled)
		{
			if (data.RenderRequests.Length > 0)
			{
				ProfilerMarker marker = ProfilerMarkers.s_ShadowCacheAtlasRenderMarker;
				if (FrameDebugger.enabled)
				{
					marker = ProfilerMarkers.s_ShadowCacheAtlasRenderMarkerFrameDebugger;
				}
				context.cmd.BeginSample(marker);
				context.cmd.SetRenderTarget(data.CachedShadowMapAtlasTexture);
				nativeArray = data.RenderRequestsForCache.AsArray();
				Span<ShadowRenderRequest> span = nativeArray.AsSpan();
				for (int i = 0; i < span.Length; i++)
				{
					DrawShadows(context, in span[i]);
				}
				context.cmd.EndSample(marker);
			}
			if (data.ShadowCacheCopyRequests.Length > 0)
			{
				context.cmd.BeginSample(ProfilerMarkers.s_ShadowAtlasCopyMarker);
				context.cmd.SetRenderTarget(data.ShadowMapAtlasTexture);
				context.cmd.SetGlobalTexture(PropertyId._InputDepthTex, data.CachedShadowMapAtlasTexture);
				context.cmd.DrawProcedural(Matrix4x4.identity, data.CopyShadowsMaterial, 0, MeshTopology.Quads, 4, data.ShadowCacheCopyRequests.Length);
				context.cmd.EndSample(ProfilerMarkers.s_ShadowAtlasCopyMarker);
			}
		}
		if (data.RenderRequests.Length > 0)
		{
			context.cmd.BeginSample(ProfilerMarkers.s_ShadowAtlasRenderMarker);
			context.cmd.SetRenderTarget(data.ShadowMapAtlasTexture);
			nativeArray = data.RenderRequests.AsArray();
			Span<ShadowRenderRequest> span = nativeArray.AsSpan();
			for (int i = 0; i < span.Length; i++)
			{
				DrawShadows(context, in span[i]);
			}
			context.cmd.EndSample(ProfilerMarkers.s_ShadowAtlasRenderMarker);
		}
	}

	private static void BindShadowData(ShadowCasterPassData data, UnsafeGraphContext context)
	{
		CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.SHADOWS_HARD, data.ShadowQuality == ShadowQuality.HardOnly);
		CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.SHADOWS_SOFT, data.ShadowQuality == ShadowQuality.All);
		context.cmd.SetGlobalTexture(GlobalTextureShaderPropertyId._ShadowmapRT, data.ShadowMapAtlasTexture);
		data.ShadowManager.PushShadowConstantBuffer(context.cmd);
	}

	private static void DrawShadows(UnsafeGraphContext context, in ShadowRenderRequest request)
	{
		if (request.SlopeBias > 0f)
		{
			context.cmd.SetGlobalDepthBias(1f, 5f * request.SlopeBias);
		}
		context.cmd.SetGlobalInt(ShaderPropertyId._ShadowEntryIndex, request.ConstantBufferIndex);
		context.cmd.SetGlobalFloat(ShaderPropertyId._OffsetFactor, request.DepthBias);
		context.cmd.SetGlobalFloat(ShaderPropertyId._OffsetUnits, request.DepthBias);
		switch (request.LightType)
		{
		case LightType.Spot:
			CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.GEOMETRY_CLIP, state: false);
			context.cmd.SetViewport(MakeViewportRect(in request.ShadowMapViewports[0]));
			if (request.NeedClear)
			{
				context.cmd.ClearRenderTarget(clearDepth: true, clearColor: false, Color.black, 1f);
			}
			break;
		case LightType.Point:
			CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.GEOMETRY_CLIP, state: true);
			context.cmd.SetViewport(MakeViewportRect(in request.ShadowMapViewports[0]));
			if (request.NeedClear)
			{
				context.cmd.ClearRenderTarget(clearDepth: true, clearColor: false, Color.black, 1f);
			}
			break;
		case LightType.Directional:
			CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.GEOMETRY_CLIP, state: false);
			break;
		}
		for (int i = 0; i < request.FaceCount; i++)
		{
			if (request.LightType == LightType.Directional)
			{
				context.cmd.SetViewport(MakeViewportRect(in request.ShadowMapViewports[i]));
				context.cmd.ClearRenderTarget(clearDepth: true, clearColor: false, Color.black, 1f);
			}
			context.cmd.SetGlobalInt(ShaderPropertyId._FaceId, i);
			context.cmd.SetGlobalFloat(ShaderPropertyId._ZClip, (request.LightType != LightType.Directional) ? 1 : 0);
			context.cmd.DrawRendererList(request.RendererListArray[i]);
		}
		if (request.SlopeBias > 0f)
		{
			context.cmd.SetGlobalDepthBias(0f, 0f);
		}
	}

	private static void GetScaleAndBiasForLinearDistanceFade(float fadeDistance, out float scale, out float bias)
	{
		float num = 0.9f * fadeDistance;
		scale = 1f / (fadeDistance - num);
		bias = (0f - num) / (fadeDistance - num);
	}

	private static Rect MakeViewportRect(in float4 v)
	{
		return new Rect(v.x, v.y, v.z, v.w);
	}
}
