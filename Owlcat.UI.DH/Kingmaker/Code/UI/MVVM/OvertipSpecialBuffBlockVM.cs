using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.View.Covers;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipSpecialBuffBlockVM : ViewModel, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IInterruptTurnStartHandler
{
	private const int BuffsShowCount = 3;

	private static readonly IReadOnlyList<BuffVM> m_EmptyList = new List<BuffVM>();

	private static Comparison<BuffVM> m_CommonComparison;

	private static Comparison<BuffVM> m_SpecialComparison;

	private readonly OvertipCoverBlockVM m_CoverBlockVM;

	private readonly UnitBuffBlockVM m_BuffBlockVM;

	private readonly List<BuffVM> m_CommonBuffsSorted = new List<BuffVM>();

	private readonly List<BuffVM> m_ImportantBuffsSorted = new List<BuffVM>();

	private readonly ReactiveProperty<IReadOnlyList<BuffVM>> m_ImportantBuffs;

	private readonly ReactiveProperty<IReadOnlyList<BuffVM>> m_CommonBuffs;

	public MechanicEntityUIState EntityUIState { get; }

	public Observable<IReadOnlyList<BuffVM>> ImportantBuffs => m_ImportantBuffs;

	public Observable<IReadOnlyList<BuffVM>> CommonBuffs => m_CommonBuffs;

	public OvertipSpecialBuffBlockVM(MechanicEntity target, UnitBuffBlockVM buffBlockVM, OvertipCoverBlockVM coverBlockVM)
	{
		EntityUIState = UnitUIStateHolder.Instance.GetOrCreateUnitState(target);
		if (m_CommonComparison == null)
		{
			m_CommonComparison = delegate(BuffVM lhs, BuffVM rhs)
			{
				int num2 = CommonBuffsSortingOrder(lhs.Buff);
				int value2 = CommonBuffsSortingOrder(rhs.Buff);
				return num2.CompareTo(value2);
			};
		}
		if (m_SpecialComparison == null)
		{
			m_SpecialComparison = delegate(BuffVM lhs, BuffVM rhs)
			{
				int num = ImportantBuffSortingOrder(lhs.Buff);
				int value = ImportantBuffSortingOrder(rhs.Buff);
				return num.CompareTo(value);
			};
		}
		m_CoverBlockVM = coverBlockVM;
		m_BuffBlockVM = buffBlockVM;
		m_ImportantBuffs = new ReactiveProperty<IReadOnlyList<BuffVM>>().AddTo(this);
		m_CommonBuffs = new ReactiveProperty<IReadOnlyList<BuffVM>>().AddTo(this);
		if (!EntityUIState.IsOvertipPartHiddenBySettings(UnitOvertipUIPart.SpecialBuffs))
		{
			m_BuffBlockVM.Buffs.ObserveAdd().Subscribe(delegate
			{
				UpdateBuffs();
			}).AddTo(this);
			m_BuffBlockVM.Buffs.ObserveRemove().Subscribe(delegate
			{
				UpdateBuffs();
			}).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(m_BuffBlockVM.Buffs.ObserveReset(), delegate
			{
				UpdateBuffs();
			}).AddTo(this);
			m_CoverBlockVM.CoverType.Subscribe(HandleCoverTypeChanged).AddTo(this);
			UpdateBuffs();
			EventBus.Subscribe(this).AddTo(this);
		}
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		UpdateBuffs();
	}

	void IInterruptTurnStartHandler.HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		UpdateBuffs();
	}

	private void UpdateBuffs()
	{
		m_CommonBuffsSorted.Clear();
		m_ImportantBuffsSorted.Clear();
		if (EntityUIState.IsOvertipPartHiddenBySettings(UnitOvertipUIPart.SpecialBuffs))
		{
			return;
		}
		foreach (BuffVM buff in m_BuffBlockVM.Buffs)
		{
			if (buff.Buff?.Blueprint != null && buff.IsSpecialBuff())
			{
				(buff.IsImportantBuff() ? m_ImportantBuffsSorted : m_CommonBuffsSorted).Add(buff);
			}
		}
		m_ImportantBuffsSorted.Sort(m_SpecialComparison);
		m_CommonBuffsSorted.Sort(m_CommonComparison);
		UpdateBuffsLists(m_CoverBlockVM.CoverType.CurrentValue);
	}

	private void HandleCoverTypeChanged(LosCalculations.CoverType? coverType)
	{
		UpdateBuffsLists(coverType);
	}

	private void UpdateBuffsLists(LosCalculations.CoverType? coverType)
	{
		int num = 3 - (coverType.HasValue ? 1 : 0);
		UpdateBuffsList(m_ImportantBuffs, m_ImportantBuffsSorted, num);
		int count = m_ImportantBuffs.CurrentValue.Count;
		int desiredCount = Mathf.Max(0, num - count);
		UpdateBuffsList(m_CommonBuffs, m_CommonBuffsSorted, desiredCount);
	}

	private void UpdateBuffsList(ReactiveProperty<IReadOnlyList<BuffVM>> buffsToUpdate, IReadOnlyList<BuffVM> sortedBuffs, int desiredCount)
	{
		if (sortedBuffs.Count < 1 || desiredCount < 1)
		{
			buffsToUpdate.Value = m_EmptyList;
			return;
		}
		int count = Mathf.Min(desiredCount, sortedBuffs.Count);
		List<BuffVM> value = sortedBuffs.Take(count).ToList();
		buffsToUpdate.Value = value;
	}

	private static int ImportantBuffSortingOrder(Buff buff)
	{
		if (buff.Blueprint.HasBuffOverrideUIOrder)
		{
			return 0;
		}
		if (!buff.Blueprint.IsDOTVisual)
		{
			return 1;
		}
		return 2;
	}

	private static int CommonBuffsSortingOrder(Buff buff)
	{
		if (buff.Blueprint.HasBuffOverrideUIOrder)
		{
			return 0;
		}
		if (!buff.Blueprint.IsDOTVisual)
		{
			return 2;
		}
		return 1;
	}
}
