using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Framework.Mechanics.Utility.Damage;
using Kingmaker.Gameplay.Components.Damage;
using Kingmaker.Predictions;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.CodeTimer;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Predictions.PredictionProviders;

public class DamagePredictionProvider : IPredictionProvider<UIDamagePredictionData, UIPredictionContext>
{
	private static readonly string m_ProfilerScopeId = "DamagePredictionProvider.Get)";

	private static readonly IReadOnlyList<Modifier> m_EmptyModifiersList;

	public UIDamagePredictionData Get(UIPredictionContext ctx)
	{
		using (ProfileScope.New(m_ProfilerScopeId))
		{
			if (ctx.Ability.IsHeal)
			{
				return default(UIDamagePredictionData);
			}
			return GetDamage(ctx.Ability, ctx.Target, ctx.CasterPosition);
		}
	}

	private UIDamagePredictionData GetDamage(AbilityData ability, MechanicEntity target, Vector3 casterPosition)
	{
		using PredictionHackContext predictionHackContext = ContextData<PredictionHackContext>.Request();
		predictionHackContext.VeilDeltaBeforeCast = ability.GetPredictedVeilDeltaBeforeCast();
		if (ability.HasCustomDirectMovement())
		{
			AbilityData abilityData = ability.GetWeaponForCharge(target)?.Abilities.FirstOrDefault()?.Data;
			return (abilityData != null) ? GetDamage(abilityData, target, casterPosition) : default(UIDamagePredictionData);
		}
		IntermediateDamage intermediateDamage = ((ability.Weapon != null) ? ability.GetWeaponDamageData(target) : null);
		return (intermediateDamage != null) ? GetWeaponDamage(ability, target, casterPosition, intermediateDamage) : GetWeaponlessDamage(ability, target, casterPosition);
	}

	private UIDamagePredictionData GetWeaponDamage(AbilityData ability, MechanicEntity target, Vector3 casterPosition, IntermediateDamage weaponDamage)
	{
		RolledDamage rolledDamage = new RolledDamage(ability.Caster, target, weaponDamage, 0);
		RolledDamage rolledDamage2 = new RolledDamage(ability.Caster, target, weaponDamage, 100);
		DamagePredictionData triggeredDamage = GetTriggeredDamage(ability, target, casterPosition);
		int minDamagePerAttack = rolledDamage.ResultDamageValue + triggeredDamage.MinDamage;
		int num = rolledDamage2.ResultDamageValue + triggeredDamage.MaxDamage;
		int vitalDamage = rolledDamage2.ResultVitalDamageValue + triggeredDamage.VitalDamage;
		int hPDamageBonus = weaponDamage.BonusDamageToHealth + triggeredDamage.HPDamageBonus;
		int armorDamageBonus = weaponDamage.ArmorDamageModifiers.Value + triggeredDamage.ArmorDamageBonus;
		int num2 = target.GetOptional<PartArmor>()?.DurabilityLeft ?? 0;
		int maxDamage = num * ability.BurstAttacksCount;
		(int healthDamage, int armorDamage) tuple = CalculateMaxDamage(num2, maxDamage, weaponDamage.ApplyStrategy);
		int item = tuple.healthDamage;
		int item2 = tuple.armorDamage;
		bool isVitalBodyPart = weaponDamage.BodyPart?.IsVital ?? false;
		(int, VitalDamageResult) tuple2 = CalculateVitalDamage(isVitalBodyPart, vitalDamage, item > 0, num2 > 0, rolledDamage2.ApplyVitalStrategyValue);
		int item3 = tuple2.Item1;
		VitalDamageResult item4 = tuple2.Item2;
		int damageReduction = GetDamageReduction(target, ability.Caster, weaponDamage.Type, weaponDamage.BodyPart);
		UIDamagePredictionData result = default(UIDamagePredictionData);
		result.BaseMinDamage = weaponDamage.MinValueBase;
		result.BaseMaxDamage = weaponDamage.MaxValueBase;
		result.MinDamagePerAttack = minDamagePerAttack;
		result.MaxDamagePerAttack = num;
		result.DamageModifiers = m_EmptyModifiersList;
		result.ArmorMaxDamage = item2;
		result.HealthMaxDamage = item;
		result.VitalDamage = item3;
		result.VitalDamageResult = item4;
		result.HPDamageBonus = hPDamageBonus;
		result.ArmorDamageBonus = armorDamageBonus;
		result.DamageReduction = damageReduction;
		return result;
	}

	private UIDamagePredictionData GetWeaponlessDamage(AbilityData ability, MechanicEntity target, Vector3 casterPosition)
	{
		DamagePredictionData damagePredictionData = AbilityDataHelper.ApplyActionEffects(new DamagePredictionData(), ability, target, casterPosition, null);
		damagePredictionData += GetTriggeredDamage(ability, target, casterPosition);
		int damageReduction = GetDamageReduction(target, ability.Caster);
		int armorDurability = target.GetOptional<PartArmor>()?.DurabilityLeft ?? 0;
		int maxDamage = damagePredictionData.MaxDamage * ability.BurstAttacksCount;
		(int healthDamage, int armorDamage) tuple = CalculateMaxDamage(armorDurability, maxDamage, DamageStrategy.Default);
		int item = tuple.healthDamage;
		int item2 = tuple.armorDamage;
		UIDamagePredictionData result = default(UIDamagePredictionData);
		result.BaseMinDamage = damagePredictionData.MinDamage;
		result.BaseMaxDamage = damagePredictionData.MaxDamage;
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

	private static DamagePredictionData GetTriggeredDamage(AbilityData ability, MechanicEntity target, Vector3 casterPosition)
	{
		DamagePredictionData damage = new DamagePredictionData();
		CollectFromFactComponents<DamageTriggerInitiator>(ability.Caster.Facts);
		CollectFromFactComponents<DamageTriggerTarget>(target.Facts);
		return damage;
		void CollectFromFactComponents<T>(EntityFactsManager facts) where T : DamageTrigger
		{
			foreach (MechanicEntityFact item in facts.GetAll((MechanicEntityFact _) => true))
			{
				MechanicsContext maybeContext = item.MaybeContext;
				if (maybeContext != null)
				{
					foreach (T component in item.GetComponents<T>(null))
					{
						DamagePredictionData actionsDamage = AbilityDataHelper.GetActionsDamage(ability, component.ActionOnTarget, casterPosition, target, maybeContext);
						damage += actionsDamage;
					}
				}
			}
		}
	}

	private static int GetDamageReduction(MechanicEntity target, MechanicEntity caster, DamageType? damageType = null, BlueprintBodyPart bodyPart = null)
	{
		return target.Actor.GetStat(StatType.ArmorDamageReduction, null, new StatContext(null, caster.Actor, null, null, damageType, bodyPart), "GetDamageReduction");
	}

	private static (int healthDamage, int armorDamage) CalculateMaxDamage(int armorDurability, int maxDamage, DamageStrategy strategy)
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

	private (int, VitalDamageResult) CalculateVitalDamage(bool isVitalBodyPart, int vitalDamage, bool hasHealthDamage, bool hasArmorLeft, VitalDamageStrategy strategy)
	{
		if (!isVitalBodyPart || vitalDamage <= 0)
		{
			return (0, VitalDamageResult.None);
		}
		switch (strategy)
		{
		case VitalDamageStrategy.Never:
			return (0, VitalDamageResult.LockedByStrategy);
		case VitalDamageStrategy.Always:
			if (hasHealthDamage)
			{
				return (vitalDamage, VitalDamageResult.Applied);
			}
			break;
		}
		if (!hasHealthDamage || hasArmorLeft)
		{
			return (0, VitalDamageResult.LockedByArmor);
		}
		return (vitalDamage, VitalDamageResult.Applied);
	}
}
