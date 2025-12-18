using Code.View.UI.UIUtils;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class UnitMoraleVM : ViewModel
{
	private readonly ReactiveProperty<int> m_MoraleValue = new ReactiveProperty<int>();

	private readonly ReactiveProperty<UIMoralePredictionData> m_MoraleDeltaValue = new ReactiveProperty<UIMoralePredictionData>();

	private readonly ReactiveProperty<MoralePhaseType> m_MoralePhase = new ReactiveProperty<MoralePhaseType>();

	private readonly ReactiveProperty<bool> m_WillBecomeHeroic = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_WillBecomeBroken = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsVisibleTrigger = new ReactiveProperty<bool>();

	private readonly ReadOnlyReactiveProperty<bool> m_HasMorale;

	public readonly MechanicEntityUIState MechanicEntityUIState;

	private MechanicEntity Unit => MechanicEntityUIState.MechanicEntity.MechanicEntity;

	public ReadOnlyReactiveProperty<int> MoraleValue => m_MoraleValue;

	public ReadOnlyReactiveProperty<UIMoralePredictionData> MoraleDeltaValue => m_MoraleDeltaValue;

	public ReadOnlyReactiveProperty<MoralePhaseType> MoralePhase => m_MoralePhase;

	public ReadOnlyReactiveProperty<bool> WillBecomeHeroic => m_WillBecomeHeroic;

	public ReadOnlyReactiveProperty<bool> WillBecomeBroken => m_WillBecomeBroken;

	public ReadOnlyReactiveProperty<bool> IsMoraleLeader => MechanicEntityUIState.IsMoraleLeader;

	public Observable<Unit> UpdateMoraleValue { get; }

	public Observable<Unit> UpdateVisibility { get; }

	public UnitMoraleVM(MechanicEntityUIState mechanicEntityUIState)
	{
		MechanicEntityUIState = mechanicEntityUIState;
		m_HasMorale = MechanicEntityUIState.IsInCombat.And(MechanicEntityUIState.IsDeadOrUnconsciousIsDead.Not()).And(MechanicEntityUIState.Morale.Select((IUIUnitMoraleData v) => v != null)).ToReadOnlyReactiveProperty(initialValue: false)
			.AddTo(this);
		MechanicEntityUIState.MoraleChanged.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(OnMoraleChanged).AddTo(this);
		MechanicEntityUIState.MoralePrediction.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(OnMoralPredictionChanged).AddTo(this);
		MechanicEntityUIState.IsInCombat.CombineLatest(MechanicEntityUIState.IsVisibleForPlayer, MechanicEntityUIState.IsDeadOrUnconsciousIsDead, MechanicEntityUIState.IsMouseOverUnit, MechanicEntityUIState.IsAoETarget, (bool isInCombat, bool isVisibleForPlayer, bool isDead, bool isHover, bool isAoETarget) => isInCombat && isVisibleForPlayer && !isDead && (isHover || isAoETarget)).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate(bool value)
		{
			m_IsVisibleTrigger.Value = value;
		})
			.AddTo(this);
		m_IsVisibleTrigger.CombineLatest(MechanicEntityUIState.AbilityTargetUIData, (bool visible, AbilityTargetUIData uiData) => visible).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(OnVisibilityChanged)
			.AddTo(this);
		UpdateMoraleValue = MoraleValue.CombineLatest(MoraleDeltaValue, MoralePhase, WillBecomeBroken, WillBecomeHeroic, (int _, UIMoralePredictionData _, MoralePhaseType _, bool _, bool _) => default(Unit)).DebounceFrame(1, UnityFrameProvider.PreLateUpdate);
		UpdateVisibility = MechanicEntityUIState.IsMouseOverUnit.CombineLatest(m_HasMorale, MechanicEntityUIState.ForceHotKeyPressed, MechanicEntityUIState.IsInCombat, MoraleValue, MoraleDeltaValue, MoralePhase, (bool _, bool _, bool _, bool _, int _, UIMoralePredictionData _, MoralePhaseType _) => default(Unit)).DebounceFrame(1, UnityFrameProvider.PreLateUpdate);
		OnMoraleChanged();
	}

	public bool IsVisible()
	{
		if (!m_HasMorale.CurrentValue)
		{
			return false;
		}
		if (m_MoraleValue.CurrentValue == 0)
		{
			return !m_MoraleDeltaValue.Value.Equals(default(UIMoralePredictionData));
		}
		return true;
	}

	private void OnVisibilityChanged(bool state)
	{
		if (!state)
		{
			ClearMoraleValuePrediction();
		}
		else
		{
			UpdateMoraleValuePrediction();
		}
	}

	private void OnMoralPredictionChanged(IUIUnitMoraleData morale)
	{
		if (morale == null)
		{
			m_WillBecomeHeroic.Value = false;
			m_WillBecomeBroken.Value = false;
		}
		else
		{
			IUIUnitMoraleData currentValue = MechanicEntityUIState.Morale.CurrentValue;
			m_WillBecomeHeroic.Value = UIUtilityUnit.MoraleWillBecomeHeroic(currentValue, morale);
			m_WillBecomeBroken.Value = UIUtilityUnit.MoraleWillBecomeBroken(currentValue, morale);
		}
	}

	private void OnMoraleChanged()
	{
		IUIUnitMoraleData currentValue = MechanicEntityUIState.Morale.CurrentValue;
		if (currentValue != null)
		{
			m_MoralePhase.Value = currentValue?.MoralePhase ?? MoralePhaseType.Regular;
			m_MoraleValue.Value = currentValue?.Morale ?? 0;
		}
	}

	private void UpdateMoraleValuePrediction()
	{
		if (MechanicEntityUIState.Ability.CurrentValue == null)
		{
			ClearMoraleValuePrediction();
			return;
		}
		TargetWrapper targetForDesiredPosition = Game.Instance.Controllers.SelectedAbilityHandler.GetTargetForDesiredPosition(Unit.View.gameObject, Game.Instance.Controllers.ClickEventsController.WorldPosition);
		if (targetForDesiredPosition != null && MechanicEntityUIState.Ability.CurrentValue.CanTargetFromDesiredPosition(targetForDesiredPosition))
		{
			AbilityTargetUIData currentValue = MechanicEntityUIState.AbilityTargetUIData.CurrentValue;
			m_MoraleDeltaValue.Value = currentValue.Morale;
		}
	}

	private void ClearMoraleValuePrediction()
	{
		m_MoraleDeltaValue.Value = default(UIMoralePredictionData);
	}
}
