using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes.PostProcess;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class FinalBlitPass : ScriptableRenderPass<FinalBlitPass.PassData>
{
	public class PassData : PassDataBase
	{
		public BlitMaterialData BlitMaterialData;

		public bool RequireSRGBConversion;

		public bool EnableAlphaOutput;

		public TextureHandle Source;

		public TextureHandle Destination;

		public Vector4 ScaleBias;

		public WaaaghCameraData CameraData;

		public bool RenderToBackBuffer;

		public Rect CameraPixelRect;

		public AccessibilityPostProcessing.Parameters AccessibilityParameters;
	}

	private static class BlitPassNames
	{
		public const string NearestSampler = "NearestDebugDraw";

		public const string BilinearSampler = "BilinearDebugDraw";
	}

	public struct BlitMaterialData
	{
		public Material Material;

		public int NearestSamplerPass;

		public int BilinearSamplerPass;
	}

	private enum BlitType
	{
		Core,
		HDR,
		Count
	}

	private BlitMaterialData[] m_BlitMaterialData;

	public override string Name => "FinalBlitPass";

	public FinalBlitPass(RenderPassEvent evt, Material blitMaterial, Material blitHDRMaterial)
		: base(evt)
	{
		m_BlitMaterialData = new BlitMaterialData[2];
		for (int i = 0; i < 2; i++)
		{
			m_BlitMaterialData[i].Material = ((i == 0) ? blitMaterial : blitHDRMaterial);
			m_BlitMaterialData[i].NearestSamplerPass = m_BlitMaterialData[i].Material?.FindPass("NearestDebugDraw") ?? (-1);
			m_BlitMaterialData[i].BilinearSamplerPass = m_BlitMaterialData[i].Material?.FindPass("BilinearDebugDraw") ?? (-1);
		}
	}

	private void InitPassData(WaaaghCameraData cameraData, ref PassData passData, BlitType blitType, bool enableAlphaOutput)
	{
		passData.RequireSRGBConversion = cameraData.requireSrgbConversion;
		passData.EnableAlphaOutput = enableAlphaOutput;
		passData.BlitMaterialData = m_BlitMaterialData[(int)blitType];
	}

	protected override void Setup(RenderGraphBuilder builder, PassData passData, ContextContainer frameData)
	{
		WaaaghCameraData cameraData = frameData.Get<WaaaghCameraData>();
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		bool isHDROutputActive = cameraData.isHDROutputActive;
		bool isAlphaOutputEnabled = cameraData.isAlphaOutputEnabled;
		InitPassData(cameraData, ref passData, isHDROutputActive ? BlitType.HDR : BlitType.Core, isAlphaOutputEnabled);
		PassData passData2 = passData;
		TextureHandle input = waaaghResourceData.CameraResolveColorBuffer;
		passData2.Destination = builder.UseColorBuffer(in input, 0);
		PassData passData3 = passData;
		input = waaaghResourceData.CameraColorBuffer;
		passData3.Source = builder.ReadTexture(in input);
		passData.CameraData = cameraData;
		passData.RenderToBackBuffer = !cameraData.isSceneViewCamera;
		passData.CameraPixelRect = cameraData.pixelRect;
		passData.AccessibilityParameters = AccessibilityPostProcessing.GetParameters(in cameraData);
		builder.AllowPassCulling(value: false);
	}

	protected override void Render(PassData data, RenderGraphContext context)
	{
		int bilinearSamplerPass = data.BlitMaterialData.BilinearSamplerPass;
		Vector4 finalBlitScaleBias = RenderingUtils.GetFinalBlitScaleBias(ref data.Destination, data.CameraData);
		if (data.RenderToBackBuffer)
		{
			context.cmd.SetViewport(data.CameraPixelRect);
		}
		context.cmd.SetKeyword(in ShaderGlobalKeywords._LINEAR_TO_SRGB_CONVERSION, data.RequireSRGBConversion);
		AccessibilityPostProcessing.SetupGlobalShaderParameters(context.cmd, in data.AccessibilityParameters);
		Blitter.BlitTexture(context.cmd, data.Source, finalBlitScaleBias, data.BlitMaterialData.Material, bilinearSamplerPass);
	}
}
