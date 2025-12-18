using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

[BurstCompile]
internal struct ProbeData
{
	public int ProbeId;

	public float4 Sphere;
}
