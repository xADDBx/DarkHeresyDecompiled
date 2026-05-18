using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.HBAO;

[CreateAssetMenu(menuName = "Renderer Features/HBAO Renderer Feature")]
public sealed class HbaoRendererFeatureAsset : RendererFeatureAsset
{
	public override IRendererFeature CreateRendererFeature()
	{
		return new HbaoRendererFeature(this);
	}
}
