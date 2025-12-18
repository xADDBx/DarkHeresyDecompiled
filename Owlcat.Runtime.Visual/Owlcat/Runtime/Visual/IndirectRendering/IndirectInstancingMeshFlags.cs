using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.IndirectRendering;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@01c4fcbc474f\\Runtime\\IndirectRendering\\IndirectInstanceData.cs")]
[Flags]
public enum IndirectInstancingMeshFlags : uint
{
	None = 0u,
	ShapeCullingEnabled = 1u,
	ShapeCullingOutside = 2u
}
