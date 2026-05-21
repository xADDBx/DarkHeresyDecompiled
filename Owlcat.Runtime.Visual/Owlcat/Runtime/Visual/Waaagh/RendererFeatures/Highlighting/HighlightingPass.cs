using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting;

internal static class HighlightingPass
{
	private sealed class PassData
	{
		public TextureHandle CameraColorRT;

		public TextureHandle DepthBuffer;

		public TextureHandle HighlightRT;

		public TextureHandle Blur1RT;

		public TextureHandle Blur2RT;

		public Materials Materials;

		public readonly List<RendererInfo> RendererInfos = new List<RendererInfo>();

		public TestPlanesResults[] RendererVisibility = Array.Empty<TestPlanesResults>();

		public Settings Settings;
	}

	public static void Record(in RecordContext context, in Settings settings, in Materials materials, List<RendererInfo> rendererInfos, ReadOnlySpan<TestPlanesResults> rendererVisibility)
	{
		PassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("Draw Highlight", out passData2, WaaaghProfileId.DrawHighlight.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\Highlighting\\HighlightingPass.cs", 38);
		int downsampleFactor = Mathf.Max(1, (int)settings.DownsampleFactor);
		passData2.Materials = materials;
		passData2.CameraColorRT = context.FrameResources.CameraStackTargets.Color;
		PassData passData3 = passData2;
		TextureDesc desc = GetHighlightTextureDesc(in context.CameraData);
		passData3.HighlightRT = unsafeRenderGraphBuilder.CreateTransientTexture(in desc);
		PassData passData4 = passData2;
		desc = GetDownsampleTextureDesc(in context.CameraData, "Blur1RT", downsampleFactor);
		passData4.Blur1RT = unsafeRenderGraphBuilder.CreateTransientTexture(in desc);
		PassData passData5 = passData2;
		desc = GetDownsampleTextureDesc(in context.CameraData, "Blur2RT", downsampleFactor);
		passData5.Blur2RT = unsafeRenderGraphBuilder.CreateTransientTexture(in desc);
		passData2.Settings = settings;
		passData2.RendererInfos.Clear();
		passData2.RendererInfos.AddRange(rendererInfos);
		if (passData2.RendererVisibility.Length < rendererVisibility.Length)
		{
			Array.Resize(ref passData2.RendererVisibility, math.ceilpow2(rendererVisibility.Length));
		}
		rendererVisibility.CopyTo(passData2.RendererVisibility);
		switch (settings.ZTestMode)
		{
		case ZTestMode.SceneBuffer:
			passData2.DepthBuffer = context.FrameResources.CameraStackTargets.Depth;
			unsafeRenderGraphBuilder.UseTexture(in passData2.DepthBuffer, AccessFlags.ReadWrite);
			break;
		case ZTestMode.EmptyBuffer:
		{
			PassData passData6 = passData2;
			desc = GetDepthTextureDesc(in context.CameraData);
			passData6.DepthBuffer = unsafeRenderGraphBuilder.CreateTransientTexture(in desc);
			break;
		}
		}
		unsafeRenderGraphBuilder.UseTexture(in passData2.CameraColorRT, AccessFlags.Write);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData passData, UnsafeGraphContext context)
		{
			Render(passData, context);
		});
	}

	private static void Render(PassData passData, UnsafeGraphContext context)
	{
		CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
		if (passData.Settings.ZTestMode == ZTestMode.None)
		{
			context.cmd.SetRenderTarget(passData.HighlightRT);
		}
		else
		{
			context.cmd.SetRenderTarget(passData.HighlightRT, passData.DepthBuffer);
		}
		context.cmd.ClearRenderTarget(passData.Settings.ZTestMode != ZTestMode.SceneBuffer, clearColor: true, Color.clear);
		context.cmd.SetGlobalFloat(HighlightConstantBuffer._ZTest, (passData.Settings.ZTestMode == ZTestMode.SceneBuffer) ? 3 : 4);
		context.cmd.SetGlobalFloat(HighlightConstantBuffer._ZWrite, (passData.Settings.ZTestMode == ZTestMode.EmptyBuffer) ? 1 : 0);
		List<Material> value;
		using (CollectionPool<List<Material>, Material>.Get(out value))
		{
			List<RendererInfo> rendererInfos = passData.RendererInfos;
			int i = 0;
			for (int count = rendererInfos.Count; i < count; i++)
			{
				if (passData.RendererVisibility[i] == TestPlanesResults.Outside)
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
				context.cmd.SetGlobalColor(HighlightConstantBuffer._Color, rendererInfo.Highlighter.CurrentColor);
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
						RendererFeatureUtils.DrawRendererWithGPUDrivenInstanceData(nativeCommandBuffer, rendererInfo.Renderer, passData.Materials.Highlighter, j, 0);
					}
				}
			}
		}
		nativeCommandBuffer.Blit(passData.HighlightRT, passData.Blur1RT);
		CoreUtils.SetKeyword(passData.Materials.Blur, HighlightKeywords.STRAIGHT_DIRECTIONS, passData.Settings.BlurDirections == BlurDirections.Straight);
		CoreUtils.SetKeyword(passData.Materials.Blur, HighlightKeywords.ALL_DIRECTIONS, passData.Settings.BlurDirections == BlurDirections.All);
		bool flag = true;
		for (int k = 0; k < passData.Settings.BlurIterations; k++)
		{
			float value3 = passData.Settings.BlurMinSpread + passData.Settings.BlurSpread * (float)k;
			context.cmd.SetGlobalFloat(HighlightConstantBuffer._HighlightingBlurOffset, value3);
			if (flag)
			{
				nativeCommandBuffer.Blit(passData.Blur1RT, passData.Blur2RT, passData.Materials.Blur);
			}
			else
			{
				nativeCommandBuffer.Blit(passData.Blur2RT, passData.Blur1RT, passData.Materials.Blur);
			}
			flag = !flag;
		}
		nativeCommandBuffer.Blit(flag ? passData.Blur1RT : passData.Blur2RT, passData.HighlightRT, passData.Materials.Cut);
		nativeCommandBuffer.Blit(passData.HighlightRT, passData.CameraColorRT, passData.Materials.Composite);
	}

	private static TextureDesc GetHighlightTextureDesc(in WaaaghCameraData cameraData)
	{
		TextureDesc result = RenderingUtils.CreateTextureDesc("HighlightRT", cameraData.cameraTargetDescriptor);
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

	private static TextureDesc GetDepthTextureDesc(in WaaaghCameraData cameraData)
	{
		TextureDesc result = RenderingUtils.CreateTextureDesc("HighlightDepthRT", cameraData.cameraTargetDescriptor);
		result.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
		result.filterMode = FilterMode.Bilinear;
		result.wrapMode = TextureWrapMode.Clamp;
		result.depthBufferBits = DepthBits.Depth32;
		return result;
	}
}
