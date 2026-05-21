using UnityEngine.Rendering;

namespace Owlcat.ShaderLibrary.Visual.Debug;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\ShaderLibrary\\Debug\\DebugViewEnums.cs")]
public enum DebugOverdrawMode
{
	None,
	Overdraw,
	QuadOverdraw
}
