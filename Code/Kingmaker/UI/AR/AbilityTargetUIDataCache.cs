using System.Collections.Generic;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using UnityEngine;

namespace Kingmaker.UI.AR;

public class AbilityTargetUIDataCache : MonoBehaviour, IAbilityTargetSelectionUIHandler, ISubscriber, IVirtualPositionUIHandler, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler
{
	private readonly Dictionary<(AbilityData ability, Vector3 casterPosition, MechanicEntity target, BlueprintBodyPart bodyPart, MechanicEntity pointerTarget), AbilityTargetUIData> m_UIDataCache = new Dictionary<(AbilityData, Vector3, MechanicEntity, BlueprintBodyPart, MechanicEntity), AbilityTargetUIData>();

	public static AbilityTargetUIDataCache Instance { get; private set; }

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

	private void Clear()
	{
		m_UIDataCache.Clear();
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		Clear();
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
	}

	public void HandleVirtualPositionChanged(Vector3? position)
	{
		Clear();
	}

	public void HandleUnitChangeActiveEquipmentSet()
	{
		Clear();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		Clear();
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		Clear();
	}

	public AbilityTargetUIData GetOrCreate(AbilityData ability, Vector3 casterPosition, MechanicEntity target, BlueprintBodyPart bodyPart, MechanicEntity pointerTarget, IReadOnlyList<MechanicEntity> targetsInPattern)
	{
		bool flag = HasDynamicDamageBuff(ability.Caster);
		if (!m_UIDataCache.TryGetValue((ability, casterPosition, target, bodyPart, pointerTarget), out var value))
		{
			flag = false;
			value = new AbilityTargetUIData(ability, casterPosition, target, bodyPart, pointerTarget, targetsInPattern);
			m_UIDataCache.Add((ability, casterPosition, target, bodyPart, pointerTarget), value);
		}
		if (flag)
		{
			value.UpdateSingleAttack();
		}
		return value;
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
}
