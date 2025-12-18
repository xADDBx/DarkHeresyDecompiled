using Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;
using Owlcat.Runtime.Visual.VirtualTexture;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public class WaaaghResourceData : WaaaghResourceDataBase
{
	public const GraphicsFormat kDepthFormat = GraphicsFormat.D24_UNorm_S8_UInt;

	public const DepthBits kDepthBufferBits = DepthBits.Depth32;

	private TextureHandle m_CameraScaledColorBuffer;

	private TextureHandle m_CameraScaledDepthBuffer;

	private TextureHandle m_CameraNonScaledColorBuffer;

	private TextureHandle m_CameraNonScaledDepthBuffer;

	private TextureHandle m_CameraResolveColorBuffer;

	private TextureHandle m_CameraResolveDepthBuffer;

	private TextureHandle m_CameraColorBuffer;

	private TextureHandle m_CameraDepthBuffer;

	private TextureHandle m_Shadowmap;

	private TextureHandle m_CachedShadowmap;

	private BufferHandle m_VTPackedFeedbackBufferUAV;

	private TextureHandle m_VTFeedbackRT;

	private TextureHandle m_OverlayUITexture;

	private TextureHandle m_ColorGradingLUT;

	public TextureHandle CameraColorPyramidRT;

	public TextureHandle CameraDepthCopyRT;

	public TextureHandle CameraAlbedoRT;

	public TextureHandle CameraSpecularRT;

	public TextureHandle CameraNormalsRT;

	public TextureHandle CameraBakedGIRT;

	public TextureHandle CameraShadowmaskRT;

	public TextureHandle CameraTranslucencyRT;

	public TextureHandle TilesMinMaxZTexture;

	public TextureHandle CameraDepthPyramidRT;

	public TextureHandle SsrRT;

	public TextureHandle CameraMotionVectorsRT;

	public TextureHandle VolumetricScatter;

	public TextureHandle PackedReprojectedDepth;

	public GPUDrivenDepthReprojectionUtils.ReprojectionParameters DepthReprojectionParameters;

	public BufferHandle LightDataConstantBuffer;

	public BufferHandle LightVolumeDataConstantBuffer;

	public BufferHandle ZBinsConstantBuffer;

	public BufferHandle LightTilesBuffer;

	public BufferHandle DeferredLightingFeatureTilesBuffer;

	public BufferHandle DeferredLightingFeatureTilesListsBuffer;

	public BufferHandle DeferredLightingIndirectArgsBuffer;

	public TextureHandle CameraScaledColorBuffer
	{
		get
		{
			return CheckAndGetTextureHandle(ref m_CameraScaledColorBuffer);
		}
		set
		{
			CheckAndSetTextureHandle(ref m_CameraScaledColorBuffer, value);
		}
	}

	public TextureHandle CameraScaledDepthBuffer
	{
		get
		{
			return CheckAndGetTextureHandle(ref m_CameraScaledDepthBuffer);
		}
		set
		{
			CheckAndSetTextureHandle(ref m_CameraScaledDepthBuffer, value);
		}
	}

	public TextureHandle CameraNonScaledColorBuffer
	{
		get
		{
			return CheckAndGetTextureHandle(ref m_CameraNonScaledColorBuffer);
		}
		set
		{
			CheckAndSetTextureHandle(ref m_CameraNonScaledColorBuffer, value);
		}
	}

	public TextureHandle CameraNonScaledDepthBuffer
	{
		get
		{
			return CheckAndGetTextureHandle(ref m_CameraNonScaledDepthBuffer);
		}
		set
		{
			CheckAndSetTextureHandle(ref m_CameraNonScaledDepthBuffer, value);
		}
	}

	public TextureHandle CameraResolveColorBuffer
	{
		get
		{
			return CheckAndGetTextureHandle(ref m_CameraResolveColorBuffer);
		}
		set
		{
			CheckAndSetTextureHandle(ref m_CameraResolveColorBuffer, value);
		}
	}

	public TextureHandle CameraResolveDepthBuffer
	{
		get
		{
			return CheckAndGetTextureHandle(ref m_CameraResolveDepthBuffer);
		}
		set
		{
			CheckAndSetTextureHandle(ref m_CameraResolveDepthBuffer, value);
		}
	}

	public TextureHandle CameraColorBuffer
	{
		get
		{
			return CheckAndGetTextureHandle(ref m_CameraColorBuffer);
		}
		set
		{
			CheckAndSetTextureHandle(ref m_CameraColorBuffer, value);
		}
	}

	public TextureHandle CameraDepthBuffer
	{
		get
		{
			return CheckAndGetTextureHandle(ref m_CameraDepthBuffer);
		}
		set
		{
			CheckAndSetTextureHandle(ref m_CameraDepthBuffer, value);
		}
	}

	public TextureHandle Shadowmap
	{
		get
		{
			return CheckAndGetTextureHandle(ref m_Shadowmap);
		}
		set
		{
			CheckAndSetTextureHandle(ref m_Shadowmap, value);
		}
	}

	public TextureHandle CachedShadowmap
	{
		get
		{
			return CheckAndGetTextureHandle(ref m_CachedShadowmap);
		}
		set
		{
			CheckAndSetTextureHandle(ref m_CachedShadowmap, value);
		}
	}

	public BufferHandle VTPackedFeedbackBufferUAV
	{
		get
		{
			return CheckAndGetBufferHandle(ref m_VTPackedFeedbackBufferUAV);
		}
		set
		{
			CheckAndSetBufferHandle(ref m_VTPackedFeedbackBufferUAV, value);
		}
	}

	public TextureHandle VTFeedbackRT
	{
		get
		{
			return CheckAndGetTextureHandle(ref m_VTFeedbackRT);
		}
		set
		{
			CheckAndSetTextureHandle(ref m_VTFeedbackRT, value);
		}
	}

	public TextureHandle OverlayUITexture
	{
		get
		{
			return CheckAndGetTextureHandle(ref m_OverlayUITexture);
		}
		internal set
		{
			CheckAndSetTextureHandle(ref m_OverlayUITexture, value);
		}
	}

	public TextureHandle ColorGradingLUT
	{
		get
		{
			return CheckAndGetTextureHandle(ref m_ColorGradingLUT);
		}
		set
		{
			CheckAndSetTextureHandle(ref m_ColorGradingLUT, value);
		}
	}

	public void ImportCameraData(RenderGraph renderGraph, WaaaghCameraData cameraData)
	{
		TextureDesc desc = RenderingUtils.CreateTextureDesc(null, cameraData.cameraTargetDescriptor);
		desc.filterMode = FilterMode.Bilinear;
		desc.wrapMode = TextureWrapMode.Clamp;
		desc.enableRandomWrite = true;
		TextureDesc desc2 = RenderingUtils.CreateTextureDesc(null, cameraData.cameraTargetDescriptor);
		desc2.depthBufferBits = DepthBits.Depth32;
		desc2.filterMode = FilterMode.Point;
		desc2.wrapMode = TextureWrapMode.Clamp;
		new int2(cameraData.pixelWidth, cameraData.pixelHeight);
		int2 @int = new int2(cameraData.scaledWidth, cameraData.scaledHeight);
		m_CameraNonScaledColorBuffer = renderGraph.ImportTexture(cameraData.Buffer.ColorBuffer);
		m_CameraNonScaledDepthBuffer = renderGraph.ImportTexture(cameraData.Buffer.DepthBuffer);
		desc.name = "CameraColorScaled";
		desc.width = @int.x;
		desc.height = @int.y;
		m_CameraScaledColorBuffer = renderGraph.CreateTexture(in desc);
		desc2.name = "CameraDepthScaled";
		desc2.width = @int.x;
		desc2.height = @int.y;
		m_CameraScaledDepthBuffer = renderGraph.CreateTexture(in desc2);
		RenderTargetInfo renderTargetInfo = default(RenderTargetInfo);
		RenderTargetInfo renderTargetInfo2 = default(RenderTargetInfo);
		int msaaSamples = 1;
		renderTargetInfo.width = Screen.width;
		renderTargetInfo.height = Screen.height;
		renderTargetInfo.volumeDepth = 1;
		renderTargetInfo.msaaSamples = msaaSamples;
		renderTargetInfo.format = WaaaghPipeline.MakeRenderTextureGraphicsFormat(cameraData.isHdrEnabled, cameraData.hdrColorBufferPrecision, Graphics.preserveFramebufferAlpha);
		renderTargetInfo2 = renderTargetInfo;
		renderTargetInfo2.format = SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil);
		TextureHandle nullHandle = TextureHandle.nullHandle;
		TextureHandle nullHandle2 = TextureHandle.nullHandle;
		if (cameraData.targetTexture != null)
		{
			renderTargetInfo.width = cameraData.targetTexture.width;
			renderTargetInfo.height = cameraData.targetTexture.height;
			renderTargetInfo.format = cameraData.targetTexture.graphicsFormat;
		}
		if (cameraData.TargetDepthTexture != null)
		{
			renderTargetInfo2.width = cameraData.TargetDepthTexture.width;
			renderTargetInfo2.height = cameraData.TargetDepthTexture.height;
			renderTargetInfo2.format = cameraData.TargetDepthTexture.depthStencilFormat;
			if (renderTargetInfo2.format == GraphicsFormat.None)
			{
				renderTargetInfo2.format = SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil);
				Debug.LogWarning("In the render graph API, the output Render Texture must have a depth buffer. When you select a Render Texture in any camera's Output Texture property, the Depth Stencil Format property of the texture must be set to a value other than None. Camera: " + cameraData.camera.name + " Texture: " + cameraData.TargetDepthTexture.name);
			}
		}
		else if (cameraData.targetTexture != null)
		{
			renderTargetInfo2.width = cameraData.targetTexture.width;
			renderTargetInfo2.height = cameraData.targetTexture.height;
			renderTargetInfo2.format = cameraData.targetTexture.depthStencilFormat;
			if (renderTargetInfo2.format == GraphicsFormat.None)
			{
				renderTargetInfo2.format = SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil);
				Debug.LogWarning("In the render graph API, the output Render Texture must have a depth buffer. When you select a Render Texture in any camera's Output Texture property, the Depth Stencil Format property of the texture must be set to a value other than None. Camera: " + cameraData.camera.name + " Texture: " + cameraData.targetTexture.name);
			}
		}
		nullHandle = renderGraph.ImportTexture(cameraData.renderer.CurrentColorBuffer, renderTargetInfo);
		nullHandle2 = renderGraph.ImportTexture(cameraData.renderer.CurrentDepthBuffer, renderTargetInfo2);
		switch (cameraData.CameraRenderTargetBufferType)
		{
		case CameraRenderTargetType.Scaled:
			CameraColorBuffer = m_CameraScaledColorBuffer;
			CameraDepthBuffer = m_CameraScaledDepthBuffer;
			break;
		default:
			CameraColorBuffer = m_CameraNonScaledColorBuffer;
			CameraDepthBuffer = m_CameraNonScaledDepthBuffer;
			break;
		}
		switch (cameraData.CameraResolveTargetBufferType)
		{
		case CameraResolveTargetType.NonScaled:
			CameraResolveColorBuffer = m_CameraNonScaledColorBuffer;
			CameraResolveDepthBuffer = m_CameraNonScaledDepthBuffer;
			break;
		case CameraResolveTargetType.Backbuffer:
			CameraResolveColorBuffer = nullHandle;
			CameraResolveDepthBuffer = nullHandle2;
			break;
		default:
			CameraResolveColorBuffer = TextureHandle.nullHandle;
			CameraResolveDepthBuffer = TextureHandle.nullHandle;
			break;
		}
	}

	internal void ImportShadowData(RenderGraph renderGraph, WaaaghShadowData shadowData)
	{
		Shadowmap = ((shadowData.ShadowManager.ShadowMapAtlas != null) ? renderGraph.ImportTexture(shadowData.ShadowManager.ShadowMapAtlas.Texture) : TextureHandle.nullHandle);
		CachedShadowmap = ((shadowData.ShadowManager.CachedShadowMapAtlas != null) ? renderGraph.ImportTexture(shadowData.ShadowManager.CachedShadowMapAtlas.Texture) : TextureHandle.nullHandle);
	}

	internal void ImportVTResources(RenderGraph renderGraph, VirtualTextureManager virtualTextureManager)
	{
		VTPackedFeedbackBufferUAV = renderGraph.ImportBuffer(virtualTextureManager.PackedFeedbackBufferUAV);
		VTFeedbackRT = renderGraph.ImportTexture(virtualTextureManager.FeedbackRT);
	}

	internal void CreateGBuffer(RenderGraph renderGraph, WaaaghCameraData cameraData)
	{
		TextureDesc textureDesc = RenderingUtils.CreateTextureDesc("GBuffer0", cameraData.cameraTargetDescriptor);
		textureDesc.filterMode = FilterMode.Point;
		textureDesc.wrapMode = TextureWrapMode.Clamp;
		textureDesc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
		TextureDesc desc = textureDesc;
		desc.name = "CameraAlbedoRT";
		desc.colorFormat = ((QualitySettings.activeColorSpace == ColorSpace.Linear) ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm);
		CameraAlbedoRT = renderGraph.CreateTexture(in desc);
		TextureDesc desc2 = textureDesc;
		desc2.name = "CameraSpecularRT";
		desc2.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
		CameraSpecularRT = renderGraph.CreateTexture(in desc2);
		TextureDesc desc3 = textureDesc;
		desc3.name = "CameraNormalsRT";
		CameraNormalsRT = renderGraph.CreateTexture(in desc3);
		TextureDesc desc4 = textureDesc;
		desc4.name = "CameraBakedGIRT";
		desc4.colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
		CameraBakedGIRT = renderGraph.CreateTexture(in desc4);
		TextureDesc desc5 = textureDesc;
		desc5.name = "CameraShadowmaskRT";
		CameraShadowmaskRT = renderGraph.CreateTexture(in desc5);
		TextureDesc desc6 = textureDesc;
		desc6.name = "CameraTranslucencyRT";
		CameraTranslucencyRT = renderGraph.CreateTexture(in desc6);
		TextureDesc desc7 = textureDesc;
		desc7.name = "CameraDepthCopyRT";
		desc7.colorFormat = GraphicsFormat.R32_SFloat;
		CameraDepthCopyRT = renderGraph.CreateTexture(in desc7);
		TextureDesc desc8 = textureDesc;
		desc8.name = "CameraColorPyramidRT";
		desc8.colorFormat = cameraData.cameraTargetDescriptor.graphicsFormat;
		desc8.useMipMap = true;
		desc8.autoGenerateMips = false;
		desc8.filterMode = FilterMode.Trilinear;
		CameraColorPyramidRT = renderGraph.CreateTexture(in desc8);
	}

	public override void Reset()
	{
		m_CameraScaledColorBuffer = TextureHandle.nullHandle;
		m_CameraScaledDepthBuffer = TextureHandle.nullHandle;
		m_CameraNonScaledColorBuffer = TextureHandle.nullHandle;
		m_CameraNonScaledDepthBuffer = TextureHandle.nullHandle;
		m_CameraResolveColorBuffer = TextureHandle.nullHandle;
		m_CameraResolveDepthBuffer = TextureHandle.nullHandle;
		m_CameraColorBuffer = TextureHandle.nullHandle;
		m_CameraDepthBuffer = TextureHandle.nullHandle;
		m_OverlayUITexture = TextureHandle.nullHandle;
		m_ColorGradingLUT = TextureHandle.nullHandle;
		CameraColorPyramidRT = TextureHandle.nullHandle;
		CameraDepthCopyRT = TextureHandle.nullHandle;
		CameraAlbedoRT = TextureHandle.nullHandle;
		CameraSpecularRT = TextureHandle.nullHandle;
		CameraNormalsRT = TextureHandle.nullHandle;
		CameraBakedGIRT = TextureHandle.nullHandle;
		CameraShadowmaskRT = TextureHandle.nullHandle;
		CameraTranslucencyRT = TextureHandle.nullHandle;
		TilesMinMaxZTexture = TextureHandle.nullHandle;
		CameraDepthPyramidRT = TextureHandle.nullHandle;
		SsrRT = TextureHandle.nullHandle;
		CameraMotionVectorsRT = TextureHandle.nullHandle;
		VolumetricScatter = TextureHandle.nullHandle;
		PackedReprojectedDepth = TextureHandle.nullHandle;
		DepthReprojectionParameters = default(GPUDrivenDepthReprojectionUtils.ReprojectionParameters);
		m_Shadowmap = TextureHandle.nullHandle;
		m_CachedShadowmap = TextureHandle.nullHandle;
		m_VTPackedFeedbackBufferUAV = BufferHandle.nullHandle;
		m_VTFeedbackRT = TextureHandle.nullHandle;
		LightDataConstantBuffer = BufferHandle.nullHandle;
		LightVolumeDataConstantBuffer = BufferHandle.nullHandle;
		ZBinsConstantBuffer = BufferHandle.nullHandle;
		LightTilesBuffer = BufferHandle.nullHandle;
		DeferredLightingFeatureTilesBuffer = BufferHandle.nullHandle;
		DeferredLightingIndirectArgsBuffer = BufferHandle.nullHandle;
		DeferredLightingFeatureTilesListsBuffer = BufferHandle.nullHandle;
	}
}
