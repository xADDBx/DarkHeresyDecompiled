using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.ParticleAttachments;

public struct ParticleAttachmentDataSoASlice
{
	public NativeSlice<int> IndexInBody;

	public NativeSlice<float3> PositionOffset;
}
