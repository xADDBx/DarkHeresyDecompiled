using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FullscreenBlur;

[CreateAssetMenu(menuName = "Renderer Features/Fullscreen Blur Renderer Feature")]
public sealed class FullscreenBlurRendererFeatureAsset : RendererFeatureAsset
{
	public RecordExtensionPoint ExtensionPoint;

	public BlurType BlurType;

	public Downsample Downsample = Downsample.Downsample2x2;

	[Range(0f, 10f)]
	public float BlurSize;

	[Range(1f, 4f)]
	public int BlurIterations = 1;

	public override IRendererFeature CreateRendererFeature()
	{
		return new FullscreenBlurRendererFeature(this);
	}
}
