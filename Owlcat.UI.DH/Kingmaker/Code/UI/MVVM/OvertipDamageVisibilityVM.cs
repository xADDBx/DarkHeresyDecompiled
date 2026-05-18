using Kingmaker.Predictions;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipDamageVisibilityVM : ViewModel
{
	private readonly MechanicEntityUIState m_EntityUIState;

	public OvertipDamageVisibilityVM(MechanicEntityUIState entityUIState)
	{
		m_EntityUIState = entityUIState;
	}

	public bool IsVisible()
	{
		return IsVisibleInternal(m_EntityUIState.IsInCombat.CurrentValue, m_EntityUIState.IsVisibleForPlayer.CurrentValue, m_EntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue, m_EntityUIState.IsMouseOverUnit.CurrentValue, m_EntityUIState.IsTarget.CurrentValue, m_EntityUIState.AbilityTargetUIData.CurrentValue.Ability);
	}

	public bool CanTargetByCurrentAbility()
	{
		AbilityTargetUIData currentValue = m_EntityUIState.AbilityTargetUIData.CurrentValue;
		AbilityData ability = currentValue.Ability;
		if (ability == null)
		{
			return false;
		}
		if (ability.IsPrecise && !Game.Instance.Controllers.PreciseAttackController.HasTarget)
		{
			return false;
		}
		if (ability.IsHeal && m_EntityUIState.IsPlayerFaction.CurrentValue)
		{
			return m_EntityUIState.IsMouseOverUnit.CurrentValue;
		}
		if (ability.IsSingleTarget && !ability.IsRanged)
		{
			return m_EntityUIState.IsMouseOverUnit.CurrentValue;
		}
		if (!m_EntityUIState.IsTarget.CurrentValue && !currentValue.CanTargetFromDesiredPosition)
		{
			return m_EntityUIState.IsMouseOverUnit.CurrentValue;
		}
		return true;
	}

	private bool IsVisibleInternal(bool isInCombat, bool isVisibleForPlayer, bool isDead, bool isHovered, bool isTarget, AbilityData ability)
	{
		if (isDead || !isVisibleForPlayer || ability == null)
		{
			return false;
		}
		if (!isInCombat)
		{
			if (m_EntityUIState.IsDestructible.CurrentValue)
			{
				return isTarget || isHovered;
			}
			return false;
		}
		if (ability.IsAoe || ability.IsBurst)
		{
			return isTarget;
		}
		return isHovered;
	}
}
