using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CareerPathSelectionTabsPCView : CareerPathSelectionTabsCommonView
{
	[SerializeField]
	protected CareerPathDescriptionPCView m_CareerPathDescriptionPCView;

	[SerializeField]
	protected CareerPathSelectionsSummaryPCView m_CareerPathSelectionsSummaryPCView;

	[SerializeField]
	protected RankEntryFeatureDescriptionPCView m_RankEntryFeatureDescriptionPCView;

	[SerializeField]
	protected RankEntryFeatureSelectionPCView m_RankEntryFeatureSelectionPCView;

	private CareerButtonsBlock m_ButtonsBlock;

	public void SetButtonsBlock(CareerButtonsBlock buttonsBlock)
	{
		m_ButtonsBlock = buttonsBlock;
	}

	public override void Initialize()
	{
		Tabs = new List<ICareerPathSelectionTabView> { m_CareerPathDescriptionPCView, m_CareerPathSelectionsSummaryPCView, m_RankEntryFeatureDescriptionPCView, m_RankEntryFeatureSelectionPCView };
		Tabs.ForEach(delegate(ICareerPathSelectionTabView tab)
		{
			tab.Initialize();
			(tab as ICareerPathSelectionTabPCView)?.SetButtonsBlock(m_ButtonsBlock);
		});
	}

	protected override void SetNewTab(SelectionTab newTab, IRankEntrySelectItem currentItem)
	{
		switch (newTab)
		{
		case SelectionTab.CareerPathDescription:
			m_CareerPathDescriptionPCView.Bind(base.ViewModel);
			break;
		case SelectionTab.FeatureDescription:
			m_RankEntryFeatureDescriptionPCView.Bind(currentItem as RankEntryFeatureItemVM);
			break;
		case SelectionTab.FeatureSelection:
			m_RankEntryFeatureSelectionPCView.Bind(currentItem as RankEntrySelectionVM);
			break;
		}
	}
}
