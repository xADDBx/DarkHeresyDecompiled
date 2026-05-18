using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Abilities;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Framework.Mechanics.Utility.Damage;
using Kingmaker.Pathfinding;
using Kingmaker.Predictions;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Utility;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.PatternAttack;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[TypeId("e6da52cf4bc945a1b3276a25991d7a68")]
public class AbilityAttackDelivery : AbilityCustomLogic, IAbilityPrediction, IAbilityAoEPatternProviderHolder, IAbilityAttackTypeProvider
{
	[SerializeField]
	private AbilityAttackType AbilityAttack;

	[SerializeField]
	private bool m_OverrideAttackStatType;

	[SerializeField]
	[ShowIf("m_OverrideAttackStatType")]
	private AttributeType m_AttackStatType;

	[Tooltip("If any shot of the attack would hit an ally, that shot instead misses everyone")]
	[SerializeField]
	[ShowIf("IsBurst")]
	private bool ControlledBurst;

	[SerializeField]
	[ShowIf("IsRangedAoe")]
	private bool m_PatternSpreadWithProjectile;

	[SerializeField]
	[ShowIf("CanBePrecise")]
	private bool m_PreciseAttack;

	[InfoBox("Applies damage (1 + AdditionalDamageInstancesCount) times to each target")]
	[SerializeField]
	[ShowIf("IsRangedAoe")]
	public int AdditionalDamageInstancesCount;

	public bool UseBestShootingPosition;

	[SerializeField]
	[ShowIf("ShowPatternSettings")]
	private AbilityAoEPatternSettings m_PatternSettings;

	[SerializeField]
	[ShowIf("IsAoe")]
	private AbilitySpawnAreaEffectSettings m_SpawnAreaEffect = new AbilitySpawnAreaEffectSettings();

	[SerializeField]
	[ShowIf("IsWeaponAttack")]
	private bool m_DisableWeaponAttackDamage;

	[SerializeField]
	[ShowIf("IsWeaponAttack")]
	private bool m_DisableDodgeForAlly;

	[SerializeField]
	[ShowIf("IsWeaponAttack")]
	private bool m_DisableOverpenetration;

	[SerializeField]
	[ShowIf("IsWeaponAttack")]
	private bool m_AutoHit;

	[CanBeNull]
	private BurstPattern m_BurstPatternCached;

	[SerializeField]
	[ShowIf("IsBurst")]
	private bool m_IgnoreLos;

	[SerializeField]
	[ShowIf("IsBurst")]
	private bool m_IgnoreLevelDifference;

	[SerializeField]
	[ShowIf("IsBurst")]
	private int m_PatternAngle = 30;

	public AbilityAttackType AttackType => AbilityAttack;

	public bool IsMelee
	{
		get
		{
			AbilityAttackType abilityAttack = AbilityAttack;
			return abilityAttack == AbilityAttackType.SingleMelee || abilityAttack == AbilityAttackType.AoeMelee;
		}
	}

	public bool IsRanged
	{
		get
		{
			AbilityAttackType abilityAttack = AbilityAttack;
			return abilityAttack == AbilityAttackType.SingleRanged || abilityAttack == AbilityAttackType.BurstRanged || abilityAttack == AbilityAttackType.AoeRanged;
		}
	}

	public bool IsThrow => AbilityAttack == AbilityAttackType.AoeThrow;

	public bool IsSingle
	{
		get
		{
			AbilityAttackType abilityAttack = AbilityAttack;
			return abilityAttack == AbilityAttackType.SingleMelee || abilityAttack == AbilityAttackType.SingleRanged;
		}
	}

	public bool IsBurst => AbilityAttack == AbilityAttackType.BurstRanged;

	public bool IsControlledBurst => ControlledBurst;

	public bool IsAoe
	{
		get
		{
			AbilityAttackType abilityAttack = AbilityAttack;
			return abilityAttack == AbilityAttackType.AoeMelee || abilityAttack == AbilityAttackType.AoeRanged || abilityAttack == AbilityAttackType.AoeThrow;
		}
	}

	public bool IsPrecise
	{
		get
		{
			if (CanBePrecise)
			{
				return m_PreciseAttack;
			}
			return false;
		}
	}

	public bool IsRangedAoe => AbilityAttack == AbilityAttackType.AoeRanged;

	public bool IsRangedBurstOrAoe
	{
		get
		{
			AbilityAttackType abilityAttack = AbilityAttack;
			return abilityAttack == AbilityAttackType.BurstRanged || abilityAttack == AbilityAttackType.AoeRanged;
		}
	}

	public bool IsWeaponAttack => AbilityAttack != AbilityAttackType.AoeThrow;

	public bool PatternSpreadWithProjectile
	{
		get
		{
			if (IsRangedAoe)
			{
				return m_PatternSpreadWithProjectile;
			}
			return false;
		}
	}

	public bool ShowPatternSettings
	{
		get
		{
			if (IsAoe)
			{
				if (m_SpawnAreaEffect.Blueprint != null)
				{
					return m_SpawnAreaEffect.UseAttackPattern;
				}
				return true;
			}
			return false;
		}
	}

	[CanBeNull]
	public IAbilityAoEPatternProvider PatternProvider => GetPattern();

	public bool CanBePrecise
	{
		get
		{
			if (IsSingle)
			{
				return !((BlueprintAbility)base.OwnerBlueprint).CanTargetPoint;
			}
			return false;
		}
	}

	public bool IsRespectCover
	{
		get
		{
			if (IsAoe)
			{
				return m_PatternSettings.RespectCovers;
			}
			return true;
		}
	}

	public StatType? OverrideAttackStatType
	{
		get
		{
			if (!m_OverrideAttackStatType || m_AttackStatType == AttributeType.Unknown)
			{
				return null;
			}
			return m_AttackStatType.ToStatType();
		}
	}

	public void CollectPrediction(AbilityPredictionContext context)
	{
		AbilityPredictionContext context = context;
		if (!IsSingle || IsBurst || IsAoe)
		{
			return;
		}
		AbilityData ability = context.ExecutionContext.Ability;
		MechanicEntity entity = context.ExecutionContext.ClickedTarget.Entity;
		if (entity == null)
		{
			return;
		}
		MechanicEntity target = context.State.GetPredictionEntity(entity);
		if (!ability.IsValidTargetForAttack(target))
		{
			return;
		}
		context.WithTarget(target, delegate
		{
			UIHitChancePredictionData hitChance = CalculateSingleHitChance(ability, target, ability.Caster.Position);
			DamagePredictionData damagePredictionData = CollectDamage(ability, target);
			context.AddDeliveryTarget(new AbilityDeliveryTarget(new TargetWrapper(target)));
			context.RecordHitChance(hitChance);
			if (damagePredictionData != null)
			{
				context.RecordDamage(damagePredictionData);
			}
		});
	}

	private static UIHitChancePredictionData CalculateSingleHitChance(AbilityData ability, MechanicEntity target, Vector3 casterPosition)
	{
		RuleCalculateHitChances ruleCalculateHitChances = Rulebook.Trigger(new RuleCalculateHitChances(ability.Caster, target, ability, 0, casterPosition, target.Position));
		float defenceChance = GetDefenceChance(ability, target, ruleCalculateHitChances.IsAutoHit);
		UIHitChancePredictionData result = default(UIHitChancePredictionData);
		result.InitialHitChance = ruleCalculateHitChances.ResultHitChance;
		result.DefenceChance = defenceChance;
		result.HitWithAvoidanceChance = AbilityDataHelper.CalculateHitChanceWithAvoidance(ruleCalculateHitChances.ResultHitChance, defenceChance, 0f, 0f);
		return result;
	}

	private static DamagePredictionData? CollectDamage(AbilityData ability, MechanicEntity target)
	{
		IntermediateDamage weaponDamageData = ability.GetWeaponDamageData(target);
		if (weaponDamageData == null)
		{
			return null;
		}
		RolledDamage rolledDamage = new RolledDamage(ability.Caster, target, weaponDamageData, 0);
		RolledDamage rolledDamage2 = new RolledDamage(ability.Caster, target, weaponDamageData, 100);
		return new DamagePredictionData
		{
			MinDamage = rolledDamage.ResultDamageValue,
			MaxDamage = rolledDamage2.ResultDamageValue,
			HPDamageBonus = weaponDamageData.BonusDamageToHealth,
			ArmorDamageBonus = weaponDamageData.ArmorDamageModifiers.Value,
			VitalDamage = rolledDamage2.ResultVitalDamageValue
		};
	}

	private static float GetDefenceChance(AbilityData ability, MechanicEntity target, bool isAutoHit)
	{
		if (isAutoHit || ability.Blueprint.IsRangedAoE || (bool)target.Features.DefenceDisabled)
		{
			return 0f;
		}
		StatContext ctx = new StatContext(null, ability.Caster.Actor);
		return target.GetEffectiveDefence(ctx);
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		if (!IsRanged || IsAoe)
		{
			if (!IsAoe)
			{
				return DeliverSingle(context, target);
			}
			return DeliverAoe(context, target);
		}
		return DeliverRanged(context, target);
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	private AbilityProjectileAttack DeliverRanged(AbilityExecutionContext context, TargetWrapper target)
	{
		AbilityProjectileAttack abilityProjectileAttack = ((AbilityProjectileAttack)context.PreparedDeliveryProcess) ?? ((!IsBurst) ? AbilityProjectileAttack.CreateSingleTarget(context, context.Caster.CurrentUnwalkableNode, target.Entity, 1) : (context.Ability.CanTargetPoint ? AbilityProjectileAttack.CreatePatternBurst(context, context.Caster.CurrentUnwalkableNode, target, context.Ability.BurstAttacksCount, ControlledBurst) : AbilityProjectileAttack.CreateBurst(context, context.Caster.CurrentUnwalkableNode, target, context.Ability.BurstAttacksCount, ControlledBurst)));
		if (IsWeaponAttack)
		{
			if (m_AutoHit)
			{
				abilityProjectileAttack.AutoHit();
			}
			if (m_DisableOverpenetration)
			{
				abilityProjectileAttack.DisableOverpenetration();
			}
		}
		else
		{
			abilityProjectileAttack.DisableAttacks();
			abilityProjectileAttack.DisableOverpenetration();
		}
		if (m_DisableWeaponAttackDamage)
		{
			abilityProjectileAttack.DisableWeaponAttackDamage();
		}
		if (m_DisableDodgeForAlly)
		{
			abilityProjectileAttack.DisableDodgeForAlly();
		}
		return abilityProjectileAttack;
	}

	private AbilityAoEPatternAttack DeliverAoe(AbilityExecutionContext context, TargetWrapper target)
	{
		GridNodeBase nearestNodeXZUnwalkable = target.Point.GetNearestNodeXZUnwalkable();
		GridNodeBase gridNodeBase = (UseBestShootingPosition ? LosCalculations.GetBestShootingNode(context.Caster.CurrentUnwalkableNode, context.Caster.SizeRect, nearestNodeXZUnwalkable, target.SizeRect) : context.Caster.CurrentUnwalkableNode);
		GridNodeBase actualCastNode = AoEPatternHelper.GetActualCastNode(context.Caster, gridNodeBase.Vector3Position(), target.Point);
		AbilityAoEPatternAttack abilityAoEPatternAttack = new AbilityAoEPatternAttack(context, this, PartAbilityPatternSettings.GetAbilityPatternSettings(context.Ability, m_PatternSettings), m_SpawnAreaEffect, gridNodeBase, actualCastNode);
		if (m_DisableWeaponAttackDamage)
		{
			abilityAoEPatternAttack.DisableWeaponAttackDamage();
		}
		if (m_DisableDodgeForAlly)
		{
			abilityAoEPatternAttack.DisableDodgeForAlly();
		}
		return abilityAoEPatternAttack;
	}

	private IEnumerator<AbilityDeliveryTarget> DeliverSingle(AbilityExecutionContext context, TargetWrapper target)
	{
		if (target.Entity == null || context.Ability.IsValidTargetForAttack(target.Entity))
		{
			RulePerformAttack rulePerformAttack = null;
			if (target.Entity != null)
			{
				rulePerformAttack = new RulePerformAttack(context.Caster, target.Entity, context.Ability, 0, m_DisableWeaponAttackDamage, m_DisableDodgeForAlly)
				{
					AdditionalDamageInstancesCount = AdditionalDamageInstancesCount
				};
				rulePerformAttack.RollPerformAttackRule.CanApplyCriticalEffect = true;
				Rulebook.Trigger(rulePerformAttack);
			}
			yield return new AbilityDeliveryTarget(target)
			{
				AttackRule = rulePerformAttack
			};
		}
	}

	[CanBeNull]
	private IAbilityAoEPatternProvider GetPattern()
	{
		if (IsAoe)
		{
			BlueprintAreaEffect blueprint = m_SpawnAreaEffect.Blueprint;
			if (blueprint != null && !m_SpawnAreaEffect.UseAttackPattern)
			{
				return blueprint;
			}
			return m_PatternSettings;
		}
		if (IsBurst)
		{
			if (m_BurstPatternCached == null || m_BurstPatternCached.IsIgnoreLos != m_IgnoreLos || m_BurstPatternCached.IsIgnoreLevelDifference != m_IgnoreLevelDifference || m_BurstPatternCached.PatternAngle != m_PatternAngle)
			{
				m_BurstPatternCached = new BurstPattern(m_IgnoreLos, m_IgnoreLevelDifference, m_PatternAngle);
			}
			return m_BurstPatternCached;
		}
		return null;
	}
}
