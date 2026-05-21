using System;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FullscreenBlur;

internal sealed class FullscreenBlurRendererFeature : IRendererFeature, IDisposable
{
	private sealed class PassData
	{
		public FullscreenBlurRendererFeatureAsset Feature;

		public Material BlurMaterial;

		public TextureHandle CameraColorRT;

		public TextureHandle BlurRT0;

		public TextureHandle BlurRT1;
	}

	private readonly FullscreenBlurRendererFeatureAsset m_Asset;

	private readonly Material m_BlurMaterial;

	public FullscreenBlurRendererFeature(FullscreenBlurRendererFeatureAsset asset)
	{
		m_Asset = asset;
		FullscreenBlurFeatureResources renderPipelineSettings = GraphicsSettings.GetRenderPipelineSettings<FullscreenBlurFeatureResources>();
		m_BlurMaterial = CoreUtils.CreateEngineMaterial(renderPipelineSettings.BlurShader);
	}

	public void Dispose()
	{
		CoreUtils.Destroy(m_BlurMaterial);
	}

	public void RegisterExtensions(RendererFeatureExtensionRegistry registry)
	{
		registry.AddRecordDelegate(OnRecord);
	}

	private void OnRecord(RecordExtensionPoint extensionPoint, in RecordContext context)
	{
		if (m_Asset.BlurSize <= 0f || extensionPoint != m_Asset.ExtensionPoint)
		{
			return;
		}
		PassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("Draw Fullscreen Blur", out passData2, WaaaghProfileId.FullscreenBlur.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\FullscreenBlur\\FullscreenBlurRendererFeature.cs", 62);
		passData2.Feature = m_Asset;
		passData2.BlurMaterial = m_BlurMaterial;
		passData2.CameraColorRT = context.FrameResources.CameraStackTargets.Color;
		unsafeRenderGraphBuilder.UseTexture(in passData2.CameraColorRT, AccessFlags.ReadWrite);
		float sizeMod = 1f / (float)passData2.Feature.Downsample;
		PassData passData3 = passData2;
		TextureDesc desc = GetBlurTextureDesc(in context.CameraData, "BlurRT0", sizeMod);
		passData3.BlurRT0 = unsafeRenderGraphBuilder.CreateTransientTexture(in desc);
		PassData passData4 = passData2;
		desc = GetBlurTextureDesc(in context.CameraData, "BlurRT1", sizeMod);
		passData4.BlurRT1 = unsafeRenderGraphBuilder.CreateTransientTexture(in desc);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData passData, UnsafeGraphContext context)
		{
			float num = 1f / (float)passData.Feature.Downsample;
			context.cmd.SetGlobalVector("_Parameter", new Vector4(passData.Feature.BlurSize * num, (0f - passData.Feature.BlurSize) * num, 0f, 0f));
			CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
			nativeCommandBuffer.Blit(passData.CameraColorRT, passData.BlurRT0);
			int num2 = ((passData.Feature.BlurType != 0) ? 2 : 0);
			for (int i = 0; i < passData.Feature.BlurIterations; i++)
			{
				float num3 = (float)i * 1f;
				context.cmd.SetGlobalVector("_Parameter", new Vector4(passData.Feature.BlurSize * num + num3, (0f - passData.Feature.BlurSize) * num - num3, 0f, 0f));
				nativeCommandBuffer.Blit(passData.BlurRT0, passData.BlurRT1, passData.BlurMaterial, 1 + num2);
				nativeCommandBuffer.Blit(passData.BlurRT1, passData.BlurRT0, passData.BlurMaterial, 2 + num2);
			}
			nativeCommandBuffer.Blit(passData.BlurRT0, passData.CameraColorRT);
		});
	}

	private static TextureDesc GetBlurTextureDesc(in WaaaghCameraData cameraData, string name, float sizeMod)
	{
		TextureDesc result = RenderingUtils.CreateTextureDesc(name, cameraData.cameraTargetDescriptor);
		result.width = (int)((float)result.width * sizeMod);
		result.height = (int)((float)result.height * sizeMod);
		result.filterMode = FilterMode.Bilinear;
		result.wrapMode = TextureWrapMode.Clamp;
		return result;
	}
}
