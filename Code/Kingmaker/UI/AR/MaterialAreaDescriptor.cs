using Unity.Burst;
using Unity.Mathematics;

namespace Kingmaker.UI.AR;

[BurstCompile]
internal struct MaterialAreaDescriptor
{
	public byte materialId;

	public int2 coordsMin;

	public int2 coordsMax;
}
