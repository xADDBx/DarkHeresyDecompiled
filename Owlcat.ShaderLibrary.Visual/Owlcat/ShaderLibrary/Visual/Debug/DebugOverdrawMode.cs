using UnityEngine.Rendering;

namespace Owlcat.ShaderLibrary.Visual.Debug;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\ShaderLibrary\\Debug\\DebugViewEnums.cs")]
public enum DebugOverdrawMode
{
	None,
	All,
	TransparentOnly,
	OpaqueOnly,
	QuadOverdraw
}
