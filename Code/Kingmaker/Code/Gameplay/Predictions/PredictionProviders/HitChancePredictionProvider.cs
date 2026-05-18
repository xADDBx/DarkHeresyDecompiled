using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Predictions;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.RuleBurst;
using Kingmaker.RuleSystem.Rules.Utility;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Predictions.PredictionProviders;

public class HitChancePredictionProvider : IPredictionProvider<UIHitChancePredictionData, UIPredictionContext>
{
	private static readonly string m_ProfilerScopeId = "HitChancePredictionProvider.Get)";

	public UIHitChancePredictionData Get(UIPredictionContext ctx)
	{
		using (ProfileScope.New(m_ProfilerScopeId))
		{
			if (ctx.Target == null)
			{
				return default(UIHitChancePredictionData);
			}
			AbilityData.UnavailabilityReasonType? unavailabilityReason;
			bool canTargetFromDesiredPosition = ctx.Ability.CanTargetFromDesiredPosition(ctx.Target, out unavailabilityReason);
			UIHitChancePredictionData hitChance = GetHitChance(ctx, canTargetFromDesiredPosition);
			hitChance.CanTargetFromDesiredPosition = canTargetFromDesiredPosition;
			return hitChance;
		}
	}

	private UIHitChancePredictionData GetHitChance(UIPredictionContext ctx, bool canTargetFromDesiredPosition)
	{
		AbilityData ability = ctx.Ability;
		if (ability.IsPrecise && ctx.BodyPart == null)
		{
			return default(UIHitChancePredictionData);
		}
		if (ability.HasCustomDirectMovement())
		{
			if (!canTargetFromDesiredPosition)
			{
				return default(UIHitChancePredictionData);
			}
			AbilityData abilityData = ability.GetWeaponForCharge(ctx.Target)?.Abilities.FirstOrDefault()?.Data;
			if (!(abilityData != null))
			{
				return default(UIHitChancePredictionData);
			}
			return GetSingleHitChanceData(abilityData, ctx.CasterPosition, ctx.Target);
		}
		if (ability.IsBurstAttack)
		{
			return GetBurstHitChanceData(ability, ctx.CasterPosition, ctx.Target, ctx.PointerTarget, ctx.TargetsInPattern);
		}
		if (ability.Weapon != null && ability.HasAttackTypeProvider())
		{
			return GetSingleHitChanceData(ability, ctx.CasterPosition, ctx.Target);
		}
		return GetAbilityHitChanceData(ability, ctx.CasterPosition, ctx.Target);
	}

	private UIHitChancePredictionData GetAbilityHitChanceData(AbilityData ability, Vector3 casterPosition, MechanicEntity target)
	{
		bool flag = ability.IsValid(target, casterPosition);
		float num = (flag ? 100f : 0f);
		UIHitChancePredictionData result = default(UIHitChancePredictionData);
		result.HitAlways = flag;
		result.InitialHitChance = num;
		result.HitWithAvoidanceChance = num;
		return result;
	}

	private UIHitChancePredictionData GetSingleHitChanceData(AbilityData ability, Vector3 casterPosition, MechanicEntity target)
	{
		UIHitChancePredictionData result = default(UIHitChancePredictionData);
		MechanicEntity caster = ability.Caster;
		bool isCoverIgnore = ability.Blueprint.IsCoverIgnore;
		bool flag = target is DestructibleEntity;
		Vector3 bestShootingPosition = LosCalculations.GetBestShootingPosition(casterPosition + LosCalculations.EyeShift, caster.SizeRect, target.Position, target.SizeRect);
		RuleCalculateHitChances ruleCalculateHitChances = Rulebook.Trigger(new RuleCalculateHitChances(caster, target, ability, 0, bestShootingPosition, target.Position));
		result.CoverChance = GetCoverChance(ability, casterPosition, bestShootingPosition, target, out var isLosBlocker, out var isCover2);
		result.HitAlways = ability.Blueprint.IsRangedAoE && (flag || !isCover2 || isCoverIgnore) && !isLosBlocker;
		result.DefenceChance = GetDefenceChance(ability, target, ruleCalculateHitChances.IsAutoHit);
		result.CriticalEffectsAvoidanceChance = GetCriticalEffectsAvoidanceChance(ruleCalculateHitChances);
		result.CasterHasCriticalEffects = result.CriticalEffectsAvoidanceChance != 0f;
		if (isLosBlocker)
		{
			return result;
		}
		int num = GetHitChanceWithModifiers(ruleCalculateHitChances.ResultHitChance, isCoverIgnore, isCover2, result.HitAlways);
		result.InitialHitChance = ruleCalculateHitChances.ResultHitChanceWithoutCritEffects;
		result.HitWithAvoidanceChance = AbilityDataHelper.CalculateHitChanceWithAvoidance(num, result.DefenceChance, result.CoverChance, result.OverpenetraionChance);
		result.TargetHasCriticalEffects = IsAffectedByCriticalEffect(ability.Caster);
		return result;
		static int GetHitChanceWithModifiers(int chance, bool ignoreCover, bool isCover, bool hitAlways)
		{
			if (isCover && hitAlways && !ignoreCover)
			{
				return 0;
			}
			if (!hitAlways)
			{
				return Mathf.Max(chance, 0);
			}
			return 100;
		}
	}

	private UIHitChancePredictionData GetBurstHitChanceData(AbilityData ability, Vector3 casterPosition, MechanicEntity target, MechanicEntity pointerTarget, IEnumerable<MechanicEntity> targetsInPattern)
	{
		bool flag = pointerTarget == target;
		UIHitChancePredictionData uIHitChancePredictionData = default(UIHitChancePredictionData);
		uIHitChancePredictionData.IsAdditionalTarget = !flag;
		UIHitChancePredictionData result = uIHitChancePredictionData;
		List<MechanicEntity> list = TempList.Get<MechanicEntity>();
		foreach (MechanicEntity item in targetsInPattern)
		{
			if (pointerTarget != item)
			{
				list.Add(item);
			}
		}
		RuleCalculateHitChances ruleCalculateHitChances = Rulebook.Trigger(new RuleCalculateHitChances(ability.Caster, target, ability, 0, casterPosition, target.Position));
		result.CoverChance = AbilityDataHelper.CalculateCoverHitChance(ruleCalculateHitChances.ResultLos, target);
		result.DefenceChance = GetDefenceChance(ability, target, ruleCalculateHitChances.IsAutoHit);
		int resultHitChance = ruleCalculateHitChances.ResultHitChance;
		if (flag)
		{
			result.InitialHitChance = resultHitChance;
		}
		else if (list != null)
		{
			RuleCalculateTargetInBurst ruleCalculateTargetInBurst = Rulebook.Trigger(RuleCalculateTargetInBurst.Setup(ability.Caster).WithTargets(list).Create());
			ruleCalculateTargetInBurst.TargetWeights.TryGetValue(target, out var value);
			float num = (float)value / (float)ruleCalculateTargetInBurst.TotalWeight;
			result.InitialHitChance = UtilityMath.ToPercent(UtilityMath.ToFraction(resultHitChance) * num);
		}
		result.HitWithAvoidanceChance = AbilityDataHelper.CalculateHitChanceWithAvoidance(result.InitialHitChance, result.DefenceChance, result.CoverChance, result.OverpenetraionChance);
		result.TargetHasCriticalEffects = IsAffectedByCriticalEffect(ability.Caster);
		return result;
	}

	private static float GetDefenceChance(AbilityData ability, MechanicEntity target, bool isAutoHit)
	{
		if (isAutoHit || ability.Blueprint.IsRangedAoE || (bool)target.Features.DefenceDisabled)
		{
			return 0f;
		}
		StatContext ctx = new StatContext(null, ability.Caster.Actor);
		return Mathf.Max(0f, target.GetEffectiveDefence(ctx));
	}

	private static float GetCoverChance(AbilityData ability, Vector3 casterPosition, Vector3 castPosition, MechanicEntity target, out bool isLosBlocker, out bool isCover)
	{
		MechanicEntity caster = ability.Caster;
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
		if (coverType != LosCalculations.CoverType.Cover || ability.IsPrecise)
		{
			return 0f;
		}
		return AbilityDataHelper.CalculateCoverHitChance(coverType, caster);
	}

	private static float GetCriticalEffectsAvoidanceChance(RuleCalculateHitChances hitChanceRule)
	{
		float num = Mathf.Max(hitChanceRule.ResultHitChance, 0);
		return (float)Mathf.Max(hitChanceRule.ResultHitChanceWithoutCritEffects, 0) - num;
	}

	private float GetOverpenetrationHitChance(AbilityData ability, Vector3 casterPosition, MechanicEntity pointerTarget)
	{
		if (pointerTarget == null)
		{
			return 0f;
		}
		return UtilityMath.ToPercent(UtilityMath.ToFraction(GetSingleHitChanceData(ability, casterPosition, pointerTarget).HitWithAvoidanceChance) * UtilityMath.ToFraction(ability.Weapon?.OverpenetrationChance ?? 0));
	}

	private static bool IsAffectedByCriticalEffect(MechanicEntity caster)
	{
		foreach (Buff buff in caster.Buffs)
		{
			if (buff.HasNegativeHitChanceModifier() && buff.Blueprint.CriticalEffect)
			{
				return true;
			}
		}
		return false;
	}
}
