using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures;

public abstract class RendererFeatureAsset : ScriptableObject
{
	public abstract IRendererFeature CreateRendererFeature();
}
