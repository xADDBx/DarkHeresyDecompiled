using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.ParticleAttachments;

public struct ParticleAttachmentDescriptorSoASlice
{
	public NativeSlice<int2> ParticleDataRange;

	public NativeSlice<int2> BodyParticlesRange;

	public NativeSlice<float4x4> LocalToWorld;
}
