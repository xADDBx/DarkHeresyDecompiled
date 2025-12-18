using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.PatternAttack;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;
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
public class AbilityAttackDelivery : AbilityCustomLogic, IAbilityAoEPatternProviderHolder, IAbilityAttackTypeProvider
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
		AbilityProjectileAttack abilityProjectileAttack = ((!IsBurst) ? AbilityProjectileAttack.CreateSingleTarget(context, target.Entity, 1) : (context.Ability.CanTargetPoint ? AbilityProjectileAttack.CreatePatternBurst(context, target, context.Ability.BurstAttacksCount, ControlledBurst) : AbilityProjectileAttack.CreateBurst(context, target, context.Ability.BurstAttacksCount, ControlledBurst)));
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
			return m_BurstPatternCached ?? (m_BurstPatternCached = new BurstPattern());
		}
		return null;
	}
}
