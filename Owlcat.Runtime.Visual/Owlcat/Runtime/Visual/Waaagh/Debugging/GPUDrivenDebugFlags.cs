using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

[Flags]
[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Debugging\\GPUDrivenDebugFlags.cs")]
public enum GPUDrivenDebugFlags
{
	None = 0,
	ShowOcclusionTest = 1
}
