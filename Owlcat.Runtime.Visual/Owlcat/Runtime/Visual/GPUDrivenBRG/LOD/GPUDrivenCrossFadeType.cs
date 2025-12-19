using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.LOD;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\GPUDrivenBRG\\LOD\\GPUDrivenLODGroupData.cs")]
public enum GPUDrivenCrossFadeType
{
	Disabled,
	CrossFadeOut,
	CrossFadeIn,
	Visible
}
