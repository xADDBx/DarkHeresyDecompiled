using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Solvers.GPU;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\XPBD\\Solvers\\GPU\\SkinnedMeshConstantBuffer.cs", needAccessors = false, generateCBuffer = true)]
public struct SkinnedMeshConstantBuffer
{
	public const int kMaxSkinnedBodiesPerDispatch = 26;

	[HLSLArray(26, typeof(Vector4))]
	public unsafe fixed float _XpbdSkinnedBodyIndices[104];
}
