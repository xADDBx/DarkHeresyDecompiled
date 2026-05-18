using Unity.Burst;

namespace Kingmaker.UI.AR;

[BurstCompile]
public struct SurfaceCellFilterData
{
	public uint belongToAllAreaMask;

	public uint belongToAnyAreasMask;

	public uint notBelongToAnyAreasMask;
}
