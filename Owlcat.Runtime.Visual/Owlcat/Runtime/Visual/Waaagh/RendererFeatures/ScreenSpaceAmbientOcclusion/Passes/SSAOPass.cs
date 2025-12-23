using System;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ScreenSpaceAmbientOcclusion.Passes;

public class SSAOPass : ScriptableRenderPass
{
	private enum BlurTypes
	{
		Bilateral,
		Gaussian,
		Kawase
	}

	private enum ShaderPasses
	{
		AmbientOcclusion,
		BilateralBlurHorizontal,
		BilateralBlurVertical,
		BilateralBlurFinal,
		BilateralAfterOpaque,
		GaussianBlurHorizontal,
		GaussianBlurVertical,
		GaussianAfterOpaque,
		KawaseBlur,
		KawaseAfterOpaque
	}

	private struct SSAOMaterialParams
	{
		internal bool orthographicCamera;

		internal bool aoBlueNoise;

		internal bool aoInterleavedGradient;

		internal bool sampleCountHigh;

		internal bool sampleCountMedium;

		internal bool sampleCountLow;

		internal bool sourceDepthNormals;

		internal bool sourceDepthHigh;

		internal bool sourceDepthMedium;

		internal bool sourceDepthLow;

		internal Vector4 ssaoParams;

		internal SSAOMaterialParams(ref ScreenSpaceAmbientOcclusionSettings settings, bool isOrthographic)
		{
			bool flag = settings.Source == ScreenSpaceAmbientOcclusionSettings.DepthSource.DepthNormals;
			float num = ((settings.AOMethod == ScreenSpaceAmbientOcclusionSettings.AOMethodOptions.BlueNoise) ? 1.5f : 1f);
			orthographicCamera = isOrthographic;
			aoBlueNoise = settings.AOMethod == ScreenSpaceAmbientOcclusionSettings.AOMethodOptions.BlueNoise;
			aoInterleavedGradient = settings.AOMethod == ScreenSpaceAmbientOcclusionSettings.AOMethodOptions.InterleavedGradient;
			sampleCountHigh = settings.Samples == ScreenSpaceAmbientOcclusionSettings.AOSampleOption.High;
			sampleCountMedium = settings.Samples == ScreenSpaceAmbientOcclusionSettings.AOSampleOption.Medium;
			sampleCountLow = settings.Samples == ScreenSpaceAmbientOcclusionSettings.AOSampleOption.Low;
			sourceDepthNormals = settings.Source == ScreenSpaceAmbientOcclusionSettings.DepthSource.DepthNormals;
			sourceDepthHigh = !flag && settings.NormalSamples == ScreenSpaceAmbientOcclusionSettings.NormalQuality.High;
			sourceDepthMedium = !flag && settings.NormalSamples == ScreenSpaceAmbientOcclusionSettings.NormalQuality.Medium;
			sourceDepthLow = !flag && settings.NormalSamples == ScreenSpaceAmbientOcclusionSettings.NormalQuality.Low;
			ssaoParams = new Vector4(settings.Intensity, settings.Radius * num, 1f / (float)((!settings.Downsample) ? 1 : 2), settings.Falloff);
		}

		internal bool Equals(ref SSAOMaterialParams other)
		{
			if (orthographicCamera == other.orthographicCamera && aoBlueNoise == other.aoBlueNoise && aoInterleavedGradient == other.aoInterleavedGradient && sampleCountHigh == other.sampleCountHigh && sampleCountMedium == other.sampleCountMedium && sampleCountLow == other.sampleCountLow && sourceDepthNormals == other.sourceDepthNormals && sourceDepthHigh == other.sourceDepthHigh && sourceDepthMedium == other.sourceDepthMedium && sourceDepthLow == other.sourceDepthLow)
			{
				return ssaoParams == other.ssaoParams;
			}
			return false;
		}
	}

	private SSAOMaterialParams m_SSAOParamsPrev;

	private const string k_SSAOTextureName = "_ScreenSpaceOcclusionTexture";

	private const string k_AmbientOcclusionParamName = "_AmbientOcclusionParam";

	private const string _SCREEN_SPACE_OCCLUSION = "_SCREEN_SPACE_OCCLUSION";

	private readonly bool m_SupportsR8RenderTextureFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8);

	private ScreenSpaceAmbientOcclusionSettings m_Settings;

	private Material m_Material;

	private Texture2D[] m_BlueNoiseTextures;

	private BlurTypes m_BlurType;

	private Matrix4x4[] m_CameraViewProjections = new Matrix4x4[2];

	private Vector4[] m_CameraTopLeftCorner = new Vector4[2];

	private Vector4[] m_CameraXExtent = new Vector4[2];

	private Vector4[] m_CameraYExtent = new Vector4[2];

	private Vector4[] m_CameraZExtent = new Vector4[2];

	private int m_BlueNoiseTextureIndex;

	private ProfilingSampler m_ProfilingSampler = ProfilingSampler.Get(WaaaghProfileId.SSAO);

	internal static readonly int s_AmbientOcclusionParamID = Shader.PropertyToID("_AmbientOcclusionParam");

	private static readonly int s_SSAOParamsID = Shader.PropertyToID("_SSAOParams");

	private static readonly int s_SSAOBlueNoiseParamsID = Shader.PropertyToID("_SSAOBlueNoiseParams");

	private static readonly int s_BlueNoiseTextureID = Shader.PropertyToID("_BlueNoiseTexture");

	private static readonly int s_SSAOFinalTextureID = Shader.PropertyToID("_ScreenSpaceOcclusionTexture");

	private static readonly int s_CameraViewXExtentID = Shader.PropertyToID("_CameraViewXExtent");

	private static readonly int s_CameraViewYExtentID = Shader.PropertyToID("_CameraViewYExtent");

	private static readonly int s_CameraViewZExtentID = Shader.PropertyToID("_CameraViewZExtent");

	private static readonly int s_ProjectionParams2ID = Shader.PropertyToID("_ProjectionParams2");

	private static readonly int s_CameraViewProjectionsID = Shader.PropertyToID("_CameraViewProjections");

	private static readonly int s_CameraViewTopLeftCornerID = Shader.PropertyToID("_CameraViewTopLeftCorner");

	private static readonly int s_CameraDepthTextureID = Shader.PropertyToID("_CameraDepthTexture");

	private static readonly int s_CameraNormalsTextureID = Shader.PropertyToID("_CameraNormalsTexture");

	private static readonly int _SourceSize = Shader.PropertyToID("_SourceSize");

	private static readonly int _ScaleBiasRt = Shader.PropertyToID("_ScaleBiasRt");

	public static GlobalKeyword ScreenSpaceOcclusion = GlobalKeyword.Create("_SCREEN_SPACE_OCCLUSION");

	public override string Name => "SSAOPass";

	public SSAOPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	public override void RecordRenderGraph(ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghCameraData cameraData = frameData.Get<WaaaghCameraData>();
		RenderGraph renderGraph = waaaghRenderingData.RenderGraph;
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		CreateRenderTextureHandles(renderGraph, cameraData, out var aoTexture, out var blurTexture, out var finalTexture);
		SetupKeywordsAndParameters(ref m_Settings, cameraData);
		SSAOPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<SSAOPassData>("SSAO", out passData, m_ProfilingSampler, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\RendererFeatures\\ScreenSpaceAmbientOcclusion\\Passes\\SSAOPass.cs", 168);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		InitSSAOPassData(ref passData);
		passData.cameraColor = waaaghResourceData.CameraColorBuffer;
		passData.AOTexture = aoTexture;
		passData.finalTexture = finalTexture;
		passData.blurTexture = blurTexture;
		passData.cameraDepthTexture = waaaghResourceData.CameraDepthCopyRT;
		passData.cameraNormalsTexture = waaaghResourceData.CameraNormalsRT;
		passData.ScaleBiasRt = RenderingUtils.CalculateScaleBiasRt(cameraData);
		unsafeRenderGraphBuilder.UseTexture(in passData.AOTexture, AccessFlags.ReadWrite);
		if (passData.BlurQuality != ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.Low)
		{
			unsafeRenderGraphBuilder.UseTexture(in passData.blurTexture, AccessFlags.ReadWrite);
		}
		unsafeRenderGraphBuilder.UseTexture(in waaaghResourceData.CameraDepthCopyRT);
		unsafeRenderGraphBuilder.UseTexture(in waaaghResourceData.CameraNormalsRT);
		if (finalTexture.IsValid())
		{
			unsafeRenderGraphBuilder.UseTexture(in passData.finalTexture, AccessFlags.ReadWrite);
			unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in finalTexture, s_SSAOFinalTextureID);
		}
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(SSAOPassData data, UnsafeGraphContext rgContext)
		{
			if (data.cameraDepthTexture.IsValid())
			{
				data.material.SetTexture(s_CameraDepthTextureID, data.cameraDepthTexture);
			}
			CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(rgContext.cmd);
			RenderBufferLoadAction loadAction = RenderBufferLoadAction.DontCare;
			if (data.cameraColor.IsValid())
			{
				SetSourceSize(nativeCommandBuffer, data.cameraColor);
			}
			if (data.cameraDepthTexture.IsValid())
			{
				data.material.SetTexture(s_CameraDepthTextureID, data.cameraDepthTexture);
			}
			if (data.cameraNormalsTexture.IsValid())
			{
				data.material.SetTexture(s_CameraNormalsTextureID, data.cameraNormalsTexture);
			}
			Blitter.BlitCameraTexture(nativeCommandBuffer, data.AOTexture, data.AOTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, data.material, 0);
			switch (data.BlurQuality)
			{
			case ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.High:
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.AOTexture, data.blurTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, data.material, 1);
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.blurTexture, data.AOTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, data.material, 2);
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.AOTexture, data.finalTexture, loadAction, RenderBufferStoreAction.Store, data.material, 3);
				break;
			case ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.Medium:
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.AOTexture, data.blurTexture, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, data.material, 5);
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.blurTexture, data.finalTexture, loadAction, RenderBufferStoreAction.Store, data.material, 6);
				break;
			case ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.Low:
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.AOTexture, data.finalTexture, loadAction, RenderBufferStoreAction.Store, data.material, 8);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			rgContext.cmd.SetKeyword(in ScreenSpaceOcclusion, value: true);
			rgContext.cmd.SetGlobalVector(s_AmbientOcclusionParamID, new Vector4(1f, 0f, 0f, data.directLightingStrength));
			rgContext.cmd.SetGlobalVector(_ScaleBiasRt, passData.ScaleBiasRt);
		});
	}

	internal static void SetSourceSize(CommandBuffer cmd, RTHandle source)
	{
		SetSourceSize(CommandBufferHelpers.GetRasterCommandBuffer(cmd), source);
	}

	internal static void SetSourceSize(RasterCommandBuffer cmd, RTHandle source)
	{
		float num = source.rt.width;
		float num2 = source.rt.height;
		if (source.rt.useDynamicScale)
		{
			num *= ScalableBufferManager.widthScaleFactor;
			num2 *= ScalableBufferManager.heightScaleFactor;
		}
		cmd.SetGlobalVector(_SourceSize, new Vector4(num, num2, 1f / num, 1f / num2));
	}

	private void InitSSAOPassData(ref SSAOPassData data)
	{
		data.material = m_Material;
		data.BlurQuality = m_Settings.BlurQuality;
		data.directLightingStrength = m_Settings.DirectLightingStrength;
	}

	internal bool Setup(ref ScreenSpaceAmbientOcclusionSettings featureSettings, ref Material material, ref Texture2D[] blueNoiseTextures)
	{
		m_BlueNoiseTextures = blueNoiseTextures;
		m_Material = material;
		m_Settings = featureSettings;
		switch (m_Settings.BlurQuality)
		{
		case ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.High:
			m_BlurType = BlurTypes.Bilateral;
			break;
		case ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.Medium:
			m_BlurType = BlurTypes.Gaussian;
			break;
		case ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.Low:
			m_BlurType = BlurTypes.Kawase;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (m_Material != null && m_Settings.Intensity > 0f && m_Settings.Radius > 0f)
		{
			return m_Settings.Falloff > 0f;
		}
		return false;
	}

	private void CreateRenderTextureHandles(RenderGraph renderGraph, WaaaghCameraData cameraData, out TextureHandle aoTexture, out TextureHandle blurTexture, out TextureHandle finalTexture)
	{
		RenderTextureDescriptor cameraTargetDescriptor = cameraData.cameraTargetDescriptor;
		TextureDesc desc = new TextureDesc(cameraTargetDescriptor.width, cameraTargetDescriptor.height);
		desc.colorFormat = (m_SupportsR8RenderTextureFormat ? GraphicsFormat.R8_UNorm : GraphicsFormat.R8G8B8A8_UNorm);
		desc.msaaSamples = MSAASamples.None;
		desc.filterMode = FilterMode.Bilinear;
		desc.wrapMode = TextureWrapMode.Clamp;
		desc.dimension = TextureDimension.Tex2D;
		desc.useMipMap = false;
		desc.clearBuffer = false;
		desc.name = "_ScreenSpaceOcclusionTexture";
		int num = ((!m_Settings.Downsample) ? 1 : 2);
		bool flag = m_SupportsR8RenderTextureFormat && m_BlurType > BlurTypes.Bilateral;
		TextureDesc desc2 = desc;
		desc2.colorFormat = (flag ? GraphicsFormat.R8_UNorm : GraphicsFormat.R8G8B8A8_UNorm);
		desc2.width /= num;
		desc2.height /= num;
		desc2.name = "_SSAO_OcclusionTexture0";
		aoTexture = renderGraph.CreateTexture(in desc2);
		finalTexture = renderGraph.CreateTexture(in desc);
		if (m_Settings.BlurQuality != ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.Low)
		{
			desc2.name = "_SSAO_OcclusionTexture1";
			blurTexture = renderGraph.CreateTexture(in desc2);
		}
		else
		{
			blurTexture = TextureHandle.nullHandle;
		}
	}

	private void SetupKeywordsAndParameters(ref ScreenSpaceAmbientOcclusionSettings settings, WaaaghCameraData cameraData)
	{
		int num = 1;
		for (int i = 0; i < num; i++)
		{
			Matrix4x4 viewMatrix = cameraData.GetViewMatrix(i);
			Matrix4x4 projectionMatrix = cameraData.GetProjectionMatrix(i);
			m_CameraViewProjections[i] = projectionMatrix * viewMatrix;
			Matrix4x4 matrix4x = viewMatrix;
			matrix4x.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));
			Matrix4x4 inverse = (projectionMatrix * matrix4x).inverse;
			Vector4 vector = inverse.MultiplyPoint(new Vector4(-1f, 1f, -1f, 1f));
			Vector4 vector2 = inverse.MultiplyPoint(new Vector4(1f, 1f, -1f, 1f));
			Vector4 vector3 = inverse.MultiplyPoint(new Vector4(-1f, -1f, -1f, 1f));
			Vector4 vector4 = inverse.MultiplyPoint(new Vector4(0f, 0f, 1f, 1f));
			m_CameraTopLeftCorner[i] = vector;
			m_CameraXExtent[i] = vector2 - vector;
			m_CameraYExtent[i] = vector3 - vector;
			m_CameraZExtent[i] = vector4;
		}
		m_Material.SetVector(s_ProjectionParams2ID, new Vector4(1f / cameraData.camera.nearClipPlane, 0f, 0f, 0f));
		m_Material.SetMatrixArray(s_CameraViewProjectionsID, m_CameraViewProjections);
		m_Material.SetVectorArray(s_CameraViewTopLeftCornerID, m_CameraTopLeftCorner);
		m_Material.SetVectorArray(s_CameraViewXExtentID, m_CameraXExtent);
		m_Material.SetVectorArray(s_CameraViewYExtentID, m_CameraYExtent);
		m_Material.SetVectorArray(s_CameraViewZExtentID, m_CameraZExtent);
		if (settings.AOMethod == ScreenSpaceAmbientOcclusionSettings.AOMethodOptions.BlueNoise)
		{
			m_BlueNoiseTextureIndex = (m_BlueNoiseTextureIndex + 1) % m_BlueNoiseTextures.Length;
			Texture2D value = m_BlueNoiseTextures[m_BlueNoiseTextureIndex];
			Vector4 value2 = new Vector4((float)cameraData.cameraTargetDescriptor.width / (float)m_BlueNoiseTextures[m_BlueNoiseTextureIndex].width, (float)cameraData.cameraTargetDescriptor.height / (float)m_BlueNoiseTextures[m_BlueNoiseTextureIndex].height, UnityEngine.Random.value, UnityEngine.Random.value);
			m_Material.SetTexture(s_BlueNoiseTextureID, value);
			m_Material.SetVector(s_SSAOBlueNoiseParamsID, value2);
		}
		SSAOMaterialParams other = new SSAOMaterialParams(ref settings, cameraData.camera.orthographic);
		bool num2 = !m_SSAOParamsPrev.Equals(ref other);
		bool flag = m_Material.HasProperty(s_SSAOParamsID);
		if (!(!num2 && flag))
		{
			m_SSAOParamsPrev = other;
			CoreUtils.SetKeyword(m_Material, "_ORTHOGRAPHIC", other.orthographicCamera);
			CoreUtils.SetKeyword(m_Material, "_BLUE_NOISE", other.aoBlueNoise);
			CoreUtils.SetKeyword(m_Material, "_INTERLEAVED_GRADIENT", other.aoInterleavedGradient);
			CoreUtils.SetKeyword(m_Material, "_SAMPLE_COUNT_HIGH", other.sampleCountHigh);
			CoreUtils.SetKeyword(m_Material, "_SAMPLE_COUNT_MEDIUM", other.sampleCountMedium);
			CoreUtils.SetKeyword(m_Material, "_SAMPLE_COUNT_LOW", other.sampleCountLow);
			CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_NORMALS", other.sourceDepthNormals);
			CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_HIGH", other.sourceDepthHigh);
			CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_MEDIUM", other.sourceDepthMedium);
			CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_LOW", other.sourceDepthLow);
			m_Material.SetVector(s_SSAOParamsID, other.ssaoParams);
		}
	}
}
