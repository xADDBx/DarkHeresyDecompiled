using System;
using Owlcat.Runtime.Visual.OccludedObjectHighlighting;
using Owlcat.Runtime.Visual.OcclusionClipping;
using Owlcat.Runtime.Visual.OcclusionGeometryClip;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CameraObjectClip;

public sealed class CameraObjectClipRendererFeature : IRendererFeature, IDisposable
{
	private sealed class SetupPassData
	{
		public CameraObjectClipRendererFeatureAsset Asset;

		public OcclusionClippingSettings ClippingSettings;

		public RenderTexture NoiseTexture;
	}

	private sealed class DrawDepthClipMaskPassData
	{
		public Material Material;

		public int Count;

		public Matrix4x4[] Matrices = Array.Empty<Matrix4x4>();
	}

	private static readonly int _OccludedObjectNoiseMap3D = Shader.PropertyToID("_OccludedObjectNoiseMap3D");

	private static readonly int _NoiseTiling = Shader.PropertyToID("_NoiseTiling");

	private static readonly int _NoiseSlice = Shader.PropertyToID("_NoiseSlice");

	private readonly CameraObjectClipRendererFeatureAsset m_Asset;

	private readonly OcclusionClippingSettings m_ClippingSettings;

	private readonly Material m_DepthClippingMaterial;

	private readonly Material m_NoiseBakeMaterial;

	private readonly RenderTexture m_NoiseTexture;

	private float m_NoiseTiling = float.MinValue;

	public CameraObjectClipRendererFeature(CameraObjectClipRendererFeatureAsset asset)
	{
		CameraObjectClipFeatureResources renderPipelineSettings = GraphicsSettings.GetRenderPipelineSettings<CameraObjectClipFeatureResources>();
		m_Asset = asset;
		m_ClippingSettings = GraphicsSettings.GetRenderPipelineSettings<OcclusionClippingSettings>();
		m_DepthClippingMaterial = CoreUtils.CreateEngineMaterial(renderPipelineSettings.OccludedObjectShader);
		m_DepthClippingMaterial.enableInstancing = true;
		m_NoiseBakeMaterial = CoreUtils.CreateEngineMaterial(renderPipelineSettings.NoiseBakeShader);
		m_NoiseTexture = CreateNoiseTexture();
		UpdateNoiseTexture();
		Owlcat.Runtime.Visual.OcclusionGeometryClip.System.SetEnabled(asset.Settings.OcclusionGeometryClipEnabled);
		Owlcat.Runtime.Visual.OcclusionGeometryClip.System.SetSettings(asset.Settings.OcclusionGeometryClipSettings);
	}

	public void Dispose()
	{
		CoreUtils.Destroy(m_DepthClippingMaterial);
		CoreUtils.Destroy(m_NoiseBakeMaterial);
	}

	public void RegisterExtensions(RendererFeatureExtensionRegistry registry)
	{
		registry.AddSetupDelegate(OnSetup);
		registry.AddRecordDelegate(RecordExtensionPoint.BeforeRendering, OnBeforeRendering);
		registry.AddRecordDelegate(RecordExtensionPoint.BeforeDrawDepthPrePass, OnBeforeDrawDepthPrePass);
	}

	private void OnSetup(in SetupContext context)
	{
		UpdateNoiseTexture();
	}

	private void OnBeforeRendering(in RecordContext context)
	{
		SetupGlobalState(in context);
	}

	private void OnBeforeDrawDepthPrePass(in RecordContext context)
	{
		DrawDepthClippingMask(in context);
	}

	private void UpdateNoiseTexture()
	{
		if (Mathf.Approximately(m_NoiseTiling, m_Asset.Settings.NoiseTiling))
		{
			return;
		}
		m_NoiseTiling = m_Asset.Settings.NoiseTiling;
		CommandBuffer commandBuffer = CommandBufferPool.Get("Bake Noise3D");
		try
		{
			commandBuffer.SetGlobalFloat(_NoiseTiling, m_NoiseTiling);
			int volumeDepth = m_NoiseTexture.volumeDepth;
			for (int i = 0; i < volumeDepth; i++)
			{
				commandBuffer.SetGlobalFloat(_NoiseSlice, i);
				commandBuffer.Blit((Texture)null, m_NoiseTexture, m_NoiseBakeMaterial, 0, i);
			}
			Graphics.ExecuteCommandBuffer(commandBuffer);
		}
		finally
		{
			CommandBufferPool.Release(commandBuffer);
		}
	}

	private void SetupGlobalState(in RecordContext context)
	{
		SetupPassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<SetupPassData>("Setup Camera Object Clip", out passData2, WaaaghProfileId.CameraObjectClipSetup.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\CameraObjectClip\\CameraObjectClipRendererFeature.cs", 115);
		passData2.Asset = m_Asset;
		passData2.ClippingSettings = m_ClippingSettings;
		passData2.NoiseTexture = m_NoiseTexture;
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(SetupPassData passData, UnsafeGraphContext context)
		{
			context.cmd.SetGlobalFloat(ShaderPropertyId._OccludedObjectAlphaScale, passData.Asset.Settings.AlphaScale);
			context.cmd.SetGlobalFloat(ShaderPropertyId._OccludedObjectClipNoiseTiling, passData.Asset.Settings.NoiseTiling);
			context.cmd.SetGlobalFloat(ShaderPropertyId._OccludedObjectClipTreshold, passData.Asset.Settings.ClipTreshold);
			context.cmd.SetGlobalFloat(ShaderPropertyId._OccludedObjectClipNearCameraDistance, passData.Asset.Settings.NearCameraClipDistance);
			context.cmd.SetGlobalFloat(ShaderPropertyId._OccludedObjectHighlightingFeatureEnabled, 1f);
			context.cmd.SetGlobalFloat(ShaderPropertyId._OccludedObjectShadowClippingEnabled, (passData.ClippingSettings.ShadowClippingType == OcclusionClippingShadowType.OccluderObjectFullOpacity) ? 1 : 0);
			context.cmd.SetGlobalFloat(ShaderPropertyId._OccludedObjectSampleDepthTexture, 0f);
			context.cmd.SetGlobalTexture(_OccludedObjectNoiseMap3D, passData.NoiseTexture);
		});
	}

	public void DrawDepthClippingMask(in RecordContext context)
	{
		if (OccludedObjectDepthClipper.All.Count == 0)
		{
			return;
		}
		TextureHandle input = CreateDepthClippingTexture(in context);
		DrawDepthClipMaskPassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<DrawDepthClipMaskPassData>("Draw Depth Clip Mask", out passData2, WaaaghProfileId.CameraObjectClipDrawMask.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\CameraObjectClip\\CameraObjectClipRendererFeature.cs", 143);
		passData2.Material = m_DepthClippingMaterial;
		passData2.Count = OccludedObjectDepthClipper.All.Count;
		SetupDepthClippingMatrices(in context.CameraData, ref passData2.Matrices);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.SetRenderAttachment(input, 0);
		unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in input, ShaderPropertyId._OccludedObjectDepthClipTexture);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(DrawDepthClipMaskPassData passData, UnsafeGraphContext context)
		{
			context.cmd.DrawMeshInstanced(RenderingUtils.QuadFlippedMesh, 0, passData.Material, 0, passData.Matrices, passData.Count);
			context.cmd.SetGlobalFloat(ShaderPropertyId._OccludedObjectSampleDepthTexture, 1f);
		});
	}

	private static void SetupDepthClippingMatrices(in WaaaghCameraData cameraData, ref Matrix4x4[] matrices)
	{
		if (matrices.Length < OccludedObjectDepthClipper.All.Count)
		{
			int newSize = (int)((float)OccludedObjectDepthClipper.All.Count * 1.5f);
			Array.Resize(ref matrices, newSize);
		}
		Vector3 position = cameraData.camera.transform.position;
		int num = 0;
		foreach (OccludedObjectDepthClipper item in OccludedObjectDepthClipper.All)
		{
			Vector3 vector = position - item.transform.position;
			vector.Normalize();
			Quaternion q = Quaternion.FromToRotation(Vector3.forward, vector);
			matrices[num] = Matrix4x4.TRS(item.transform.position + vector * item.OffsetToCamera, q, item.Radius * Vector3.one);
			num++;
		}
	}

	private static TextureHandle CreateDepthClippingTexture(in RecordContext context)
	{
		int width = context.CameraData.cameraTargetDescriptor.width / 4;
		int height = context.CameraData.cameraTargetDescriptor.height / 4;
		TextureDesc textureDesc = new TextureDesc(width, height);
		textureDesc.name = "OccludedObjectDepthClippingRT";
		textureDesc.colorFormat = GraphicsFormat.R16G16_SFloat;
		textureDesc.msaaSamples = MSAASamples.None;
		textureDesc.filterMode = FilterMode.Bilinear;
		textureDesc.wrapMode = TextureWrapMode.Clamp;
		textureDesc.dimension = TextureDimension.Tex2D;
		textureDesc.useMipMap = false;
		textureDesc.clearBuffer = true;
		textureDesc.clearColor = Color.clear;
		TextureDesc desc = textureDesc;
		return context.RenderGraph.CreateTexture(in desc);
	}

	private static RenderTexture CreateNoiseTexture()
	{
		RenderTexture renderTexture = new RenderTexture(32, 32, GraphicsFormat.R8_UNorm, GraphicsFormat.None, 1);
		renderTexture.name = "CameraObjectClipNoise3D";
		renderTexture.hideFlags = HideFlags.DontSave;
		renderTexture.dimension = TextureDimension.Tex3D;
		renderTexture.volumeDepth = 32;
		renderTexture.filterMode = FilterMode.Trilinear;
		renderTexture.wrapMode = TextureWrapMode.Repeat;
		renderTexture.Create();
		return renderTexture;
	}
}
