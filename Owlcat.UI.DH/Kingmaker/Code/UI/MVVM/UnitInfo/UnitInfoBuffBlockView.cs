using System.Collections.Generic;
using System.Linq;
using Code.Enums;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoBuffBlockView : View<UnitInfoBuffBlockVM>
{
	[Header("Groups")]
	[SerializeField]
	private UnitInfoBuffGroupWidget m_CriticalGroup;

	[SerializeField]
	private UnitInfoBuffGroupWidget m_StatusGroup;

	[SerializeField]
	private UnitInfoBuffGroupWidget m_DOTGroup;

	[SerializeField]
	private UnitInfoBuffGroupWidget m_NegativeGroup;

	[SerializeField]
	private UnitInfoBuffGroupWidget m_PositiveGroup;

	[Header("Icons")]
	[SerializeField]
	private BuffsGroupWidget m_CriticalWidget;

	[SerializeField]
	private BuffsGroupWidget m_StatusWidget;

	[SerializeField]
	private DOTGroupWidget m_DOTWidget;

	protected override void OnBind()
	{
		m_CriticalGroup.SetHeaderText(base.ViewModel.CriticalGroupHeader);
		m_StatusGroup.SetHeaderText(base.ViewModel.StatusGroupHeader);
		m_DOTGroup.SetHeaderText(base.ViewModel.DOTGroupHeader);
		m_NegativeGroup.SetHeaderText(base.ViewModel.NegativeGroupHeader);
		m_PositiveGroup.SetHeaderText(base.ViewModel.PositiveGroupHeader);
		base.ViewModel.BuffBlockVM.CriticalEffects.Subscribe(HandleCriticalEffectsChanged).AddTo(this);
		base.ViewModel.BuffBlockVM.StatusEffects.Subscribe(HandleStatusEffectsChanged).AddTo(this);
		base.ViewModel.BuffBlockVM.DOTEffects.Subscribe(HandleDOTEffectsChanged).AddTo(this);
		base.ViewModel.CriticalEffects.Buffs.Subscribe(delegate(IReadOnlyList<BuffVM> buffs)
		{
			m_CriticalGroup.SetBuffs(buffs);
		}).AddTo(this);
		base.ViewModel.StatusEffects.Buffs.Subscribe(delegate(IReadOnlyList<BuffVM> buffs)
		{
			m_StatusGroup.SetBuffs(buffs);
		}).AddTo(this);
		base.ViewModel.DOTEffects.Buffs.Subscribe(delegate(IReadOnlyList<BuffVM> buffs)
		{
			m_DOTGroup.SetBuffs(buffs);
		}).AddTo(this);
		base.ViewModel.NegativeEffects.Buffs.Subscribe(delegate(IReadOnlyList<BuffVM> buffs)
		{
			m_NegativeGroup.SetBuffs(buffs);
		}).AddTo(this);
		base.ViewModel.PositiveEffects.Buffs.Subscribe(delegate(IReadOnlyList<BuffVM> buffs)
		{
			m_PositiveGroup.SetBuffs(buffs);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_CriticalGroup.ClearBuffs();
		m_StatusGroup.ClearBuffs();
		m_DOTGroup.ClearBuffs();
		m_NegativeGroup.ClearBuffs();
		m_PositiveGroup.ClearBuffs();
	}

	private void HandleCriticalEffectsChanged(CriticalEffectsUIData data)
	{
		bool flag = data.Count > 0;
		m_CriticalWidget.SetActive(flag);
		if (flag)
		{
			string activeLayer = ((data.Count == 1) ? $"Single_{data.HighestRank}" : $"Multiple_{data.HighestRank}");
			m_CriticalWidget.SetActiveLayer(activeLayer);
		}
	}

	private void HandleStatusEffectsChanged(StatusEffectsUIData data)
	{
		bool flag = data.Count > 0;
		m_StatusWidget.SetActive(flag);
		if (flag)
		{
			string activeLayer = data.HighestSeverity.ToString();
			m_StatusWidget.SetCount(data.Count);
			m_StatusWidget.SetActiveLayer(activeLayer);
		}
	}

	private void HandleDOTEffectsChanged(DOTEffectsUIData data)
	{
		int count = data.DotEffects.Count;
		m_DOTWidget.SetEffectsCount(count);
		if (count == 1)
		{
			DOT item = data.DotEffects.First().dotType;
			m_DOTWidget.SetActiveLayerSingle(item.ToString());
			return;
		}
		int num = 0;
		foreach (var dotEffect in data.DotEffects)
		{
			DOT item2 = dotEffect.dotType;
			if (num >= m_DOTWidget.MaxEffectsCount)
			{
				break;
			}
			m_DOTWidget.SetActiveLayerMultiple(item2.ToString(), num);
			num++;
		}
	}
}
