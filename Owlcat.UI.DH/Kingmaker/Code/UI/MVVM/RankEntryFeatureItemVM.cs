using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Levelup.Selections;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class RankEntryFeatureItemVM : BaseRankEntryFeatureVM, IRankEntrySelectItem, IHasTooltipTemplates
{
	private InfoSectionVM m_InfoVM;

	private readonly Action<IRankEntrySelectItem> m_SelectAction;

	private readonly ReactiveProperty<bool> m_HasUnavailableFeatures = new ReactiveProperty<bool>();

	public InfoSectionVM InfoVM => m_InfoVM ?? CreateInfoVM();

	public int EntryRank => Rank.GetValueOrDefault();

	public ReadOnlyReactiveProperty<bool> HasUnavailableFeatures => m_HasUnavailableFeatures;

	public RankEntryFeatureItemVM(int rank, CareerPathVM careerPathVM, UIFeature uiFeature, Action<IRankEntrySelectItem> selectAction)
		: base(careerPathVM, uiFeature)
	{
		Rank = rank;
		m_SelectAction = selectAction;
		AddDisposable(CareerPathVM.FeaturesToVisit.ObserveCountChanged().Subscribe(delegate
		{
			DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(UpdateFeatureState);
		}));
		UpdateFeatureState();
	}

	private InfoSectionVM CreateInfoVM()
	{
		AddDisposable(m_InfoVM = new InfoSectionVM());
		InfoVM.SetTemplate(base.Tooltip.CurrentValue);
		return m_InfoVM;
	}

	protected sealed override void UpdateFeatureState()
	{
		RankFeatureState value = RankFeatureState.NotActive;
		(int, int) currentLevelupRange = CareerPathVM.GetCurrentLevelupRange();
		if ((CareerPathVM.IsInLevelupProcess || (currentLevelupRange.Item1 == 1 && currentLevelupRange.Item2 == 1)) && currentLevelupRange.Item1 != -1 && Rank >= currentLevelupRange.Item1 && Rank <= currentLevelupRange.Item2)
		{
			value = (CareerPathVM.IsVisited(this) ? RankFeatureState.Selected : RankFeatureState.Selectable);
		}
		else if (Rank <= CareerPathVM.CurrentRank.CurrentValue)
		{
			value = RankFeatureState.Committed;
		}
		m_FeatureState.Value = value;
	}

	public override void Select()
	{
		m_SelectAction?.Invoke(this);
	}

	public FeatureGroup? GetFeatureGroup()
	{
		return null;
	}

	public override bool CanSelect()
	{
		(int, int) currentLevelupRange = CareerPathVM.GetCurrentLevelupRange();
		if (base.FeatureState.CurrentValue != RankFeatureState.NotActive && Rank >= currentLevelupRange.Item1 && Rank <= currentLevelupRange.Item2)
		{
			return CareerPathVM.IsInLevelupProcess;
		}
		return false;
	}

	public void UpdateFeatures()
	{
	}

	public void UpdateReadOnlyState()
	{
	}

	public void ToggleShowUnavailableFeatures()
	{
	}

	public bool ContainsFeature(string key)
	{
		return false;
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return new List<TooltipBaseTemplate>
		{
			base.Tooltip.CurrentValue,
			base.HintTooltip
		};
	}
}
