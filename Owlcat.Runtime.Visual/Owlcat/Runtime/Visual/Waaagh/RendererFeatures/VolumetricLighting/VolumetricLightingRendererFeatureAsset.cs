using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting;

[CreateAssetMenu(menuName = "Renderer Features/Volumetric Lighting Renderer Feature")]
public sealed class VolumetricLightingRendererFeatureAsset : RendererFeatureAsset
{
	public VolumetricLightingSettings Settings;

	public override IRendererFeature CreateRendererFeature()
	{
		return new VolumetricLightingRendererFeature(this);
	}
}
