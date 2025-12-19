using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Constraints;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\XPBD\\Constraints\\ConstraintType.cs")]
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
