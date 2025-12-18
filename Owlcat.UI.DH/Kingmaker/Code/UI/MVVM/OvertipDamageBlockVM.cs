using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Utility;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipDamageBlockVM : ViewModel
{
	public readonly MechanicEntityUIState MechanicEntityUIState;

	private readonly ReactiveProperty<bool> m_HasHit = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<int> m_MinDamage = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_MaxDamage = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<bool> m_IsVisibleTrigger = new ReactiveProperty<bool>();

	private MechanicEntity Unit => MechanicEntityUIState.MechanicEntity.MechanicEntity;

	public ReadOnlyReactiveProperty<bool> HasHit => m_HasHit;

	public ReadOnlyReactiveProperty<int> MinDamage => m_MinDamage;

	public ReadOnlyReactiveProperty<int> MaxDamage => m_MaxDamage;

	public ReadOnlyReactiveProperty<bool> IsVisibleTrigger => m_IsVisibleTrigger;

	public bool IsHeal { get; private set; }

	public OvertipDamageBlockVM(MechanicEntityUIState mechanicEntityUIState)
	{
		MechanicEntityUIState = mechanicEntityUIState;
		MechanicEntityUIState.IsInCombat.CombineLatest(MechanicEntityUIState.IsVisibleForPlayer, MechanicEntityUIState.IsDeadOrUnconsciousIsDead, MechanicEntityUIState.Ability, MechanicEntityUIState.IsMouseOverUnit, MechanicEntityUIState.IsTarget, MechanicEntityUIState.IsAoETarget, MechanicEntityUIState.IsDestructible, (bool isInCombat, bool isVisibleForPlayer, bool isDead, AbilityData ability, bool isHover, bool isTarget, bool isAoETarget, bool isDestructible) => (isInCombat || isDestructible) && isVisibleForPlayer && !isDead && ability != null && (isHover || isTarget || isAoETarget)).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate(bool value)
		{
			m_IsVisibleTrigger.Value = value;
		})
			.AddTo(this);
		IsVisibleTrigger.CombineLatest(MechanicEntityUIState.AbilityTargetUIData, (bool visible, AbilityTargetUIData uiData) => visible).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(OnVisibilityChanged)
			.AddTo(this);
	}

	private void OnVisibilityChanged(bool state)
	{
		if (!state)
		{
			ClearProperties();
		}
		else
		{
			UpdateProperties();
		}
	}

	private void UpdateProperties()
	{
		ClearProperties();
		AbilityData currentValue = MechanicEntityUIState.Ability.CurrentValue;
		if (currentValue == null || (currentValue.IsPrecise && !Game.Instance.Controllers.PreciseAttackController.HasTarget))
		{
			return;
		}
		TargetWrapper targetForDesiredPosition = Game.Instance.Controllers.SelectedAbilityHandler.GetTargetForDesiredPosition(Unit.View.gameObject, Game.Instance.Controllers.ClickEventsController.WorldPosition);
		bool flag = ((!(targetForDesiredPosition != null) || currentValue.TargetAnchor != AbilityTargetAnchor.Point || currentValue.HasCustomDirectMovement()) ? (targetForDesiredPosition != null && currentValue.CanTargetFromDesiredPosition(targetForDesiredPosition)) : (MechanicEntityUIState.IsAoETarget.CurrentValue && CanAoETarget(MechanicEntityUIState, currentValue.Blueprint.AoETargets)));
		m_HasHit.Value = flag;
		if (flag)
		{
			AbilityTargetUIData currentValue2 = MechanicEntityUIState.AbilityTargetUIData.CurrentValue;
			UIDamagePredictionData damage = currentValue2.Damage;
			UIHealPredictionData heal = currentValue2.Heal;
			if (!damage.Equals(default(UIDamagePredictionData)))
			{
				m_MinDamage.Value = damage.MinDamagePerAttack;
				m_MaxDamage.Value = damage.MaxDamagePerAttack * currentValue2.AttacksCount;
				IsHeal = false;
			}
			else if (!heal.Equals(default(UIHealPredictionData)))
			{
				m_MinDamage.Value = heal.MinHeal;
				m_MaxDamage.Value = heal.MaxHeal;
				IsHeal = true;
			}
		}
	}

	private void ClearProperties()
	{
		m_HasHit.Value = false;
		m_MinDamage.Value = 0;
		m_MaxDamage.Value = 0;
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
}
