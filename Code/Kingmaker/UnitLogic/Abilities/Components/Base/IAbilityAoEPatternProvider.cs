using JetBrains.Annotations;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Pathfinding;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityAoEPatternProvider
{
	bool IsIgnoreLos { get; }

	bool RespectCovers => false;

	bool RespectCoversEvenInCloseRange => false;

	bool UseMeleeLos { get; }

	bool IsIgnoreLevelDifference { get; }

	int PatternAngle { get; }

	bool CalculateAttackFromPatternCentre { get; }

	TargetType Targets { get; }

	[CanBeNull]
	AoEPattern Pattern { get; }

	int? HaloSize { get; }

	void OverrideHaloSize(int? haloSize);

	OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, GridNodeBase casterNode, GridNodeBase targetNode, Size targetSize = Size.Medium, bool coveredTargetsOnly = false);

	OrientedPatternData GetOrientedHaloPattern(IAbilityDataProviderForPattern ability, int haloSize, GridNodeBase casterNode, GridNodeBase targetNode, Size targetSize = Size.Medium, bool coveredTargetsOnly = false);
}
