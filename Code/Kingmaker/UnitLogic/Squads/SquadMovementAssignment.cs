using Kingmaker.Pathfinding;
using Pathfinding;

namespace Kingmaker.UnitLogic.Squads;

public readonly struct SquadMovementAssignment
{
	public readonly GraphNode TargetCell;

	public readonly ForcedPath Path;

	public readonly float PathCost;

	public SquadMovementAssignment(GraphNode targetCell, ForcedPath path, float pathCost)
	{
		TargetCell = targetCell;
		Path = path;
		PathCost = pathCost;
	}
}
