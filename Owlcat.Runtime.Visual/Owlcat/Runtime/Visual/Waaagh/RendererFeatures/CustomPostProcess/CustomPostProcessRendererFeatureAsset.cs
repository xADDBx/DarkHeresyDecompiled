using System.Collections.Generic;
using Owlcat.Runtime.Visual.CustomPostProcess;
using Owlcat.Runtime.Visual.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CustomPostProcess;

[CreateAssetMenu(menuName = "Renderer Features/Custom Post Process/Feature", fileName = "CustomPostProcessFeature")]
public class CustomPostProcessRendererFeatureAsset : RendererFeatureAsset
{
	public List<CustomPostProcessEffect> Effects;

	public StencilMaskTextureSettings StencilMaskSettings = new StencilMaskTextureSettings
	{
		Ref = StencilRef.ShaderGraphOverride,
		ReadMask = StencilRef.ShaderGraphOverride,
		CompareFunction = CompareFunction.Equal,
		FilterMode = FilterMode.Bilinear
	};

	public override IRendererFeature CreateRendererFeature()
	{
		return new CustomPostProcessRendererFeature(this);
	}
}
