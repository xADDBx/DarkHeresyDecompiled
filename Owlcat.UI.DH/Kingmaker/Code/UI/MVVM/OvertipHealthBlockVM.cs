using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Predictions;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipHealthBlockVM : ViewModel, IDamageHandler, ISubscriber, IHealingHandler, ITurnBasedModeHandler, IActorStatChangedHandler, ISubscriber<IMechanicEntity>
{
	private readonly OvertipDamageVisibilityVM m_VisibilityVM;

	private readonly ReactiveProperty<(int health, int armor)> m_MaxDamage = new ReactiveProperty<(int, int)>((0, 0));

	private readonly ReactiveProperty<int> m_MinHeal = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_MaxHeal = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<DamageStrategy> m_HealStrategy = new ReactiveProperty<DamageStrategy>(DamageStrategy.Default);

	private readonly ReactiveProperty<bool> m_CanDie = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<int> m_HitPointLeft = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_ArmorLeft = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_ArmorMax = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_HitPointTotalLeft = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_HitPointMax = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_HitPointTotalMax = new ReactiveProperty<int>(0);

	private readonly bool m_VisibleInExploration;

	private bool m_CanTarget;

	public readonly MechanicEntityUIState EntityUIState;

	private MechanicEntity Unit => EntityUIState.MechanicEntity.MechanicEntity;

	private MechanicEntityUIWrapper MechanicEntityUIWrapper => EntityUIState.MechanicEntity;

	public ReadOnlyReactiveProperty<(int health, int armor)> MaxDamage => m_MaxDamage;

	public ReadOnlyReactiveProperty<int> MaxHeal => m_MaxHeal;

	public ReadOnlyReactiveProperty<DamageStrategy> HealStrategy => m_HealStrategy;

	public ReadOnlyReactiveProperty<bool> CanDie => m_CanDie;

	public ReadOnlyReactiveProperty<int> HitPointLeft => m_HitPointLeft;

	public ReadOnlyReactiveProperty<int> ArmorLeft => m_ArmorLeft;

	public ReadOnlyReactiveProperty<int> ArmorMax => m_ArmorMax;

	public ReadOnlyReactiveProperty<int> HitPointMax => m_HitPointMax;

	public ReadOnlyReactiveProperty<bool> HitChanceBlockVisible { get; }

	public bool HideRealHealthInUI
	{
		get
		{
			if (Unit != null)
			{
				return Unit.HasMechanicFeature(MechanicsFeatureType.HideRealHealthInUI);
			}
			return false;
		}
	}

	public OvertipHealthBlockVM(MechanicEntityUIState mechanicEntityUIState, ReadOnlyReactiveProperty<bool> hitChanceBlockVisible)
	{
		m_VisibilityVM = new OvertipDamageVisibilityVM(mechanicEntityUIState).AddTo(this);
		EntityUIState = mechanicEntityUIState;
		HitChanceBlockVisible = hitChanceBlockVisible;
		m_VisibleInExploration = Unit is DestructibleEntity destructibleEntity && destructibleEntity.View.VisibleInExploration;
		UpdateProperties(initial: true);
		EntityUIState.IsInCombat.CombineLatest(EntityUIState.IsDeadOrUnconsciousIsDead, EntityUIState.IsVisibleForPlayer, EntityUIState.Ability, EntityUIState.IsMouseOverUnit, EntityUIState.IsTarget, EntityUIState.AbilityTargetUIData, (bool _, bool _, bool _, AbilityData _, bool _, bool _, AbilityTargetUIData _) => true).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate
		{
			UpdateVisibility();
		})
			.AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	public bool ShowBlock()
	{
		bool currentValue = EntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue;
		bool currentValue2 = EntityUIState.IsInCombat.CurrentValue;
		bool currentValue3 = EntityUIState.HideOvertip.CurrentValue;
		if (currentValue || (!currentValue2 && !m_VisibleInExploration) || currentValue3 || EntityUIState.IsOvertipPartHiddenBySettings(UnitOvertipUIPart.HealthBar))
		{
			return false;
		}
		if (EntityUIState.IsDestructible.CurrentValue)
		{
			if (!EntityUIState.IsDestructibleNotCover.CurrentValue && (!(EntityUIState.Ability.CurrentValue != null) || !EntityUIState.IsTarget.CurrentValue))
			{
				return EntityUIState.IsMouseOverUnit.CurrentValue;
			}
			return true;
		}
		bool currentValue4 = EntityUIState.IsPreparationTurn.CurrentValue;
		bool currentValue5 = EntityUIState.IsEnemy.CurrentValue;
		return !currentValue4 || currentValue5;
	}

	private void UpdateVisibility()
	{
		bool show = m_VisibilityVM.IsVisible();
		CollectAbilityProperty(show);
		CheckCanDie(show);
	}

	private void UpdateProperties(bool initial = false)
	{
		if (Unit != null && (!Unit.HasMechanicFeature(MechanicsFeatureType.HideRealHealthInUI) || initial))
		{
			int num = MechanicEntityUIWrapper.Health?.MaxHitPoints ?? 0;
			int num2 = MechanicEntityUIWrapper.Health?.HitPointsLeft ?? 0;
			int num3 = MechanicEntityUIWrapper.Armor?.DurabilityValue ?? 0;
			int num4 = MechanicEntityUIWrapper.Armor?.DurabilityLeft ?? 0;
			m_HitPointMax.Value = num;
			m_HitPointTotalMax.Value = num + num3;
			m_HitPointLeft.Value = num2;
			m_ArmorMax.Value = num3;
			m_ArmorLeft.Value = num4;
			m_HitPointTotalLeft.Value = num2 + num4;
		}
	}

	public void HandleDamageDealt(RuleDealDamage dealDamage)
	{
		UpdateProperties();
	}

	public void HandleHealing(RuleHealDamage healDamage)
	{
		UpdateProperties();
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		UpdateProperties();
	}

	void IActorStatChangedHandler.HandleActorStatChanged(StatChangeSet stats)
	{
		if (EventInvokerExtensions.MechanicEntity == Unit && (stats.Contains(StatType.MaxHitPoints) || stats.Contains(StatType.MaxArmorDurability)))
		{
			UpdateProperties();
		}
	}

	private void CollectAbilityProperty(bool show)
	{
		if (!show || Unit.HasMechanicFeature(MechanicsFeatureType.HideRealHealthInUI))
		{
			ResetValues();
			return;
		}
		m_CanTarget = m_VisibilityVM.CanTargetByCurrentAbility();
		if (!m_CanTarget)
		{
			ResetValues();
			return;
		}
		AbilityTargetUIData currentValue = EntityUIState.AbilityTargetUIData.CurrentValue;
		UIDamagePredictionData damage = currentValue.Damage;
		UIHealPredictionData heal = currentValue.Heal;
		m_MaxDamage.Value = (damage.HealthMaxDamage, damage.ArmorMaxDamage);
		m_MinHeal.Value = heal.MinHeal;
		m_MaxHeal.Value = heal.MaxHeal;
		m_HealStrategy.Value = heal.HealStrategy;
		if (damage.Equals(default(UIDamagePredictionData)) && heal.Equals(default(UIHealPredictionData)))
		{
			ResetValues();
		}
		void ResetValues()
		{
			m_CanTarget = false;
			m_MaxDamage.Value = (0, 0);
			m_MinHeal.Value = 0;
			m_MaxHeal.Value = 0;
			m_HealStrategy.Value = DamageStrategy.Default;
		}
	}

	private void CheckCanDie(bool show)
	{
		if (!show || Unit.HasMechanicFeature(MechanicsFeatureType.HideRealHealthInUI))
		{
			m_CanDie.Value = false;
			return;
		}
		if (EntityUIState.Ability.CurrentValue.IsPrecise && !Game.Instance.Controllers.PreciseAttackController.HasTarget)
		{
			m_CanDie.Value = false;
			return;
		}
		bool currentValue = EntityUIState.IsTarget.CurrentValue;
		UIDamagePredictionData damage = EntityUIState.AbilityTargetUIData.CurrentValue.Damage;
		if (!currentValue || damage.Equals(default(UIDamagePredictionData)))
		{
			m_CanDie.Value = false;
		}
		else
		{
			m_CanDie.Value = (EntityUIState.IsCountHpAsArmor ? (damage.ArmorMaxDamage >= m_ArmorLeft.CurrentValue) : (damage.HealthMaxDamage >= m_HitPointLeft.CurrentValue));
		}
	}
}
