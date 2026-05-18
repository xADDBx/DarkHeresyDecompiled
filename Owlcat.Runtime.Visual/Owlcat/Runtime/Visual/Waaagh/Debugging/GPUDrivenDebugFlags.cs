using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

[Flags]
[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Debugging\\GPUDrivenDebugFlags.cs")]
public enum GPUDrivenDebugFlags
{
	None = 0,
	ShowOcclusionTest = 1
}
