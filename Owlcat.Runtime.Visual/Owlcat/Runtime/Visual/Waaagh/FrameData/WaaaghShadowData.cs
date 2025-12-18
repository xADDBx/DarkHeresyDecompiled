using Owlcat.Runtime.Visual.Waaagh.Shadows;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public class WaaaghShadowData : ContextItem
{
	public ShadowUpdateDistances ShadowUpdateDistances;

	public bool StaticShadowsCacheEnabled;

	internal ShadowManager ShadowManager;

	public ShadowQuality ShadowQuality;

	public Owlcat.Runtime.Visual.Waaagh.Shadows.ShadowResolution AtlasSize;

	public Owlcat.Runtime.Visual.Waaagh.Shadows.ShadowResolution CacheAtlasSize;

	public float ShadowNearPlane;

	public Owlcat.Runtime.Visual.Waaagh.Shadows.ShadowResolution DirectionalLightCascadeResolution;

	public ShadowResolutionSettings PointLightResolution;

	public ShadowResolutionSettings SpotLightResolution;

	public Cascades DirectionalLightCascades;

	public float DepthBias;

	public float NormalBias;

	public float ReceiverNormalBias;

	public float DirectionalSlopeBias;

	public float PointSlopeBias;

	public override void Reset()
	{
		ShadowUpdateDistances = null;
		StaticShadowsCacheEnabled = false;
		ShadowManager = null;
		ShadowQuality = ShadowQuality.Disable;
		AtlasSize = (Owlcat.Runtime.Visual.Waaagh.Shadows.ShadowResolution)0;
		CacheAtlasSize = (Owlcat.Runtime.Visual.Waaagh.Shadows.ShadowResolution)0;
		ShadowNearPlane = 0f;
		DirectionalLightCascadeResolution = (Owlcat.Runtime.Visual.Waaagh.Shadows.ShadowResolution)0;
		PointLightResolution = default(ShadowResolutionSettings);
		SpotLightResolution = default(ShadowResolutionSettings);
		DirectionalLightCascades = null;
		DepthBias = 0f;
		NormalBias = 0f;
		ReceiverNormalBias = 0f;
		DirectionalSlopeBias = 0f;
		PointSlopeBias = 0f;
	}
}
