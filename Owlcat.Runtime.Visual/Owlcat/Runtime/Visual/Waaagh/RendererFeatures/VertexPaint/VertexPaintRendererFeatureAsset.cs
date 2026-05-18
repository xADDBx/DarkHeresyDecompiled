using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VertexPaint;

[CreateAssetMenu(menuName = "Renderer Features/Vertex Paint Renderer Feature")]
internal sealed class VertexPaintRendererFeatureAsset : RendererFeatureAsset
{
	public VertexPaintManagerParameters ManagerParameters;

	public override IRendererFeature CreateRendererFeature()
	{
		return new VertexPaintRendererFeature(this);
	}
}
