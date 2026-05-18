using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.Shadows;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.Debugging;

public static class ShadowsDebug
{
	public class ShadowsDebugPassData
	{
		internal ShadowManager ShadowManager;

		public WaaaghDebugData DebugData;

		public Material ShadowsDebugMaterial;

		public float2 ScreenSize;

		public TextureHandle CameraFinalTarget;

		public TextureHandle ShadowBuffer;

		public bool ShadowsCacheEnabled;

		internal NativeQuadTreeDebugger QuadTreeDebugger;
	}

	private static class ShaderPropertyId
	{
		public static readonly int _Debug_ShadowAtlasScaleOffset = Shader.PropertyToID("_Debug_ShadowAtlasScaleOffset");

		public static readonly int _Debug_ShadowAtlasColor = Shader.PropertyToID("_Debug_ShadowAtlasColor");

		public static readonly int _Debug_ShadowAtlasTex = Shader.PropertyToID("_Debug_ShadowAtlasTex");

		public static readonly int _Debug_ShadowAtlasMip = Shader.PropertyToID("_Debug_ShadowAtlasMip");
	}

	public static void Record(in RecordContext context)
	{
		DebugContext debugContext = context.DebugContext;
		if (debugContext.DebugData.ShadowsDebug.ViewAtlas == DebugShadowBufferType.None && debugContext.DebugData.ShadowsDebug.AtlasOccupancy == DebugShadowBufferType.None)
		{
			return;
		}
		ShadowsDebugPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<ShadowsDebugPassData>("DEBUG - Shadows", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\Debugging\\ShadowsDebug.cs", 49);
		passData.QuadTreeDebugger = debugContext.QuadTreeDebugger;
		passData.ShadowManager = context.ShadowData.ShadowManager;
		passData.DebugData = debugContext.DebugData;
		passData.ScreenSize = new float2(context.CameraData.cameraTargetDescriptor.width, context.CameraData.cameraTargetDescriptor.height);
		passData.ShadowsDebugMaterial = debugContext.MaterialLibrary.ShadowsDebug;
		passData.CameraFinalTarget = context.FrameResources.CameraStackTargets.Color;
		unsafeRenderGraphBuilder.UseTexture(in passData.CameraFinalTarget, AccessFlags.Write);
		passData.ShadowsCacheEnabled = context.ShadowData.StaticShadowsCacheEnabled;
		if (passData.DebugData.ShadowsDebug.ViewAtlas != 0)
		{
			switch (passData.DebugData.ShadowsDebug.ViewAtlas)
			{
			case DebugShadowBufferType.ShadowmapAtlas:
				passData.ShadowBuffer = context.FrameResources.Shadows.Shadowmap;
				unsafeRenderGraphBuilder.UseTexture(in passData.ShadowBuffer);
				break;
			case DebugShadowBufferType.CachedShadowmapAtlas:
				if (passData.ShadowsCacheEnabled)
				{
					passData.ShadowBuffer = context.FrameResources.Shadows.CachedShadowmap;
					unsafeRenderGraphBuilder.UseTexture(in passData.ShadowBuffer);
				}
				break;
			}
		}
		unsafeRenderGraphBuilder.SetRenderFunc<ShadowsDebugPassData>(ExecutePass);
	}

	private static void ExecutePass(ShadowsDebugPassData data, UnsafeGraphContext context)
	{
		if (data.DebugData.ShadowsDebug.AtlasOccupancy != 0)
		{
			DrawNativeAtlasOccupancy(data, context);
		}
		if (data.DebugData.ShadowsDebug.ViewAtlas != 0 && (data.DebugData.ShadowsDebug.ViewAtlas != DebugShadowBufferType.CachedShadowmapAtlas || data.ShadowsCacheEnabled))
		{
			float2 screenSize = data.ScreenSize;
			float2 @float = (float2)(math.min(screenSize.x, screenSize.y) * data.DebugData.ShadowsDebug.DebugScale) / screenSize;
			float2 float2 = 1f - @float;
			Vector4 value = new Vector4(@float.x, @float.y, float2.x, float2.y);
			float4 float3 = data.DebugData.ShadowsDebug.DebugColorMultiplier;
			context.cmd.SetGlobalVector(ShaderPropertyId._Debug_ShadowAtlasColor, float3);
			context.cmd.SetGlobalVector(ShaderPropertyId._Debug_ShadowAtlasScaleOffset, value);
			context.cmd.SetGlobalTexture(ShaderPropertyId._Debug_ShadowAtlasTex, data.ShadowBuffer);
			context.cmd.SetGlobalFloat(ShaderPropertyId._Debug_ShadowAtlasMip, 0f);
			CommandBufferHelpers.GetNativeCommandBuffer(context.cmd).Blit(context.defaultResources.whiteTexture, data.CameraFinalTarget, data.ShadowsDebugMaterial, 0);
		}
	}

	private static void DrawNativeAtlasOccupancy(ShadowsDebugPassData data, UnsafeGraphContext context)
	{
		ShadowManager shadowManager = data.ShadowManager;
		ShadowAtlas shadowAtlas = ((data.DebugData.ShadowsDebug.AtlasOccupancy == DebugShadowBufferType.ShadowmapAtlas) ? shadowManager.ShadowMapAtlas : shadowManager.CachedShadowMapAtlas);
		if (shadowAtlas != null)
		{
			data.QuadTreeDebugger.Refresh(shadowAtlas.Allocator.QuadTree, data.DebugData.ShadowsDebug.AtlasNodesPartiallyOccupied, data.DebugData.ShadowsDebug.AtlasNodesOccupied, data.DebugData.ShadowsDebug.AtlasNodesOccupiedInHierarchy);
			Texture2D allocationTexture = data.QuadTreeDebugger.AllocationTexture;
			int levels = shadowAtlas.Allocator.Levels;
			float num = data.ScreenSize.x / (float)levels;
			float2 screenSize = data.ScreenSize;
			float2 @float = num / screenSize;
			float2 float2 = (num - 5f) / screenSize;
			float2 float3 = -1f + @float;
			for (int i = 0; i < levels; i++)
			{
				Vector4 value = new Vector4(float2.x, float2.y, float3.x + (float)i * @float.x * 2f, float3.y);
				Vector4 value2 = new Vector4(1f, 1f, 1f, 1f);
				context.cmd.SetGlobalVector(ShaderPropertyId._Debug_ShadowAtlasColor, value2);
				context.cmd.SetGlobalVector(ShaderPropertyId._Debug_ShadowAtlasScaleOffset, value);
				context.cmd.SetGlobalTexture(ShaderPropertyId._Debug_ShadowAtlasTex, allocationTexture);
				context.cmd.SetGlobalFloat(ShaderPropertyId._Debug_ShadowAtlasMip, i);
				CommandBufferHelpers.GetNativeCommandBuffer(context.cmd).Blit(context.defaultResources.whiteTexture, data.CameraFinalTarget, data.ShadowsDebugMaterial, 0);
			}
		}
	}
}
