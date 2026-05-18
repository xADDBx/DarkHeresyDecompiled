using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Framework.ContextContract;
using Kingmaker.Framework.Mechanics;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

[RuleRoles(Initiator = "attacker", Target = "defender")]
public class RulePerformAttack : RulebookTargetEvent
{
	private readonly bool m_DisableWeaponAttackDamage;

	private readonly bool m_DisableDodgeForAlly;

	private int _damageCount;

	[NotNull]
	public AbilityData Ability { get; }

	public RulePerformAttackRoll RollPerformAttackRule { get; }

	public RulePerformBodyPartHitRoll RollPerformBodyPartHitRule => RollPerformAttackRule.RollPerformBodyPartHitRule;

	public RulePerformDefenceRoll RollPerformDefenceRule => RollPerformAttackRule.RollPerformDefenceRule;

	public RuleRollDamage RuleRollDamage { get; private set; }

	public bool FromOverpenetration { get; set; }

	public Projectile Projectile { get; set; }

	public int AdditionalDamageInstancesCount { get; set; }

	[CanBeNull]
	public RuleDealDamage ResultDamageRule { get; private set; }

	[CanBeNull]
	public RuleDealDamage[] ResultAdditionalDamageRules { get; private set; }

	public int ResultDamageValue
	{
		get
		{
			RuleDealDamage[] resultAdditionalDamageRules = ResultAdditionalDamageRules;
			if (resultAdditionalDamageRules == null || resultAdditionalDamageRules.Length <= 0)
			{
				return ResultDamageRule?.ResultValue ?? 0;
			}
			return (ResultDamageRule?.ResultValue ?? 0) + ResultAdditionalDamageRules.Sum((RuleDealDamage i) => i.ResultValue);
		}
	}

	public AttackResult Result => RollPerformAttackRule.Result;

	public bool ResultIsHit => RollPerformAttackRule.ResultIsHit;

	public bool ResultIsCoverHit => RollPerformAttackRule.ResultIsCoverHit;

	[CanBeNull]
	public MechanicEntity ResultCoverEntity => RollPerformAttackRule.ResultCoverEntity;

	public BlueprintBodyPart ResultHitLocation => RollPerformAttackRule.ResultHitLocation;

	public LosCalculations.CoverType ResultLos => RollPerformAttackRule.HitChanceRule.ResultLos;

	public bool ResultForceCoverHit => RollPerformAttackRule.HitChanceRule.ResultForceCoverHit;

	public Modifier? ResultForcedCoverReason => RollPerformAttackRule.HitChanceRule.ResultForceCoverHitReason;

	public int BurstIndex => RollPerformAttackRule.BurstIndex;

	public bool IsMelee => Ability.Weapon?.Blueprint.IsMelee ?? false;

	public override AbilityData MaybeAbility => Ability;

	public RulePerformAttack([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] AbilityData ability, int burstIndex, bool disableWeaponAttackDamage, bool disableDefenceForAlly, [CanBeNull] RulePerformAttackRoll performAttackRoll)
		: base(initiator, target)
	{
		Ability = ability;
		RollPerformAttackRule = performAttackRoll ?? new RulePerformAttackRoll(initiator, target, ability, burstIndex, null, null);
		m_DisableWeaponAttackDamage = disableWeaponAttackDamage;
	}

	public RulePerformAttack([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] AbilityData ability, int burstIndex, bool disableWeaponAttackDamage = false, bool disableDefenceForAlly = false, Vector3? effectiveCasterPosition = null, Vector3? abilityTargetPosition = null, bool useSpecificAttackHitPolicy = false, AttackHitPolicyType attackHitPolicy = AttackHitPolicyType.Default)
		: base(initiator, target)
	{
		Ability = ability;
		RollPerformAttackRule = new RulePerformAttackRoll(initiator, target, ability, burstIndex, effectiveCasterPosition, abilityTargetPosition)
		{
			OverrideAttackHitPolicy = useSpecificAttackHitPolicy,
			AttackHitPolicyType = attackHitPolicy
		};
		m_DisableWeaponAttackDamage = disableWeaponAttackDamage;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Rulebook.Trigger(RollPerformAttackRule);
		if (RollPerformAttackRule.ResultIsHit)
		{
			OnHit();
		}
		EventBus.RaiseEvent(delegate(IWarhammerAttackHandler h)
		{
			h.HandleAttack(this);
		});
	}

	private void OnHit()
	{
		if (!m_DisableWeaponAttackDamage)
		{
			ResultDamageRule = DealDamage();
		}
		List<RuleDealDamage> list = TempList.Get<RuleDealDamage>();
		for (int i = 0; i < AdditionalDamageInstancesCount; i++)
		{
			list.Add(DealDamage());
		}
		if (list.Count > 0)
		{
			ResultAdditionalDamageRules = list.ToArray();
		}
		if (!Ability.IsPrecise)
		{
			return;
		}
		ContextActionsList maybeBlueprint = ResultHitLocation.ActionsOnPreciseAttackHit.MaybeBlueprint;
		if (maybeBlueprint != null)
		{
			using (EvalContext.Current.PushTarget(Target))
			{
				maybeBlueprint.Run();
			}
		}
	}

	[NotNull]
	private RuleDealDamage DealDamage()
	{
		RuleCalculateDamage ruleCalculateDamage = new RuleCalculateDamage(base.ConcreteInitiator, base.ConcreteTarget, Ability, RollPerformAttackRule, null, ResultHitLocation);
		Rulebook.Trigger(ruleCalculateDamage);
		IntermediateDamage resultDamage = ruleCalculateDamage.ResultDamage;
		if (AdditionalDamageInstancesCount > 0)
		{
			int value = Mathf.RoundToInt(1f / (float)(AdditionalDamageInstancesCount + 1) * 100f);
			resultDamage.Modifiers.Add(ModifierType.PctMul_Extra, value, this, ModifierDescriptor.Weapon);
		}
		resultDamage.CanApplyCriticalEffects = RollPerformAttackRule.CanApplyCriticalEffect && _damageCount++ == 0;
		RuleRollDamage = new RuleRollDamage(base.ConcreteInitiator, base.ConcreteTarget, resultDamage);
		RuleDealDamage obj = new RuleDealDamage(base.ConcreteInitiator, base.ConcreteTarget, RuleRollDamage)
		{
			DisableGameLog = false,
			FromRuleWarhammerAttackRoll = true,
			SourceAbility = Ability,
			Projectile = Projectile
		};
		Rulebook.Trigger(obj);
		return obj;
	}
}
