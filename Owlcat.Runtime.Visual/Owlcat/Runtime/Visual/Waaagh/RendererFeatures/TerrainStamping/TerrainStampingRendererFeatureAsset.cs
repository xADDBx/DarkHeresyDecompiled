using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping;

[CreateAssetMenu(menuName = "Renderer Features/Terrain Stamping Renderer Feature")]
internal sealed class TerrainStampingRendererFeatureAsset : RendererFeatureAsset
{
	public TerrainStampingManagerParameters ManagerParameters;

	public override IRendererFeature CreateRendererFeature()
	{
		return new TerrainStampingRendererFeature(this);
	}
}
