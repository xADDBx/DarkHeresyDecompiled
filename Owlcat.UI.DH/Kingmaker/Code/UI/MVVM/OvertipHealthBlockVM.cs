using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipHealthBlockVM : ViewModel, IDamageHandler, ISubscriber, IHealingHandler, ITurnBasedModeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, ITurnEndHandler, IModifiableValueChangedHandler
{
	private bool m_CanTarget;

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

	public readonly MechanicEntityUIState MechanicEntityUIState;

	private MechanicEntity Unit => MechanicEntityUIState.MechanicEntity.MechanicEntity;

	private MechanicEntityUIWrapper MechanicEntityUIWrapper => MechanicEntityUIState.MechanicEntity;

	public ReadOnlyReactiveProperty<(int health, int armor)> MaxDamage => m_MaxDamage;

	public ReadOnlyReactiveProperty<int> MaxHeal => m_MaxHeal;

	public ReadOnlyReactiveProperty<DamageStrategy> HealStrategy => m_HealStrategy;

	public ReadOnlyReactiveProperty<bool> CanDie => m_CanDie;

	public ReadOnlyReactiveProperty<int> HitPointLeft => m_HitPointLeft;

	public ReadOnlyReactiveProperty<int> ArmorLeft => m_ArmorLeft;

	public ReadOnlyReactiveProperty<int> ArmorMax => m_ArmorMax;

	public ReadOnlyReactiveProperty<int> HitPointMax => m_HitPointMax;

	public ReadOnlyReactiveProperty<bool> HitChanceBlockVisible { get; }

	public bool VisibleInExploration => (Unit as DestructibleEntity)?.View.VisibleInExploration ?? false;

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
		MechanicEntityUIState = mechanicEntityUIState;
		HitChanceBlockVisible = hitChanceBlockVisible;
		UpdateProperties(initial: true);
		MechanicEntityUIState.IsInCombat.CombineLatest(MechanicEntityUIState.IsDeadOrUnconsciousIsDead, MechanicEntityUIState.IsVisibleForPlayer, MechanicEntityUIState.Ability, MechanicEntityUIState.IsMouseOverUnit, MechanicEntityUIState.IsTarget, MechanicEntityUIState.AbilityTargetUIData, (bool _, bool _, bool _, AbilityData _, bool _, bool _, AbilityTargetUIData _) => true).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate
		{
			UpdateVisibility();
		})
			.AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	private void UpdateVisibility()
	{
		bool num = (MechanicEntityUIState.IsInCombat.CurrentValue || MechanicEntityUIState.IsDestructible.CurrentValue) && MechanicEntityUIState.IsVisibleForPlayer.CurrentValue && !MechanicEntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue && MechanicEntityUIState.Ability.CurrentValue != null;
		bool flag = MechanicEntityUIState.IsMouseOverUnit.CurrentValue || MechanicEntityUIState.IsTarget.CurrentValue;
		bool show = num && flag;
		CollectAbilityProperty(show);
		CheckCanDie(show);
	}

	private void UpdateProperties(bool initial = false)
	{
		if (Unit != null && (!Unit.HasMechanicFeature(MechanicsFeatureType.HideRealHealthInUI) || initial))
		{
			int num = MechanicEntityUIWrapper.Health?.MaxHitPoints ?? 0;
			int num2 = MechanicEntityUIWrapper.Health?.HitPointsLeft ?? 0;
			ModifiableValue modifiableValue = MechanicEntityUIWrapper.Armor?.Durability;
			int num3 = ((modifiableValue != null) ? ((int)modifiableValue) : 0);
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

	void IModifiableValueChangedHandler.HandleModifiableValueChanged(ModifiableValue modifiableValue)
	{
		if (modifiableValue.Owner == Unit && modifiableValue is ModifiableValueHitPoints)
		{
			UpdateProperties();
		}
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			HandleEntityEvent(EventInvokerExtensions.Entity);
		}
	}

	void ITurnEndHandler.HandleUnitEndTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			HandleEntityEvent(EventInvokerExtensions.Entity);
		}
	}

	private void HandleEntityEvent(Entity entity)
	{
		if (entity != null && entity == Unit)
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
		AbilityData currentValue = MechanicEntityUIState.Ability.CurrentValue;
		if (currentValue.IsPrecise && !Game.Instance.Controllers.PreciseAttackController.HasTarget)
		{
			return;
		}
		m_CanTarget = ((Unit == null || currentValue.TargetAnchor != AbilityTargetAnchor.Point || currentValue.HasCustomDirectMovement()) ? currentValue.CanTargetFromDesiredPosition(Unit) : (MechanicEntityUIState.IsAoETarget.CurrentValue && CanAoETarget(MechanicEntityUIState, currentValue.Blueprint.AoETargets)));
		if (!m_CanTarget)
		{
			ResetValues();
			return;
		}
		AbilityTargetUIData currentValue2 = MechanicEntityUIState.AbilityTargetUIData.CurrentValue;
		UIDamagePredictionData damage = currentValue2.Damage;
		UIHealPredictionData heal = currentValue2.Heal;
		if (!damage.Equals(default(UIDamagePredictionData)))
		{
			int healthMaxDamage = damage.HealthMaxDamage;
			int armorMaxDamage = damage.ArmorMaxDamage;
			m_MaxDamage.Value = (healthMaxDamage, armorMaxDamage);
		}
		else if (!heal.Equals(default(UIHealPredictionData)))
		{
			m_MinHeal.Value = heal.MinHeal;
			m_MaxHeal.Value = heal.MaxHeal;
			m_HealStrategy.Value = heal.HealStrategy;
		}
		else
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
		if (MechanicEntityUIState.Ability.CurrentValue.IsPrecise && !Game.Instance.Controllers.PreciseAttackController.HasTarget)
		{
			m_CanDie.Value = false;
			return;
		}
		bool currentValue = MechanicEntityUIState.IsTarget.CurrentValue;
		UIDamagePredictionData damage = MechanicEntityUIState.AbilityTargetUIData.CurrentValue.Damage;
		if (!currentValue || damage.Equals(default(UIDamagePredictionData)))
		{
			m_CanDie.Value = false;
		}
		else
		{
			m_CanDie.Value = damage.HealthMaxDamage >= m_HitPointLeft.CurrentValue;
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
}
