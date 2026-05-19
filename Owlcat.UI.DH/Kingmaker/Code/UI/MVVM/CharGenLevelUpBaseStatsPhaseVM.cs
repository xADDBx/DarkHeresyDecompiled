using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Levelup.Selections.Stats;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpBaseStatsPhaseVM<TSelectorItem> : CharGenLevelUpBasePhaseVM<TSelectorItem> where TSelectorItem : CharGenLevelUpCharacteristicsItemVM
{
	private static int POINT = 1;

	protected ReactiveProperty<int> m_RemainingPoints = new ReactiveProperty<int>();

	private CharGenLevelUpCharacteristicsItemVM m_PrevSelectedItem;

	public readonly ObservableList<LevelUpSkillLinkedAttributeVM> BaseAttributeList = new ObservableList<LevelUpSkillLinkedAttributeVM>();

	public ReadOnlyReactiveProperty<int> RemainingPoints => m_RemainingPoints;

	public SelectionStateStats SelectionStats { get; private set; }

	public bool IsInChargen => m_CharGenContext.CharGenConfig.Mode != CharGenMode.LevelUp;

	public CharGenLevelUpBaseStatsPhaseVM(CharGenContext charGenContext, SelectionStateStats selectionStats, CharGenPhaseType phaseType, InfoSectionVM infoSectionVM, int rank = 0)
		: base(charGenContext, phaseType, infoSectionVM, rank)
	{
		SelectionStats = selectionStats;
		base.BlueprintSelectionWithUI = selectionStats.Blueprint;
		SetPhaseHint(base.BlueprintSelectionWithUI?.CallToAction?.Text ?? string.Empty);
		CreateItemList();
		m_PhaseName.Value = selectionStats.Blueprint.Title;
		base.DisplayMode = ((charGenContext.CharGenConfig.Mode != CharGenMode.LevelUp) ? CharGenDisplayMode.DollOnly : CharGenDisplayMode.PortraitOnly);
	}

	private void OnPointsChanged(CharGenLevelUpCharacteristicsItemVM item, bool isAdding)
	{
		if ((!isAdding || SelectionStats.PointsSpentTotal < SelectionStats.Blueprint.PointsTotal) && (isAdding || SelectionStats.GetPointsTotal(item.Stat) > 0))
		{
			if (isAdding)
			{
				SelectionStats.AddPoints(item.Stat, POINT);
			}
			else
			{
				SelectionStats.RemovePoints(item.Stat, POINT);
			}
			m_RemainingPoints.Value = SelectionStats.Blueprint.PointsTotal - SelectionStats.PointsSpentTotal;
			Items.ForEach(delegate(TSelectorItem i)
			{
				i.UpdatePointsState();
			});
			SaveSelection();
		}
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
		SelectionStateStats selectionStats = SelectionStats;
		if (selectionStats != null && selectionStats.PointsTotal == 1)
		{
			SaveBySelection();
		}
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleUISelectionChanged();
		});
		UpdateIsCompleted();
		UpdateTooltip();
	}

	private void SaveBySelection()
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
	}

	protected override void CreateItemList()
	{
		if (base.Unit == null)
		{
			Debug.LogError("CareerPath or Unit is null");
			return;
		}
		m_RemainingPoints.Value = SelectionStats.Blueprint.PointsTotal - SelectionStats.PointsSpentTotal;
		SelectionStats.Blueprint.Advancements.OrderBy((BlueprintStatAdvancement a) => StatTypeHelper.DisplayOrder.IndexOf(a.Stat)).ForEach(delegate(BlueprintStatAdvancement f)
		{
			AddItem(new CharGenLevelUpCharacteristicsItemVM(f, SelectionStats, m_CharGenContext.LevelUpManager.CurrentValue, OnPointsChanged, OnItemHovered) as TSelectorItem);
		});
	}

	protected override void OnItemHovered(SelectionGroupEntityVM item)
	{
		base.OnItemHovered(item);
		Items.ForEach(delegate(TSelectorItem i)
		{
			i.UpdatePointsState();
		});
	}
}
