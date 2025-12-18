using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ColoredShadows.Passes;

public class ColoredShadowsSetupPass : ScriptableRenderPass<ColoredShadowsSetupPassData>
{
	private static class ShaderPropertyId
	{
		public static readonly int _ColoredShadows_Color = Shader.PropertyToID("_ColoredShadows_Color");

		public static readonly int _ColoredShadows_Ramps = Shader.PropertyToID("_ColoredShadows_Ramps");

		public static readonly int _ColoredShadows_Ramps2 = Shader.PropertyToID("_ColoredShadows_Ramps2");
	}

	private readonly ColoredShadowsSettings m_FallbackSettings;

	public override string Name => "ColoredShadowsSetupPass";

	public ColoredShadowsSetupPass(RenderPassEvent evt, ColoredShadowsSettings fallbackSettings)
		: base(evt)
	{
		m_FallbackSettings = fallbackSettings;
	}

	protected override void Setup(RenderGraphBuilder builder, ColoredShadowsSetupPassData data, ContextContainer frameData)
	{
		ColoredShadowsSettings coloredShadowsSettings = ColoredShadowsSettingsOverride.Resolve(m_FallbackSettings);
		data.Color = coloredShadowsSettings.Color;
		data.Ramps.x = coloredShadowsSettings.ShadowThreshold;
		data.Ramps.y = coloredShadowsSettings.ShadowSmoothness;
		data.Ramps.z = coloredShadowsSettings.DistanceThreshold;
		data.Ramps.w = coloredShadowsSettings.DistanceSmoothness;
		data.Ramps2.x = coloredShadowsSettings.DiffuseThreshold + coloredShadowsSettings.DiffuseSmoothness * 0.5f;
		data.Ramps2.y = coloredShadowsSettings.DiffuseThreshold - coloredShadowsSettings.DiffuseSmoothness * 0.5f;
	}

	protected override void Render(ColoredShadowsSetupPassData data, RenderGraphContext context)
	{
		CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.COLORED_SHADOWS, state: true);
		context.cmd.SetGlobalVector(ShaderPropertyId._ColoredShadows_Color, data.Color);
		context.cmd.SetGlobalVector(ShaderPropertyId._ColoredShadows_Ramps, data.Ramps);
		context.cmd.SetGlobalVector(ShaderPropertyId._ColoredShadows_Ramps2, data.Ramps2);
	}
}
