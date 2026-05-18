using System;
using Code.View.UI.UIUtils;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.Morale;

public class ActionBarMoraleVM : ActionBarBasePartVM
{
	private readonly ReactiveProperty<int> m_MoraleValue = new ReactiveProperty<int>();

	private readonly ReactiveProperty<MoralePhaseType> m_MoralePhase = new ReactiveProperty<MoralePhaseType>();

	private readonly ReactiveProperty<bool> m_HasMorale = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_WillBecomeHeroic = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_WillBecomeBroken = new ReactiveProperty<bool>();

	private readonly ReactiveCommand<Unit> m_AbilitiesListUpdated = new ReactiveCommand<Unit>();

	private IDisposable m_HasMoraleSubscription;

	private IDisposable m_MoraleSubscription;

	private IDisposable m_MoralePredictionSubscription;

	private MechanicEntityUIState m_EntityUIState;

	public readonly AutoDisposingList<ActionBarSlotVM> HeroicSlots = new AutoDisposingList<ActionBarSlotVM>();

	public readonly AutoDisposingList<ActionBarSlotVM> BrokenSlots = new AutoDisposingList<ActionBarSlotVM>();

	public ReadOnlyReactiveProperty<int> MoraleValue => m_MoraleValue;

	public ReadOnlyReactiveProperty<MoralePhaseType> MoralePhase => m_MoralePhase;

	public ReadOnlyReactiveProperty<bool> HasMorale => m_HasMorale;

	public ReadOnlyReactiveProperty<bool> WillBecomeHeroic => m_WillBecomeHeroic;

	public ReadOnlyReactiveProperty<bool> WillBecomeBroken => m_WillBecomeBroken;

	public Observable<Unit> AbilitiesListUpdated => m_AbilitiesListUpdated;

	public int MinMorale { get; private set; }

	public int MaxMorale { get; private set; }

	public TooltipBaseTemplate GetMoraleTooltipTemplate()
	{
		if (!(m_EntityUIState?.MechanicEntity.MechanicEntity?.IsInPlayerParty).GetValueOrDefault())
		{
			return null;
		}
		return new TooltipTemplateMoraleUnit(m_EntityUIState);
	}

	protected override void OnUnitChanged()
	{
		DisposeSubscriptions();
		m_EntityUIState = UnitUIStateHolder.Instance.GetOrCreateUnitState((BaseUnitEntity)Unit);
		m_HasMoraleSubscription = m_EntityUIState.IsInCombat.CombineLatest(m_EntityUIState.IsPlayerFaction, m_EntityUIState.IsDeadOrUnconsciousIsDead, m_EntityUIState.Morale, (bool isInCombat, bool isPlayerFaction, bool isDeadOrUnconsciousIsDead, IUIUnitMoraleData morale) => isInCombat && isPlayerFaction && !isDeadOrUnconsciousIsDead && morale != null).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(OnHasMoraleChanged)
			.AddTo(this);
		m_MoraleSubscription = m_EntityUIState.MoraleChanged.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(OnMoraleChanged).AddTo(this);
		m_MoralePredictionSubscription = m_EntityUIState.MoralePrediction.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(OnMoralPredictionChanged).AddTo(this);
		CreateSlots();
		OnMoraleChanged();
	}

	protected override void OnDispose()
	{
		DisposeSubscriptions();
		ClearSlots();
	}

	private void DisposeSubscriptions()
	{
		m_HasMoraleSubscription?.Dispose();
		m_MoraleSubscription?.Dispose();
		m_MoralePredictionSubscription?.Dispose();
		m_HasMoraleSubscription = null;
		m_MoraleSubscription = null;
		m_MoralePredictionSubscription = null;
	}

	private void OnHasMoraleChanged(bool hasMorale)
	{
		m_HasMorale.Value = hasMorale;
	}

	private void OnMoralPredictionChanged(IUIUnitMoraleData morale)
	{
		if (morale != null)
		{
			m_WillBecomeHeroic.Value = UIUtilityUnit.MoraleWillBecomeHeroic(m_EntityUIState.Morale.CurrentValue, morale);
			m_WillBecomeBroken.Value = UIUtilityUnit.MoraleWillBecomeBroken(m_EntityUIState.Morale.CurrentValue, morale);
		}
	}

	private void OnMoraleChanged()
	{
		IUIUnitMoraleData currentValue = m_EntityUIState.Morale.CurrentValue;
		m_MoralePhase.Value = currentValue?.MoralePhase ?? MoralePhaseType.Regular;
		m_MoraleValue.Value = currentValue?.Morale ?? 0;
		MinMorale = currentValue?.MinValue ?? 0;
		MaxMorale = currentValue?.MaxValue ?? 0;
		UpdateSlots();
	}

	protected override void ClearSlots()
	{
		HeroicSlots.Clear();
		BrokenSlots.Clear();
	}

	private void CreateSlots()
	{
		ClearSlots();
		if (!(m_EntityUIState.MechanicEntity.MechanicEntity is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		int num = 0;
		foreach (MechanicActionBarSlot heroicBrokenSlot in baseUnitEntity.ActionBar.HeroicBrokenSlots)
		{
			ActionBarSlotVM item = new ActionBarSlotVM(heroicBrokenSlot, num);
			AbilityData abilityData = heroicBrokenSlot.GetContentData() as AbilityData;
			if (!(abilityData == null))
			{
				if (abilityData.Blueprint.IsHeroic)
				{
					HeroicSlots.Add(item);
					num++;
				}
				else if (abilityData.Blueprint.IsBroken)
				{
					BrokenSlots.Add(item);
					num++;
				}
			}
		}
		m_AbilitiesListUpdated.Execute(R3.Unit.Default);
	}

	private void UpdateSlots()
	{
		foreach (ActionBarSlotVM heroicSlot in HeroicSlots)
		{
			heroicSlot.UpdateResources();
		}
		foreach (ActionBarSlotVM brokenSlot in BrokenSlots)
		{
			brokenSlot.UpdateResources();
		}
	}
}
