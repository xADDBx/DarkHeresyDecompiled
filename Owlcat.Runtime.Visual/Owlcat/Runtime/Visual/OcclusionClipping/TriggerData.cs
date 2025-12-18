using Unity.Burst;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

[BurstCompile]
internal struct TriggerData
{
	public uint NodeId;
}
