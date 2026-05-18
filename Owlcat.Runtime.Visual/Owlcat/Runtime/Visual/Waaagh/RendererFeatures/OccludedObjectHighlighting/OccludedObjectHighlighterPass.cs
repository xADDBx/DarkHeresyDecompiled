using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.OccludedObjectHighlighting;

internal static class OccludedObjectHighlighterPass
{
	private static class ShaderPropertyId
	{
		public static readonly int _CompositeParameters = Shader.PropertyToID("_CompositeParameters");
	}

	private sealed class PassData
	{
		public Settings Settings;

		public Materials Materials;

		public readonly List<RendererInfo> RendererInfos = new List<RendererInfo>();

		public TestPlanesResults[] RendererVisibility = Array.Empty<TestPlanesResults>();

		public float Intensity;

		public Vector4 CompositeParams;

		public TextureHandle CameraColorBuffer;

		public TextureHandle CameraDepthBuffer;

		public TextureHandle HighlightRT;

		public TextureHandle Blur1RT;

		public TextureHandle Blur2RT;
	}

	private static TextureDesc GetHighlightTextureDesc(in WaaaghCameraData cameraData)
	{
		TextureDesc result = RenderingUtils.CreateTextureDesc("OccludedObjectHighlightRT", cameraData.cameraTargetDescriptor);
		result.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
		result.filterMode = FilterMode.Bilinear;
		result.wrapMode = TextureWrapMode.Clamp;
		return result;
	}

	private static TextureDesc GetDownsampleTextureDesc(in WaaaghCameraData cameraData, string name, int downsampleFactor)
	{
		TextureDesc result = RenderingUtils.CreateTextureDesc(name, cameraData.cameraTargetDescriptor);
		result.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
		result.filterMode = FilterMode.Bilinear;
		result.wrapMode = TextureWrapMode.Clamp;
		result.width /= downsampleFactor;
		result.height /= downsampleFactor;
		return result;
	}

	public static void Record(in RecordContext context, in Settings settings, in Materials materials, List<RendererInfo> rendererInfos, ReadOnlySpan<TestPlanesResults> rendererVisibility)
	{
		Owlcat.Runtime.Visual.Overrides.OccludedObjectHighlighting component = VolumeManager.instance.stack.GetComponent<Owlcat.Runtime.Visual.Overrides.OccludedObjectHighlighting>();
		float num = (component.IsActive() ? component.Intensity.value : 0f);
		if (num <= 0f)
		{
			return;
		}
		PassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("Draw Occluded Highlight", out passData2, WaaaghProfileId.OccludedObjectHighlight.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\OccludedObjectHighlighting\\OccludedObjectHighlighterPass.cs", 71);
		passData2.Settings = settings;
		passData2.Materials = materials;
		passData2.RendererInfos.Clear();
		passData2.RendererInfos.AddRange(rendererInfos);
		if (passData2.RendererVisibility.Length < rendererVisibility.Length)
		{
			Array.Resize(ref passData2.RendererVisibility, Mathf.NextPowerOfTwo(rendererVisibility.Length));
		}
		rendererVisibility.CopyTo(passData2.RendererVisibility);
		passData2.Intensity = num;
		passData2.CompositeParams = new Vector4(settings.ScanLineFreq0, settings.ScanLineFreq1, settings.ScanLineSpeed * Time.time, settings.ScanLineOpacity);
		passData2.CameraColorBuffer = context.FrameResources.CameraStackTargets.Color;
		unsafeRenderGraphBuilder.UseTexture(in passData2.CameraColorBuffer, AccessFlags.Write);
		passData2.CameraDepthBuffer = context.FrameResources.CameraStackTargets.Depth;
		unsafeRenderGraphBuilder.UseTexture(in passData2.CameraDepthBuffer);
		PassData passData3 = passData2;
		TextureDesc desc = GetHighlightTextureDesc(in context.CameraData);
		passData3.HighlightRT = unsafeRenderGraphBuilder.CreateTransientTexture(in desc);
		int downsampleFactor = Mathf.Max(1, (int)settings.DownsampleFactor);
		PassData passData4 = passData2;
		desc = GetDownsampleTextureDesc(in context.CameraData, "Blur1RT", downsampleFactor);
		passData4.Blur1RT = unsafeRenderGraphBuilder.CreateTransientTexture(in desc);
		PassData passData5 = passData2;
		desc = GetDownsampleTextureDesc(in context.CameraData, "Blur2RT", downsampleFactor);
		passData5.Blur2RT = unsafeRenderGraphBuilder.CreateTransientTexture(in desc);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData passData, UnsafeGraphContext context)
		{
			Render(passData, context);
		});
	}

	private static void Render(PassData data, UnsafeGraphContext context)
	{
		CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
		context.cmd.SetRenderTarget(data.HighlightRT, data.CameraDepthBuffer);
		context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.clear);
		context.cmd.SetGlobalFloat(HighlightConstantBuffer._ZTest, 7f);
		context.cmd.SetGlobalFloat(HighlightConstantBuffer._ZWrite, 0f);
		List<Material> value;
		using (CollectionPool<List<Material>, Material>.Get(out value))
		{
			List<RendererInfo> rendererInfos = data.RendererInfos;
			int i = 0;
			for (int count = rendererInfos.Count; i < count; i++)
			{
				if (data.RendererVisibility[i] == TestPlanesResults.Outside)
				{
					continue;
				}
				RendererInfo rendererInfo = rendererInfos[i];
				if (rendererInfo.Renderer == null || !rendererInfo.Renderer.enabled || !rendererInfo.Renderer.gameObject.activeInHierarchy)
				{
					continue;
				}
				rendererInfo.Renderer.GetSharedMaterials(value);
				if (value.Count == 0)
				{
					continue;
				}
				context.cmd.SetGlobalColor(HighlightConstantBuffer._Color, rendererInfo.Highlighter.Color * data.Intensity);
				int num = ((rendererInfo.ExpectedMaterialsCount > 0) ? Mathf.Min(value.Count, rendererInfo.ExpectedMaterialsCount) : value.Count);
				for (int j = 0; j < num; j++)
				{
					Material material = value[j];
					if (!(material == null))
					{
						bool state = false;
						Texture texture = null;
						float value2 = 0f;
						CullMode cullMode = CullMode.Back;
						if (material.HasProperty(HighlightConstantBuffer._Alphatest))
						{
							state = material.GetFloat(HighlightConstantBuffer._Alphatest) > 0f;
						}
						if (material.shader.name == "Owlcat/Particles")
						{
							state = true;
						}
						if (material.HasProperty(HighlightConstantBuffer._CullMode))
						{
							cullMode = (CullMode)material.GetFloat(HighlightConstantBuffer._CullMode);
						}
						if (material.HasProperty(HighlightConstantBuffer._BaseMap))
						{
							texture = material.GetTexture(HighlightConstantBuffer._BaseMap);
						}
						if (material.HasProperty(HighlightConstantBuffer._Cutoff))
						{
							value2 = material.GetFloat(HighlightConstantBuffer._Cutoff);
						}
						context.cmd.SetGlobalFloat(HighlightConstantBuffer._CullMode, (float)cullMode);
						if (texture != null)
						{
							context.cmd.SetGlobalTexture(HighlightConstantBuffer._BaseMap, texture);
						}
						else
						{
							context.cmd.SetGlobalTexture(HighlightConstantBuffer._BaseMap, context.defaultResources.whiteTexture);
						}
						context.cmd.SetGlobalFloat(HighlightConstantBuffer._Cutoff, value2);
						CoreUtils.SetKeyword(context.cmd, HighlightKeywords._ALPHATEST_ON, state);
						if (string.IsNullOrEmpty(material.GetTag("ShaderGraphGUID", searchFallbacks: false)))
						{
							bool state2 = material.IsKeywordEnabled(HighlightKeywords.XPBD_MESH);
							CoreUtils.SetKeyword(context.cmd, HighlightKeywords.XPBD_MESH, state2);
							bool state3 = material.IsKeywordEnabled(HighlightKeywords.XPBD_SKELETON);
							CoreUtils.SetKeyword(context.cmd, HighlightKeywords.XPBD_SKELETON, state3);
							bool state4 = material.IsKeywordEnabled(HighlightKeywords.XPBD_DEFORM);
							CoreUtils.SetKeyword(context.cmd, HighlightKeywords.XPBD_DEFORM, state4);
						}
						else
						{
							string tag = material.GetTag("XPBD_MODE", searchFallbacks: false);
							CoreUtils.SetKeyword(state: tag == "XPBD_MESH", cmd: context.cmd, keyword: HighlightKeywords.XPBD_MESH);
							CoreUtils.SetKeyword(state: tag == "XPBD_SKELETON", cmd: context.cmd, keyword: HighlightKeywords.XPBD_SKELETON);
							bool state7 = tag == "XPBD_DEFORM";
							CoreUtils.SetKeyword(context.cmd, HighlightKeywords.XPBD_DEFORM, state7);
						}
						RendererFeatureUtils.DrawRendererWithGPUDrivenInstanceData(nativeCommandBuffer, rendererInfo.Renderer, data.Materials.Highlighter, j, 0);
					}
				}
			}
		}
		nativeCommandBuffer.Blit(data.HighlightRT, data.Blur1RT);
		CoreUtils.SetKeyword(data.Materials.Blur, HighlightKeywords.STRAIGHT_DIRECTIONS, data.Settings.BlurDirections == BlurDirections.Straight);
		CoreUtils.SetKeyword(data.Materials.Blur, HighlightKeywords.ALL_DIRECTIONS, data.Settings.BlurDirections == BlurDirections.All);
		bool flag = true;
		for (int k = 0; k < data.Settings.BlurIterations; k++)
		{
			float value3 = data.Settings.BlurMinSpread + data.Settings.BlurSpread * (float)k;
			context.cmd.SetGlobalFloat(HighlightConstantBuffer._HighlightingBlurOffset, value3);
			if (flag)
			{
				nativeCommandBuffer.Blit(data.Blur1RT, data.Blur2RT, data.Materials.Blur);
			}
			else
			{
				nativeCommandBuffer.Blit(data.Blur2RT, data.Blur1RT, data.Materials.Blur);
			}
			flag = !flag;
		}
		TextureHandle textureHandle = (flag ? data.Blur1RT : data.Blur2RT);
		context.cmd.SetRenderTarget(data.CameraColorBuffer);
		context.cmd.SetGlobalVector(ShaderPropertyId._CompositeParameters, data.CompositeParams);
		nativeCommandBuffer.Blit(textureHandle, data.CameraColorBuffer, data.Materials.Composite, 0);
	}
}
