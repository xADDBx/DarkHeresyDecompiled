using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Pathfinding;

namespace Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;

public class BurstPattern : IAbilityAoEPatternProvider
{
	public bool IsIgnoreLos => false;

	public bool UseMeleeLos => false;

	public bool IsIgnoreLevelDifference => false;

	public int PatternAngle => 0;

	public bool CalculateAttackFromPatternCentre => false;

	TargetType IAbilityAoEPatternProvider.Targets => TargetType.Any;

	public AoEPattern Pattern => null;

	public int? HaloSize { get; private set; }

	public void OverrideHaloSize(int? haloSize)
	{
		HaloSize = haloSize;
	}

	public OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, GridNodeBase casterNode, GridNodeBase targetNode, Size targetSize = Size.Medium, bool coveredTargetsOnly = false)
	{
		return AbilityProjectileAttackLineHelper.GetOrientedPattern(ability, casterNode, targetNode, coveredTargetsOnly, HaloSize);
	}

	public OrientedPatternData GetOrientedHaloPattern(IAbilityDataProviderForPattern ability, int haloSize, GridNodeBase casterNode, GridNodeBase targetNode, Size targetSize = Size.Medium, bool coveredTargetsOnly = false)
	{
		return AbilityProjectileAttackLineHelper.GetOrientedHaloPattern(ability, casterNode, targetNode, coveredTargetsOnly, haloSize);
	}
}
