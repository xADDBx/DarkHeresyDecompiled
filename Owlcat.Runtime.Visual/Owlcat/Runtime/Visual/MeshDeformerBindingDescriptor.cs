using Unity.Mathematics;

namespace Owlcat.Runtime.Visual;

public struct MeshDeformerBindingDescriptor
{
	public int4 Offsets;

	public int MasterIndexInSimulation;

	public float4x4 BodyToDeformer;
}
