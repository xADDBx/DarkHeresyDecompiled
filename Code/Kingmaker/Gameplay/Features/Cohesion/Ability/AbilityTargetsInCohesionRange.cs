using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Kingmaker.Gameplay.Features.Cohesion.Ability;

[Serializable]
[AllowedOn(typeof(BlueprintAbilityModifier))]
[ComponentName("Cohesion/AbilityTargetsInCohesionRange")]
[TypeId("7f6cc823e91148949ccb11dec7f1e9d7")]
public sealed class AbilityTargetsInCohesionRange : AbilitySelectTarget, IAbilityAoEPatternProvider
{
	public RestrictionCalculator Restriction = new RestrictionCalculator();

	public TargetType TargetFilter;

	public bool IncludeCaster;

	private static BlueprintAreaEffect AreaEffectSettings => ConfigRoot.Instance.CombatRoot.CohesionAreaEffect;

	bool IAbilityAoEPatternProvider.IsIgnoreLos => AreaEffectSettings.IgnoreLosWhenSpread;

	bool IAbilityAoEPatternProvider.UseMeleeLos => AreaEffectSettings.UseMeleeLos;

	bool IAbilityAoEPatternProvider.IsIgnoreLevelDifference => AreaEffectSettings.IgnoreLevelDifferenceWhenSpread;

	int IAbilityAoEPatternProvider.PatternAngle => 360;

	bool IAbilityAoEPatternProvider.CalculateAttackFromPatternCentre => true;

	TargetType IAbilityAoEPatternProvider.Targets => TargetFilter;

	AoEPattern IAbilityAoEPatternProvider.Pattern => AreaEffectSettings.Pattern;

	int? IAbilityAoEPatternProvider.HaloSize => null;

	void IAbilityAoEPatternProvider.OverrideHaloSize(int? haloSize)
	{
	}

	OrientedPatternData IAbilityAoEPatternProvider.GetOrientedPattern(IAbilityDataProviderForPattern ability, GridNodeBase casterNode, GridNodeBase targetNode, Size targetSize, bool coveredTargetsOnly)
	{
		return AreaEffectSettings.GetOrientedPattern(ability, casterNode, targetNode, targetSize, coveredTargetsOnly);
	}

	OrientedPatternData IAbilityAoEPatternProvider.GetOrientedHaloPattern(IAbilityDataProviderForPattern ability, int haloSize, GridNodeBase casterNode, GridNodeBase targetNode, Size targetSize, bool coveredTargetsOnly)
	{
		return AreaEffectSettings.GetOrientedHaloPattern(ability, haloSize, casterNode, targetNode, targetSize, coveredTargetsOnly);
	}

	public override IEnumerable<TargetWrapper> Select(AbilityExecutionContext context, TargetWrapper anchor)
	{
		MechanicEntity caster = context.Caster;
		PartCohesion cohesion = caster.GetRequired<PartCohesion>();
		if (IncludeCaster && IsSuitableTarget(context, caster))
		{
			yield return new TargetWrapper(caster);
		}
		foreach (UnitEntity item in cohesion.UnitsInRange)
		{
			if (IsSuitableTarget(context, item))
			{
				yield return new TargetWrapper(item);
			}
		}
	}

	private bool IsSuitableTarget(AbilityExecutionContext context, MechanicEntity target)
	{
		if (TargetFilter switch
		{
			TargetType.Enemy => context.Caster.IsEnemy(target), 
			TargetType.Ally => context.Caster.IsAlly(target), 
			TargetType.Any => true, 
			_ => throw new ArgumentOutOfRangeException(), 
		})
		{
			return Restriction.IsPassed(context, target);
		}
		return false;
	}
}
