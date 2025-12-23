using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public struct MeshDeformerDescriptorSoASlice
{
	public NativeSlice<int2> DeformableVerticesRange;

	public NativeSlice<int2> BindingsRange;

	public NativeSlice<int2> VertexToSkinnedVertexMapRange;

	public NativeSlice<float4x4> LocalToWorld;

	public NativeSlice<int2> SkinnedVerticesRange;

	public NativeSlice<float4x4> WorldToLocal;
}
