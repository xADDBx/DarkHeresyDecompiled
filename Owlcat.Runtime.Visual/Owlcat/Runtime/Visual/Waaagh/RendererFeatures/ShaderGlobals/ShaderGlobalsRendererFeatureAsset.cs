using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ShaderGlobals;

[CreateAssetMenu(menuName = "Renderer Features/Shader Globals Renderer Feature")]
public sealed class ShaderGlobalsRendererFeatureAsset : RendererFeatureAsset
{
	public override IRendererFeature CreateRendererFeature()
	{
		return new ShaderGlobalsRendererFeature(this);
	}
}
