using Pathfinding;

namespace Kingmaker.UnitLogic.Squads.Goals;

public readonly struct SquadCandidateCell
{
	public readonly GraphNode Node;

	public readonly float Desirability;

	public SquadCandidateCell(GraphNode node, float desirability)
	{
		Node = node;
		Desirability = desirability;
	}
}
