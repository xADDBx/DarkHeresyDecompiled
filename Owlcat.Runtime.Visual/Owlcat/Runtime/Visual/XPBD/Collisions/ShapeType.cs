using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\XPBD\\Collisions\\ColliderShape.cs")]
public enum ShapeType
{
	Sphere,
	Box,
	Capsule,
	FrustumCapsule
}
