using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Features.Channeling;
using Kingmaker.UI.Pointer;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EntityOvertipVisibilityVM : ViewModel
{
	private readonly MechanicEntityUIState m_EntityState;

	private readonly ReadOnlyReactiveProperty<bool> m_IsBarkActive;

	private readonly ReadOnlyReactiveProperty<bool> m_HasCombatText;

	private readonly OvertipDistanceVisibilityStrategy m_DistanceVisibility;

	public EntityOvertipVisibilityVM(MechanicEntityUIState entityState, ReadOnlyReactiveProperty<bool> isBarkActive, ReadOnlyReactiveProperty<bool> hasCombatText)
	{
		m_EntityState = entityState;
		m_IsBarkActive = isBarkActive;
		m_HasCombatText = hasCombatText;
		m_DistanceVisibility = new OvertipDistanceVisibilityStrategy(() => CursorController.CursorPosition);
	}

	public bool IsOvertipActive()
	{
		if (!m_EntityState.IsVisibleForPlayer.CurrentValue)
		{
			return false;
		}
		if (m_EntityState.IsDead.CurrentValue)
		{
			return HasOngoingEvents();
		}
		return true;
	}

	public float GetVisibilityRate(Vector3 viewportPos)
	{
		return m_DistanceVisibility.GetVisibilityRate(viewportPos);
	}

	public bool IsForcedHidden()
	{
		if (m_EntityState.IsVisibleForPlayer.CurrentValue && (!m_EntityState.IsDead.CurrentValue || HasOngoingEvents()) && IsValidGameMode() && RootVM.Instance.HUDContext?.CombatEndWindowVM.CurrentValue == null)
		{
			return Game.Instance.Controllers.PreciseAttackController.HasTarget;
		}
		return true;
	}

	public bool IsForcedVisible()
	{
		if (!m_IsBarkActive.CurrentValue && !m_HasCombatText.CurrentValue && !m_EntityState.IsTarget.CurrentValue)
		{
			IUIChanneling currentValue = m_EntityState.Channeling.CurrentValue;
			if ((currentValue == null || !currentValue.IsActive) && m_EntityState.ConcentrationBuff.CurrentValue == null && !IsHeroicOrBroken(m_EntityState) && !m_EntityState.ForceHotKeyPressed.CurrentValue && !m_EntityState.IsPingUnit.CurrentValue && !m_EntityState.IsCurrentUnitTurn.CurrentValue && !IsAbilityPossibleTarget(m_EntityState))
			{
				return m_EntityState.IsMouseOverUnit.CurrentValue;
			}
		}
		return true;
		static bool IsAbilityPossibleTarget(MechanicEntityUIState state)
		{
			AbilityData currentValue2 = state.Ability.CurrentValue;
			if (currentValue2 == null)
			{
				return false;
			}
			MechanicEntity mechanicEntity = state.MechanicEntity.MechanicEntity;
			bool canTargetFromDesiredPosition = state.AbilityTargetUIData.CurrentValue.CanTargetFromDesiredPosition;
			if (!(currentValue2.IsRanged && canTargetFromDesiredPosition) || !mechanicEntity.IsPlayerEnemy)
			{
				if (currentValue2.IsHeal && canTargetFromDesiredPosition)
				{
					return mechanicEntity.IsPlayerFaction;
				}
				return false;
			}
			return true;
		}
		static bool IsHeroicOrBroken(MechanicEntityUIState state)
		{
			return state.Morale.CurrentValue.MoralePhase != MoralePhaseType.Regular;
		}
	}

	public bool IsUnreachableTarget()
	{
		if (m_EntityState.Ability.CurrentValue != null)
		{
			return !m_EntityState.CheckCanBeTargeted;
		}
		return false;
	}

	private bool HasOngoingEvents()
	{
		if (!m_HasCombatText.CurrentValue && !m_IsBarkActive.CurrentValue)
		{
			return UtilityUnit.GetUnitInteractionFrom(m_EntityState?.MechanicEntity.MechanicEntity) != null;
		}
		return true;
	}

	private bool IsValidGameMode()
	{
		GameModeType currentModeType = Game.Instance.CurrentModeType;
		if (currentModeType != GameModeType.Cutscene)
		{
			return currentModeType != GameModeType.Dialog;
		}
		return false;
	}
}
