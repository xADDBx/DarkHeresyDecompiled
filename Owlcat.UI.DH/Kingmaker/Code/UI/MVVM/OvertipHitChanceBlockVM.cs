using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility;
using ObservableCollections;
using Owlcat.UI;
using Pathfinding;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipHitChanceBlockVM : ViewModel
{
	private readonly ReactiveProperty<bool> m_CanTarget = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<float> m_HitChance = new ReactiveProperty<float>(0f);

	private readonly ReactiveProperty<bool> m_HitAlways = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<float> m_InitialHitChance = new ReactiveProperty<float>(0f);

	private readonly ReactiveProperty<int> m_BurstIndex = new ReactiveProperty<int>(0);

	private readonly ObservableList<HitChanceEntityVM> m_BurstHitChancesCollection = new ObservableList<HitChanceEntityVM>();

	private readonly ReactiveProperty<float> m_DefenceChance = new ReactiveProperty<float>(0f);

	private readonly ReactiveProperty<float> m_CoverChance = new ReactiveProperty<float>(0f);

	private readonly ReactiveProperty<bool> m_IsVisible = new ReactiveProperty<bool>();

	public readonly MechanicEntityUIState MechanicEntityUIState;

	private MechanicEntity Unit => MechanicEntityUIState.MechanicEntity.MechanicEntity;

	private MechanicEntityUIWrapper MechanicEntityUIWrapper => MechanicEntityUIState.MechanicEntity;

	public ReadOnlyReactiveProperty<bool> CanTarget => m_CanTarget;

	public ReadOnlyReactiveProperty<bool> IsCaster => MechanicEntityUIState.IsCaster;

	public ReadOnlyReactiveProperty<float> HitChance => m_HitChance;

	public ReadOnlyReactiveProperty<bool> HitAlways => m_HitAlways;

	public ReadOnlyReactiveProperty<float> InitialHitChance => m_InitialHitChance;

	public ReadOnlyReactiveProperty<int> BurstIndex => m_BurstIndex;

	public ReadOnlyReactiveProperty<float> DefenceChance => m_DefenceChance;

	public ReadOnlyReactiveProperty<float> CoverChance => m_CoverChance;

	public ReadOnlyReactiveProperty<bool> IsVisible => m_IsVisible;

	public AbilityData Ability => MechanicEntityUIState.AbilityTargetUIData.CurrentValue.Ability;

	public bool IsAttack { get; private set; }

	public OvertipHitChanceBlockVM(MechanicEntityUIState mechanicEntityUIState)
	{
		MechanicEntityUIState = mechanicEntityUIState;
		EventBus.Subscribe(this).AddTo(this);
		MechanicEntityUIState.IsInCombat.CombineLatest(MechanicEntityUIState.IsVisibleForPlayer, MechanicEntityUIState.IsDeadOrUnconsciousIsDead, MechanicEntityUIState.IsMouseOverUnit, MechanicEntityUIState.IsTarget, MechanicEntityUIState.Ability, MechanicEntityUIState.AbilityTargetUIData, (bool isInCombat, bool isVisibleForPlayer, bool isDead, bool isHovered, bool isTarget, AbilityData ability, AbilityTargetUIData abilityUIData) => new { isInCombat, isVisibleForPlayer, isDead, isHovered, isTarget, ability, abilityUIData }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(data =>
		{
			UpdateHitChanceVisibility(data.isInCombat, data.isVisibleForPlayer, data.isDead, data.isHovered, data.isTarget, data.ability);
		})
			.AddTo(this);
	}

	protected override void OnDispose()
	{
		ClearProperties();
	}

	private void UpdateHitChanceVisibility(bool isInCombat, bool isVisibleForPlayer, bool isDead, bool isHovered, bool isTarget, AbilityData ability)
	{
		bool isVisible = CheckIsVisible(isInCombat, isVisibleForPlayer, isDead, isHovered, isTarget, ability);
		OnVisibilityChanged(isVisible);
	}

	private bool CheckIsVisible(bool isInCombat, bool isVisibleForPlayer, bool isDead, bool isHovered, bool isTarget, AbilityData ability)
	{
		if (!isInCombat || !isVisibleForPlayer || isDead || ability == null)
		{
			return false;
		}
		if (ability.IsAoe || ability.IsBurst)
		{
			return isTarget;
		}
		bool canTargetFromDesiredPosition = MechanicEntityUIState.AbilityTargetUIData.CurrentValue.CanTargetFromDesiredPosition;
		MechanicEntity mechanicEntity = MechanicEntityUIState.MechanicEntity.MechanicEntity;
		if (ability.IsSingleTarget && ability.IsRanged)
		{
			return (!mechanicEntity.IsPlayerFaction && canTargetFromDesiredPosition) || isHovered;
		}
		if (ability.IsHeal)
		{
			return (mechanicEntity.IsPlayerFaction && canTargetFromDesiredPosition) || isHovered;
		}
		return isHovered;
	}

	private void OnVisibilityChanged(bool isVisible)
	{
		if (!isVisible)
		{
			ClearProperties();
		}
		else
		{
			UpdateProperties();
		}
		m_IsVisible.Value = isVisible;
		if (isVisible)
		{
			m_IsVisible.ForceNotify();
		}
	}

	private void UpdateProperties()
	{
		AbilityData currentValue = MechanicEntityUIState.Ability.CurrentValue;
		if (currentValue == null)
		{
			ClearProperties();
			return;
		}
		ClearBurstHitChances();
		AbilityTargetUIData currentValue2 = MechanicEntityUIState.AbilityTargetUIData.CurrentValue;
		m_CanTarget.Value = MechanicEntityUIState.IsTarget.CurrentValue || CanTargetByAbility(currentValue);
		m_HitAlways.Value = currentValue2.HitChance.HitAlways;
		m_BurstIndex.Value = currentValue2.AttacksCount;
		m_HitChance.Value = currentValue2.HitChance.HitWithAvoidanceChance;
		m_InitialHitChance.Value = currentValue2.HitChance.InitialHitChance;
		m_DefenceChance.Value = currentValue2.HitChance.DefenceChance;
		m_CoverChance.Value = currentValue2.HitChance.CoverChance;
		IsAttack = currentValue.Blueprint.AttackType.HasValue;
	}

	private bool CanTargetByAbility(AbilityData ability)
	{
		VirtualPositionController virtualPositionController = Game.Instance.Controllers.VirtualPositionController;
		ClickWithSelectedAbilityHandler selectedAbilityHandler = Game.Instance.Controllers.SelectedAbilityHandler;
		PointerController clickEventsController = Game.Instance.Controllers.ClickEventsController;
		TargetWrapper targetWrapper = ((MechanicEntityUIState.IsMouseOverUnit.CurrentValue && (bool)clickEventsController.PointerOn) ? selectedAbilityHandler.GetTargetForDesiredPosition(clickEventsController.PointerOn, clickEventsController.WorldPosition) : ((TargetWrapper)Unit));
		AbilityData.UnavailabilityReasonType? unavailabilityReason = null;
		bool flag = targetWrapper != null && ability.CanTargetFromDesiredPosition(targetWrapper, out unavailabilityReason);
		bool flag2 = IsValidAoETarget(ability.Blueprint.AoETargets);
		if (ability.IsSingleTarget || !flag2)
		{
			return flag && flag2;
		}
		if (ability.NeedLoS && unavailabilityReason.HasValue && unavailabilityReason.GetValueOrDefault() == AbilityData.UnavailabilityReasonType.HasNoLosToTarget)
		{
			return false;
		}
		Vector3 desiredPosition = virtualPositionController.GetDesiredPosition(ability.Caster);
		OrientedPatternData pattern = ability.GetPattern(Unit, desiredPosition);
		NodeList occupiedNodes = Unit.GetOccupiedNodes();
		bool result = false;
		foreach (GridNodeBase node in pattern.Nodes)
		{
			if (occupiedNodes.Contains(node))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private void ClearProperties()
	{
		m_HitAlways.Value = false;
		m_HitChance.Value = 0f;
		m_InitialHitChance.Value = 0f;
		m_BurstIndex.Value = 0;
		m_DefenceChance.Value = 0f;
		m_CoverChance.Value = 0f;
		m_CanTarget.Value = false;
		IsAttack = false;
		ClearBurstHitChances();
	}

	private void ClearBurstHitChances()
	{
		m_BurstHitChancesCollection.Clear();
	}

	private bool IsValidAoETarget(TargetType targetType)
	{
		switch (targetType)
		{
		case TargetType.Enemy:
			return MechanicEntityUIWrapper.IsPlayerEnemy;
		case TargetType.Ally:
			if (!MechanicEntityUIWrapper.IsPlayer)
			{
				return MechanicEntityUIWrapper.IsPlayerFaction;
			}
			return true;
		case TargetType.Any:
			return true;
		default:
			return false;
		}
	}
}
