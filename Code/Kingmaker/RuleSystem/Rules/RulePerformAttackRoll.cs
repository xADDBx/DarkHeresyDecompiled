using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.RuleSystem.Rules.Utility;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Enums;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

public class RulePerformAttackRoll : RulebookTargetEvent, IRuleWithChanceRoll, IRulebookEvent
{
	public readonly CompositeModifiersManager HitChanceValueModifiers = new CompositeModifiersManager();

	public readonly HashSet<GridNodeBase> DangerArea = new HashSet<GridNodeBase>();

	public AbilityData Ability { get; }

	public int BurstIndex { get; }

	public RuleCalculateHitChances HitChanceRule { get; }

	public RulePerformBodyPartHitRoll RollPerformBodyPartHitRule { get; }

	public RulePerformDefenceRoll RollPerformDefenceRule { get; }

	public bool IsControlledScatterAutoMiss { get; set; }

	public bool IsOverpenetration { get; private set; }

	public bool CanApplyCriticalEffect { get; set; }

	public bool OverrideAttackHitPolicy { get; set; }

	public AttackHitPolicyType AttackHitPolicyType { get; set; }

	public RuleRollChance ResultChanceRule { get; private set; }

	public RuleRollD100 ResultOverpenetrationRoll { get; private set; }

	public StatQueryOutput ResultTargetDefenceOutput { get; private set; }

	public int ResultTargetDefenceBase { get; private set; }

	public AttackResult Result { get; private set; }

	public bool ResultIsHit
	{
		get
		{
			AttackResult result = Result;
			return result == AttackResult.Hit || result == AttackResult.CoverHit;
		}
	}

	public bool ResultIsCoverHit => Result == AttackResult.CoverHit;

	[CanBeNull]
	public MechanicEntity ResultCoverEntity
	{
		get
		{
			if (Result != AttackResult.CoverHit)
			{
				return null;
			}
			return HitChanceRule.ResultCoverEntity;
		}
	}

	public BlueprintBodyPart ResultHitLocation => RollPerformBodyPartHitRule.ResultHitLocation;

	public bool IsMelee => Ability.IsMelee;

	public bool MissCausedByCritOnSelf { get; private set; }

	int IRuleWithChanceRoll.Chance => HitChanceRule.ResultHitChance;

	StatType? IRuleWithChanceRoll.Stat => HitChanceRule.ResultAttackStatType;

	MechanicEntity IRuleWithChanceRoll.AttackInitiator => base.Self;

	ChanceRollType IRuleWithChanceRoll.RollType => ChanceRollType.Attack;

	public RulePerformAttackRoll([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] AbilityData ability, int burstIndex, Vector3? effectiveCasterPosition, Vector3? abilityTargetPosition)
		: base(initiator, target)
	{
		Ability = ability;
		BurstIndex = burstIndex;
		HitChanceRule = new RuleCalculateHitChances(initiator, target, ability, burstIndex, effectiveCasterPosition ?? initiator.Position, abilityTargetPosition ?? target.Position);
		RollPerformBodyPartHitRule = new RulePerformBodyPartHitRoll(target);
		RollPerformDefenceRule = new RulePerformDefenceRoll(initiator, target, ability);
	}

	public RulePerformAttackRoll([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, [NotNull] AbilityData ability, int burstIndex, Vector3? effectiveCasterPosition, Vector3? abilityTargetPosition)
		: this((MechanicEntity)initiator, (MechanicEntity)target, ability, burstIndex, effectiveCasterPosition, abilityTargetPosition)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		ProcessTargetStats();
		Rulebook.Trigger(HitChanceRule);
		Rulebook.Trigger(RollPerformBodyPartHitRule);
		AttackHitPolicyType attackHitPolicyType = (OverrideAttackHitPolicy ? AttackHitPolicyType : (IsControlledScatterAutoMiss ? AttackHitPolicyType.AutoMiss : ((IsOverpenetration || (!Game.Instance.Controllers.TurnController.TurnBasedModeActive && !(Target is BaseUnitEntity))) ? AttackHitPolicyType.AutoHit : AttackHitPolicyContextData.Current)));
		HitChanceValueModifiers.CopyFrom(HitChanceRule.Modifiers);
		ResultChanceRule = RuleRollChance.Roll(this);
		bool flag = attackHitPolicyType switch
		{
			AttackHitPolicyType.Default => ResultChanceRule.Success, 
			AttackHitPolicyType.AutoHit => true, 
			AttackHitPolicyType.AutoMiss => false, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		bool flag2 = HitChanceRule.ResultLos != 0 && (ResultHitLocation.ReplaceableByCover || HitChanceRule.ResultForceCoverHit);
		if (flag)
		{
			bool flag3 = Ability.IsAoe && (Ability.IsRanged || Ability.IsThrow);
			if (!HitChanceRule.IsAutoHit && !flag2 && !base.Initiator.Features.IgnoreDefence && (!flag3 || (bool)Target.Features.AllowDefenceAgainstRangedAttacksWithAoe))
			{
				if (Ability.IsMelee && base.Initiator.IsAlly(Target) && Target.HasMechanicFeature(MechanicsFeatureType.AutoDefenceAgainstAllyMeleeAttacks))
				{
					RollPerformDefenceRule.OverridenResult = true;
				}
				else if (Target.HasMechanicFeature(MechanicsFeatureType.DefenceDisabled))
				{
					RollPerformDefenceRule.OverridenResult = false;
				}
				Rulebook.Trigger(RollPerformDefenceRule);
				if (!RollPerformDefenceRule.IsDefended)
				{
					ResultOverpenetrationRoll = RulebookEvent.RollD100();
					IsOverpenetration = ResultOverpenetrationRoll.Result <= Ability.GetWeaponStats(base.Self, Target).ResultOverpenetrationChance();
				}
			}
		}
		else if (HitChanceRule.ResultHitChanceWithoutCritEffects > HitChanceRule.ResultHitChance && ResultChanceRule.Result <= HitChanceRule.ResultHitChanceWithoutCritEffects)
		{
			MissCausedByCritOnSelf = true;
		}
		Result = CalculateResult(flag, flag2, RollPerformDefenceRule.IsDefended);
	}

	private void ProcessTargetStats()
	{
		ResultTargetDefenceOutput = new StatQueryOutput();
		StatContext ctx = new StatContext(null, base.Initiator?.Actor);
		Target.Actor.GetStat(StatType.Defence, ResultTargetDefenceOutput, ctx, "ProcessTargetStats");
		ResultTargetDefenceBase = Target.Actor.GetStatBase(StatType.Defence);
	}

	private static AttackResult CalculateResult(bool isHit, bool isCoverHit, bool isDefended)
	{
		if (!isHit)
		{
			return AttackResult.Miss;
		}
		if (!isCoverHit)
		{
			if (!isDefended)
			{
				return AttackResult.Hit;
			}
			return AttackResult.Defended;
		}
		return AttackResult.CoverHit;
	}
}
