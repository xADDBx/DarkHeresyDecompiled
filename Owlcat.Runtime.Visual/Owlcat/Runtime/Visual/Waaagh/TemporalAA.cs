using System;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Serialization;

namespace Owlcat.Runtime.Visual.Waaagh;

public static class TemporalAA
{
	internal static class ShaderConstants
	{
		public static readonly int _TaaAccumulationTex = Shader.PropertyToID("_TaaAccumulationTex");

		public static readonly int _TaaMotionVectorTex = Shader.PropertyToID("_TaaMotionVectorTex");

		public static readonly int _TaaFilterWeights = Shader.PropertyToID("_TaaFilterWeights");

		public static readonly int _TaaFrameInfluence = Shader.PropertyToID("_TaaFrameInfluence");

		public static readonly int _TaaVarianceClampScale = Shader.PropertyToID("_TaaVarianceClampScale");

		public static readonly int _CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
	}

	internal static class ShaderKeywords
	{
		public static readonly string TAA_LOW_PRECISION_SOURCE = "TAA_LOW_PRECISION_SOURCE";
	}

	[Serializable]
	public struct Settings
	{
		[SerializeField]
		[FormerlySerializedAs("quality")]
		internal TemporalAAQuality m_Quality;

		[SerializeField]
		[FormerlySerializedAs("frameInfluence")]
		internal float m_FrameInfluence;

		[SerializeField]
		[FormerlySerializedAs("jitterScale")]
		internal float m_JitterScale;

		[SerializeField]
		[FormerlySerializedAs("varianceClampScale")]
		internal float m_VarianceClampScale;

		[SerializeField]
		[FormerlySerializedAs("contrastAdaptiveSharpening")]
		internal float m_ContrastAdaptiveSharpening;

		[NonSerialized]
		internal int resetHistoryFrames;

		[NonSerialized]
		internal int jitterFrameCountOffset;

		public TemporalAAQuality quality
		{
			get
			{
				return m_Quality;
			}
			set
			{
				m_Quality = (TemporalAAQuality)Mathf.Clamp((int)value, 0, 4);
			}
		}

		public float baseBlendFactor
		{
			get
			{
				return 1f - m_FrameInfluence;
			}
			set
			{
				m_FrameInfluence = Mathf.Clamp01(1f - value);
			}
		}

		public float jitterScale
		{
			get
			{
				return Mathf.Clamp(m_JitterScale, 0.5f, 1f);
			}
			set
			{
				m_JitterScale = Mathf.Clamp(value, 0.5f, 1f);
			}
		}

		public float varianceClampScale
		{
			get
			{
				return m_VarianceClampScale;
			}
			set
			{
				m_VarianceClampScale = Mathf.Clamp(value, 0.001f, 10f);
			}
		}

		public float contrastAdaptiveSharpening
		{
			get
			{
				return m_ContrastAdaptiveSharpening;
			}
			set
			{
				m_ContrastAdaptiveSharpening = Mathf.Clamp01(value);
			}
		}

		public static Settings Create()
		{
			Settings result = default(Settings);
			result.m_Quality = TemporalAAQuality.High;
			result.m_FrameInfluence = 0.1f;
			result.m_JitterScale = 1f;
			result.m_VarianceClampScale = 0.9f;
			result.m_ContrastAdaptiveSharpening = 0f;
			result.resetHistoryFrames = 0;
			result.jitterFrameCountOffset = 0;
			return result;
		}
	}

	internal delegate void JitterFunc(int frameIndex, out Vector2 jitter, out bool allowScaling);

	private class TaaPassData
	{
		internal TextureHandle dstTex;

		internal TextureHandle srcColorTex;

		internal TextureHandle srcDepthTex;

		internal TextureHandle srcMotionVectorTex;

		internal TextureHandle srcTaaAccumTex;

		internal Material material;

		internal int passIndex;

		internal float taaFrameInfluence;

		internal float taaVarianceClampScale;

		internal float[] taaFilterWeights;

		internal bool taaLowPrecisionSource;

		internal bool taaAlphaOutput;
	}

	internal static JitterFunc s_JitterFunc = CalculateJitter;

	private static readonly Vector2[] taaFilterOffsets = new Vector2[9]
	{
		new Vector2(0f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 0f),
		new Vector2(-1f, 0f),
		new Vector2(0f, -1f),
		new Vector2(-1f, 1f),
		new Vector2(1f, -1f),
		new Vector2(1f, 1f),
		new Vector2(-1f, -1f)
	};

	private static readonly float[] taaFilterWeights = new float[taaFilterOffsets.Length + 1];

	internal static GraphicsFormat[] AccumulationFormatList = new GraphicsFormat[4]
	{
		GraphicsFormat.R16G16B16A16_SFloat,
		GraphicsFormat.B10G11R11_UFloatPack32,
		GraphicsFormat.R8G8B8A8_UNorm,
		GraphicsFormat.B8G8R8A8_UNorm
	};

	internal static Matrix4x4 CalculateJitterMatrix(WaaaghCameraData cameraData, JitterFunc jitterFunc)
	{
		Matrix4x4 result = Matrix4x4.identity;
		if (cameraData.IsTemporalAAEnabled())
		{
			int jitterFrameCountOffset = cameraData.taaSettings.jitterFrameCountOffset;
			int frameIndex = Time.frameCount + jitterFrameCountOffset;
			float num = cameraData.cameraTargetDescriptor.width;
			float num2 = cameraData.cameraTargetDescriptor.height;
			float jitterScale = cameraData.taaSettings.jitterScale;
			jitterFunc(frameIndex, out var jitter, out var allowScaling);
			if (allowScaling)
			{
				jitter *= jitterScale;
			}
			float x = jitter.x * (2f / num);
			float y = jitter.y * (2f / num2);
			result = Matrix4x4.Translate(new Vector3(x, y, 0f));
		}
		return result;
	}

	internal static float GetAutoMipBias(WaaaghCameraData cameraData)
	{
		if (!cameraData.IsTemporalAAEnabled())
		{
			return 0f;
		}
		if (!cameraData.IsSTPEnabled())
		{
			return 0f - cameraData.taaSettings.jitterScale;
		}
		return -1f;
	}

	internal static void CalculateJitter(int frameIndex, out Vector2 jitter, out bool allowScaling)
	{
		float x = HaltonSequence.Get((frameIndex & 0x3FF) + 1, 2) - 0.5f;
		float y = HaltonSequence.Get((frameIndex & 0x3FF) + 1, 3) - 0.5f;
		jitter = new Vector2(x, y);
		allowScaling = true;
	}

	internal static float[] CalculateFilterWeights(float jitterScale)
	{
		float num = 0f;
		for (int i = 0; i < 9; i++)
		{
			CalculateJitter(Time.frameCount, out var jitter, out var _);
			jitter *= jitterScale;
			float num2 = taaFilterOffsets[i].x - jitter.x;
			float num3 = taaFilterOffsets[i].y - jitter.y;
			float num4 = num2 * num2 + num3 * num3;
			taaFilterWeights[i] = Mathf.Exp(-2.2727273f * num4);
			num += taaFilterWeights[i];
		}
		for (int j = 0; j < 9; j++)
		{
			taaFilterWeights[j] /= num;
		}
		return taaFilterWeights;
	}

	internal static RenderTextureDescriptor TemporalAADescFromCameraDesc(ref RenderTextureDescriptor cameraDesc)
	{
		RenderTextureDescriptor result = cameraDesc;
		result.width = cameraDesc.width;
		result.height = cameraDesc.height;
		result.msaaSamples = 1;
		result.volumeDepth = cameraDesc.volumeDepth;
		result.mipCount = 0;
		result.graphicsFormat = cameraDesc.graphicsFormat;
		result.sRGB = false;
		result.depthBufferBits = 0;
		result.dimension = cameraDesc.dimension;
		result.vrUsage = cameraDesc.vrUsage;
		result.memoryless = RenderTextureMemoryless.None;
		result.useMipMap = false;
		result.autoGenerateMips = false;
		result.enableRandomWrite = false;
		result.bindMS = false;
		result.useDynamicScale = false;
		if (!SystemInfo.IsFormatSupported(result.graphicsFormat, GraphicsFormatUsage.Render))
		{
			result.graphicsFormat = GraphicsFormat.None;
			for (int i = 0; i < AccumulationFormatList.Length; i++)
			{
				if (SystemInfo.IsFormatSupported(AccumulationFormatList[i], GraphicsFormatUsage.Render))
				{
					result.graphicsFormat = AccumulationFormatList[i];
					break;
				}
			}
		}
		return result;
	}

	internal static string ValidateAndWarn(WaaaghCameraData cameraData)
	{
		string text = null;
		if (text == null && !cameraData.postProcessEnabled)
		{
			text = "Disabling TAA because camera has post-processing disabled.";
		}
		if (cameraData.taaHistory == null)
		{
			text = "Disabling TAA due to invalid persistent data.";
		}
		if (text == null && cameraData.cameraTargetDescriptor.msaaSamples != 1)
		{
			text = "Disabling TAA because MSAA is on.";
		}
		if (text == null && cameraData.camera.TryGetComponent<WaaaghAdditionalCameraData>(out var component) && (component.RenderType == CameraRenderType.Overlay || component.CameraStack.Count > 0))
		{
			text = "Disabling TAA because camera is stacked.";
		}
		if (text == null && cameraData.camera.allowDynamicResolution)
		{
			text = "Disabling TAA because camera has dynamic resolution enabled. You can use a constant render scale instead.";
		}
		if (text == null && !cameraData.renderer.SupportsPipelineFeature(PipelineFeature.MotionVectors))
		{
			text = "Disabling TAA because the renderer does not implement motion vectors. Motion vectors are required for TAA.";
		}
		if (Time.frameCount % 60 == 0)
		{
			Debug.LogWarning(text);
		}
		return text;
	}

	internal static void Render(RenderGraph renderGraph, Material taaMaterial, WaaaghCameraData cameraData, in TextureHandle srcColor, in TextureHandle srcDepth, in TextureHandle srcMotionVectors, ref TextureHandle dstColor)
	{
		int eyeIndex = 0;
		ref Settings taaSettings = ref cameraData.taaSettings;
		bool flag = cameraData.taaHistory.GetAccumulationVersion(eyeIndex) != Time.frameCount;
		float taaFrameInfluence = ((taaSettings.resetHistoryFrames == 0) ? taaSettings.m_FrameInfluence : 1f);
		RTHandle accumulationTexture = cameraData.taaHistory.GetAccumulationTexture(eyeIndex);
		TextureHandle input = renderGraph.ImportTexture(accumulationTexture);
		TextureHandle input2 = (flag ? srcMotionVectors : renderGraph.defaultResources.blackTexture);
		TaaPassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<TaaPassData>("Temporal Anti-aliasing", out passData, WaaaghProfileId.TAA.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\TemporalAA.cs", 413))
		{
			passData.dstTex = dstColor;
			rasterRenderGraphBuilder.SetRenderAttachment(dstColor, 0);
			passData.srcColorTex = srcColor;
			rasterRenderGraphBuilder.UseTexture(in srcColor);
			passData.srcDepthTex = srcDepth;
			rasterRenderGraphBuilder.UseTexture(in srcDepth);
			passData.srcMotionVectorTex = input2;
			rasterRenderGraphBuilder.UseTexture(in input2);
			passData.srcTaaAccumTex = input;
			rasterRenderGraphBuilder.UseTexture(in input);
			passData.material = taaMaterial;
			passData.passIndex = (int)taaSettings.quality;
			passData.taaFrameInfluence = taaFrameInfluence;
			passData.taaVarianceClampScale = taaSettings.varianceClampScale;
			if (taaSettings.quality == TemporalAAQuality.VeryHigh)
			{
				passData.taaFilterWeights = CalculateFilterWeights(taaSettings.jitterScale);
			}
			else
			{
				passData.taaFilterWeights = null;
			}
			GraphicsFormat graphicsFormat = accumulationTexture.rt.graphicsFormat;
			if (graphicsFormat == GraphicsFormat.R8G8B8A8_UNorm || graphicsFormat == GraphicsFormat.B8G8R8A8_UNorm || graphicsFormat == GraphicsFormat.B10G11R11_UFloatPack32)
			{
				passData.taaLowPrecisionSource = true;
			}
			else
			{
				passData.taaLowPrecisionSource = false;
			}
			passData.taaAlphaOutput = cameraData.isAlphaOutputEnabled;
			rasterRenderGraphBuilder.SetRenderFunc(delegate(TaaPassData data, RasterGraphContext context)
			{
				data.material.SetFloat(ShaderConstants._TaaFrameInfluence, data.taaFrameInfluence);
				data.material.SetFloat(ShaderConstants._TaaVarianceClampScale, data.taaVarianceClampScale);
				data.material.SetTexture(ShaderConstants._TaaAccumulationTex, data.srcTaaAccumTex);
				data.material.SetTexture(ShaderConstants._TaaMotionVectorTex, data.srcMotionVectorTex);
				data.material.SetTexture(ShaderConstants._CameraDepthTexture, data.srcDepthTex);
				CoreUtils.SetKeyword(data.material, ShaderKeywords.TAA_LOW_PRECISION_SOURCE, data.taaLowPrecisionSource);
				CoreUtils.SetKeyword(data.material, "_ENABLE_ALPHA_OUTPUT", data.taaAlphaOutput);
				if (data.taaFilterWeights != null)
				{
					data.material.SetFloatArray(ShaderConstants._TaaFilterWeights, data.taaFilterWeights);
				}
				Blitter.BlitTexture(context.cmd, data.srcColorTex, Vector2.one, data.material, data.passIndex);
			});
		}
		if (!flag)
		{
			return;
		}
		int passIndex = taaMaterial.shader.passCount - 1;
		TaaPassData passData2;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder2 = renderGraph.AddRasterRenderPass<TaaPassData>("Temporal Anti-aliasing Copy History", out passData2, WaaaghProfileId.TAACopyHistory.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\TemporalAA.cs", 472))
		{
			passData2.dstTex = input;
			rasterRenderGraphBuilder2.SetRenderAttachment(input, 0);
			passData2.srcColorTex = dstColor;
			rasterRenderGraphBuilder2.UseTexture(in dstColor);
			passData2.material = taaMaterial;
			passData2.passIndex = passIndex;
			rasterRenderGraphBuilder2.SetRenderFunc(delegate(TaaPassData data, RasterGraphContext context)
			{
				Blitter.BlitTexture(context.cmd, data.srcColorTex, Vector2.one, data.material, data.passIndex);
			});
		}
		cameraData.taaHistory.SetAccumulationVersion(eyeIndex, Time.frameCount);
	}
}
