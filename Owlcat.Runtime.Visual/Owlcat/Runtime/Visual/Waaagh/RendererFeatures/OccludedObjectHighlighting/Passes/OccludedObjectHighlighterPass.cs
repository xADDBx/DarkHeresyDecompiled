using System.Collections.Generic;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.OccludedObjectHighlighting.Passes;

internal class OccludedObjectHighlighterPass : ScriptableRenderPass<OccludedObjectHighlighterPassData>
{
	private static class ShaderPropertyId
	{
		public static readonly int _CompositeParameters = Shader.PropertyToID("_CompositeParameters");
	}

	private OccludedObjectHighlightingFeature m_Feature;

	private Material m_HighlighterMaterial;

	private Material m_BlurMaterial;

	private Material m_CompositeMaterial;

	public override string Name => "OccludedObjectHighlighterPass";

	public OccludedObjectHighlighterPass(RenderPassEvent evt, OccludedObjectHighlightingFeature feature, Material highlighterMaterial, Material blurMaterial, Material compositeMaterial)
		: base(evt)
	{
		m_Feature = feature;
		m_HighlighterMaterial = highlighterMaterial;
		m_BlurMaterial = blurMaterial;
		m_CompositeMaterial = compositeMaterial;
	}

	protected override void Setup(RenderGraphBuilder builder, OccludedObjectHighlighterPassData data, ContextContainer frameData)
	{
		Owlcat.Runtime.Visual.Overrides.OccludedObjectHighlighting component = VolumeManager.instance.stack.GetComponent<Owlcat.Runtime.Visual.Overrides.OccludedObjectHighlighting>();
		if (component.IsActive())
		{
			WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
			WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
			data.Intensity = component.Intensity.value;
			data.HighlighterMaterial = m_HighlighterMaterial;
			data.BlurMaterial = m_BlurMaterial;
			data.CompositeMaterial = m_CompositeMaterial;
			data.Feature = m_Feature;
			TextureHandle input = waaaghResourceData.CameraColorBuffer;
			data.CameraColorBuffer = builder.WriteTexture(in input);
			input = waaaghResourceData.CameraDepthBuffer;
			data.CameraDepthBuffer = builder.WriteTexture(in input);
			TextureDesc desc = RenderingUtils.CreateTextureDesc("OccludedObjectHighlightRT", waaaghCameraData.cameraTargetDescriptor);
			desc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
			desc.filterMode = FilterMode.Bilinear;
			desc.wrapMode = TextureWrapMode.Clamp;
			data.HighlightRT = builder.CreateTransientTexture(in desc);
			int num = Mathf.Max(1, (int)m_Feature.DownsampleFactor);
			TextureDesc desc2 = desc;
			desc2.width = desc.width / num;
			desc2.height = desc.height / num;
			desc2.name = "Blur1RT";
			data.Blur1RT = builder.CreateTransientTexture(in desc2);
			desc2.name = "Blur2RT";
			data.Blur2RT = builder.CreateTransientTexture(in desc2);
			data.BlurDirections = m_Feature.BlurDirectons;
			data.BlurIterations = m_Feature.BlurIterations;
			data.BlurMinSpread = m_Feature.BlurMinSpread;
			data.BlurSpread = m_Feature.BlurSpread;
			data.CompositeParams = new Vector4(m_Feature.ScanLineFreq0, m_Feature.ScanLineFreq1, m_Feature.ScanLineSpeed * Time.time, m_Feature.ScanLineOpacity);
		}
		else
		{
			data.Intensity = 0f;
		}
	}

	protected override void Render(OccludedObjectHighlighterPassData data, RenderGraphContext context)
	{
		if (data.Intensity <= 0f)
		{
			return;
		}
		context.cmd.SetRenderTarget(data.HighlightRT, data.CameraDepthBuffer);
		context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.clear);
		context.cmd.SetGlobalFloat(HighlightConstantBuffer._ZTest, 7f);
		context.cmd.SetGlobalFloat(HighlightConstantBuffer._ZWrite, 0f);
		List<Material> value;
		using (CollectionPool<List<Material>, Material>.Get(out value))
		{
			List<OccludedObjectHighlightingFeature.RendererInfo> rendererInfos = data.Feature.RendererInfos;
			int i = 0;
			for (int count = rendererInfos.Count; i < count; i++)
			{
				if (!data.Feature.IsRendererVisible(i))
				{
					continue;
				}
				OccludedObjectHighlightingFeature.RendererInfo rendererInfo = rendererInfos[i];
				if (rendererInfo.renderer == null || !rendererInfo.renderer.enabled || !rendererInfo.renderer.gameObject.activeInHierarchy)
				{
					continue;
				}
				rendererInfo.renderer.GetSharedMaterials(value);
				if (value.Count == 0)
				{
					continue;
				}
				context.cmd.SetGlobalColor(HighlightConstantBuffer._Color, rendererInfo.highlighter.Color * data.Intensity);
				int num = ((rendererInfo.expectedMaterialsCount > 0) ? Mathf.Min(value.Count, rendererInfo.expectedMaterialsCount) : value.Count);
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
						RendererFeatureUtils.DrawRendererWithGPUDrivenInstanceData(context.cmd, rendererInfo.renderer, data.HighlighterMaterial, j, 0);
					}
				}
			}
		}
		context.cmd.Blit(data.HighlightRT, data.Blur1RT);
		CoreUtils.SetKeyword(data.BlurMaterial, HighlightKeywords.STRAIGHT_DIRECTIONS, data.BlurDirections == OccludedObjectHighlightingFeature.BlurDirections.Straight);
		CoreUtils.SetKeyword(data.BlurMaterial, HighlightKeywords.ALL_DIRECTIONS, data.BlurDirections == OccludedObjectHighlightingFeature.BlurDirections.All);
		bool flag = true;
		for (int k = 0; k < data.BlurIterations; k++)
		{
			float value3 = data.BlurMinSpread + data.BlurSpread * (float)k;
			context.cmd.SetGlobalFloat(HighlightConstantBuffer._HighlightingBlurOffset, value3);
			if (flag)
			{
				context.cmd.Blit(data.Blur1RT, data.Blur2RT, data.BlurMaterial);
			}
			else
			{
				context.cmd.Blit(data.Blur2RT, data.Blur1RT, data.BlurMaterial);
			}
			flag = !flag;
		}
		TextureHandle textureHandle = (flag ? data.Blur1RT : data.Blur2RT);
		context.cmd.SetRenderTarget(data.CameraColorBuffer);
		context.cmd.SetGlobalVector(ShaderPropertyId._CompositeParameters, data.CompositeParams);
		context.cmd.Blit(textureHandle, data.CameraColorBuffer, data.CompositeMaterial, 0);
	}

	private void SetFloat(int property, CommandBuffer cmd, Material mat)
	{
		if (mat.HasProperty(property))
		{
			cmd.SetGlobalFloat(property, mat.GetFloat(property));
		}
	}

	private void SetTexture(int property, CommandBuffer cmd, Material mat)
	{
		if (mat.HasProperty(property))
		{
			cmd.SetGlobalTexture(property, mat.GetTexture(property));
		}
	}
}
