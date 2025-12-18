using Kingmaker.View.Covers;

namespace Kingmaker.Pathfinding;

public readonly struct GridObstacleDescription
{
	public readonly LosCalculations.CoverType Type;

	public readonly int Top;

	public readonly int Bottom;

	public GridObstacleDescription(LosCalculations.CoverType type, int top, int bottom)
	{
		Type = type;
		Top = top;
		Bottom = bottom;
	}
}
