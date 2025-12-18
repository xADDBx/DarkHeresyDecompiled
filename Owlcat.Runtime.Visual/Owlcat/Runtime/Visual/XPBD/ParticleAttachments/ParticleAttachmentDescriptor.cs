using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.ParticleAttachments;

[BurstCompile]
public struct ParticleAttachmentDescriptor
{
	public int2 ParticleDataRange;

	public int2 BodyParticlesRange;

	public float4x4 LocalToWorld;
}
