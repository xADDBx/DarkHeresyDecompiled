using System;
using Owlcat.Runtime.Visual.VirtualTexture;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

public class InitializeRenderStatePass : ScriptableRenderPass
{
	private class PassData
	{
		public bool ShadowmaskEnabled;

		public bool GlobalShadowsEnabled;

		public TextureHandle DefaultDepthTexture;

		public Vector4 DefaultRTHandleScale;

		public bool IsVTDisabled;

		internal Vector4 ScaleBiasRt;

		public VirtualTextureManager VTManager;

		public float2 GlobalMipBias;
	}

	private WaaaghLights m_WaaaghLights;

	private static readonly BaseRenderFunc<PassData, RenderGraphContext> s_RenderFunc = ExecutePass;

	public override string Name => "InitializeRenderStatePass";

	public InitializeRenderStatePass(RenderPassEvent evt, WaaaghLights waaaghLights)
		: base(evt)
	{
		m_WaaaghLights = waaaghLights;
	}

	public override void RecordRenderGraph(ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		PassData passData;
		RenderGraphBuilder renderGraphBuilder = waaaghRenderingData.RenderGraph.AddRenderPass<PassData>(Name, out passData, ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\Passes\\Base\\InitializeRenderStatePass.cs", 42);
		try
		{
			RenderGraphDefaultResources defaultResources = waaaghRenderingData.RenderGraph.defaultResources;
			bool flag = true;
			PassData passData2 = passData;
			TextureHandle defaultDepthTexture;
			if (!flag)
			{
				TextureHandle input = defaultResources.whiteTexture;
				defaultDepthTexture = renderGraphBuilder.ReadTexture(in input);
			}
			else
			{
				TextureHandle input2 = defaultResources.blackTexture;
				defaultDepthTexture = renderGraphBuilder.ReadTexture(in input2);
			}
			passData2.DefaultDepthTexture = defaultDepthTexture;
			passData.ShadowmaskEnabled = m_WaaaghLights.ShadowmaskEnabled;
			passData.GlobalShadowsEnabled = waaaghCameraData.maxShadowDistance > 0f;
			passData.DefaultRTHandleScale = Vector4.one;
			passData.ScaleBiasRt = RenderingUtils.CalculateScaleBiasRt(waaaghCameraData);
			bool flag2 = waaaghCameraData.cameraType != CameraType.SceneView && waaaghCameraData.cameraType != CameraType.Game && waaaghCameraData.cameraType != CameraType.Reflection;
			passData.IsVTDisabled = flag2 || !waaaghRenderingData.VirtualTextureManager.VTEnabledGlobal;
			passData.VTManager = waaaghRenderingData.VirtualTextureManager;
			float num = ((waaaghCameraData.CameraRenderTargetBufferType == CameraRenderTargetType.Scaled) ? Math.Min((float)(0.0 - Math.Log(1f / waaaghCameraData.renderScale, 2.0)), 0f) : 0f);
			passData.GlobalMipBias = new Vector2(num, Mathf.Pow(2f, num));
			renderGraphBuilder.SetRenderFunc(s_RenderFunc);
		}
		finally
		{
			((IDisposable)renderGraphBuilder).Dispose();
		}
	}

	private static void ExecutePass(PassData passData, RenderGraphContext context)
	{
		CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.SHADOWS_SHADOWMASK, passData.ShadowmaskEnabled);
		CoreUtils.SetKeyword(context.cmd, "_SCREEN_SPACE_OCCLUSION", state: false);
		CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.VT_DISABLED_GLOBAL, passData.IsVTDisabled);
		context.cmd.SetGlobalFloat(ShaderPropertyId._VolumetricLightingEnabled, 0f);
		context.cmd.SetGlobalFloat(ShaderPropertyId._GlobalShadowsEnabled, passData.GlobalShadowsEnabled ? 1 : 0);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDepthRT, passData.DefaultDepthTexture);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDepthTexture, passData.DefaultDepthTexture);
		context.cmd.SetGlobalTexture(ShaderPropertyId._ShadowmapRT, passData.DefaultDepthTexture);
		context.cmd.SetGlobalVector(ShaderPropertyId._RTHandleScale, passData.DefaultRTHandleScale);
		context.cmd.SetGlobalVector(ShaderPropertyId._ScaleBiasRt, passData.ScaleBiasRt);
		passData.VTManager?.PushGlobals(context.cmd, in passData.GlobalMipBias);
	}
}
