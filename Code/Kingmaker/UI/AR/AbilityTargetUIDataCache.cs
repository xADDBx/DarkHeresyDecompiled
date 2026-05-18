using System;
using System.Collections.Generic;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.Gameplay.Predictions;
using Kingmaker.Code.Gameplay.Predictions.PredictionProviders;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Predictions;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using UnityEngine;

namespace Kingmaker.UI.AR;

public class AbilityTargetUIDataCache : MonoBehaviour, IAbilityTargetSelectionUIHandler, ISubscriber, IVirtualPositionUIHandler, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler
{
	private readonly Dictionary<int, AbilityTargetUIData> m_UIDataCache = new Dictionary<int, AbilityTargetUIData>();

	private readonly IPredictionProvider<UIDamagePredictionData, UIPredictionContext> m_DamagePrediction = new DamagePredictionProvider();

	private readonly IPredictionProvider<UIHitChancePredictionData, UIPredictionContext> m_HitChancePrediction = new HitChancePredictionProvider();

	private readonly IPredictionProvider<UIHealPredictionData, UIPredictionContext> m_HealPrediction = new HealPredictionProvider();

	private readonly IPredictionProvider<UIMoralePredictionData, UIPredictionContext> m_MoralePrediction = new MoralePredictionProvider();

	private readonly IPredictionProvider<UIBuffsPredictionData, UIPredictionContext> m_BuffsPrediction = new BuffsPredictionProvider();

	public static AbilityTargetUIDataCache Instance { get; private set; }

	public AbilityTargetUIData GetOrCreate(AbilityData ability, Vector3 casterPosition, MechanicEntity target, BlueprintBodyPart bodyPart, MechanicEntity pointerTarget, IReadOnlyList<MechanicEntity> targetsInPattern)
	{
		int dictionaryKey = GetDictionaryKey(ability.OriginalBlueprint, casterPosition, ability.Caster, target, bodyPart, pointerTarget);
		UIPredictionContext uIPredictionContext;
		if (m_UIDataCache.TryGetValue(dictionaryKey, out var value))
		{
			if (!HasDynamicDamageBuff(ability.Caster))
			{
				return value;
			}
			uIPredictionContext = default(UIPredictionContext);
			uIPredictionContext.Ability = ability;
			uIPredictionContext.Target = target;
			uIPredictionContext.PointerTarget = pointerTarget;
			uIPredictionContext.CasterPosition = casterPosition;
			uIPredictionContext.TargetsInPattern = targetsInPattern;
			uIPredictionContext.BodyPart = bodyPart;
			UIPredictionContext ctx = uIPredictionContext;
			UIDamagePredictionData damage = m_DamagePrediction.Get(ctx);
			UIHitChancePredictionData hitChance = m_HitChancePrediction.Get(ctx);
			value.UpdateSingleAttack(damage, hitChance);
			return value;
		}
		AbilityData abilityData = ability.Clone();
		abilityData.PreciseBodyPart = bodyPart;
		uIPredictionContext = default(UIPredictionContext);
		uIPredictionContext.Ability = abilityData;
		uIPredictionContext.Target = target;
		uIPredictionContext.PointerTarget = pointerTarget;
		uIPredictionContext.CasterPosition = casterPosition;
		uIPredictionContext.TargetsInPattern = targetsInPattern;
		uIPredictionContext.BodyPart = bodyPart;
		UIPredictionContext ctx2 = uIPredictionContext;
		value = new AbilityTargetUIData(abilityData, casterPosition, target, bodyPart, m_HitChancePrediction.Get(ctx2), m_DamagePrediction.Get(ctx2), m_HealPrediction.Get(ctx2), m_MoralePrediction.Get(ctx2), m_BuffsPrediction.Get(ctx2));
		m_UIDataCache.Add(dictionaryKey, value);
		return value;
	}

	void IAbilityTargetSelectionUIHandler.HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		Clear();
	}

	void IAbilityTargetSelectionUIHandler.HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
	}

	void IVirtualPositionUIHandler.HandleVirtualPositionChanged(Vector3? position)
	{
		Clear();
	}

	void IUnitActiveEquipmentSetHandler.HandleUnitChangeActiveEquipmentSet()
	{
		Clear();
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		Clear();
	}

	void IInterruptTurnStartHandler.HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		Clear();
	}

	private int GetDictionaryKey(BlueprintAbility ability, Vector3 casterPosition, MechanicEntity caster, MechanicEntity target, BlueprintBodyPart bodyPart, MechanicEntity pointerTarget)
	{
		return HashCode.Combine(ability?.GetHashCode(), casterPosition.GetHashCode(), caster?.GetHashCode(), target?.GetHashCode(), bodyPart?.GetHashCode(), pointerTarget?.GetHashCode());
	}

	private void Clear()
	{
		m_UIDataCache.Clear();
	}

	private static bool HasDynamicDamageBuff(MechanicEntity caster)
	{
		foreach (Buff buff in caster.Buffs)
		{
			if (buff.Blueprint.DynamicDamage)
			{
				return true;
			}
		}
		return false;
	}

	private void OnEnable()
	{
		Instance = this;
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		Instance = null;
		Clear();
		EventBus.Unsubscribe(this);
	}
}
