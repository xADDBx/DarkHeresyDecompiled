using Unity.Burst;

namespace Kingmaker.UI.AR;

[BurstCompile]
internal readonly struct OutlinePlotCommand
{
	public readonly OutlinePlotCommandCode code;

	public readonly int firstSegmentCellIndex;

	public readonly int secondSegmentCellIndex;

	public OutlinePlotCommand(OutlinePlotCommandCode code, int firstSegmentCellIndex, int secondSegmentCellIndex)
	{
		this.code = code;
		this.firstSegmentCellIndex = firstSegmentCellIndex;
		this.secondSegmentCellIndex = secondSegmentCellIndex;
	}
}
