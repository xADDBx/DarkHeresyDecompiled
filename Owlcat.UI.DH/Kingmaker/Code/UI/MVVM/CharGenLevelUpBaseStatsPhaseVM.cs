using System;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Levelup.Selections.Stats;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.Utility.DotNetExtensions;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpBaseStatsPhaseVM<TSelectorItem> : CharGenLevelUpBasePhaseVM<TSelectorItem> where TSelectorItem : CharGenLevelUpCharacteristicsItemVM
{
	private static int POINT = 1;

	protected ReactiveProperty<int> m_RemainingPoints = new ReactiveProperty<int>();

	private CharGenLevelUpCharacteristicsItemVM m_PrevSelectedItem;

	public ReadOnlyReactiveProperty<int> RemainingPoints => m_RemainingPoints;

	public SelectionStateStats SelectionStats { get; private set; }

	private event Action m_OnPointsChanged;

	public CharGenLevelUpBaseStatsPhaseVM(CharGenContext charGenContext, SelectionStateStats selectionStats, CharGenPhaseType phaseType, InfoSectionVM infoSectionVM, int rank = 0)
		: base(charGenContext, phaseType, infoSectionVM, rank)
	{
		this.m_OnPointsChanged = OnPointsChanged;
		SelectionStats = selectionStats;
		CreateItemList();
	}

	private void OnPointsChanged()
	{
		m_RemainingPoints.Value = SelectionStats.Blueprint.PointsTotal - SelectionStats.PointsSpentTotal;
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleUISelectionChanged();
		});
		UpdateIsCompleted();
	}

	protected override void OnBeginDetailedView()
	{
		base.OnBeginDetailedView();
		Items.ForEach(delegate(TSelectorItem i)
		{
			i.UpdatePointsState();
		});
	}

	protected override bool CheckIsCompleted()
	{
		if (SelectionStats != null && SelectionStats.IsMade)
		{
			return SelectionStats.IsValid;
		}
		return false;
	}

	protected override void SaveSelection()
	{
		if (m_PrevSelectedItem != null)
		{
			SelectionStats.RemovePoints(m_PrevSelectedItem.Stat, POINT);
			m_PrevSelectedItem.UpdatePointsState();
		}
		if (base.SelectedItem.CurrentValue != null)
		{
			SelectionStats.AddPoints(base.SelectedItem.CurrentValue.Stat, POINT);
			base.SelectedItem.CurrentValue.UpdatePointsState();
		}
		m_PrevSelectedItem = base.SelectedItem.CurrentValue;
		UpdateIsCompleted();
		UpdateTooltip();
	}

	protected override void CreateItemList()
	{
		if (CareerPath == null || base.Unit == null)
		{
			Debug.LogError("CareerPath or Unit is null");
			return;
		}
		SelectionStats.Blueprint.Advancements.OrderBy((BlueprintStatAdvancement a) => StatTypeHelper.DisplayOrder.IndexOf(a.Stat)).ForEach(delegate(BlueprintStatAdvancement f)
		{
			AddItem(new CharGenLevelUpCharacteristicsItemVM(f, SelectionStats, CharGenContext.LevelUpManager.CurrentValue, this.m_OnPointsChanged, OnItemHovered) as TSelectorItem);
		});
	}
}
