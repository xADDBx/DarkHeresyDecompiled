using System;
using Owlcat.Runtime.Visual.Overrides;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.PostProcess;

public static class BloomRecorder
{
	private struct BloomMaterialParams
	{
		internal Vector4 parameters;

		internal bool highQualityFiltering;

		internal bool useRGBM;

		internal bool enableAlphaOutput;

		internal bool Equals(ref BloomMaterialParams other)
		{
			if (parameters == other.parameters && highQualityFiltering == other.highQualityFiltering && useRGBM == other.useRGBM)
			{
				return enableAlphaOutput == other.enableAlphaOutput;
			}
			return false;
		}
	}

	private class BloomEnhancedPassData
	{
		public Material Material;

		public TextureHandle Source;

		public TextureHandle[] BloomMipDown;

		public TextureHandle[] BloomMipUp;

		public int MipCount;
	}

	private class BloomPassData
	{
		internal int mipCount;

		internal Material material;

		internal Material[] upsampleMaterials;

		internal TextureHandle sourceTexture;

		internal TextureHandle[] bloomMipUp;

		internal TextureHandle[] bloomMipDown;
	}

	private static BloomMaterialParams s_PrevParams;

	public static void RenderBloomEnhancedTexture(PostProcessor postProcessor, RenderGraph renderGraph, out TextureHandle destination)
	{
		RenderTextureDescriptor descriptor = postProcessor.FrameState.Descriptor;
		int num = descriptor.width / 2;
		int num2 = descriptor.height / 2;
		BloomEnhanced bloomEnhanced = postProcessor.Overrides.BloomEnhanced;
		float num3 = Mathf.Log(num2, 2f) + bloomEnhanced.radius.value - 8f;
		int num4 = (int)num3;
		int num5 = Mathf.Clamp(num4, 1, 16);
		float thresholdLinear = bloomEnhanced.thresholdLinear;
		float y = (bloomEnhanced.antiFlicker.value ? (-0.5f) : 0f);
		float z = 0.5f + num3 - (float)num4;
		Material bloomEnhanced2 = postProcessor.MatLib.BloomEnhanced;
		bloomEnhanced2.SetVector(PostProcessor.ShaderIDs._Params, new Vector4(thresholdLinear, y, z, bloomEnhanced.dirtIntensity.value));
		bloomEnhanced2.SetVector(PostProcessor.ShaderIDs._Params1, new Vector4(bloomEnhanced.clamp.value, 0f, 0f, 0f));
		float num6 = thresholdLinear * bloomEnhanced.softKnee.value + 1E-05f;
		bloomEnhanced2.SetVector(value: new Vector3(thresholdLinear - num6, num6 * 2f, 0.25f / num6), nameID: PostProcessor.ShaderIDs._Curve);
		CoreUtils.SetKeyword(bloomEnhanced2, ShaderKeywordStrings.ANTI_FLICKER, bloomEnhanced.antiFlicker.value);
		CoreUtils.SetKeyword(bloomEnhanced2, ShaderKeywordStrings.UseRGBM, postProcessor.StaticState.DefaultColorFormatUseRGBM);
		RenderTextureDescriptor compatibleDescriptor = PostProcessor.GetCompatibleDescriptor(descriptor, num, num2, postProcessor.StaticState.DefaultColorFormat);
		TextureHandle[] bloomMipDown = postProcessor.StaticState.BloomMipDown;
		TextureHandle[] bloomMipUp = postProcessor.StaticState.BloomMipUp;
		bloomMipDown[0] = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, postProcessor.StaticState.BloomMipDownName[0], clear: false, FilterMode.Bilinear);
		bloomMipUp[0] = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, postProcessor.StaticState.BloomMipUpName[0], clear: false, FilterMode.Bilinear);
		for (int i = 1; i < num5; i++)
		{
			num = Mathf.Max(1, num >> 1);
			num2 = Mathf.Max(1, num2 >> 1);
			ref TextureHandle reference = ref bloomMipDown[i];
			ref TextureHandle reference2 = ref bloomMipUp[i];
			compatibleDescriptor.width = num;
			compatibleDescriptor.height = num2;
			reference = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, postProcessor.StaticState.BloomMipDownName[i], clear: false, FilterMode.Bilinear);
			reference2 = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, postProcessor.StaticState.BloomMipUpName[i], clear: false, FilterMode.Bilinear);
		}
		BloomEnhancedPassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<BloomEnhancedPassData>("Bloom Enhanced", out passData2, WaaaghProfileId.BloomEnhanced.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\PostProcess\\BloomRecorder.cs", 93);
		TextureHandle input = (passData2.Source = postProcessor.CameraStackTargets.CurrentPostProcessSource);
		unsafeRenderGraphBuilder.UseTexture(in input);
		passData2.Material = postProcessor.MatLib.BloomEnhanced;
		passData2.BloomMipDown = postProcessor.StaticState.BloomMipDown;
		passData2.BloomMipUp = postProcessor.StaticState.BloomMipUp;
		passData2.MipCount = num5;
		for (int j = 0; j < num5; j++)
		{
			unsafeRenderGraphBuilder.UseTexture(in passData2.BloomMipDown[j], AccessFlags.ReadWrite);
			unsafeRenderGraphBuilder.UseTexture(in passData2.BloomMipUp[j], AccessFlags.ReadWrite);
		}
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(BloomEnhancedPassData passData, UnsafeGraphContext context)
		{
			CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
			Blitter.BlitCameraTexture(nativeCommandBuffer, passData.Source, passData.BloomMipDown[0], passData.Material, 0);
			TextureHandle textureHandle = passData.BloomMipDown[0];
			for (int k = 0; k < passData.MipCount; k++)
			{
				TextureHandle textureHandle2 = passData.BloomMipDown[k];
				TextureHandle textureHandle3 = passData.BloomMipUp[k];
				Blitter.BlitCameraTexture(nativeCommandBuffer, textureHandle, textureHandle3, passData.Material, 1);
				Blitter.BlitCameraTexture(nativeCommandBuffer, textureHandle3, textureHandle2, passData.Material, 2);
				textureHandle = textureHandle2;
			}
			for (int num7 = passData.MipCount - 2; num7 >= 0; num7--)
			{
				TextureHandle value2 = passData.BloomMipDown[num7];
				context.cmd.SetGlobalTexture(PostProcessor.ShaderIDs._BaseTex, value2);
				Blitter.BlitCameraTexture(nativeCommandBuffer, textureHandle, passData.BloomMipUp[num7], passData.Material, 3);
				textureHandle = passData.BloomMipUp[num7];
			}
			context.cmd.SetGlobalTexture(PostProcessor.ShaderIDs._Bloom_Texture, passData.BloomMipUp[0]);
		});
		destination = passData2.BloomMipUp[0];
	}

	internal static void RenderBloomTexture(PostProcessor postProcessor, RenderGraph renderGraph, out TextureHandle destination, bool enableAlphaOutput)
	{
		RenderTextureDescriptor descriptor = postProcessor.FrameState.Descriptor;
		Bloom bloom = postProcessor.Overrides.Bloom;
		int num = 1;
		num = bloom.downscale.value switch
		{
			BloomDownscaleMode.Half => 1, 
			BloomDownscaleMode.Quarter => 2, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		int num2 = descriptor.width >> num;
		int num3 = descriptor.height >> num;
		int num4 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Log(Mathf.Max(num2, num3), 2f) - 1f), 1, bloom.maxIterations.value);
		using (new ProfilingScope(WaaaghProfileId.BloomSetup.Sampler()))
		{
			float value = bloom.clamp.value;
			float num5 = Mathf.GammaToLinearSpace(bloom.threshold.value);
			float w = num5 * 0.5f;
			float x = Mathf.Lerp(0.05f, 0.95f, bloom.scatter.value);
			BloomMaterialParams other = default(BloomMaterialParams);
			other.parameters = new Vector4(x, value, num5, w);
			other.highQualityFiltering = bloom.highQualityFiltering.value;
			other.useRGBM = postProcessor.StaticState.DefaultColorFormatUseRGBM;
			other.enableAlphaOutput = enableAlphaOutput;
			Material bloom2 = postProcessor.MatLib.Bloom;
			bool num6 = !s_PrevParams.Equals(ref other);
			bool flag = bloom2.HasProperty(PostProcessor.ShaderIDs._Params);
			if (num6 || !flag)
			{
				bloom2.SetVector(PostProcessor.ShaderIDs._Params, other.parameters);
				CoreUtils.SetKeyword(bloom2, ShaderKeywordStrings.BloomHQ, other.highQualityFiltering);
				CoreUtils.SetKeyword(bloom2, ShaderKeywordStrings.UseRGBM, other.useRGBM);
				CoreUtils.SetKeyword(bloom2, "_ENABLE_ALPHA_OUTPUT", other.enableAlphaOutput);
				for (uint num7 = 0u; num7 < 16; num7++)
				{
					Material obj = postProcessor.MatLib.BloomUpsample[num7];
					obj.SetVector(PostProcessor.ShaderIDs._Params, other.parameters);
					CoreUtils.SetKeyword(obj, ShaderKeywordStrings.BloomHQ, other.highQualityFiltering);
					CoreUtils.SetKeyword(obj, ShaderKeywordStrings.UseRGBM, other.useRGBM);
					CoreUtils.SetKeyword(obj, "_ENABLE_ALPHA_OUTPUT", other.enableAlphaOutput);
				}
				s_PrevParams = other;
			}
			TextureHandle[] bloomMipDown = postProcessor.StaticState.BloomMipDown;
			TextureHandle[] bloomMipUp = postProcessor.StaticState.BloomMipUp;
			string[] bloomMipDownName = postProcessor.StaticState.BloomMipDownName;
			string[] bloomMipUpName = postProcessor.StaticState.BloomMipUpName;
			RenderTextureDescriptor compatibleDescriptor = PostProcessor.GetCompatibleDescriptor(descriptor, num2, num3, postProcessor.StaticState.DefaultColorFormat);
			bloomMipDown[0] = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, bloomMipDownName[0], clear: false, FilterMode.Bilinear);
			bloomMipUp[0] = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, bloomMipUpName[0], clear: false, FilterMode.Bilinear);
			for (int i = 1; i < num4; i++)
			{
				num2 = Mathf.Max(1, num2 >> 1);
				num3 = Mathf.Max(1, num3 >> 1);
				ref TextureHandle reference = ref bloomMipDown[i];
				ref TextureHandle reference2 = ref bloomMipUp[i];
				compatibleDescriptor.width = num2;
				compatibleDescriptor.height = num3;
				reference = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, bloomMipDownName[i], clear: false, FilterMode.Bilinear);
				reference2 = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, bloomMipUpName[i], clear: false, FilterMode.Bilinear);
			}
		}
		BloomPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<BloomPassData>("Bloom", out passData, WaaaghProfileId.Bloom.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\PostProcess\\BloomRecorder.cs", 255);
		TextureHandle input = postProcessor.CameraStackTargets.CurrentPostProcessSource;
		passData.mipCount = num4;
		passData.material = postProcessor.MatLib.Bloom;
		passData.upsampleMaterials = postProcessor.MatLib.BloomUpsample;
		passData.sourceTexture = input;
		passData.bloomMipDown = postProcessor.StaticState.BloomMipDown;
		passData.bloomMipUp = postProcessor.StaticState.BloomMipUp;
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.UseTexture(in input);
		for (int j = 0; j < num4; j++)
		{
			unsafeRenderGraphBuilder.UseTexture(in postProcessor.StaticState.BloomMipDown[j], AccessFlags.ReadWrite);
			unsafeRenderGraphBuilder.UseTexture(in postProcessor.StaticState.BloomMipUp[j], AccessFlags.ReadWrite);
		}
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(BloomPassData data, UnsafeGraphContext context)
		{
			CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
			Material material = data.material;
			int mipCount = data.mipCount;
			RenderBufferLoadAction loadAction = RenderBufferLoadAction.DontCare;
			RenderBufferStoreAction storeAction = RenderBufferStoreAction.Store;
			using (new ProfilingScope(nativeCommandBuffer, WaaaghProfileId.BloomPrefilter.Sampler()))
			{
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.sourceTexture, data.bloomMipDown[0], loadAction, storeAction, material, 0);
			}
			using (new ProfilingScope(nativeCommandBuffer, WaaaghProfileId.BloomDownsample.Sampler()))
			{
				TextureHandle textureHandle = data.bloomMipDown[0];
				for (int k = 1; k < mipCount; k++)
				{
					TextureHandle textureHandle2 = data.bloomMipDown[k];
					TextureHandle textureHandle3 = data.bloomMipUp[k];
					Blitter.BlitCameraTexture(nativeCommandBuffer, textureHandle, textureHandle3, loadAction, storeAction, material, 1);
					Blitter.BlitCameraTexture(nativeCommandBuffer, textureHandle3, textureHandle2, loadAction, storeAction, material, 2);
					textureHandle = textureHandle2;
				}
			}
			using (new ProfilingScope(nativeCommandBuffer, WaaaghProfileId.BloomUpsample.Sampler()))
			{
				for (int num8 = mipCount - 2; num8 >= 0; num8--)
				{
					TextureHandle textureHandle4 = ((num8 == mipCount - 2) ? data.bloomMipDown[num8 + 1] : data.bloomMipUp[num8 + 1]);
					TextureHandle textureHandle5 = data.bloomMipDown[num8];
					TextureHandle textureHandle6 = data.bloomMipUp[num8];
					Material material2 = data.upsampleMaterials[num8];
					material2.SetTexture(PostProcessor.ShaderIDs._SourceTexLowMip, textureHandle4);
					Blitter.BlitCameraTexture(nativeCommandBuffer, textureHandle5, textureHandle6, loadAction, storeAction, material2, 3);
				}
			}
		});
		destination = passData.bloomMipUp[0];
	}
}
