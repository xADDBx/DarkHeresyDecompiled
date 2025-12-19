using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

[Flags]
[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\GPUDrivenBRG\\GPUDrivenVisibilityInfo.cs")]
public enum GPUDrivenVisibilityFlags
{
	None = 0,
	OccludeOnly = 1,
	ForceVisibleForCPUCulling = 2,
	SceneVisibilityHidden = 4,
	ReflectionProbeStatic = 8
}
