using System.Collections.Generic;
using System.Linq;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CareerPathRankEntryVM : ViewModel
{
	private readonly CareerPathVM m_CareerPathVM;

	public readonly int Rank;

	public readonly bool IsEmpty;

	private readonly ReactiveProperty<bool> m_IsSelected = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsFirstSelectable = new ReactiveProperty<bool>();

	public readonly AutoDisposingList<RankEntryFeatureItemVM> Features = new AutoDisposingList<RankEntryFeatureItemVM>();

	public readonly AutoDisposingList<RankEntrySelectionVM> Selections = new AutoDisposingList<RankEntrySelectionVM>();

	public ReadOnlyReactiveProperty<bool> IsSelected => m_IsSelected;

	public ReadOnlyReactiveProperty<bool> IsFirstSelectable => m_IsFirstSelectable;

	public CareerPathRankEntryVM(int rank, CareerPathVM careerPathVM, BlueprintPath.RankEntry rankEntry)
	{
		CareerPathRankEntryVM careerPathRankEntryVM = this;
		Rank = rank;
		IsEmpty = rankEntry.Features.Length == 0 && rankEntry.Selections.Length == 0;
		m_CareerPathVM = careerPathVM;
		foreach (BlueprintFeature feature in rankEntry.Features)
		{
			RankEntryFeatureItemVM item = new RankEntryFeatureItemVM(rank, careerPathVM, new UIFeature(feature), SelectRankEntryItem).AddTo(this);
			Features.Add(item);
		}
		foreach (BlueprintSelection selection in rankEntry.Selections)
		{
			if (selection is BlueprintSelectionFeature selectionFeature)
			{
				RankEntrySelectionVM item2 = new RankEntrySelectionVM(rank, careerPathVM, selectionFeature, SelectRankEntryItem).AddTo(this);
				Selections.Add(item2);
			}
		}
		careerPathVM.CurrentRank.Subscribe(delegate(int currentRank)
		{
			careerPathRankEntryVM.m_IsSelected.Value = rank <= currentRank;
			careerPathRankEntryVM.m_IsFirstSelectable.Value = currentRank == rank;
		}).AddTo(this);
	}

	protected override void OnDispose()
	{
		Clear();
	}

	public void SelectRankEntryItem(IRankEntrySelectItem item)
	{
		m_CareerPathVM.SetRankEntry(item);
	}

	public void UpdateState(LevelUpManager levelUpManager)
	{
		Features.ForEach(delegate(RankEntryFeatureItemVM vm)
		{
			vm.UpdateState(levelUpManager);
		});
		Selections.ForEach(delegate(RankEntrySelectionVM vm)
		{
			vm.UpdateState(levelUpManager);
		});
	}

	private void Clear()
	{
		Features.Clear();
		Selections.Clear();
	}

	public IRankEntrySelectItem GetNextFor(IRankEntrySelectItem item)
	{
		int num = Features.IndexOf(item);
		if (num < 0)
		{
			if (num == -1)
			{
				int num2 = Selections.IndexOf(item);
				if (num2 >= Selections.Count - 1)
				{
					return null;
				}
				return Selections.ElementAt(num2 + 1);
			}
			return null;
		}
		if (num >= Features.Count - 1)
		{
			return Selections.FirstOrDefault();
		}
		return Features.ElementAt(num + 1);
	}

	public IRankEntrySelectItem GetPreviousFor(IRankEntrySelectItem item)
	{
		int num = Selections.IndexOf(item);
		int num2 = num;
		if (num2 <= 0)
		{
			if (num2 == 0 && Features.Count > 0)
			{
				return Features.LastItem();
			}
			int num3 = Features.IndexOf(item);
			if (num3 <= 0)
			{
				return null;
			}
			return Features.ElementAt(num3 - 1);
		}
		return Selections.ElementAt(num - 1);
	}

	public IRankEntrySelectItem GetFirstItem()
	{
		if (Features.Count > 0)
		{
			return Features.FirstItem();
		}
		return Selections.FirstItem();
	}

	public IRankEntrySelectItem GetLastItem()
	{
		if (Selections.Count > 0)
		{
			return Selections.LastItem();
		}
		return Features.LastItem();
	}

	public List<IRankEntrySelectItem> GetRankSlice()
	{
		List<IRankEntrySelectItem> list = new List<IRankEntrySelectItem>();
		IRankEntrySelectItem rankEntrySelectItem = GetFirstItem();
		if (rankEntrySelectItem == null)
		{
			return list;
		}
		while (rankEntrySelectItem != null)
		{
			list.Add(rankEntrySelectItem);
			rankEntrySelectItem = GetNextFor(rankEntrySelectItem);
		}
		return list;
	}
}
