using System;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ScreenSpaceCloudShadows;

public class ScreenSpaceCloudShadowsRendererFeature : IRendererFeature, IDisposable
{
	private class ScreenSpaceCloudShadowsPassData
	{
		public Material Material;
	}

	private static class ShaderConstants
	{
		public static readonly int _Texture0 = Shader.PropertyToID("_Texture0");

		public static readonly int _Texture1 = Shader.PropertyToID("_Texture1");

		public static readonly int _Texture0ScaleBias = Shader.PropertyToID("_Texture0ScaleBias");

		public static readonly int _Texture1ScaleBias = Shader.PropertyToID("_Texture1ScaleBias");

		public static readonly int _Texture0Color = Shader.PropertyToID("_Texture0Color");

		public static readonly int _Texture1Color = Shader.PropertyToID("_Texture1Color");

		public static readonly int _Intensity = Shader.PropertyToID("_Intensity");
	}

	private ScreenSpaceCloudShadowsRendererFeatureAsset m_Asset;

	private Material m_Material;

	private Owlcat.Runtime.Visual.Overrides.ScreenSpaceCloudShadows m_Settings;

	private bool m_IsActive;

	private Vector2 m_CloudScroll0;

	private Vector2 m_CloudScroll1;

	private int m_FrameId;

	public ScreenSpaceCloudShadowsRendererFeature(ScreenSpaceCloudShadowsRendererFeatureAsset asset)
	{
		m_Asset = asset;
		PostProcessRuntimeShaders renderPipelineSettings = GraphicsSettings.GetRenderPipelineSettings<PostProcessRuntimeShaders>();
		m_Material = CoreUtils.CreateEngineMaterial(renderPipelineSettings.ScreenSpaceCloudShadowsShader);
	}

	public void Dispose()
	{
		if ((bool)m_Material)
		{
			CoreUtils.Destroy(m_Material);
		}
	}

	public void RegisterExtensions(RendererFeatureExtensionRegistry registry)
	{
		registry.AddSetupDelegate(Setup);
		registry.AddRecordDelegate(RecordExtensionPoint.AfterDeferredLighting, Record);
	}

	private void Setup(in SetupContext context)
	{
		VolumeStack stack = VolumeManager.instance.stack;
		m_Settings = stack.GetComponent<Owlcat.Runtime.Visual.Overrides.ScreenSpaceCloudShadows>();
		m_IsActive = m_Settings.IsActive();
		if (m_IsActive && m_FrameId != context.RenderingData.TimeData.FrameId)
		{
			m_CloudScroll0 += m_Settings.Texture0ScrollSpeed.value * Time.deltaTime;
			m_CloudScroll1 += m_Settings.Texture1ScrollSpeed.value * Time.deltaTime;
			m_FrameId = context.RenderingData.TimeData.FrameId;
		}
	}

	private void Record(in RecordContext context)
	{
		if (!m_IsActive)
		{
			return;
		}
		ScreenSpaceCloudShadowsPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<ScreenSpaceCloudShadowsPassData>("Screen Space Cloud Shadows", out passData, WaaaghProfileId.ScreenSpaceCloudShadows.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\ScreenSpaceCloudShadows\\ScreenSpaceCloudShadowsRendererFeature.cs", 77);
		passData.Material = m_Material;
		rasterRenderGraphBuilder.UseTexture(in context.FrameResources.CameraAdditionalTargets.DepthCopy);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraBakedGIRT);
		rasterRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraDepthRT);
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		passData.Material.SetTexture(ShaderConstants._Texture0, m_Settings.Texture0.value);
		passData.Material.SetTexture(ShaderConstants._Texture1, m_Settings.Texture1.value);
		Vector2 value = m_Settings.Texture0Tiling.value;
		passData.Material.SetVector(ShaderConstants._Texture0ScaleBias, new Vector4(value.x, value.y, m_CloudScroll0.x, m_CloudScroll0.y));
		value = m_Settings.Texture1Tiling.value;
		passData.Material.SetVector(ShaderConstants._Texture1ScaleBias, new Vector4(value.x, value.y, m_CloudScroll1.x, m_CloudScroll1.y));
		passData.Material.SetColor(ShaderConstants._Texture0Color, m_Settings.Texture0Color.value);
		passData.Material.SetColor(ShaderConstants._Texture1Color, m_Settings.Texture1Color.value);
		passData.Material.SetFloat(ShaderConstants._Intensity, m_Settings.Intensity.value);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(ScreenSpaceCloudShadowsPassData data, RasterGraphContext context)
		{
			context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3);
		});
	}
}
