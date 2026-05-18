using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Code.Gameplay.Components;
using Kingmaker.ElementsSystem;
using Kingmaker.Gameplay.Features.Concentration;
using Kingmaker.Predictions;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipConcentrationActionVM : ViewModel
{
	private readonly MechanicEntityUIState m_EntityUIState;

	private readonly ReactiveProperty<Sprite> m_Icon = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<bool> m_HasAction = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanBreak = new ReactiveProperty<bool>();

	private TooltipBaseTemplate m_ActionAbilityTooltip;

	private Buff m_Buff;

	public TooltipBaseTemplate ActionAbilityTooltip
	{
		get
		{
			if (m_ActionAbilityTooltip != null)
			{
				return m_ActionAbilityTooltip;
			}
			m_ActionAbilityTooltip = new TooltipTemplateConcentrationBuff(m_Buff);
			return m_ActionAbilityTooltip;
		}
	}

	public ReadOnlyReactiveProperty<Sprite> Icon => m_Icon;

	public ReadOnlyReactiveProperty<bool> HasAction => m_HasAction;

	public ReadOnlyReactiveProperty<bool> CanBreak => m_CanBreak;

	public OvertipConcentrationActionVM(MechanicEntityUIState mechanicEntityUIState)
	{
		m_EntityUIState = mechanicEntityUIState;
		m_EntityUIState.ConcentrationBuff.Subscribe(UpdateConcentration).AddTo(this);
		if (!m_EntityUIState.MechanicEntity.HasSteadyConcentration && !m_EntityUIState.IsOvertipPartHiddenBySettings(UnitOvertipUIPart.Concentration))
		{
			m_EntityUIState.ConcentrationBuff.CombineLatest(m_EntityUIState.IsMouseOverUnit, m_EntityUIState.AbilityTargetUIData, m_EntityUIState.Ability, m_EntityUIState.IsTarget, (Buff _, bool _, AbilityTargetUIData abilityTarget, AbilityData _, bool isTarget) => (abilityTarget: abilityTarget, isTarget: isTarget)).Subscribe(UpdateBreakMarker).AddTo(this);
		}
	}

	private void UpdateConcentration(Buff buff)
	{
		m_Buff = buff;
		m_Icon.Value = buff?.Icon;
		m_HasAction.Value = m_Buff != null;
		m_ActionAbilityTooltip = null;
	}

	private void UpdateBreakMarker((AbilityTargetUIData abilityTargetData, bool isTarget) data)
	{
		m_CanBreak.Value = CanBreakConcentration(data.abilityTargetData, data.isTarget);
	}

	private bool CanBreakConcentration(AbilityTargetUIData abilityTargetData, bool isTarget)
	{
		AbilityData ability = abilityTargetData.Ability;
		if (!HasAction.CurrentValue || ability == null || !isTarget)
		{
			return false;
		}
		if (ability.Blueprint.BreakConcentration() || ability.IsPrecise)
		{
			return true;
		}
		if (abilityTargetData.Morale.MaxDelta + m_EntityUIState.Morale.CurrentValue.Morale <= MoraleRoot.Instance.MinValue)
		{
			return true;
		}
		if (AppliesConcentrationBreakingBuff(ability))
		{
			return true;
		}
		return false;
	}

	private bool AppliesConcentrationBreakingBuff(AbilityData ability)
	{
		IEnumerable<AbilityEffectRunAction> components = ability.Blueprint.GetComponents<AbilityEffectRunAction>();
		bool flag = ability.Caster.IsEnemy(m_EntityUIState.MechanicEntity.MechanicEntity);
		foreach (AbilityEffectRunAction item in components)
		{
			if (AppliesConcentrationBreakingBuff(item.Actions))
			{
				return true;
			}
			if (flag && AppliesConcentrationBreakingBuff(item.ActionsOnEnemy))
			{
				return true;
			}
			if (!flag && AppliesConcentrationBreakingBuff(item.ActionsOnAlly))
			{
				return true;
			}
		}
		return false;
	}

	private bool AppliesConcentrationBreakingBuff(ActionList actions)
	{
		GameAction[] actions2 = actions.Actions;
		for (int i = 0; i < actions2.Length; i++)
		{
			if (actions2[i] is ContextActionApplyBuff contextActionApplyBuff && contextActionApplyBuff.Buff.IsHardCrowdControl)
			{
				return true;
			}
		}
		return false;
	}
}
