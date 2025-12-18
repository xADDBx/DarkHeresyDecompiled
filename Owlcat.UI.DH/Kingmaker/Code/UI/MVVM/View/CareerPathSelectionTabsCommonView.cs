using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.View;

public abstract class CareerPathSelectionTabsCommonView : View<CareerPathVM>
{
	public enum SelectionTab
	{
		CareerPathDescription,
		FeatureDescription,
		FeatureSelection
	}

	protected List<ICareerPathSelectionTabView> Tabs;

	protected SelectionTab? SavedTab;

	protected IRankEntrySelectItem SavedItem;

	public virtual void Initialize()
	{
	}

	protected override void OnBind()
	{
		base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Subscribe(delegate
		{
			UpdateActiveTab();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.OnUpdateData, delegate
		{
			UpdateActiveTab();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		UnbindTabs();
		SavedTab = null;
	}

	protected void UnbindTabs()
	{
		Tabs.ForEach(delegate(ICareerPathSelectionTabView tab)
		{
			tab.Unbind();
		});
	}

	protected virtual void UpdateActiveTab()
	{
		IRankEntrySelectItem currentValue = base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.CurrentValue;
		SelectionTab activeTab = GetActiveTab(currentValue);
		if (SavedTab != activeTab || currentValue != SavedItem)
		{
			SavedTab = activeTab;
			SavedItem = currentValue;
			UnbindTabs();
			SetNewTab(activeTab, currentValue);
		}
		UpdateState();
	}

	protected void UpdateState()
	{
		Tabs.Where((ICareerPathSelectionTabView tab) => tab.IsTabActive()).ForEach(delegate(ICareerPathSelectionTabView tab)
		{
			tab.UpdateState();
		});
	}

	private SelectionTab GetActiveTab(IRankEntrySelectItem currentItem)
	{
		if (!(currentItem is RankEntryFeatureItemVM))
		{
			if (currentItem is RankEntrySelectionVM)
			{
				return SelectionTab.FeatureSelection;
			}
			return SelectionTab.CareerPathDescription;
		}
		return SelectionTab.FeatureDescription;
	}

	protected abstract void SetNewTab(SelectionTab newTab, IRankEntrySelectItem currentItem);
}
