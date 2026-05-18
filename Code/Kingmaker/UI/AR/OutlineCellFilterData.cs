using Unity.Burst;

namespace Kingmaker.UI.AR;

[BurstCompile]
public struct OutlineCellFilterData
{
	public uint belongToAllAreaMask;

	public uint belongToAnyAreasMask;

	public uint notBelongToAnyAreasMask;

	public SurfaceBufferMask surfaceBuffer;
}
