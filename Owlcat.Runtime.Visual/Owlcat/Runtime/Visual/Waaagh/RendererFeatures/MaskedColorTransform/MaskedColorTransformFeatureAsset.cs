using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.MaskedColorTransform;

[CreateAssetMenu(menuName = "Renderer Features/Masked Color Transform", fileName = "MaskedColorTransform")]
public class MaskedColorTransformFeatureAsset : RendererFeatureAsset
{
	public override IRendererFeature CreateRendererFeature()
	{
		return new MaskedColorTransformFeature(this);
	}
}
