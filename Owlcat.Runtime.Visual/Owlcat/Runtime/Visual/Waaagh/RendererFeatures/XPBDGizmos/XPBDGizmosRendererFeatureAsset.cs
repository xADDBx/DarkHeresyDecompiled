using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.XPBDGizmos;

[CreateAssetMenu(menuName = "Renderer Features/XPBD Gizmos Renderer Feature")]
public sealed class XPBDGizmosRendererFeatureAsset : RendererFeatureAsset
{
	public override IRendererFeature CreateRendererFeature()
	{
		return new XPBDGizmosRendererFeature();
	}
}
