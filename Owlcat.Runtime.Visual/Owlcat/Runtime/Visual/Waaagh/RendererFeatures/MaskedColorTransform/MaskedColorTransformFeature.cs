using System;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.MaskedColorTransform;

public class MaskedColorTransformFeature : IRendererFeature, IDisposable
{
	private class MaskedColorTransformPassData
	{
		public Material Material;
	}

	private static class ShaderConstants
	{
		public static readonly int _MaskedColorTransformParams = Shader.PropertyToID("_MaskedColorTransformParams");
	}

	private MaskedColorTransformFeatureAsset m_Asset;

	private Material m_Material;

	private Owlcat.Runtime.Visual.Overrides.MaskedColorTransform m_Settings;

	private bool m_IsActive;

	public MaskedColorTransformFeature(MaskedColorTransformFeatureAsset asset)
	{
		m_Asset = asset;
		PostProcessRuntimeShaders renderPipelineSettings = GraphicsSettings.GetRenderPipelineSettings<PostProcessRuntimeShaders>();
		m_Material = CoreUtils.CreateEngineMaterial(renderPipelineSettings.MaskedColorTransformPS);
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
		m_Settings = stack.GetComponent<Owlcat.Runtime.Visual.Overrides.MaskedColorTransform>();
		m_IsActive = m_Settings != null && m_Settings.IsActive();
	}

	private void Record(in RecordContext context)
	{
		if (!m_IsActive)
		{
			return;
		}
		MaskedColorTransformPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<MaskedColorTransformPassData>("Masked Color Transform", out passData, WaaaghProfileId.MaskedColorTransform.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\MaskedColorTransform\\MaskedColorTransformFeature.cs", 62);
		rasterRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(context.FrameResources.CameraStackTargets.Depth);
		passData.Material = m_Material;
		passData.Material.SetFloat(ShaderPropertyId._StencilRef, (float)m_Settings.StencilRef.value);
		var (x, y) = AccessibilityPostProcessing.BuildBrightnessContrastMad(m_Settings.Brightness.value, m_Settings.Contrast.value);
		passData.Material.SetVector(ShaderConstants._MaskedColorTransformParams, new Vector4(x, y, 0f, 0f));
		rasterRenderGraphBuilder.SetRenderFunc(delegate(MaskedColorTransformPassData data, RasterGraphContext context)
		{
			context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3, 1);
		});
	}
}
