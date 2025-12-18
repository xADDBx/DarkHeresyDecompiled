using UnityEngine.Rendering;

namespace Owlcat.ShaderLibrary.Visual.Debug;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\ShaderLibrary\\Debug\\DebugViewEnums.cs")]
public enum DebugLightingMode
{
	None,
	ShadowCascades,
	BakedGI,
	Shadowmask,
	LightAttenuation,
	Diffuse,
	Specular,
	Reflections,
	Emission
}
