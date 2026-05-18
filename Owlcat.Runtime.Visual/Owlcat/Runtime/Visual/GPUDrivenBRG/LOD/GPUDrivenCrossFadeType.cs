using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.LOD;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\GPUDrivenBRG\\LOD\\GPUDrivenLODGroupData.cs")]
public enum GPUDrivenCrossFadeType
{
	Disabled,
	CrossFadeOut,
	CrossFadeIn,
	Visible
}
