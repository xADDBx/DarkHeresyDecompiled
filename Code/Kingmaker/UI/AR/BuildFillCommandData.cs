using Unity.Burst;
using Unity.Mathematics;

namespace Kingmaker.UI.AR;

[BurstCompile]
internal struct BuildFillCommandData
{
	public float3 meshOffset;
}
