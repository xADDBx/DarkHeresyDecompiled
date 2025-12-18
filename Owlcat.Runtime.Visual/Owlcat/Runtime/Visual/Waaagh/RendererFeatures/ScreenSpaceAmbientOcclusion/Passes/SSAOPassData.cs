using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ScreenSpaceAmbientOcclusion.Passes;

public class SSAOPassData
{
	internal ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions BlurQuality;

	internal Material material;

	internal float directLightingStrength;

	internal TextureHandle cameraColor;

	internal TextureHandle AOTexture;

	internal TextureHandle finalTexture;

	internal TextureHandle blurTexture;

	internal TextureHandle cameraDepthTexture;

	internal TextureHandle cameraNormalsTexture;

	internal Vector4 ScaleBiasRt;
}
