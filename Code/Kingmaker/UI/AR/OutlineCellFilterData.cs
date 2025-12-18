using Unity.Burst;

namespace Kingmaker.UI.AR;

[BurstCompile]
public struct OutlineCellFilterData
{
	public int belongToAllAreaMask;

	public int belongToAnyAreasMask;

	public int notBelongToAnyAreasMask;

	public SurfaceBufferMask surfaceBuffer;
}
