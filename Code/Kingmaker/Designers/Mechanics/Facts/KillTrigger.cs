using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("6d519a34c3e747bfa7d858a3c9a4b978")]
public class KillTrigger : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IInitiatorRulebookSubscriber
{
	public enum PropertyParameter
	{
		None,
		EnemyDifficulty,
		Damage,
		DamageOverflow
	}

	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	public ActionList ActionsOnKill;

	[HideIf("TriggerOnArmorBreak")]
	public ActionList ActionsOnSurvive;

	public bool ActionsOnTarget;

	public bool RefundActionPointsOnKill;

	public bool RefundActionPointsOnSurvive;

	public bool ResetCooldownOnKill;

	public bool ResetCooldownOnSurvive;

	public bool RemoveOnKill;

	public bool RemoveOnSurvive;

	public bool OnlyEnemyKill;

	public bool TriggerOnArmorBreak;

	[ShowIf("TriggerOnArmorBreak")]
	public bool TriggerOnlyOnArmorBreak;

	[ShowIf("TriggerOnArmorBreak")]
	public bool TriggerTwiceForKillThroughArmour;

	[SerializeField]
	private BlueprintAbilityGroupReference m_AffectedGroup;

	[SerializeField]
	private BlueprintUnitFactReference[] m_FilterFacts = new BlueprintUnitFactReference[0];

	public ContextPropertyName ContextPropertyName;

	public PropertyParameter PropertyToSave;

	public BlueprintAbilityGroup AffectedGroup => m_AffectedGroup?.Get();

	public ReferenceArrayProxy<BlueprintUnitFact> FilterFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] filterFacts = m_FilterFacts;
			return filterFacts;
		}
	}

	void IRulebookHandler<RuleDealDamage>.OnEventAboutToTrigger(RuleDealDamage rule)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		if (evt.HPBeforeDamage <= 0 || !FilterFacts.ToList().All((BlueprintUnitFact p) => evt.ConcreteTarget.Facts.Contains(p)) || !m_Restrictions.IsPassed(base.Context, null, null, evt) || (TriggerOnlyOnArmorBreak && (evt.TargetArmor == null || evt.DurabilityBeforeDamage <= 0 || evt.TargetArmor.DurabilityLeft >= 0)))
		{
			return;
		}
		if (PropertyToSave != 0)
		{
			if (PropertyToSave == PropertyParameter.EnemyDifficulty)
			{
				base.Context[ContextPropertyName] = (int)(((evt.Target as UnitEntity)?.Blueprint.DifficultyType + 1) ?? UnitDifficultyType.Common);
			}
			if (PropertyToSave == PropertyParameter.Damage)
			{
				base.Context[ContextPropertyName] = evt.ResultValue;
			}
			if (PropertyToSave == PropertyParameter.DamageOverflow)
			{
				base.Context[ContextPropertyName] = Math.Max(evt.ResultValue - evt.HPBeforeDamage, 0);
			}
		}
		using (base.Context.SetScope(base.Owner.ToITargetWrapper()))
		{
			bool flag = TriggerOnArmorBreak && evt.TargetArmor != null && evt.TargetArmor.DurabilityLeft <= 0 && evt.DurabilityBeforeDamage > 0;
			bool num = !OnlyEnemyKill || base.Owner.IsEnemy(evt.Target);
			PartHealth targetHealth = evt.TargetHealth;
			bool flag2 = targetHealth != null && targetHealth.HitPointsLeft > 0;
			bool flag3 = num && ((!flag2 && !TriggerOnlyOnArmorBreak) || flag);
			if (base.Owner.IsEnemy(evt.Target) && flag2 && !TriggerOnArmorBreak)
			{
				TryRunActionOnSurvive(evt);
			}
			else if (flag3)
			{
				TryRunActionsOnKill(evt);
				if (TriggerTwiceForKillThroughArmour && flag && !flag2)
				{
					TryRunActionsOnKill(evt);
				}
			}
		}
	}

	private void TryRunActionOnSurvive(RuleDealDamage evt)
	{
		if (ActionsOnSurvive.HasActions)
		{
			base.Fact.RunActionInContext(ActionsOnSurvive, (!ActionsOnTarget) ? base.Owner.ToITargetWrapper() : evt.ConcreteTarget.ToITargetWrapper());
		}
		TryRunAdditionalActions(evt, RefundActionPointsOnSurvive, ResetCooldownOnSurvive, RemoveOnSurvive);
	}

	private void TryRunActionsOnKill(RuleDealDamage evt)
	{
		if (ActionsOnKill.HasActions)
		{
			base.Fact.RunActionInContext(ActionsOnKill, (!ActionsOnTarget) ? base.Owner.ToITargetWrapper() : evt.ConcreteTarget.ToITargetWrapper());
		}
		TryRunAdditionalActions(evt, RefundActionPointsOnKill, ResetCooldownOnKill, RemoveOnKill);
	}

	private void TryRunAdditionalActions(RuleDealDamage evt, bool refundActionPoints, bool resetCooldown, bool remove)
	{
		AbilityData sourceAbility = evt.SourceAbility;
		if (sourceAbility == null)
		{
			return;
		}
		if (refundActionPoints)
		{
			base.Owner.CombatState.GainActionPoints(sourceAbility.CalculateActionPointCost(), base.Context);
		}
		PartAbilityCooldowns abilityCooldownsOptional = base.Owner.GetAbilityCooldownsOptional();
		if (!resetCooldown || abilityCooldownsOptional == null)
		{
			return;
		}
		if (AffectedGroup != null && sourceAbility.Blueprint.AbilityGroups.Contains(AffectedGroup))
		{
			abilityCooldownsOptional.RemoveGroupCooldown(AffectedGroup);
			abilityCooldownsOptional.RemoveAbilityCooldown(sourceAbility.Blueprint.OriginalBlueprint);
		}
		if (AffectedGroup == null)
		{
			foreach (BlueprintAbilityGroup abilityGroup in sourceAbility.Blueprint.AbilityGroups)
			{
				abilityCooldownsOptional.RemoveGroupCooldown(abilityGroup);
				abilityCooldownsOptional.RemoveAbilityCooldown(sourceAbility.Blueprint.OriginalBlueprint);
			}
		}
		if (remove)
		{
			base.Owner.Facts.Remove(base.Fact);
		}
	}
}
