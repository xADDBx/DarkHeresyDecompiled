using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

[Flags]
[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\Debugging\\GPUDrivenDebugFlags.cs")]
public enum GPUDrivenDebugFlags
{
	None = 0,
	ShowOcclusionTest = 1
}
