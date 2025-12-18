using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using ObservableCollections;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class StatCheckLootUnitsPageVM : StatCheckLootPageVM
{
	public readonly ObservableList<StatCheckLootSmallUnitCardVM> SmallUnitSlotsVMs = new ObservableList<StatCheckLootSmallUnitCardVM>();

	private readonly ReactiveCommand<Unit> m_UpdateSmallUnitSlots = new ReactiveCommand<Unit>();

	private readonly ReactiveCommand<Unit> m_ClearSmallUnitSlots = new ReactiveCommand<Unit>();

	private readonly ReactiveProperty<StatCheckLootUnitCardVM> m_SelectedUnitCardVM = new ReactiveProperty<StatCheckLootUnitCardVM>();

	private BaseUnitEntity m_OldUnit;

	private BaseUnitEntity m_CurrentSelectedUnit;

	private StatType m_CurrentSelectedStatType;

	private readonly Action<BaseUnitEntity, StatType> m_CloseAction;

	public Observable<Unit> UpdateSmallUnitSlots => m_UpdateSmallUnitSlots;

	public Observable<Unit> ClearSmallUnitSlots => m_ClearSmallUnitSlots;

	public ReadOnlyReactiveProperty<StatCheckLootUnitCardVM> SelectedUnitCardVM => m_SelectedUnitCardVM;

	private List<BaseUnitEntity> m_Party => Game.Instance.Player.Party.Where((BaseUnitEntity u) => !u.LifeState.IsDead).ToList();

	public StatCheckLootUnitsPageVM(Action<BaseUnitEntity, StatType> closeAction)
	{
		EventBus.Subscribe(this).AddTo(this);
		m_CloseAction = closeAction;
	}

	protected override void OnDispose()
	{
		ClearSmallUnitSlotVMs();
	}

	public void ConfirmUnit()
	{
		ClosePage(confirmUnit: true);
	}

	public void BackWithoutConfirmUnit()
	{
		ClosePage(confirmUnit: false);
	}

	public void HandlePageOpened(BaseUnitEntity baseUnitEntity, StatType statType)
	{
		m_OldUnit = baseUnitEntity;
		m_CurrentSelectedUnit = baseUnitEntity;
		m_CurrentSelectedStatType = statType;
		SmallUnitSlotsVMs.Clear();
		foreach (BaseUnitEntity item in m_Party)
		{
			AddSmallUnitSlot(item, statType);
		}
		m_UpdateSmallUnitSlots.Execute();
		UpdateSelectedUnit();
	}

	public void HandleUnitSelected(BaseUnitEntity unitEntity)
	{
		m_CurrentSelectedUnit = unitEntity;
		UpdateSelectedUnit();
	}

	private void AddSmallUnitSlot(BaseUnitEntity unitEntity, StatType stat)
	{
		StatCheckLootSmallUnitCardVM item = new StatCheckLootSmallUnitCardVM(unitEntity, stat, HandleUnitSelected, unitEntity == m_CurrentSelectedUnit);
		SmallUnitSlotsVMs.Add(item);
	}

	private void ClearSmallUnitSlotVMs()
	{
		m_ClearSmallUnitSlots.Execute();
		SmallUnitSlotsVMs.Clear();
	}

	private void UpdateSelectedUnit()
	{
		m_SelectedUnitCardVM.Value?.Dispose();
		m_SelectedUnitCardVM.Value = new StatCheckLootUnitCardVM(m_CurrentSelectedUnit, m_CurrentSelectedStatType, null, null).AddTo(this);
	}

	private void ClosePage(bool confirmUnit)
	{
		m_CloseAction?.Invoke(confirmUnit ? m_CurrentSelectedUnit : m_OldUnit, m_CurrentSelectedStatType);
		ClearSmallUnitSlotVMs();
	}
}
