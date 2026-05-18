using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.IndirectRendering;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\IndirectRendering\\IndirectInstanceData.cs")]
[Flags]
public enum IndirectInstancingMeshFlags : uint
{
	None = 0u,
	ShapeCullingEnabled = 1u,
	ShapeCullingOutside = 2u
}
