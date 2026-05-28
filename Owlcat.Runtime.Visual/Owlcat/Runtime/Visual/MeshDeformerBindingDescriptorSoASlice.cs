using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual;

public struct MeshDeformerBindingDescriptorSoASlice
{
	public NativeSlice<int4> Offsets;

	public NativeSlice<float4x4> BodyToDeformer;

	public NativeSlice<int> MasterIndexInSimulation;
}
