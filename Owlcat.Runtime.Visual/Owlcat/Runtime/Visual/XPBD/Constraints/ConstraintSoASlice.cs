using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Constraints;

public struct ConstraintSoASlice
{
	public NativeSlice<int4> Indices;

	public NativeSlice<float4> Parameters0;

	public NativeSlice<float4> Parameters1;
}
