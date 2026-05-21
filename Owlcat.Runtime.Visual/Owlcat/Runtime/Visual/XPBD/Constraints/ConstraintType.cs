using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Constraints;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\XPBD\\Constraints\\ConstraintType.cs")]
public enum ConstraintType
{
	Distance,
	Bend,
	Angular,
	Shape,
	Simplex,
	Aerodynamics,
	Foliage
}
