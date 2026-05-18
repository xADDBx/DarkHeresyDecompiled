using Code.View.UI.UIUtils;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.Gameplay.Controllers.Combat;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Predictions;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.View.Covers;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoReactiveData
{
	public struct PredictedDamage
	{
		public UIDamagePredictionData Prediction;

		public int OverallMaxDamage;
	}

	private Buff m_ConcentrationBuff;

	private readonly bool m_IsCompareData;

	private readonly ReactiveProperty<PredictedDamage> m_Damage = new ReactiveProperty<PredictedDamage>();

	private readonly ReactiveProperty<string> m_Name = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<string> m_Description = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<bool> m_IsAdditionalCombatObjective = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsNewCombatObjective = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsEnemy = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsDeadOrUnconsciousIsDead = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsTarget = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<int> m_HPDamageBonus = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_ArmorDamageBonus = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<bool> m_HasHPDamageBonus = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_HasArmorDamageBonus = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsVitalBodyPart = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<int> m_HealBonus = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_MinHeal = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_MaxHeal = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<DamageStrategy> m_HealStrategy = new ReactiveProperty<DamageStrategy>(DamageStrategy.Default);

	private readonly ReactiveProperty<bool> m_CanDie = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<int> m_HitPointLeft = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_HitPointMax = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_HitPointTotalLeft = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_HitPointTotalMax = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_ArmorLeft = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_ArmorMax = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_BurstIndex = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<Sprite> m_AbilityIcon = new ReactiveProperty<Sprite>(null);

	private readonly ReactiveProperty<bool> m_HasLineOfSight = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasHit = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsCaster = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<float> m_HitChance = new ReactiveProperty<float>(0f);

	private readonly ReactiveProperty<bool> m_HitAlways = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_HasCriticalEffect = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<float> m_InitialHitChance = new ReactiveProperty<float>(0f);

	private readonly ReactiveProperty<float> m_CriticalEffectsAvoidanceChance = new ReactiveProperty<float>(0f);

	private readonly ReactiveProperty<float> m_DefenceChance = new ReactiveProperty<float>(0f);

	private readonly ReactiveProperty<float> m_DamageReduction = new ReactiveProperty<float>(0f);

	private readonly ReactiveProperty<float> m_CoverChance = new ReactiveProperty<float>(0f);

	private readonly ReactiveProperty<float> m_OverpenetrationChance = new ReactiveProperty<float>(0f);

	private readonly ReactiveProperty<bool> m_HoverSelfTargetAbility = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<BlueprintBodyPart> m_BodyPart = new ReactiveProperty<BlueprintBodyPart>(null);

	private readonly ReactiveProperty<bool> m_PreciseAttackHasNoTarget = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<(int min, int max, int current)> m_MoraleValue = new ReactiveProperty<(int, int, int)>();

	private readonly ReactiveProperty<MoralePhaseType> m_MoralePhase = new ReactiveProperty<MoralePhaseType>();

	private readonly ReactiveProperty<bool> m_WillBecomeHeroic = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_WillBecomeBroken = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<Sprite> m_ConcentrationIcon = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<string> m_ConcentrationName = new ReactiveProperty<string>();

	private readonly ReactiveProperty<bool> m_HasConcentration = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ConcentrationAbilityTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public ReadOnlyReactiveProperty<string> Name => m_Name;

	public ReadOnlyReactiveProperty<bool> IsAdditionalCombatObjective => m_IsAdditionalCombatObjective;

	public ReadOnlyReactiveProperty<bool> IsNewCombatObjective => m_IsNewCombatObjective;

	public ReadOnlyReactiveProperty<string> Description => m_Description;

	public ReadOnlyReactiveProperty<bool> IsEnemy => m_IsEnemy;

	public ReadOnlyReactiveProperty<bool> IsDeadOrUnconsciousIsDead => m_IsDeadOrUnconsciousIsDead;

	public ReadOnlyReactiveProperty<bool> IsTarget => m_IsTarget;

	public ReadOnlyReactiveProperty<int> HPDamageBonus => m_HPDamageBonus;

	public ReadOnlyReactiveProperty<int> ArmorDamageBonus => m_ArmorDamageBonus;

	public ReadOnlyReactiveProperty<bool> IsVitalBodyPart => m_IsVitalBodyPart;

	public ReadOnlyReactiveProperty<int> MinHeal => m_MinHeal;

	public ReadOnlyReactiveProperty<int> MaxHeal => m_MaxHeal;

	public ReadOnlyReactiveProperty<DamageStrategy> HealStrategy => m_HealStrategy;

	public ReadOnlyReactiveProperty<int> HitPointLeft => m_HitPointLeft;

	public ReadOnlyReactiveProperty<int> HitPointMax => m_HitPointMax;

	public ReadOnlyReactiveProperty<int> ArmorLeft => m_ArmorLeft;

	public ReadOnlyReactiveProperty<int> ArmorMax => m_ArmorMax;

	public ReadOnlyReactiveProperty<int> BurstIndex => m_BurstIndex;

	public ReadOnlyReactiveProperty<Sprite> AbilityIcon => m_AbilityIcon;

	public ReadOnlyReactiveProperty<bool> HasLineOfSight => m_HasLineOfSight;

	public ReadOnlyReactiveProperty<bool> HasHit => m_HasHit;

	public ReadOnlyReactiveProperty<float> HitChance => m_HitChance;

	public ReadOnlyReactiveProperty<float> InitialHitChance => m_InitialHitChance;

	public ReadOnlyReactiveProperty<float> CriticalEffectsAvoidanceChance => m_CriticalEffectsAvoidanceChance;

	public ReadOnlyReactiveProperty<float> DefenceChance => m_DefenceChance;

	public ReadOnlyReactiveProperty<float> DamageReduction => m_DamageReduction;

	public ReadOnlyReactiveProperty<float> CoverChance => m_CoverChance;

	public ReadOnlyReactiveProperty<float> OverpenetrationChance => m_OverpenetrationChance;

	public ReadOnlyReactiveProperty<BlueprintBodyPart> BodyPart => m_BodyPart;

	public ReadOnlyReactiveProperty<bool> PreciseAttackHasNoTarget => m_PreciseAttackHasNoTarget;

	public ReadOnlyReactiveProperty<Sprite> ConcentrationIcon => m_ConcentrationIcon;

	public ReadOnlyReactiveProperty<string> ConcentrationName => m_ConcentrationName;

	public ReadOnlyReactiveProperty<bool> HasConcentration => m_HasConcentration;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> ConcentrationAbilityTooltip => m_ConcentrationAbilityTooltip;

	public ReadOnlyReactiveProperty<PredictedDamage> Damage => m_Damage;

	public ReadOnlyReactiveProperty<(int min, int max, int current)> MoraleValue => m_MoraleValue;

	public UnitInfoReactiveData(bool isCompareData)
	{
		m_IsCompareData = isCompareData;
	}

	public void UpdateCommon(MechanicEntityUIState mechanicEntityUIState)
	{
		if (mechanicEntityUIState != null)
		{
			MechanicEntityUIWrapper mechanicEntity = mechanicEntityUIState.MechanicEntity;
			m_Name.Value = mechanicEntity.Name;
			m_IsEnemy.Value = mechanicEntity.IsPlayerEnemy;
			m_IsAdditionalCombatObjective.Value = mechanicEntity.AdditionalCombatObjective != null;
			ReactiveProperty<bool> isNewCombatObjective = m_IsNewCombatObjective;
			PartAdditionalCombatObjectiveUnit additionalCombatObjective = mechanicEntity.AdditionalCombatObjective;
			isNewCombatObjective.Value = additionalCombatObjective != null && !additionalCombatObjective.ObjectIsViewed;
			m_Description.Value = mechanicEntity.AdditionalCombatObjective?.GetDescription()?.Text;
		}
	}

	public void UpdateHealth(MechanicEntityUIState mechanicEntityUIState)
	{
		if (mechanicEntityUIState != null)
		{
			MechanicEntityUIWrapper mechanicEntity = mechanicEntityUIState.MechanicEntity;
			int num = mechanicEntity.Health?.MaxHitPoints ?? 0;
			int num2 = mechanicEntity.Health?.HitPointsLeft ?? 0;
			int num3 = mechanicEntity.Armor?.DurabilityValue ?? 0;
			int num4 = mechanicEntity.Armor?.DurabilityLeft ?? 0;
			m_HitPointTotalMax.Value = num + num3;
			m_HitPointMax.Value = num;
			m_ArmorMax.Value = num3;
			m_HitPointTotalLeft.Value = num2 + num4;
			m_HitPointLeft.Value = num2;
			m_ArmorLeft.Value = num4;
		}
	}

	public void UpdateMorale(MechanicEntityUIState mechanicEntityUIState)
	{
		IUIUnitMoraleData iUIUnitMoraleData = mechanicEntityUIState?.Morale.CurrentValue;
		if (iUIUnitMoraleData == null)
		{
			m_MoralePhase.Value = MoralePhaseType.Regular;
			m_MoraleValue.Value = default((int, int, int));
		}
		else
		{
			m_MoralePhase.Value = iUIUnitMoraleData.MoralePhase;
			m_MoraleValue.Value = (iUIUnitMoraleData.MinValue, iUIUnitMoraleData.MaxValue, iUIUnitMoraleData.Morale);
		}
	}

	public void UpdateMoralePrediction(MechanicEntityUIState mechanicEntityUIState, IUIUnitMoraleData moralePrediction)
	{
		if (moralePrediction != null)
		{
			IUIUnitMoraleData iUIUnitMoraleData = mechanicEntityUIState?.Morale.CurrentValue;
			if (iUIUnitMoraleData != null)
			{
				m_WillBecomeHeroic.Value = UIUtilityUnit.MoraleWillBecomeHeroic(iUIUnitMoraleData, moralePrediction);
				m_WillBecomeBroken.Value = UIUtilityUnit.MoraleWillBecomeBroken(iUIUnitMoraleData, moralePrediction);
			}
		}
	}

	public void UpdateConcentrationBuff(Buff buff)
	{
		m_ConcentrationBuff = buff;
		m_ConcentrationIcon.Value = buff?.Icon;
		m_ConcentrationName.Value = buff?.Name;
		m_HasConcentration.Value = m_ConcentrationBuff != null;
		m_ConcentrationAbilityTooltip.Value = ((buff == null) ? null : new TooltipTemplateConcentrationBuff(m_ConcentrationBuff));
	}

	public void UpdateBodyPartData(MechanicEntityUIState mechanicEntityUIState, AbilityData ability, Vector3 worldPosition, PreciseAttackController.BodyPartUIData bodyPartUIData)
	{
		bool flag = bodyPartUIData != null;
		m_HasLineOfSight.Value = mechanicEntityUIState.CoverType.CurrentValue != LosCalculations.CoverType.LosBlocker;
		m_HasHit.Value = flag;
		m_BodyPart.Value = null;
		m_IsVitalBodyPart.Value = false;
		m_HealStrategy.Value = DamageStrategy.Default;
		m_MinHeal.Value = 0;
		m_MaxHeal.Value = 0;
		if (!flag)
		{
			m_Damage.Value = default(PredictedDamage);
			return;
		}
		m_BodyPart.Value = bodyPartUIData.BodyPart;
		m_IsVitalBodyPart.Value = bodyPartUIData.BodyPart.IsVital;
		mechanicEntityUIState.UpdatePreciseAttackBodyPartData(bodyPartUIData, m_IsCompareData);
		CollectAbilityProperty(mechanicEntityUIState, ability, isPreciseAttack: true, show: true);
	}

	public void CollectAbilityProperty(MechanicEntityUIState mechanicEntityUIState, AbilityData ability, bool isPreciseAttack, bool show)
	{
		MechanicEntity mechanicEntity = mechanicEntityUIState.MechanicEntity.MechanicEntity;
		if (!show || mechanicEntity.HasMechanicFeature(MechanicsFeatureType.HideRealHealthInUI))
		{
			ClearFields();
		}
		else
		{
			if (ability == null)
			{
				return;
			}
			m_PreciseAttackHasNoTarget.Value = ability.IsPrecise && !Game.Instance.Controllers.PreciseAttackController.HasTarget;
			if (PreciseAttackHasNoTarget.CurrentValue)
			{
				ClearFields();
				return;
			}
			bool flag = (((!mechanicEntityUIState.AbilityTargetUIData.CurrentValue.CanTargetFromDesiredPosition && !mechanicEntityUIState.IsTarget.CurrentValue) || ability.TargetAnchor != AbilityTargetAnchor.Point || ability.HasCustomDirectMovement()) ? ability.CanTargetFromDesiredPosition(mechanicEntity) : (mechanicEntityUIState.IsAoETarget.CurrentValue && CanAoETarget(mechanicEntityUIState, ability.Blueprint.AoETargets)));
			if (!isPreciseAttack)
			{
				m_HasHit.Value = flag;
			}
			m_HoverSelfTargetAbility.Value = mechanicEntityUIState.HoverSelfTargetAbility.CurrentValue;
			m_IsCaster.Value = mechanicEntityUIState.IsCaster.CurrentValue;
			m_IsDeadOrUnconsciousIsDead.Value = mechanicEntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue;
			m_IsTarget.Value = mechanicEntityUIState.IsTarget.CurrentValue;
			m_HasLineOfSight.Value = mechanicEntityUIState.CoverType.CurrentValue != LosCalculations.CoverType.LosBlocker;
			AbilityTargetUIData abilityTargetUIData = (m_IsCompareData ? mechanicEntityUIState.AbilityTargetUICompareData.CurrentValue : mechanicEntityUIState.AbilityTargetUIData.CurrentValue);
			if (flag)
			{
				m_HitAlways.Value = abilityTargetUIData.HitChance.HitAlways;
				m_BurstIndex.Value = abilityTargetUIData.AttacksCount;
				m_HitChance.Value = abilityTargetUIData.HitChance.HitWithAvoidanceChance;
				m_InitialHitChance.Value = abilityTargetUIData.HitChance.InitialHitChance;
				m_CriticalEffectsAvoidanceChance.Value = abilityTargetUIData.HitChance.CriticalEffectsAvoidanceChance;
				m_HasCriticalEffect.Value = abilityTargetUIData.HitChance.CasterHasCriticalEffects;
				m_DefenceChance.Value = abilityTargetUIData.HitChance.DefenceChance;
				m_CoverChance.Value = abilityTargetUIData.HitChance.CoverChance;
				m_OverpenetrationChance.Value = abilityTargetUIData.HitChance.OverpenetraionChance;
				m_DamageReduction.Value = abilityTargetUIData.Damage.DamageReduction;
				CollectDamagePredictionData(mechanicEntityUIState, ability);
			}
		}
	}

	public void SetAbilityIcon(Sprite sprite)
	{
		m_AbilityIcon.Value = sprite;
	}

	private void CollectDamagePredictionData(MechanicEntityUIState mechanicEntityUIState, AbilityData ability)
	{
		ability.PreciseBodyPart = BodyPart.CurrentValue;
		AbilityTargetUIData abilityTargetUIData = (m_IsCompareData ? mechanicEntityUIState.AbilityTargetUICompareData.CurrentValue : mechanicEntityUIState.AbilityTargetUIData.CurrentValue);
		UIDamagePredictionData damage = abilityTargetUIData.Damage;
		UIHealPredictionData heal = abilityTargetUIData.Heal;
		if (!damage.Equals(default(UIDamagePredictionData)))
		{
			m_Damage.Value = new PredictedDamage
			{
				Prediction = damage,
				OverallMaxDamage = damage.MaxDamagePerAttack * abilityTargetUIData.AttacksCount
			};
			m_HPDamageBonus.Value = damage.HPDamageBonus;
			m_ArmorDamageBonus.Value = damage.ArmorDamageBonus;
			m_HasHPDamageBonus.Value = damage.HPDamageBonus > 0;
			m_HasArmorDamageBonus.Value = damage.ArmorDamageBonus > 0;
			m_CanDie.Value = damage.MaxDamagePerAttack >= m_HitPointTotalLeft.CurrentValue;
			return;
		}
		m_Damage.Value = default(PredictedDamage);
		if (!heal.Equals(default(UIHealPredictionData)))
		{
			m_HealStrategy.Value = heal.HealStrategy;
			m_MinHeal.Value = heal.MinHeal;
			m_MaxHeal.Value = heal.MaxHeal;
			m_HealBonus.Value = heal.Bonus;
			m_HasHPDamageBonus.Value = false;
			m_HasArmorDamageBonus.Value = false;
			m_CanDie.Value = false;
		}
		else
		{
			m_HasHPDamageBonus.Value = false;
			m_HasArmorDamageBonus.Value = false;
			m_HealStrategy.Value = DamageStrategy.Default;
			m_MinHeal.Value = 0;
			m_MaxHeal.Value = 0;
			m_CanDie.Value = false;
		}
	}

	private bool CanAoETarget(MechanicEntityUIState mechanicEntityUIState, TargetType targetType)
	{
		MechanicEntityUIWrapper mechanicEntity = mechanicEntityUIState.MechanicEntity;
		return targetType switch
		{
			TargetType.Enemy => mechanicEntity.IsPlayerEnemy, 
			TargetType.Ally => mechanicEntity.IsPlayer || mechanicEntity.IsPlayerFaction, 
			TargetType.Any => true, 
			_ => false, 
		};
	}

	public void ClearFields()
	{
		m_Damage.Value = default(PredictedDamage);
		m_HasLineOfSight.Value = false;
		m_HitAlways.Value = false;
		m_HitChance.Value = 0f;
		m_InitialHitChance.Value = 0f;
		m_HasCriticalEffect.Value = false;
		m_BurstIndex.Value = 0;
		m_HasHPDamageBonus.Value = false;
		m_HasArmorDamageBonus.Value = false;
		m_MinHeal.Value = 0;
		m_MaxHeal.Value = 0;
		m_HealStrategy.Value = DamageStrategy.Default;
		m_DefenceChance.Value = 0f;
		m_DamageReduction.Value = 0f;
		m_CoverChance.Value = 0f;
		m_HasHit.Value = false;
		m_CanDie.Value = false;
	}
}
