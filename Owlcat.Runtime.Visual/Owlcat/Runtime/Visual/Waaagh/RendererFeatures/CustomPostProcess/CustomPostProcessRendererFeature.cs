using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.CustomPostProcess;
using Owlcat.Runtime.Visual.Overrides.CustomPostProcess;
using Owlcat.Runtime.Visual.Utilities;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CustomPostProcess;

public class CustomPostProcessRendererFeature : IRendererFeature, IDisposable
{
	private class StencilMaskPassData
	{
		public Material Material;

		internal StencilRef Ref;

		internal StencilRef ReadMask;

		internal CompareFunction CompareFunction;
	}

	private static class ShaderIDs
	{
		public static readonly int _StencilMaskRef = Shader.PropertyToID("_StencilMaskRef");

		public static readonly int _StencilMaskReadMask = Shader.PropertyToID("_StencilMaskReadMask");

		public static readonly int _StencilMaskComp = Shader.PropertyToID("_StencilMaskComp");

		public static readonly int _CameraStencilMask = Shader.PropertyToID("_CameraStencilMask");
	}

	private class CustomPostProcessPassData
	{
		public Material Material;

		public TextureHandle Source;
	}

	private CustomPostProcessRendererFeatureAsset m_Asset;

	private Dictionary<CustomPostProcessRenderEvent, List<EffectDescriptor>> m_Effects = new Dictionary<CustomPostProcessRenderEvent, List<EffectDescriptor>>();

	private Material m_StencilMaskMaterial;

	private Owlcat.Runtime.Visual.Overrides.CustomPostProcess.CustomPostProcess m_Override;

	private bool m_IsActive;

	private bool m_StencilRequired;

	private Dictionary<CustomPostProcessRenderEvent, ProfilingSampler> m_ProfilingSamplers = new Dictionary<CustomPostProcessRenderEvent, ProfilingSampler>
	{
		{
			CustomPostProcessRenderEvent.BeforeMainPostProcess,
			new ProfilingSampler("Custom Post Process - Before Main Post Process")
		},
		{
			CustomPostProcessRenderEvent.AfterMainPostProcess,
			new ProfilingSampler("Custom Post Process - After Main Post Process")
		}
	};

	public CustomPostProcessRendererFeature(CustomPostProcessRendererFeatureAsset asset)
	{
		m_Asset = asset;
		RenderRuntimeShaders renderPipelineSettings = GraphicsSettings.GetRenderPipelineSettings<RenderRuntimeShaders>();
		m_StencilMaskMaterial = CoreUtils.CreateEngineMaterial(renderPipelineSettings.StencilMaskPS);
		InitEffects();
	}

	public void Dispose()
	{
		DestroyEffects();
		CoreUtils.Destroy(m_StencilMaskMaterial);
	}

	private void InitEffects()
	{
		foreach (CustomPostProcessEffect effect in m_Asset.Effects)
		{
			if (!m_Effects.TryGetValue(effect.Event, out var value))
			{
				value = new List<EffectDescriptor>();
				m_Effects.Add(effect.Event, value);
			}
			if (effect.Validate())
			{
				EffectDescriptor item = new EffectDescriptor(effect);
				value.Add(item);
			}
		}
	}

	private void DestroyEffects()
	{
		foreach (KeyValuePair<CustomPostProcessRenderEvent, List<EffectDescriptor>> effect in m_Effects)
		{
			foreach (EffectDescriptor item in effect.Value)
			{
				item.Dispose();
			}
			effect.Value.Clear();
		}
	}

	public void RegisterExtensions(RendererFeatureExtensionRegistry registry)
	{
		registry.AddSetupDelegate(Setup);
		registry.AddRecordDelegate(RecordExtensionPoint.BeforeDrawPostProcess, RecordBeforePostProcess);
		registry.AddRecordDelegate(RecordExtensionPoint.AfterDrawPostProcess, RecordAfterPostProcess);
	}

	private void Setup(in SetupContext context)
	{
		m_Override = VolumeManager.instance.stack.GetComponent<Owlcat.Runtime.Visual.Overrides.CustomPostProcess.CustomPostProcess>();
		m_IsActive = context.CameraData.postProcessEnabled && m_Override != null && m_Override.IsActive();
		m_StencilRequired = IsStencilRequired(m_Override);
	}

	private bool IsStencilRequired(Owlcat.Runtime.Visual.Overrides.CustomPostProcess.CustomPostProcess customPostProcessSettings)
	{
		foreach (KeyValuePair<CustomPostProcessRenderEvent, List<EffectDescriptor>> effect in m_Effects)
		{
			foreach (EffectDescriptor item in effect.Value)
			{
				if (item.UseStencilMask && customPostProcessSettings.IsEffectActive(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void RecordBeforePostProcess(in RecordContext context)
	{
		if (m_IsActive)
		{
			CustomPostProcessRenderEvent key = CustomPostProcessRenderEvent.BeforeMainPostProcess;
			List<EffectDescriptor> effects = m_Effects[key];
			Invalidate(effects);
			if (m_StencilRequired)
			{
				RecordStencilMaskPass(in context, in m_Asset.StencilMaskSettings, m_StencilMaskMaterial);
			}
			RecordEffects(in context, effects, m_Override, m_ProfilingSamplers[key]);
		}
	}

	private void RecordAfterPostProcess(in RecordContext context)
	{
		if (m_IsActive)
		{
			CustomPostProcessRenderEvent key = CustomPostProcessRenderEvent.AfterMainPostProcess;
			List<EffectDescriptor> effects = m_Effects[key];
			Invalidate(effects);
			RecordEffects(in context, effects, m_Override, m_ProfilingSamplers[key]);
		}
	}

	private void Invalidate(List<EffectDescriptor> effects)
	{
		bool flag = false;
		foreach (EffectDescriptor effect in effects)
		{
			if (!effect.Validate())
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			DestroyEffects();
			InitEffects();
		}
	}

	private static void RecordStencilMaskPass(in RecordContext context, in StencilMaskTextureSettings settings, Material stencilMaskMaterial)
	{
		TextureDesc desc = RenderingUtils.CreateTextureDesc("StencilMask", context.CameraData.cameraTargetDescriptor);
		desc.clearBuffer = true;
		desc.colorFormat = GraphicsFormat.R8_UNorm;
		desc.clearColor = Color.clear;
		desc.filterMode = settings.FilterMode;
		TextureHandle input = context.RenderGraph.CreateTexture(in desc);
		StencilMaskPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<StencilMaskPassData>("CustomPP - Stencil Mask", out passData, WaaaghProfileId.CustomPPStencilMask.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\CustomPostProcess\\CustomPostProcessRendererFeature.cs", 287);
		rasterRenderGraphBuilder.SetRenderAttachment(input, 0);
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(context.FrameResources.CameraStackTargets.Depth, AccessFlags.Read);
		rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in input, ShaderIDs._CameraStencilMask);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		passData.Ref = settings.Ref;
		passData.ReadMask = settings.ReadMask;
		passData.CompareFunction = settings.CompareFunction;
		passData.Material = stencilMaskMaterial;
		rasterRenderGraphBuilder.SetRenderFunc(delegate(StencilMaskPassData data, RasterGraphContext context)
		{
			context.cmd.SetGlobalInteger(ShaderIDs._StencilMaskRef, (int)data.Ref);
			context.cmd.SetGlobalInteger(ShaderIDs._StencilMaskReadMask, (int)data.ReadMask);
			context.cmd.SetGlobalInteger(ShaderIDs._StencilMaskComp, (int)data.CompareFunction);
			context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3);
		});
	}

	private static void RecordEffects(in RecordContext context, List<EffectDescriptor> effects, Owlcat.Runtime.Visual.Overrides.CustomPostProcess.CustomPostProcess settings, ProfilingSampler profilingSampler)
	{
		context.RenderGraph.BeginProfilingSampler(profilingSampler, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\CustomPostProcess\\CustomPostProcessRendererFeature.cs", 319);
		TextureHandle source = context.FrameResources.CameraStackTargets.CurrentPostProcessSource;
		TextureDesc desc = RenderingUtils.CreateTextureDesc("CameraAfterPostProcessRT", context.CameraData.cameraTargetDescriptor);
		desc.filterMode = FilterMode.Bilinear;
		desc.wrapMode = TextureWrapMode.Clamp;
		desc.colorFormat = context.CameraData.cameraTargetDescriptor.graphicsFormat;
		desc.depthBufferBits = DepthBits.None;
		TextureHandle destination = context.RenderGraph.CreateTexture(in desc);
		for (int i = 0; i < effects.Count; i++)
		{
			EffectDescriptor effectDescriptor = effects[i];
			if (!settings.IsEffectActive(effectDescriptor))
			{
				continue;
			}
			context.RenderGraph.BeginProfilingSampler(effectDescriptor.ProfilingSampler, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\CustomPostProcess\\CustomPostProcessRendererFeature.cs", 342);
			for (int j = 0; j < effectDescriptor.Passes.Count; j++)
			{
				PassDescriptor passDescriptor = effectDescriptor.Passes[j];
				if (passDescriptor.Shader == null)
				{
					continue;
				}
				CustomPostProcessPassData passData;
				using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<CustomPostProcessPassData>(passDescriptor.Name, out passData, passDescriptor.ProfilingSampler, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\CustomPostProcess\\CustomPostProcessRendererFeature.cs", 352))
				{
					passData.Source = GetSource();
					rasterRenderGraphBuilder.UseTexture(in passData.Source);
					rasterRenderGraphBuilder.SetRenderAttachment(GetDestination(), 0, AccessFlags.WriteAll);
					passData.Material = passDescriptor.Material;
					settings.ApplyPropertiesOverride(effectDescriptor, passDescriptor, passData.Material);
					rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
					rasterRenderGraphBuilder.AllowPassCulling(value: false);
					if (effectDescriptor.UseStencilMask)
					{
						rasterRenderGraphBuilder.UseGlobalTexture(ShaderIDs._CameraStencilMask);
					}
					rasterRenderGraphBuilder.SetRenderFunc(delegate(CustomPostProcessPassData data, RasterGraphContext context)
					{
						context.cmd.SetGlobalTexture(ShaderPropertyId._CustomPostProcessInput, data.Source);
						context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3);
					});
				}
				Swap();
			}
			context.RenderGraph.EndProfilingSampler(effectDescriptor.ProfilingSampler, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\CustomPostProcess\\CustomPostProcessRendererFeature.cs", 379);
		}
		context.FrameResources.CameraStackTargets.SetCurrentPostProcessSource(GetSource());
		context.RenderGraph.EndProfilingSampler(profilingSampler, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\CustomPostProcess\\CustomPostProcessRendererFeature.cs", 386);
		TextureHandle GetDestination()
		{
			return destination;
		}
		TextureHandle GetSource()
		{
			return source;
		}
		void Swap()
		{
			CoreUtils.Swap(ref source, ref destination);
		}
	}
}
