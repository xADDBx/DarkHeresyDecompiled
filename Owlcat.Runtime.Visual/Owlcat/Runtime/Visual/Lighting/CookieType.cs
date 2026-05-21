using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Lighting;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Lighting\\LightCookieConstantBuffer.cs", needAccessors = false)]
public enum CookieType
{
	Directional,
	Spot,
	Point
}
