using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ColoredShadows;

internal sealed class ColoredShadowsRendererFeature : IRendererFeature, IDisposable
{
	private static class ShaderPropertyId
	{
		public static readonly int _ColoredShadows_Color = Shader.PropertyToID("_ColoredShadows_Color");

		public static readonly int _ColoredShadows_Ramps = Shader.PropertyToID("_ColoredShadows_Ramps");

		public static readonly int _ColoredShadows_Ramps2 = Shader.PropertyToID("_ColoredShadows_Ramps2");
	}

	private sealed class SetupPassData
	{
		public Vector4 Color;

		public Vector4 Ramps;

		public Vector4 Ramps2;
	}

	private readonly ColoredShadowsRendererFeatureAsset m_Asset;

	private ColoredShadowsSettings m_ResolvedSettings;

	public ColoredShadowsRendererFeature(ColoredShadowsRendererFeatureAsset asset)
	{
		m_Asset = asset;
	}

	public void Dispose()
	{
	}

	public void RegisterExtensions(RendererFeatureExtensionRegistry registry)
	{
		registry.AddRecordDelegate(RecordExtensionPoint.BeforeRendering, OnBeforeRendering);
		registry.AddRecordDelegate(RecordExtensionPoint.AfterRendering, OnAfterRendering);
	}

	private void OnBeforeRendering(in RecordContext context)
	{
		ColoredShadowsSettings coloredShadowsSettings = ColoredShadowsSettingsOverride.Resolve(m_Asset.Settings);
		if (coloredShadowsSettings != null && coloredShadowsSettings.Enable)
		{
			m_ResolvedSettings = coloredShadowsSettings;
			RecordSetup(in context, m_ResolvedSettings);
		}
	}

	private void OnAfterRendering(in RecordContext context)
	{
		ColoredShadowsSettings resolvedSettings = m_ResolvedSettings;
		if (resolvedSettings != null && resolvedSettings.Enable)
		{
			RecordCleanup(in context);
			m_ResolvedSettings = null;
		}
	}

	private static void RecordSetup(in RecordContext context, ColoredShadowsSettings settings)
	{
		SetupPassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<SetupPassData>("Colored Shadows Setup", out passData2, WaaaghProfileId.ColoredShadowsSetup.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\ColoredShadows\\ColoredShadowsRendererFeature.cs", 64);
		passData2.Color = settings.Color;
		passData2.Ramps.x = settings.ShadowThreshold;
		passData2.Ramps.y = settings.ShadowSmoothness;
		passData2.Ramps.z = settings.DistanceThreshold;
		passData2.Ramps.w = settings.DistanceSmoothness;
		passData2.Ramps2.x = settings.DiffuseThreshold + settings.DiffuseSmoothness * 0.5f;
		passData2.Ramps2.y = settings.DiffuseThreshold - settings.DiffuseSmoothness * 0.5f;
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(SetupPassData passData, UnsafeGraphContext context)
		{
			CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.COLORED_SHADOWS, state: true);
			context.cmd.SetGlobalVector(ShaderPropertyId._ColoredShadows_Color, passData.Color);
			context.cmd.SetGlobalVector(ShaderPropertyId._ColoredShadows_Ramps, passData.Ramps);
			context.cmd.SetGlobalVector(ShaderPropertyId._ColoredShadows_Ramps2, passData.Ramps2);
		});
	}

	private static void RecordCleanup(in RecordContext context)
	{
		object passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<object>("Colored Shadows Cleanup", out passData2, WaaaghProfileId.ColoredShadowsCleanup.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\ColoredShadows\\ColoredShadowsRendererFeature.cs", 89);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(object passData, UnsafeGraphContext context)
		{
			CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.COLORED_SHADOWS, state: false);
		});
	}
}
