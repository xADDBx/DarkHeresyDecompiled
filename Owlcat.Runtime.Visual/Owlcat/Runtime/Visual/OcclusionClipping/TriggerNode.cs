using Owlcat.Runtime.Visual.Experimental.Geometry;
using Unity.Burst;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

[BurstCompile]
internal struct TriggerNode
{
	public int TriggerId;

	public Obb Box;
}
