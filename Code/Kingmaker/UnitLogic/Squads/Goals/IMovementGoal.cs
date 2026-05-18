using System.Collections.Generic;

namespace Kingmaker.UnitLogic.Squads.Goals;

public interface IMovementGoal
{
	IReadOnlyList<SquadCandidateCell> GetCandidates(UnitSquad squad);
}
