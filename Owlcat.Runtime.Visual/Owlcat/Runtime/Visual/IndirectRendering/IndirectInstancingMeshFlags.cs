using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.IndirectRendering;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@f3d4bf622f68\\Runtime\\IndirectRendering\\IndirectInstanceData.cs")]
[Flags]
public enum IndirectInstancingMeshFlags : uint
{
	None = 0u,
	ShapeCullingEnabled = 1u,
	ShapeCullingOutside = 2u
}
