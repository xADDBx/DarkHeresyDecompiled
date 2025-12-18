using Unity.Burst;

namespace Kingmaker.UI.AR;

[BurstCompile]
public struct SurfaceCellFilterData
{
	public int belongToAllAreaMask;

	public int belongToAnyAreasMask;

	public int notBelongToAnyAreasMask;
}
