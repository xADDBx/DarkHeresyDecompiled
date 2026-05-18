using Owlcat.Runtime.Visual.FogOfWar;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar;

[CreateAssetMenu(menuName = "Renderer Features/Fow Of War Renderer Feature")]
public sealed class FogOfWarRendererFeatureAsset : RendererFeatureAsset
{
	[SerializeField]
	private FogOfWarSettings m_Settings;

	public FogOfWarSettings Settings => m_Settings;

	public override IRendererFeature CreateRendererFeature()
	{
		if (m_Settings != null)
		{
			m_Settings.InitSingleton(m_Settings);
		}
		return new FogOfWarRendererFeature(this);
	}
}
