using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.View.Covers;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateHitChances : RulebookTargetEvent<MechanicEntity, MechanicEntity>
{
	public readonly CompositeModifiersManager Modifiers = new CompositeModifiersManager();

	public readonly CompositeModifiersManager PreciseAttackToCoveredTargetPenaltyModifiers = new CompositeModifiersManager(0);

	public readonly FlagModifiersManager ForceHitCover = new FlagModifiersManager();

	public readonly FlagModifiersManager AutoHit = new FlagModifiersManager();

	public readonly FlagModifiersManager AutoMiss = new FlagModifiersManager();

	[NotNull]
	public AbilityData Ability { get; }

	public AbilityAttackDelivery AbilityAttackDelivery { get; }

	public Vector3 EffectiveCasterPosition { get; }

	public Vector3 AbilityTargetPosition { get; }

	public StatType? OverrideAttackStatType { get; }

	public int BurstIndex { get; }

	public LosCalculations.CoverType? FakeCover { get; private set; }

	public bool IgnoreRealCoverByFakeCover { get; private set; }

	public StatType ResultAttackStatType { get; private set; }

	public int ResultAttackStatValue { get; private set; }

	public int ResultHitChance { get; private set; }

	public int ResultHitChanceWithoutCritEffects { get; private set; }

	public LosCalculations.CoverType ResultLos { get; private set; }

	[CanBeNull]
	public MechanicEntity ResultCoverEntity { get; private set; }

	public bool IsAutoHit => AutoHit.Value;

	public bool IsAutoMiss
	{
		get
		{
			if (!IsAutoHit)
			{
				return AutoMiss.Value;
			}
			return false;
		}
	}

	public bool ResultForceCoverHit => ForceHitCover.Value;

	public Modifier? ResultForceCoverHitReason => ForceHitCover.List.FirstItem((Modifier i) => i.Value > 0);

	public IEnumerable<Modifier> AllModifiersList
	{
		get
		{
			foreach (Modifier item in Modifiers.List)
			{
				yield return item;
			}
		}
	}

	public override AbilityData MaybeAbility => Ability;

	public int RawResult { get; private set; }

	public RuleCalculateHitChances([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] AbilityData ability, int burstIndex, Vector3 effectiveCasterPosition, Vector3 abilityTargetPosition)
		: base(initiator, target)
	{
		Ability = ability;
		AbilityAttackDelivery = Ability.Blueprint.GetComponent<AbilityAttackDelivery>() ?? throw new NullReferenceException();
		EffectiveCasterPosition = effectiveCasterPosition;
		AbilityTargetPosition = abilityTargetPosition;
		OverrideAttackStatType = AbilityAttackDelivery.OverrideAttackStatType;
		BurstIndex = burstIndex;
	}

	public RuleCalculateHitChances([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] AbilityData ability, int burstIndex)
		: this(initiator, target, ability, burstIndex, initiator.Position, target.Position)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (Ability.IsPrecise)
		{
			Modifiers.Add(ModifierType.ValAdd, GetHitChancePenalty(Ability.PreciseBodyPart), this, ModifierDescriptor.PreciseAttack);
		}
		IAbilityAoEPatternProvider patternSettings = Ability.GetPatternSettings();
		LosDescription losDescription = ((patternSettings != null && patternSettings.CalculateAttackFromPatternCentre) ? LosCalculations.GetWarhammerLos(AbilityTargetPosition, default(IntRect), base.Target) : LosCalculations.GetWarhammerLos(EffectiveCasterPosition, base.Initiator.SizeRect, base.Target));
		if ((losDescription.CoverType == LosCalculations.CoverType.Obstacle || IgnoreRealCoverByFakeCover) && FakeCover.HasValue)
		{
			losDescription = new LosDescription(FakeCover.Value, losDescription.Obstacle);
		}
		ResultLos = losDescription;
		ResultCoverEntity = losDescription.ObstacleEntity;
		RuleCalculateStatsWeapon weaponStats = Ability.GetWeaponStats();
		if (Ability.IsSingleTarget)
		{
			Modifiers.CopyFrom(weaponStats.SingleAdditionalHitChanceModifiers);
		}
		if (Ability.IsBurst)
		{
			Modifiers.CopyFrom(weaponStats.BurstAdditionalHitChanceModifiers);
		}
		if (Ability.IsAoe)
		{
			Modifiers.CopyFrom(weaponStats.AoeAdditionalHitChanceModifiers);
		}
		if (base.Target is DestructibleEntity { HitChanceModifier: var hitChanceModifier } && hitChanceModifier > 0)
		{
			Modifiers.Add(ModifierType.ValAdd, hitChanceModifier, this, ModifierDescriptor.DestructibleObject);
		}
		if (AbilityAttackDelivery.IsMelee)
		{
			OnTriggerMelee();
		}
		else
		{
			AbilityAttackDelivery abilityAttackDelivery = AbilityAttackDelivery;
			if (abilityAttackDelivery == null || (!abilityAttackDelivery.IsRanged && !abilityAttackDelivery.IsThrow) || 1 == 0)
			{
				throw new InvalidOperationException("Hit Chances can be rolled for Melee and Ranged attacks only");
			}
			if (Ability.IsPrecise && (LosCalculations.CoverType)losDescription != 0)
			{
				int coveredTargetPreciseHitChancePenalty = ConfigRoot.Instance.CombatRoot.CoveredTargetPreciseHitChancePenalty;
				coveredTargetPreciseHitChancePenalty = PreciseAttackToCoveredTargetPenaltyModifiers.Apply(coveredTargetPreciseHitChancePenalty);
				Modifiers.Add(ModifierType.ValAdd, -coveredTargetPreciseHitChancePenalty, this, ModifierDescriptor.PreciseAttack);
			}
			OnTriggerRanged();
		}
		ApplyAttackHitPolicy();
	}

	public void SetFakeCover(LosCalculations.CoverType fakeCover, bool ignoreRealCover)
	{
		FakeCover = fakeCover;
		IgnoreRealCoverByFakeCover = ignoreRealCover;
	}

	private void OnTriggerMelee()
	{
		ResultAttackStatType = OverrideAttackStatType ?? StatType.WeaponSkill;
		ResultAttackStatValue = base.Initiator.GetAttributeOptional(ResultAttackStatType)?.ModifiedValue ?? 0;
		int hitChanceOverkillBorder = ConfigRoot.Instance.CombatRoot.HitChanceOverkillBorder;
		RawResult = Modifiers.Apply(ResultAttackStatValue);
		ResultHitChance = Mathf.Clamp(RawResult, 0, hitChanceOverkillBorder);
		int value = Modifiers.Apply(ResultAttackStatValue, (Modifier m) => FactIsNotCritEffectFilter(m.Fact));
		ResultHitChanceWithoutCritEffects = Mathf.Clamp(value, 0, hitChanceOverkillBorder);
	}

	private void OnTriggerRanged()
	{
		ResultAttackStatType = OverrideAttackStatType ?? StatType.BallisticSkill;
		ModifiableValueAttributeStat attributeOptional = base.Initiator.GetAttributeOptional(ResultAttackStatType);
		ResultAttackStatValue = attributeOptional?.ModifiedValue ?? 0;
		int rawResult = Modifiers.Apply(ResultAttackStatValue);
		int hitChanceOverkillBorder = ConfigRoot.Instance.CombatRoot.HitChanceOverkillBorder;
		RawResult = rawResult;
		ResultHitChance = Mathf.Clamp(RawResult, 0, hitChanceOverkillBorder);
		int value = attributeOptional?.CalculateFilteredModifiedValue((Modifier m) => FactIsNotCritEffectFilter(m.Fact)) ?? 0;
		int value2 = Modifiers.Apply(value, (Modifier m) => FactIsNotCritEffectFilter(m.Fact));
		ResultHitChanceWithoutCritEffects = Mathf.Clamp(value2, 0, hitChanceOverkillBorder);
	}

	private static bool FactIsNotCritEffectFilter(EntityFact fact)
	{
		return fact == null || !(fact.Blueprint is BlueprintBuff blueprintBuff) || !blueprintBuff.IsCriticalEffect;
	}

	private void ApplyAttackHitPolicy()
	{
		AbilityData ability = Ability;
		if ((object)ability != null && ability.IsPrecise)
		{
			BlueprintBodyPart preciseBodyPart = ability.PreciseBodyPart;
			if (preciseBodyPart != null && preciseBodyPart.Tags.HasAnyFlag(BodyPartTags.Default))
			{
				AutoHit.Add();
			}
		}
		PartMechanicFeatures features = base.Initiator.Features;
		if ((bool)features.AutoHit)
		{
			AutoHit.Add(features.AutoHit.FirstAssociatedFact);
		}
		if ((bool)features.PreciseAttackAutoHit)
		{
			ability = Ability;
			if ((object)ability != null && ability.IsPrecise)
			{
				AutoHit.Add(features.PreciseAttackAutoHit.FirstAssociatedFact);
			}
		}
		if (base.Target is DestructibleEntity { AutoHit: not false })
		{
			AutoHit.Add();
		}
		if ((bool)features.AutoMiss)
		{
			AutoMiss.Add(features.AutoMiss.FirstAssociatedFact);
		}
		if (IsAutoHit)
		{
			RawResult = 100;
			ResultHitChance = 100;
			ResultHitChanceWithoutCritEffects = ResultHitChance;
		}
		else if (IsAutoMiss)
		{
			RawResult = 0;
			ResultHitChance = 0;
			ResultHitChanceWithoutCritEffects = ResultHitChance;
		}
	}

	private int GetHitChancePenalty(BlueprintBodyPart bodyPart)
	{
		if (bodyPart == null)
		{
			return 0;
		}
		if (Ability.IsMelee)
		{
			return bodyPart.MeleePreciseHitChanceModifier;
		}
		if (Ability.IsRanged)
		{
			return bodyPart.RangedPreciseHitChanceModifier;
		}
		return 0;
	}
}
