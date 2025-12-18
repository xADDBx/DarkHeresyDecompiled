using System.Collections.Generic;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.PatternAttack;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("f8f9261ab3994be68bd4c1d12459402d")]
public class AbilityTargetsInPattern : AbilitySelectTarget, IAbilityAoEPatternProvider
{
	[SerializeField]
	public AbilityAoEPatternSettings m_PatternSettings;

	[SerializeField]
	private bool m_IncludeDead;

	[SerializeField]
	private bool m_IncludeCaster;

	[SerializeField]
	private bool m_IncludeNonUnitTargets;

	[SerializeField]
	private ConditionsChecker m_Condition;

	private int? m_HaloSize;

	public int? HaloSize => m_HaloSize;

	public bool IsIgnoreLos => m_PatternSettings.IsIgnoreLos;

	public bool UseMeleeLos => m_PatternSettings.UseMeleeLos;

	public bool IsIgnoreLevelDifference => m_PatternSettings.IsIgnoreLevelDifference;

	public int PatternAngle => m_PatternSettings.PatternAngle;

	public bool CalculateAttackFromPatternCentre => m_PatternSettings.CalculateAttackFromPatternCentre;

	public TargetType Targets => m_PatternSettings.Targets;

	public AoEPattern Pattern => m_PatternSettings.Pattern;

	public void OverrideHaloSize(int? haloSize)
	{
		m_HaloSize = haloSize;
	}

	public OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, GridNodeBase casterNode, GridNodeBase targetNode, Size targetSize = Size.Medium, bool coveredTargetsOnly = false)
	{
		return m_PatternSettings.GetOrientedPattern(ability, casterNode, targetNode, targetSize, coveredTargetsOnly);
	}

	public OrientedPatternData GetOrientedHaloPattern(IAbilityDataProviderForPattern ability, int haloSize, GridNodeBase casterNode, GridNodeBase targetNode, Size targetSize = Size.Medium, bool coveredTargetsOnly = false)
	{
		return m_PatternSettings.GetOrientedHaloPattern(ability, haloSize, casterNode, targetNode, targetSize, coveredTargetsOnly);
	}

	public override IEnumerable<TargetWrapper> Select(AbilityExecutionContext context, TargetWrapper anchor)
	{
		MechanicEntity caster = context.Caster;
		GridNodeBase targetNode = (GridNodeBase)ObstacleAnalyzer.GetNearestNode(anchor.Point).node;
		Size targetSizeForPattern = context.Ability.GetTargetSizeForPattern(anchor);
		GridNodeBase casterNode = context.Ability?.GetBestShootingPosition(anchor);
		OrientedPatternData pattern = GetOrientedPattern(context.Ability, casterNode, targetNode, targetSizeForPattern);
		foreach (MechanicEntity targetableEntity in Game.Instance.EntityPools.TargetableEntities)
		{
			if ((!m_IncludeNonUnitTargets && !(targetableEntity is BaseUnitEntity)) || (!m_IncludeDead && targetableEntity != null && targetableEntity.IsDeadOrUnconscious) || (targetableEntity == context.Caster && !m_IncludeCaster) || !AoEPatternHelper.WouldTargetEntity(pattern, targetableEntity))
			{
				continue;
			}
			if (Targets != TargetType.Any)
			{
				PartCombatGroup combatGroupOptional = targetableEntity.GetCombatGroupOptional();
				switch (Targets)
				{
				case TargetType.Enemy:
					if (combatGroupOptional != null && !combatGroupOptional.IsEnemy(caster))
					{
						continue;
					}
					break;
				case TargetType.Ally:
					if (combatGroupOptional == null || !combatGroupOptional.IsAlly(caster))
					{
						continue;
					}
					break;
				}
			}
			if (m_Condition.HasConditions)
			{
				using (context.SetScope(targetableEntity.ToITargetWrapper()))
				{
					if (!m_Condition.Check())
					{
						continue;
					}
				}
			}
			if (IsIgnoreLos || LosCalculations.GetWarhammerLos(context.Pattern.ApplicationNode.Vector3Position(), context.Caster.SizeRect, targetableEntity).CoverType != LosCalculations.CoverType.LosBlocker)
			{
				yield return new TargetWrapper(targetableEntity);
			}
		}
	}
}
