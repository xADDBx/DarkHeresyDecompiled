using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ScreenSpaceCloudShadows;

[CreateAssetMenu(menuName = "Renderer Features/Screen Space Cloud Shadows", fileName = "ScreenSpaceCloudShadows")]
public class ScreenSpaceCloudShadowsRendererFeatureAsset : RendererFeatureAsset
{
	public override IRendererFeature CreateRendererFeature()
	{
		return new ScreenSpaceCloudShadowsRendererFeature(this);
	}
}
