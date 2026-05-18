using System;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Utilities;
using Owlcat.Runtime.Visual.VirtualTexture;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class GlobalStateRecorder
{
	private class InitializeRenderStatePassData
	{
		public bool ShadowmaskEnabled;

		public Vector4 DefaultRTHandleScale;

		internal Vector4 ScaleBiasRt;

		public VirtualTextureManager VTManager;

		public WaaaghLights Lights;

		public float2 GlobalMipBias;

		public ShaderTimeData ShaderTimeData;

		public WaaaghShadowData ShadowData;

		public WaaaghCameraData CameraData;

		public WaaaghReflectionProbes ReflectionProbes;
	}

	private class SetupFogPassData
	{
		public bool IsFogEnabled;

		public FogMode FogMode;

		public Fog FogVolume;
	}

	private const float kInvisibleFogEnd = 1000000f;

	private const float kInvisibleFogStart = 999999f;

	private static readonly Color s_InvisibleFogColor = new Color(1f, 0f, 1f, 1f);

	private static readonly float4 s_InvisibleFogParams = FogUtils.MakeFogLinearParams(999999f, 1000000f);

	public static void InitializeRenderStatePass(in RecordContext context)
	{
		InitializeRenderStatePassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<InitializeRenderStatePassData>("InitializeRenderStatePass", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\GlobalStateRecorder.cs", 31);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		RenderGraphDefaultResources defaultResources = context.RenderGraph.defaultResources;
		WaaaghCameraData cameraData = context.CameraData;
		passData.ShadowmaskEnabled = context.Lights.ShadowmaskEnabled;
		passData.DefaultRTHandleScale = Vector4.one;
		passData.ScaleBiasRt = RenderingUtils.CalculateScaleBiasRt(cameraData);
		passData.VTManager = context.VirtualTextureManager;
		passData.Lights = context.Lights;
		float num = ((cameraData.StackInfo.RequiredTargets == CameraRequiredTargets.Unscaled) ? 0f : Math.Min((float)(0.0 - Math.Log(1f / cameraData.renderScale, 2.0)), 0f));
		passData.GlobalMipBias = new Vector2(num, Mathf.Pow(2f, num));
		passData.ShaderTimeData = context.RenderingData.ShaderTimeData;
		passData.ShadowData = context.ShadowData;
		passData.CameraData = cameraData;
		passData.ReflectionProbes = context.ReflectionProbes;
		TextureHandle input = defaultResources.blackTexture;
		unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in input, GlobalTextureShaderPropertyId._CameraDepthTexture);
		input = defaultResources.blackTexture;
		unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in input, GlobalTextureShaderPropertyId._CameraDepthRT);
		input = defaultResources.blackTexture;
		unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in input, GlobalTextureShaderPropertyId._ShadowmapRT);
		input = defaultResources.whiteTexture;
		unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in input, GlobalTextureShaderPropertyId._ScreenSpaceOcclusionTexture);
		unsafeRenderGraphBuilder.SetRenderFunc<InitializeRenderStatePassData>(ExecuteInitializeRenderStatePass);
	}

	private static void ExecuteInitializeRenderStatePass(InitializeRenderStatePassData data, UnsafeGraphContext context)
	{
		CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.SHADOWS_SHADOWMASK, data.ShadowmaskEnabled);
		CoreUtils.SetKeyword(context.cmd, "_SCREEN_SPACE_OCCLUSION", state: false);
		context.cmd.SetGlobalFloat(ShaderPropertyId._VolumetricLightingEnabled, 0f);
		context.cmd.SetGlobalVector(ShaderPropertyId._RTHandleScale, data.DefaultRTHandleScale);
		context.cmd.SetGlobalVector(ShaderPropertyId._ScaleBiasRt, data.ScaleBiasRt);
		CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
		data.VTManager?.PushGlobalTextures(nativeCommandBuffer);
		WaaaghShaderVariablesGlobal g = default(WaaaghShaderVariablesGlobal);
		data.Lights.FillZBins(ref g);
		data.VTManager?.FillGlobalShaderVariables(ref g, in data.GlobalMipBias);
		Translucency.FillGlobalShaderVariables(ref g);
		ShadowsDrawer.FillGlobalShaderVariables(ref g, data.ShadowData, data.CameraData);
		data.ReflectionProbes.FillGlobalShaderVariables(ref g);
		ConstantBuffer.PushGlobal(nativeCommandBuffer, in g, ShaderPropertyId.WaaaghShaderVariablesGlobal);
		data.ShaderTimeData.PushGlobal(context.cmd);
		Terrain.ConfigureTerrainTransition(context.cmd);
	}

	public static void SetupFogPass(in RecordContext context)
	{
		SetupFogPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<SetupFogPassData>("SetupFogPass", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\GlobalStateRecorder.cs", 113);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		passData.IsFogEnabled = RenderSettings.fog;
		passData.FogMode = RenderSettings.fogMode;
		passData.FogVolume = VolumeManager.instance.stack.GetComponent<Fog>();
		unsafeRenderGraphBuilder.SetRenderFunc<SetupFogPassData>(ExecuteSetupFogPass);
	}

	private static void ExecuteSetupFogPass(SetupFogPassData data, UnsafeGraphContext context)
	{
		if (data.IsFogEnabled && data.FogMode == FogMode.Linear)
		{
			Color fogColor;
			float4 fogParams;
			if (data.FogVolume.IsActive())
			{
				fogColor = CoreUtils.ConvertSRGBToActiveColorSpace(data.FogVolume.Color.value);
				fogParams = FogUtils.MakeFogLinearParams(data.FogVolume.StartDistance.value, data.FogVolume.EndDistance.value);
			}
			else
			{
				fogColor = CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.fogColor);
				fogParams = FogUtils.MakeFogLinearParams(RenderSettings.fogStartDistance, RenderSettings.fogEndDistance);
			}
			FogUtils.SetupFogProperties(context.cmd, in fogColor, in fogParams);
		}
		else
		{
			FogUtils.SetupFogMode(context.cmd, FogMode.Linear);
			FogUtils.SetupFogProperties(context.cmd, in s_InvisibleFogColor, in s_InvisibleFogParams);
		}
	}
}
