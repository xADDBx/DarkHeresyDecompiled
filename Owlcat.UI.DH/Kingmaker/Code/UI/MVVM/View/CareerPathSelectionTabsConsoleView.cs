using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CareerPathSelectionTabsConsoleView : CareerPathSelectionTabsCommonView
{
	[SerializeField]
	protected CareerPathDescriptionConsoleView m_CareerPathDescriptionConsoleView;

	[SerializeField]
	protected CareerPathSelectionsSummaryConsoleView m_CareerPathSelectionsSummaryConsoleView;

	[SerializeField]
	protected RankEntryFeatureDescriptionConsoleView m_RankEntryFeatureDescriptionConsoleView;

	[SerializeField]
	protected RankEntryFeatureSelectionConsoleView m_RankEntryFeatureSelectionConsoleView;

	public override void Initialize()
	{
		Tabs = new List<ICareerPathSelectionTabView> { m_CareerPathDescriptionConsoleView, m_CareerPathSelectionsSummaryConsoleView, m_RankEntryFeatureDescriptionConsoleView, m_RankEntryFeatureSelectionConsoleView };
		Tabs.ForEach(delegate(ICareerPathSelectionTabView tab)
		{
			tab.Initialize();
		});
	}

	protected override void SetNewTab(SelectionTab newTab, IRankEntrySelectItem currentItem)
	{
		switch (newTab)
		{
		case SelectionTab.CareerPathDescription:
			m_CareerPathDescriptionConsoleView.Bind(base.ViewModel);
			break;
		case SelectionTab.FeatureDescription:
			m_RankEntryFeatureDescriptionConsoleView.Bind(currentItem as RankEntryFeatureItemVM);
			break;
		case SelectionTab.FeatureSelection:
			m_RankEntryFeatureSelectionConsoleView.Bind(currentItem as RankEntrySelectionVM);
			break;
		}
	}

	public void AddInput()
	{
	}
}
