using Owlcat.Runtime.Visual.VirtualTexture;
using Owlcat.Runtime.Visual.Waaagh.History;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Owlcat.Runtime.Visual.Waaagh.Recorders;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

internal static class FrameResourcesFactory
{
	private static readonly CameraStackTargets s_ReusableCameraStackTargets = new CameraStackTargets();

	public static FrameResources Create(RenderGraph renderGraph, WaaaghRenderingData renderingData, WaaaghCameraData cameraData, VirtualTextureManager virtualTextureManager, WaaaghLights waaaghLights, WaaaghShadowData shadowData, FinalTargetHandles finalTargetHandles)
	{
		FrameResources result = default(FrameResources);
		result.FinalTarget = CreateFinalTarget(renderGraph, cameraData, finalTargetHandles);
		result.CameraStackTargets = CreateCameraStackTargets(renderGraph, cameraData);
		result.GBuffer = CreateGBuffer(renderGraph, cameraData);
		result.DBuffer = CreateDBuffer(renderGraph, cameraData);
		result.VTFeedbackData = CreateVTFeedbackData(renderGraph, virtualTextureManager, cameraData);
		result.CameraAdditionalTargets = CreateCameraAdditionalTargets(renderGraph, cameraData);
		result.DeferredLightingResources = CreateDeferredLightingSources(renderGraph, waaaghLights);
		result.Shadows = CreateShadows(renderGraph, shadowData);
		result.SsrTargets = CreateSsrTargets(renderGraph, cameraData);
		return result;
	}

	private static DeferredLightingResources CreateDeferredLightingSources(RenderGraph renderGraph, WaaaghLights waaaghLights)
	{
		DeferredLightingResources result = default(DeferredLightingResources);
		result.LightDataConstantBuffer = renderGraph.ImportBuffer(waaaghLights.LightDataConstantBuffer);
		result.LightVolumeDataConstantBuffer = renderGraph.ImportBuffer(waaaghLights.LightVolumeDataConstantBuffer);
		result.LightTilesBuffer = renderGraph.ImportBuffer(waaaghLights.LightTilesBuffer);
		result.FeatureTilesBuffer = renderGraph.ImportBuffer(waaaghLights.DeferredLightingFeatureTilesBuffer);
		result.FeatureTilesListsBuffer = renderGraph.ImportBuffer(waaaghLights.DeferredLightingFeatureTilesListsBuffer);
		result.FeatureIndirectArgsBuffer = renderGraph.ImportBuffer(waaaghLights.DeferredLightingIndirectArgsBuffer);
		return result;
	}

	private static CameraStackTargets CreateCameraStackTargets(RenderGraph renderGraph, WaaaghCameraData cameraData)
	{
		RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
		descriptor.depthBufferBits = 0;
		descriptor.enableRandomWrite = true;
		RenderTextureDescriptor descriptor2 = cameraData.cameraTargetDescriptor;
		descriptor2.graphicsFormat = GraphicsFormat.None;
		descriptor2.depthStencilFormat = CoreUtils.GetDefaultDepthStencilFormat();
		descriptor2.depthBufferBits = 24;
		RenderTextureDescriptor descriptor3 = descriptor;
		descriptor3.width = cameraData.pixelWidth;
		descriptor3.height = cameraData.pixelHeight;
		RenderTextureDescriptor descriptor4 = descriptor2;
		descriptor4.width = cameraData.pixelWidth;
		descriptor4.height = cameraData.pixelHeight;
		CameraStackTargetHandles stackTargetHandles = cameraData.StackInfo.StackTargetHandles;
		if (cameraData.StackInfo.IsSingleCamera)
		{
			CameraStackTargets cameraStackTargets = s_ReusableCameraStackTargets;
			switch (cameraData.StackInfo.RequiredTargets)
			{
			case CameraRequiredTargets.Unscaled:
			{
				TextureDesc desc = RenderingUtils.CreateTextureDesc(CameraStackTargetHandles._CameraColorUnscaledName, descriptor3);
				TextureHandle color3 = renderGraph.CreateTexture(in desc);
				desc = RenderingUtils.CreateTextureDesc(CameraStackTargetHandles._CameraDepthUnscaledName, descriptor4, descriptor4.depthStencilFormat);
				TextureHandle depth3 = renderGraph.CreateTexture(in desc);
				TextureHandle nullHandle3 = TextureHandle.nullHandle;
				TextureHandle nullHandle4 = TextureHandle.nullHandle;
				cameraStackTargets.SetTargets(color3, depth3, nullHandle3, nullHandle4);
				break;
			}
			case CameraRequiredTargets.Scaled:
			{
				TextureDesc desc = RenderingUtils.CreateTextureDesc(CameraStackTargetHandles._CameraColorScaledName, descriptor);
				TextureHandle color2 = renderGraph.CreateTexture(in desc);
				desc = RenderingUtils.CreateTextureDesc(CameraStackTargetHandles._CameraDepthScaledName, descriptor2, descriptor4.depthStencilFormat);
				TextureHandle depth2 = renderGraph.CreateTexture(in desc);
				TextureHandle nullHandle = TextureHandle.nullHandle;
				TextureHandle nullHandle2 = TextureHandle.nullHandle;
				cameraStackTargets.SetTargets(color2, depth2, nullHandle, nullHandle2);
				break;
			}
			case CameraRequiredTargets.Both:
			{
				TextureDesc desc = RenderingUtils.CreateTextureDesc(CameraStackTargetHandles._CameraColorScaledName, descriptor);
				TextureHandle color = renderGraph.CreateTexture(in desc);
				desc = RenderingUtils.CreateTextureDesc(CameraStackTargetHandles._CameraDepthScaledName, descriptor2, descriptor4.depthStencilFormat);
				TextureHandle depth = renderGraph.CreateTexture(in desc);
				desc = RenderingUtils.CreateTextureDesc(CameraStackTargetHandles._CameraColorUnscaledName, descriptor3);
				TextureHandle unscaledColor = renderGraph.CreateTexture(in desc);
				desc = RenderingUtils.CreateTextureDesc(CameraStackTargetHandles._CameraDepthUnscaledName, descriptor4, descriptor4.depthStencilFormat);
				TextureHandle unscaledDepth = renderGraph.CreateTexture(in desc);
				cameraStackTargets.SetTargets(color, depth, unscaledColor, unscaledDepth);
				break;
			}
			}
			return cameraStackTargets;
		}
		CameraStackTargets cameraStackTargets2 = s_ReusableCameraStackTargets;
		if (cameraData.renderType == CameraRenderType.Base)
		{
			RenderingUtils.ReAllocateHandleIfNeeded(ref stackTargetHandles.UnscaledColor, in descriptor3, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, CameraStackTargetHandles._CameraColorUnscaledName);
			RenderingUtils.ReAllocateHandleIfNeeded(ref stackTargetHandles.UnscaledDepth, in descriptor4, FilterMode.Point, TextureWrapMode.Clamp, 1, 0f, CameraStackTargetHandles._CameraDepthUnscaledName);
			if (cameraData.StackInfo.StackHasScaling)
			{
				RenderingUtils.ReAllocateHandleIfNeeded(ref stackTargetHandles.ScaledColor, in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, CameraStackTargetHandles._CameraColorScaledName);
				RenderingUtils.ReAllocateHandleIfNeeded(ref stackTargetHandles.ScaledDepth, in descriptor2, FilterMode.Point, TextureWrapMode.Clamp, 1, 0f, CameraStackTargetHandles._CameraDepthScaledName);
			}
		}
		switch (cameraData.StackInfo.RequiredTargets)
		{
		case CameraRequiredTargets.Unscaled:
		{
			TextureHandle color6 = renderGraph.ImportTexture(stackTargetHandles.UnscaledColor);
			TextureHandle depth6 = renderGraph.ImportTexture(stackTargetHandles.UnscaledDepth);
			TextureHandle nullHandle7 = TextureHandle.nullHandle;
			TextureHandle nullHandle8 = TextureHandle.nullHandle;
			cameraStackTargets2.SetTargets(color6, depth6, nullHandle7, nullHandle8);
			break;
		}
		case CameraRequiredTargets.Scaled:
		{
			TextureHandle color5 = renderGraph.ImportTexture(stackTargetHandles.ScaledColor);
			TextureHandle depth5 = renderGraph.ImportTexture(stackTargetHandles.ScaledDepth);
			TextureHandle nullHandle5 = TextureHandle.nullHandle;
			TextureHandle nullHandle6 = TextureHandle.nullHandle;
			cameraStackTargets2.SetTargets(color5, depth5, nullHandle5, nullHandle6);
			break;
		}
		case CameraRequiredTargets.Both:
		{
			TextureHandle color4 = renderGraph.ImportTexture(stackTargetHandles.ScaledColor);
			TextureHandle depth4 = renderGraph.ImportTexture(stackTargetHandles.ScaledDepth);
			TextureHandle unscaledColor2 = renderGraph.ImportTexture(stackTargetHandles.UnscaledColor);
			TextureHandle unscaledDepth2 = renderGraph.ImportTexture(stackTargetHandles.UnscaledDepth);
			cameraStackTargets2.SetTargets(color4, depth4, unscaledColor2, unscaledDepth2);
			break;
		}
		}
		return cameraStackTargets2;
	}

	private static GBufferResources CreateGBuffer(RenderGraph renderGraph, WaaaghCameraData cameraData)
	{
		TextureDesc textureDesc = RenderingUtils.CreateTextureDesc("GBuffer0", cameraData.cameraTargetDescriptor);
		textureDesc.filterMode = FilterMode.Point;
		textureDesc.wrapMode = TextureWrapMode.Clamp;
		textureDesc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
		textureDesc.clearBuffer = true;
		textureDesc.clearColor = Color.clear;
		TextureDesc desc = textureDesc;
		desc.name = "CameraAlbedoRT";
		desc.colorFormat = ((QualitySettings.activeColorSpace == ColorSpace.Linear) ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm);
		TextureHandle albedo = renderGraph.CreateTexture(in desc);
		TextureDesc desc2 = textureDesc;
		desc2.name = "CameraSpecularRT";
		desc2.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
		TextureHandle specular = renderGraph.CreateTexture(in desc2);
		TextureDesc desc3 = textureDesc;
		desc3.name = "CameraNormalsRT";
		TextureHandle normals = renderGraph.CreateTexture(in desc3);
		TextureDesc desc4 = textureDesc;
		desc4.name = "CameraBakedGIRT";
		desc4.colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
		TextureHandle bakedGI = renderGraph.CreateTexture(in desc4);
		TextureDesc desc5 = textureDesc;
		desc5.name = "CameraShadowmaskRT";
		TextureHandle shadowmask = renderGraph.CreateTexture(in desc5);
		TextureDesc desc6 = textureDesc;
		desc6.name = "CameraTranslucencyRT";
		TextureHandle translucency = renderGraph.CreateTexture(in desc6);
		GBufferResources result = default(GBufferResources);
		result.Albedo = albedo;
		result.Specular = specular;
		result.Normals = normals;
		result.Translucency = translucency;
		result.BakedGI = bakedGI;
		result.Shadowmask = shadowmask;
		return result;
	}

	private static DBufferResources CreateDBuffer(RenderGraph renderGraph, WaaaghCameraData cameraData)
	{
		DBufferResources result = default(DBufferResources);
		TextureDesc desc = Decals.GetDBufferMasksTextureDesc(in cameraData);
		result.Masks = renderGraph.CreateTexture(in desc);
		TextureDesc desc2 = Decals.GetDBufferNormalTextureDesc(in cameraData);
		result.Normals = renderGraph.CreateTexture(in desc2);
		return result;
	}

	public static VTFeedbackData CreateVTFeedbackData(RenderGraph renderGraph, VirtualTextureManager virtualTextureManager, WaaaghCameraData cameraData)
	{
		VTFeedbackData result;
		if (virtualTextureManager == null)
		{
			result = default(VTFeedbackData);
			result.VTFeedback = TextureHandle.nullHandle;
			result.VTFeedbackMRT = TextureHandle.nullHandle;
			result.VTPackedFeedbackBuffer = BufferHandle.nullHandle;
			return result;
		}
		TextureDesc desc = RenderingUtils.CreateTextureDesc("VTFeedbackMRT", cameraData.cameraTargetDescriptor);
		desc.colorFormat = GraphicsFormat.R16G16_UNorm;
		desc.filterMode = FilterMode.Point;
		desc.wrapMode = TextureWrapMode.Clamp;
		desc.clearBuffer = true;
		desc.clearColor = Color.white;
		result = default(VTFeedbackData);
		result.VTFeedback = renderGraph.ImportTexture(virtualTextureManager.FeedbackRT);
		result.VTFeedbackMRT = renderGraph.CreateTexture(in desc);
		result.VTPackedFeedbackBuffer = renderGraph.ImportBuffer(virtualTextureManager.PackedFeedbackBufferUAV);
		return result;
	}

	private static CameraAdditionalTargets CreateCameraAdditionalTargets(RenderGraph renderGraph, WaaaghCameraData cameraData)
	{
		TextureDesc textureDesc = RenderingUtils.CreateTextureDesc("GBuffer0", cameraData.cameraTargetDescriptor);
		textureDesc.filterMode = FilterMode.Point;
		textureDesc.wrapMode = TextureWrapMode.Clamp;
		textureDesc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
		TextureDesc desc = textureDesc;
		desc.name = "CameraDepthCopyRT";
		desc.colorFormat = GraphicsFormat.R32_SFloat;
		TextureDesc desc2 = textureDesc;
		desc2.name = "CameraColorPyramidRT";
		desc2.colorFormat = cameraData.cameraTargetDescriptor.graphicsFormat;
		desc2.useMipMap = true;
		desc2.autoGenerateMips = false;
		desc2.filterMode = FilterMode.Trilinear;
		TextureDesc desc3 = RenderingUtils.CreateTextureDesc("CameraMotionVectorsRT", cameraData.cameraTargetDescriptor);
		desc3.colorFormat = GraphicsFormat.R16G16_SFloat;
		desc3.filterMode = FilterMode.Bilinear;
		desc3.wrapMode = TextureWrapMode.Clamp;
		desc3.clearBuffer = true;
		TextureDesc desc4 = desc3;
		desc4.depthBufferBits = DepthBits.Depth24;
		desc4.filterMode = FilterMode.Point;
		desc4.name = "CameraMotionVectorsDepthRT";
		WaaaghCameraHistory historyManager = cameraData.historyManager;
		TextureHandle rawColorHistory = TextureHandle.nullHandle;
		TextureHandle rawDepthHistory = TextureHandle.nullHandle;
		if (historyManager != null)
		{
			RawColorHistory historyForRead = historyManager.GetHistoryForRead<RawColorHistory>();
			RawDepthHistory historyForRead2 = historyManager.GetHistoryForRead<RawDepthHistory>();
			if (historyForRead != null)
			{
				rawColorHistory = renderGraph.ImportTexture(historyForRead.GetCurrentTexture());
			}
			if (historyForRead2 != null)
			{
				rawDepthHistory = renderGraph.ImportTexture(historyForRead2.GetCurrentTexture());
			}
		}
		CameraAdditionalTargets result = default(CameraAdditionalTargets);
		result.DepthCopy = renderGraph.CreateTexture(in desc);
		result.ColorPyramid = renderGraph.CreateTexture(in desc2);
		result.MotionVectors = renderGraph.CreateTexture(in desc3);
		result.MotionVectorsDepth = renderGraph.CreateTexture(in desc4);
		result.RawColorHistory = rawColorHistory;
		result.RawDepthHistory = rawDepthHistory;
		return result;
	}

	public static FinalTarget CreateFinalTarget(RenderGraph renderGraph, WaaaghCameraData cameraData, FinalTargetHandles finalTargetHandles)
	{
		FinalTarget finalTarget = default(FinalTarget);
		SetupTargetHandles(ref finalTarget, cameraData, finalTargetHandles);
		ImportBackBuffers(ref finalTarget, renderGraph, cameraData, finalTargetHandles);
		return finalTarget;
	}

	private static void SetupTargetHandles(ref FinalTarget finalTarget, WaaaghCameraData cameraData, FinalTargetHandles finalTargetHandles)
	{
		RenderTargetIdentifier renderTargetIdentifier = ((cameraData.targetTexture != null) ? new RenderTargetIdentifier(cameraData.targetTexture) : ((RenderTargetIdentifier)BuiltinRenderTextureType.CameraTarget));
		RenderTargetIdentifier renderTargetIdentifier2 = ((cameraData.targetTexture != null) ? new RenderTargetIdentifier(cameraData.targetTexture) : ((RenderTargetIdentifier)BuiltinRenderTextureType.Depth));
		if (finalTargetHandles.ColorHandle == null)
		{
			finalTargetHandles.ColorHandle = RTHandles.Alloc(renderTargetIdentifier, "Backbuffer color");
		}
		else if (finalTargetHandles.ColorHandle.nameID != renderTargetIdentifier)
		{
			RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref finalTargetHandles.ColorHandle, renderTargetIdentifier);
		}
		if (finalTargetHandles.DepthHandle == null)
		{
			finalTargetHandles.DepthHandle = RTHandles.Alloc(renderTargetIdentifier2, "Backbuffer depth");
		}
		else if (finalTargetHandles.DepthHandle.nameID != renderTargetIdentifier2)
		{
			RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref finalTargetHandles.DepthHandle, renderTargetIdentifier2);
		}
	}

	private static void ImportBackBuffers(ref FinalTarget finalTarget, RenderGraph renderGraph, WaaaghCameraData cameraData, FinalTargetHandles finalTargetHandles)
	{
		bool clearOnFirstUse = false;
		Color black = Color.black;
		bool discardOnLastUse = false;
		bool flag = true;
		TextureUVOrigin textureUVOrigin = ((!cameraData.isSceneViewCamera && !cameraData.isPreviewCamera && cameraData.targetTexture == null) ? (SystemInfo.graphicsUVStartsAtTop ? TextureUVOrigin.TopLeft : TextureUVOrigin.BottomLeft) : TextureUVOrigin.BottomLeft);
		ImportResourceParams importParams = default(ImportResourceParams);
		importParams.clearOnFirstUse = clearOnFirstUse;
		importParams.clearColor = black;
		importParams.discardOnLastUse = discardOnLastUse;
		importParams.textureUVOrigin = textureUVOrigin;
		ImportResourceParams importParams2 = default(ImportResourceParams);
		importParams2.clearOnFirstUse = clearOnFirstUse;
		importParams2.clearColor = black;
		importParams2.discardOnLastUse = !flag;
		importParams2.textureUVOrigin = textureUVOrigin;
		RenderTargetInfo renderTargetInfo = default(RenderTargetInfo);
		RenderTargetInfo renderTargetInfo2 = default(RenderTargetInfo);
		if (cameraData.targetTexture == null)
		{
			int msaaSamples = 1;
			renderTargetInfo.width = Screen.width;
			renderTargetInfo.height = Screen.height;
			renderTargetInfo.volumeDepth = 1;
			renderTargetInfo.msaaSamples = msaaSamples;
			renderTargetInfo.format = cameraData.cameraTargetDescriptor.graphicsFormat;
			renderTargetInfo2 = renderTargetInfo;
			renderTargetInfo2.format = GraphicsFormat.D24_UNorm_S8_UInt;
		}
		else
		{
			renderTargetInfo.width = cameraData.targetTexture.width;
			renderTargetInfo.height = cameraData.targetTexture.height;
			renderTargetInfo.volumeDepth = cameraData.targetTexture.volumeDepth;
			renderTargetInfo.msaaSamples = cameraData.targetTexture.antiAliasing;
			renderTargetInfo.format = cameraData.targetTexture.graphicsFormat;
			renderTargetInfo2 = renderTargetInfo;
			renderTargetInfo2.format = cameraData.targetTexture.depthStencilFormat;
			if (renderTargetInfo2.format == GraphicsFormat.None)
			{
				renderTargetInfo2.format = SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil);
				Debug.LogWarning("In the render graph API, the output Render Texture must have a depth buffer. When you select a Render Texture in any camera's Output Texture property, the Depth Stencil Format property of the texture must be set to a value other than None.");
			}
		}
		finalTarget.Color = renderGraph.ImportTexture(finalTargetHandles.ColorHandle, renderTargetInfo, importParams);
		finalTarget.Depth = renderGraph.ImportTexture(finalTargetHandles.DepthHandle, renderTargetInfo2, importParams2);
	}

	private static ShadowsResources CreateShadows(RenderGraph renderGraph, WaaaghShadowData shadowData)
	{
		ShadowsResources result = default(ShadowsResources);
		result.Shadowmap = ((shadowData.ShadowManager.ShadowMapAtlas != null) ? renderGraph.ImportTexture(shadowData.ShadowManager.ShadowMapAtlas.Texture) : TextureHandle.nullHandle);
		result.CachedShadowmap = ((shadowData.ShadowManager.CachedShadowMapAtlas != null) ? renderGraph.ImportTexture(shadowData.ShadowManager.CachedShadowMapAtlas.Texture) : TextureHandle.nullHandle);
		return result;
	}

	private static SsrTargets CreateSsrTargets(RenderGraph renderGraph, WaaaghCameraData cameraData)
	{
		SsrTargets result = default(SsrTargets);
		SsrHistory ssrHistory = cameraData.SsrHistory;
		if (cameraData.IsSSREnabled && ssrHistory != null)
		{
			result.SsrCurrent = renderGraph.ImportTexture(ssrHistory.GetCurrentTexture());
			result.SsrPrev = renderGraph.ImportTexture(ssrHistory.GetPreviousTexture());
		}
		return result;
	}
}
