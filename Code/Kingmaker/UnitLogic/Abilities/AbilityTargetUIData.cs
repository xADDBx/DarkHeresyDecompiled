using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Utility.Damage;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.RuleBurst;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities;

public struct AbilityTargetUIData : IEquatable<AbilityTargetUIData>
{
	private bool m_HasCustomDirectMovement;

	private ItemEntityWeapon Weapon { get; }

	private MechanicEntity PointerTarget { get; }

	private IReadOnlyList<MechanicEntity> TargetsInPattern { get; }

	private Vector3 CasterPosition { get; }

	private BlueprintBodyPart BodyPart { get; }

	public AbilityData Ability { get; }

	public MechanicEntity Target { get; }

	public UIHitChancePredictionData HitChance { get; private set; }

	public UIDamagePredictionData Damage { get; private set; }

	public UIHealPredictionData Heal { get; private set; }

	public UIMoralePredictionData Morale { get; private set; }

	public int AttacksCount { get; }

	public bool CanTargetFromDesiredPosition { get; }

	public string UnavailabilityReason { get; private set; }

	public AbilityTargetUIData(AbilityData ability, Vector3 casterPosition, MechanicEntity target, BlueprintBodyPart bodyPart, MechanicEntity pointerTarget, IReadOnlyList<MechanicEntity> targetsInPattern)
	{
		using (ProfileScope.New("new AbilityTargetUIData"))
		{
			using (ContextData<DisableStatefulRandomContext>.Request())
			{
				m_HasCustomDirectMovement = ability.HasCustomDirectMovement();
				Ability = ability.Clone();
				CasterPosition = casterPosition;
				Target = target;
				BodyPart = bodyPart;
				PointerTarget = pointerTarget;
				TargetsInPattern = targetsInPattern;
				HitChance = default(UIHitChancePredictionData);
				Damage = default(UIDamagePredictionData);
				Heal = default(UIHealPredictionData);
				Morale = default(UIMoralePredictionData);
				AttacksCount = Ability.BurstAttacksCount;
				Ability.PreciseBodyPart = bodyPart;
				Weapon = GetAbilityCalculatableWeapon(Ability, Target, m_HasCustomDirectMovement);
				CanTargetFromDesiredPosition = ability.CanTargetFromDesiredPosition(target, out var unavailabilityReason);
				UnavailabilityReason = (unavailabilityReason.HasValue ? ability.GetUnavailabilityReasonString(unavailabilityReason.Value, casterPosition, target) : ((string)LocalizedTexts.Instance.Reasons.UnavailableGeneric));
				if (m_HasCustomDirectMovement)
				{
					GetChargeAttackData();
				}
				else
				{
					GetAttackData();
				}
			}
		}
	}

	public void UpdateSingleAttack()
	{
		ItemEntityWeapon abilityCalculatableWeapon = GetAbilityCalculatableWeapon(Ability, Target, m_HasCustomDirectMovement);
		if (Target != null && abilityCalculatableWeapon != null)
		{
			HitChance = GetSingleShotHitChanceData(Ability, CasterPosition, Target, null, abilityCalculatableWeapon);
		}
		Damage = GetDamage(Ability, CasterPosition, Target);
	}

	private void GetAttackData()
	{
		if (Ability.IsBurstAttack)
		{
			using (ProfileScope.New("GetBurstHitChanceData"))
			{
				HitChance = GetBurstHitChanceData(Ability, CasterPosition, Target, PointerTarget, TargetsInPattern);
			}
		}
		else if (Weapon != null && Target != null && Ability.HasAttackTypeProvider())
		{
			using (ProfileScope.New("GetSingleShotHitChanceData"))
			{
				HitChance = GetSingleShotHitChanceData(Ability, CasterPosition, Target, PointerTarget, Weapon);
			}
		}
		else
		{
			using (ProfileScope.New("GetAbilityHitChanceData"))
			{
				HitChance = GetAbilityHitChanceData(Ability, CasterPosition, Target, Weapon);
			}
		}
		using (ProfileScope.New("Update Morale Change"))
		{
			Morale = GetMorale(Ability, CasterPosition, Target);
		}
		if (Ability.IsHeal)
		{
			using (ProfileScope.New("Update Heal"))
			{
				Heal = GetHeal(Ability, Target);
				return;
			}
		}
		using (ProfileScope.New("Update Damage"))
		{
			Damage = GetDamage(Ability, CasterPosition, Target);
		}
	}

	private void GetChargeAttackData()
	{
		if (!CanTargetFromDesiredPosition)
		{
			return;
		}
		AbilityData ability = Weapon?.Abilities[0]?.Data;
		using (ProfileScope.New("GetChargeHitChanceData"))
		{
			HitChance = GetSingleShotHitChanceData(ability, CasterPosition, Target, PointerTarget, Weapon);
		}
		using (ProfileScope.New("Update Charge Damage"))
		{
			Damage = GetDamage(ability, CasterPosition, Target);
		}
	}

	private UIHitChancePredictionData GetAbilityHitChanceData(AbilityData ability, Vector3 casterPosition, MechanicEntity target, ItemEntityWeapon weapon)
	{
		UIHitChancePredictionData result = default(UIHitChancePredictionData);
		bool flag = Ability.IsValid(target, casterPosition);
		float num = (flag ? 100f : (-1f));
		result.HitAlways = flag;
		result.InitialHitChance = num;
		result.HitWithAvoidanceChance = num;
		return result;
	}

	private UIHitChancePredictionData GetBurstHitChanceData(AbilityData ability, Vector3 casterPosition, MechanicEntity target, MechanicEntity pointerTarget, IEnumerable<MechanicEntity> targetsInPattern)
	{
		UIHitChancePredictionData result = default(UIHitChancePredictionData);
		bool flag = pointerTarget == target;
		List<MechanicEntity> list = TempList.Get<MechanicEntity>();
		foreach (MechanicEntity item in targetsInPattern)
		{
			if (pointerTarget != item)
			{
				list.Add(item);
			}
		}
		RuleCalculateHitChances ruleCalculateHitChances = Rulebook.Trigger(new RuleCalculateHitChances(ability.Caster, target, ability, 0, casterPosition, target.Position));
		RuleCalculateDefence ruleCalculateDefence = Rulebook.Trigger(new RuleCalculateDefence(ability.Caster, target));
		int resultHitChance = ruleCalculateHitChances.ResultHitChance;
		result.CoverChance = CalculateCoverHitChance(ruleCalculateHitChances.ResultLos, target);
		result.DefenceChance = ruleCalculateDefence.ResultDefence;
		if (flag)
		{
			result.InitialHitChance = resultHitChance;
		}
		else if (list != null)
		{
			RuleCalculateTargetInBurst ruleCalculateTargetInBurst = Rulebook.Trigger(RuleCalculateTargetInBurst.Setup(ability.Caster).WithTargets(list).Create());
			ruleCalculateTargetInBurst.UnitsWeights.TryGetValue(target, out var value);
			float num = (float)value / (float)ruleCalculateTargetInBurst.TotalWeight;
			result.InitialHitChance = UtilityMath.ToPercent(UtilityMath.ToFraction(resultHitChance) * num);
		}
		result.HitWithAvoidanceChance = CalculateHitChanceWithAvoidance(result.InitialHitChance, result.DefenceChance, result.CoverChance, result.OverpenetraionChance);
		return result;
	}

	public static int CalculateCoverHitChance(LosCalculations.CoverType los, MechanicEntity entity)
	{
		if (los != LosCalculations.CoverType.Cover)
		{
			return 0;
		}
		int num = 0;
		foreach (BlueprintBodyPart bodyPart in entity.BodyParts)
		{
			if (bodyPart.CanBeHitRandomly && bodyPart.ReplaceableByCover)
			{
				num += bodyPart.HitChance;
			}
		}
		return num;
	}

	private UIHitChancePredictionData GetSingleShotHitChanceData(AbilityData ability, Vector3 casterPosition, MechanicEntity target, MechanicEntity pointerTarget, ItemEntityWeapon weapon)
	{
		UIHitChancePredictionData result = default(UIHitChancePredictionData);
		MechanicEntity caster = ability.Caster;
		bool flag = pointerTarget == target;
		bool isCoverIgnore = ability.Blueprint.IsCoverIgnore;
		bool flag2 = target is DestructibleEntity;
		Vector3 bestShootingPosition = LosCalculations.GetBestShootingPosition(casterPosition + LosCalculations.EyeShift, caster.SizeRect, target.Position, target.SizeRect);
		RuleCalculateHitChances evt = new RuleCalculateHitChances(caster, target, ability, 0, bestShootingPosition, target.Position);
		evt = Rulebook.Trigger(evt);
		result.CoverChance = GetCoverChance(ability, caster, casterPosition, bestShootingPosition, target, out var isLosBlocker, out var isCover2);
		result.HitAlways = ability.Blueprint.IsRangedAoE && (flag2 || !isCover2 || isCoverIgnore) && !isLosBlocker;
		result.DefenceChance = GetDefenceChance(ability, target, caster);
		result.CriticalEffectsAvoidanceChance = GetCriticalEffectsAvoidanceChance(evt);
		result.CasterHasCriticalEffects = result.CriticalEffectsAvoidanceChance != 0f;
		if (!ability.Blueprint.IsRangedAoE && !flag)
		{
			result.OverpenetraionChance = GetOverpenetrationHitChance(ability, casterPosition, target, pointerTarget, weapon);
		}
		if (isLosBlocker)
		{
			return result;
		}
		int num = GetHitChanceWithModifiers(evt.ResultHitChance, isCoverIgnore, isCover2, result.HitAlways);
		result.InitialHitChance = evt.ResultHitChanceWithoutCritEffects;
		result.HitWithAvoidanceChance = CalculateHitChanceWithAvoidance(num, result.DefenceChance, result.CoverChance, result.OverpenetraionChance);
		return result;
		static int GetHitChanceWithModifiers(int chance, bool ignoreCover, bool isCover, bool hitAlways)
		{
			if (isCover && hitAlways && !ignoreCover)
			{
				return 0;
			}
			if (!hitAlways)
			{
				return Math.Max(chance, 0);
			}
			return 100;
		}
	}

	private float GetOverpenetrationHitChance(AbilityData ability, Vector3 casterPosition, MechanicEntity target, MechanicEntity pointerTarget, ItemEntityWeapon weapon)
	{
		if (pointerTarget == null)
		{
			return 0f;
		}
		return UtilityMath.ToPercent(UtilityMath.ToFraction(GetSingleShotHitChanceData(ability, casterPosition, pointerTarget, pointerTarget, weapon).HitWithAvoidanceChance) * UtilityMath.ToFraction(Ability.Weapon?.Blueprint._OverpenetrationChance ?? 0));
	}

	public static int CalculateHitChanceWithAvoidance(float initialHitChance, float defenceChance, float coverChance, float overpenetraionChance)
	{
		return UtilityMath.ToPercent(UtilityMath.ToFraction(initialHitChance) * UtilityMath.ToFraction(100f - defenceChance) * UtilityMath.ToFraction(100f - coverChance) * ((overpenetraionChance > 0f) ? UtilityMath.ToFraction(overpenetraionChance) : 1f));
	}

	private static float GetCriticalEffectsAvoidanceChance(RuleCalculateHitChances hitChanceRule)
	{
		float num = Math.Max(hitChanceRule.ResultHitChance, 0);
		return (float)Math.Max(hitChanceRule.ResultHitChanceWithoutCritEffects, 0) - num;
	}

	private static int GetDamageReduction(MechanicEntity target)
	{
		ModifiableValue statOptional = target.GetStatOptional(StatType.ArmorDamageReduction);
		if (statOptional != null)
		{
			return statOptional;
		}
		return 0;
	}

	private float GetDefenceChance(AbilityData ability, MechanicEntity target, MechanicEntity caster)
	{
		if (ability.Blueprint.IsRangedAoE)
		{
			return 0f;
		}
		return Rulebook.Trigger(new RuleCalculateDefence(caster, target)).ResultDefence;
	}

	private float GetCoverChance(AbilityData ability, MechanicEntity caster, Vector3 casterPosition, Vector3 castPosition, MechanicEntity target, out bool isLosBlocker, out bool isCover)
	{
		LosCalculations.CoverType warhammerLos = LosCalculations.GetWarhammerLos(caster, casterPosition, target);
		LosDescription warhammerLos2 = LosCalculations.GetWarhammerLos(castPosition, caster.SizeRect, target.Position, target.SizeRect);
		LosCalculations.CoverType coverType = LosCalculations.GetWarhammerLos(castPosition, caster.SizeRect, target).CoverType;
		isLosBlocker = warhammerLos == LosCalculations.CoverType.LosBlocker && (LosCalculations.CoverType)warhammerLos2 == LosCalculations.CoverType.LosBlocker;
		isCover = coverType == LosCalculations.CoverType.Cover;
		if (ability.Blueprint.IsRangedAoE && coverType == LosCalculations.CoverType.Cover)
		{
			if (ability.Blueprint.IsCoverIgnore && !(target is DestructibleEntity))
			{
				return 0f;
			}
			return 100f;
		}
		return (coverType == LosCalculations.CoverType.Cover && !ability.IsPrecise) ? CalculateCoverHitChance(coverType, caster) : 0;
	}

	private UIDamagePredictionData GetDamage(AbilityData ability, Vector3 casterPosition, MechanicEntity target)
	{
		IntermediateDamage intermediateDamage = ((ability.Weapon != null) ? ability.GetWeaponDamageData(target) : null);
		if (intermediateDamage == null)
		{
			return GetWeaponlessDamage(ability, casterPosition, target);
		}
		int damageReduction = GetDamageReduction(target);
		RolledDamage rolledDamage = new RolledDamage(ability.Caster, Target, intermediateDamage, 0);
		RolledDamage rolledDamage2 = new RolledDamage(ability.Caster, Target, intermediateDamage, 100);
		int num = target.GetOptional<PartArmor>()?.DurabilityLeft ?? 0;
		int maxDamage = rolledDamage2.ResultDamageValue * AttacksCount;
		(int healthDamage, int armorDamage) tuple = CalculateMaxDamage(num, maxDamage, intermediateDamage.ApplyStrategy);
		int item = tuple.healthDamage;
		int item2 = tuple.armorDamage;
		int num2 = CalculateVitalDamage(rolledDamage2.ResultVitalDamageValue, item > 0, rolledDamage2.ApplyVitalStrategyValue);
		bool flag = intermediateDamage.BodyPart?.IsVital ?? false;
		bool vitalDamageLockedByArmor = num2 == 0 && flag && rolledDamage2.ResultVitalDamageValue != 0 && rolledDamage2.ApplyVitalStrategyValue == VitalDamageStrategy.Default && num > 0;
		bool vitalDamageLockedByStrategy = num2 == 0 && flag && rolledDamage2.ApplyVitalStrategyValue == VitalDamageStrategy.Never;
		int baseDamageModifiers = rolledDamage2.ResultDamageValue - num2 - intermediateDamage.MaxValueBase;
		UIDamagePredictionData result = default(UIDamagePredictionData);
		result.MinDamagePerAttack = rolledDamage.ResultDamageValue;
		result.MaxDamagePerAttack = rolledDamage2.ResultDamageValue;
		result.BaseMinDamage = intermediateDamage.MinValueBase;
		result.BaseMaxDamage = intermediateDamage.MaxValueBase;
		result.BaseDamageModifiers = baseDamageModifiers;
		result.ArmorMaxDamage = item2;
		result.HealthMaxDamage = item;
		result.VitalDamage = num2;
		result.VitalDamageLockedByArmor = vitalDamageLockedByArmor;
		result.VitalDamageLockedByStrategy = vitalDamageLockedByStrategy;
		result.HPDamageBonus = intermediateDamage.BonusDamageToHealth;
		result.ArmorDamageBonus = intermediateDamage.ArmorDamageModifiers.Value;
		result.DamageReduction = damageReduction;
		return result;
	}

	private UIDamagePredictionData GetWeaponlessDamage(AbilityData ability, Vector3 casterPosition, MechanicEntity target)
	{
		DamagePredictionData damagePredictionData = AbilityDataHelper.ApplyActionEffects(new DamagePredictionData(), ability, target, casterPosition, null);
		int damageReduction = GetDamageReduction(target);
		int armorDurability = target.GetOptional<PartArmor>()?.DurabilityLeft ?? 0;
		int maxDamage = damagePredictionData.MaxDamage * AttacksCount;
		(int healthDamage, int armorDamage) tuple = CalculateMaxDamage(armorDurability, maxDamage, DamageStrategy.Default);
		int item = tuple.healthDamage;
		int item2 = tuple.armorDamage;
		UIDamagePredictionData result = default(UIDamagePredictionData);
		result.MinDamagePerAttack = damagePredictionData.MinDamage;
		result.MaxDamagePerAttack = damagePredictionData.MaxDamage;
		result.ArmorMaxDamage = item2;
		result.HealthMaxDamage = item;
		result.VitalDamage = damagePredictionData.VitalDamage;
		result.HPDamageBonus = damagePredictionData.HPDamageBonus;
		result.ArmorDamageBonus = damagePredictionData.ArmorDamageBonus;
		result.DamageReduction = damageReduction;
		return result;
	}

	private (int healthDamage, int armorDamage) CalculateMaxDamage(int armorDurability, int maxDamage, DamageStrategy strategy)
	{
		int num = 0;
		int item = 0;
		switch (strategy)
		{
		case DamageStrategy.ArmorOnly:
			item = maxDamage;
			break;
		case DamageStrategy.HealthOnly:
			num = maxDamage;
			break;
		default:
			num = Mathf.Max(0, maxDamage - armorDurability);
			item = Mathf.Max(0, maxDamage - num);
			break;
		}
		return (healthDamage: num, armorDamage: item);
	}

	private int CalculateVitalDamage(int vitalDamage, bool hasHealthDamage, VitalDamageStrategy strategy)
	{
		switch (strategy)
		{
		case VitalDamageStrategy.Never:
			return 0;
		case VitalDamageStrategy.Always:
			return vitalDamage;
		default:
			if (!hasHealthDamage)
			{
				return 0;
			}
			return vitalDamage;
		}
	}

	private UIHealPredictionData GetHeal(AbilityData ability, MechanicEntity target)
	{
		UIHealPredictionData result = default(UIHealPredictionData);
		HealPredictionData healPrediction = ability.GetHealPrediction(target);
		result.Bonus = healPrediction?.Bonus ?? 0;
		result.MinHeal = healPrediction?.MinValue ?? 0;
		result.MaxHeal = healPrediction?.MaxValue ?? 0;
		result.HealStrategy = healPrediction?.HealStrategy ?? DamageStrategy.Default;
		return result;
	}

	private UIMoralePredictionData GetMorale(AbilityData ability, Vector3 casterPosition, MechanicEntity target)
	{
		MoralePredictionRange moralePrediction = ability.GetMoralePrediction(target, casterPosition);
		UIMoralePredictionData result = default(UIMoralePredictionData);
		result.MinDelta = moralePrediction.MinDelta;
		result.MaxDelta = moralePrediction.MaxDelta;
		return result;
	}

	private static ItemEntityWeapon GetAbilityCalculatableWeapon(AbilityData ability, MechanicEntity target, bool hasCustomDirectMovement)
	{
		if (ability.Weapon == null && hasCustomDirectMovement)
		{
			ability.OverrideWeapon = ability.GetWeaponForCharge(target);
		}
		return ability.Weapon;
	}

	public bool Equals(AbilityTargetUIData other)
	{
		if (object.Equals(Ability, other.Ability) && object.Equals(BodyPart, other.BodyPart) && object.Equals(Target, other.Target) && CasterPosition.Equals(other.CasterPosition) && HitChance.Equals(other.HitChance) && Damage.Equals(other.Damage))
		{
			return Morale.Equals(other.Morale);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is AbilityTargetUIData other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Ability, Target, CasterPosition, BodyPart);
	}

	public static bool operator ==(AbilityTargetUIData left, AbilityTargetUIData right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(AbilityTargetUIData left, AbilityTargetUIData right)
	{
		return !left.Equals(right);
	}
}
