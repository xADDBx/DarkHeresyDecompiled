using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class StatCheckLootMainPageVM : StatCheckLootPageVM
{
	public readonly AutoDisposingDictionary<StatType, StatCheckLootUnitCardVM> UnitSlotVMByStatType = new AutoDisposingDictionary<StatType, StatCheckLootUnitCardVM>();

	private readonly ReactiveCommand<Unit> m_UpdateUnitSlots = new ReactiveCommand<Unit>();

	private readonly ReactiveCommand<Unit> m_ClearUnitSlots = new ReactiveCommand<Unit>();

	private readonly Action<BaseUnitEntity, StatType> m_SwitchUnitAction;

	private readonly Action<BaseUnitEntity, StatType> m_CheckStatAction;

	private readonly Action m_CloseAction;

	public Observable<Unit> UpdateUnitSlots => m_UpdateUnitSlots;

	public Observable<Unit> ClearUnitSlots => m_ClearUnitSlots;

	private List<BaseUnitEntity> m_Party => Game.Instance.Player.Party.Where((BaseUnitEntity u) => !u.LifeState.IsDead).ToList();

	public StatCheckLootMainPageVM(Action<BaseUnitEntity, StatType> switchUnitAction, Action<BaseUnitEntity, StatType> checkStatAction, Action closeAction)
	{
		EventBus.Subscribe(this).AddTo(this);
		m_SwitchUnitAction = switchUnitAction;
		m_CheckStatAction = checkStatAction;
		m_CloseAction = closeAction;
	}

	protected override void OnDispose()
	{
		ClearUnitSlotVMs();
	}

	public void SwitchUnit()
	{
		foreach (KeyValuePair<StatType, StatCheckLootUnitCardVM> item in UnitSlotVMByStatType)
		{
			StatCheckLootUnitCardVM value = item.Value;
			if (value.IsSelected)
			{
				value.SwitchUnit();
				break;
			}
		}
	}

	public void CheckStat()
	{
		foreach (KeyValuePair<StatType, StatCheckLootUnitCardVM> item in UnitSlotVMByStatType)
		{
			StatCheckLootUnitCardVM value = item.Value;
			if (value.IsSelected)
			{
				value.CheckStat();
				break;
			}
		}
	}

	public void CloseDialog()
	{
		m_CloseAction?.Invoke();
	}

	private void AddUnitSlot(BaseUnitEntity unitEntity, StatType stat)
	{
		StatCheckLootUnitCardVM value = new StatCheckLootUnitCardVM(unitEntity, stat, m_CheckStatAction, m_SwitchUnitAction);
		UnitSlotVMByStatType.Add(stat, value);
	}

	private void ClearUnitSlotVMs()
	{
		m_ClearUnitSlots.Execute(Unit.Default);
		UnitSlotVMByStatType.Clear();
	}

	[CanBeNull]
	private BaseUnitEntity SelectBestUnit(List<BaseUnitEntity> units, StatType stat)
	{
		int bestStatValue = int.MinValue;
		BaseUnitEntity result = null;
		foreach (BaseUnitEntity item in units.Where((BaseUnitEntity u) => (int)u.Actor.GetStat(stat, null, default(StatContext), "SelectBestUnit") > bestStatValue))
		{
			bestStatValue = item.Actor.GetStat(stat, null, default(StatContext), "SelectBestUnit");
			result = item;
		}
		return result;
	}

	public void HandleConfirmSelectedUnit(BaseUnitEntity unitEntity, StatType statType)
	{
		if (UnitSlotVMByStatType.TryGetValue(statType, out var value))
		{
			value.Dispose();
			value = new StatCheckLootUnitCardVM(unitEntity, statType, m_CheckStatAction, m_SwitchUnitAction);
			UnitSlotVMByStatType[statType] = value;
		}
		m_UpdateUnitSlots.Execute(Unit.Default);
	}
}
