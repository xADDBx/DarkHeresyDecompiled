using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.ParticleAttachments;

[BurstCompile]
public struct ParticleAttachmentData
{
	public int IndexInBody;

	public float3 PositionOffset;
}
