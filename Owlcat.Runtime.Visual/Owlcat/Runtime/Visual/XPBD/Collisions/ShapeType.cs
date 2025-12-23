using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\XPBD\\Collisions\\ColliderShape.cs")]
public enum ShapeType
{
	Sphere,
	Box,
	Capsule,
	FrustumCapsule
}
