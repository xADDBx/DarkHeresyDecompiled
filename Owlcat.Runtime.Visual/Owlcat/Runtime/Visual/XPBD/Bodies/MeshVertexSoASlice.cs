using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public struct MeshVertexSoASlice
{
	public NativeSlice<float3> Normal;

	public NativeSlice<float3> Position;
}
