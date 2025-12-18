using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.LOD;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@01c4fcbc474f\\Runtime\\GPUDrivenBRG\\LOD\\GPUDrivenLODGroupData.cs")]
public enum GPUDrivenCrossFadeType
{
	Disabled,
	CrossFadeOut,
	CrossFadeIn,
	Visible
}
