using Unity.Burst;

namespace Kingmaker.UI.AR;

[BurstCompile]
public struct IntermediateCell
{
	public int indexInGrid;

	public int packedHeight;

	public PackedCornerOffsets packedCornerOffsets;

	public IntermediateCellFlags flags;
}
