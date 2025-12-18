using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\XPBD\\Collisions\\ParticleCollisionMode.cs")]
public enum ParticleCollisionMode
{
	Disabled,
	SelfOnly,
	SelfAndOther,
	OtherOnly
}
