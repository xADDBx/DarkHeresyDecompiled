using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility;
using Kingmaker.View.Covers;
using Pathfinding;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityDataProviderForPattern
{
	MechanicEntity Caster { get; }

	int RangeCells { get; }

	int BurstAttacksCount { get; }

	bool IsBurst { get; }

	AbilityData Data { get; }

	GridNodeBase GetBestShootingPosition(GridNodeBase castNode, TargetWrapper target);

	float CalculateDefenceChanceCached(UnitEntity unit, LosCalculations.CoverType coverType);

	bool HasLosCached(GridNodeBase fromNode, GridNodeBase toNode);
}
