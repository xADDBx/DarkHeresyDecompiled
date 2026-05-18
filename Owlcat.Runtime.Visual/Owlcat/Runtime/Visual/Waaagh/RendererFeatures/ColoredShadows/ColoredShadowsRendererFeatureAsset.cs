using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ColoredShadows;

[CreateAssetMenu(menuName = "Renderer Features/Colored Shadows Feature")]
public sealed class ColoredShadowsRendererFeatureAsset : RendererFeatureAsset
{
	[SerializeField]
	private ColoredShadowsSettings m_Settings = new ColoredShadowsSettings();

	public ColoredShadowsSettings Settings => m_Settings;

	public override IRendererFeature CreateRendererFeature()
	{
		return new ColoredShadowsRendererFeature(this);
	}
}
