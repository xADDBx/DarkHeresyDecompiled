using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

[Flags]
[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\GPUDrivenBRG\\GPUDrivenVisibilityInfo.cs")]
public enum GPUDrivenVisibilityFlags
{
	None = 0,
	OccludeOnly = 1,
	ForceVisibleForCPUCulling = 2,
	SceneVisibilityHidden = 4,
	ReflectionProbeStatic = 8
}
