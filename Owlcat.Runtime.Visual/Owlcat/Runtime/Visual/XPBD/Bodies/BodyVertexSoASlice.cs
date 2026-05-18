using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public struct BodyVertexSoASlice
{
	public NativeSlice<float3> Normal;

	public NativeSlice<float3> Position;

	public NativeSlice<float3> RestNormal;
}
