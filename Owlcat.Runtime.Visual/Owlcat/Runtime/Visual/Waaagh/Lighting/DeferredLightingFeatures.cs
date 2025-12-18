using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Lighting;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\Waaagh\\Lighting\\DeferredLightingFeatures.cs")]
internal static class DeferredLightingFeatures
{
	public const int kFeatureTileSize = 16;

	public const int kFeatureVariantCount = 8;

	public const int kOptimalThreadGroupSize = 32;
}
