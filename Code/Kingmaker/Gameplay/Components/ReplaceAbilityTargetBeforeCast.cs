using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("25eb288eac4b4a8e8e1b017921566697")]
public abstract class ReplaceAbilityTargetBeforeCast : MechanicEntityFactComponentDelegate
{
	public enum TargetSelectionRuleType
	{
		ClosestFromSameSide,
		FarthestFromSameSide
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public TargetSelectionRuleType TargetSelectionRule;

	public bool ExcludeOriginalTarget;

	protected void TryReplaceTarget(RulePerformAbility rule)
	{
		if (rule.ReplaceTargetSource == null && Restrictions.IsPassed(rule.Context, base.Owner, null, rule))
		{
			TargetWrapper targetWrapper = FindNewTarget(rule);
			if (targetWrapper != null && targetWrapper != rule.AbilityTarget)
			{
				rule.ReplaceTarget(targetWrapper, base.Fact);
			}
		}
	}

	[CanBeNull]
	private TargetWrapper FindNewTarget(RulePerformAbility rule)
	{
		MechanicEntity originalTargetEntity = rule.AbilityTarget.Entity;
		IEnumerable<UnitEntity> candidates = Game.Instance.Controllers.TurnController.AllUnits.OfType<UnitEntity>().Where(delegate(UnitEntity i)
		{
			if (ExcludeOriginalTarget && i == originalTargetEntity)
			{
				return false;
			}
			return (originalTargetEntity == null || (base.Owner.IsEnemy(originalTargetEntity) == base.Owner.IsEnemy(i) && base.Owner.IsAlly(originalTargetEntity) == base.Owner.IsAlly(i))) && rule.Ability.CanTarget(i);
		});
		return OrderByDistance(candidates).FirstOrDefault();
	}

	private IEnumerable<MechanicEntity> OrderByDistance(IEnumerable<MechanicEntity> candidates)
	{
		return TargetSelectionRule switch
		{
			TargetSelectionRuleType.ClosestFromSameSide => candidates.OrderBy((MechanicEntity i) => base.Owner.DistanceTo(i)), 
			TargetSelectionRuleType.FarthestFromSameSide => candidates.OrderByDescending((MechanicEntity i) => base.Owner.DistanceTo(i)), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
