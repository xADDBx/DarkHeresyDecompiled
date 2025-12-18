using System;
using System.Linq;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoFeatureGroupConsoleView : CharInfoFeatureGroupPCView
{
	[Header("Console")]
	[SerializeField]
	private int m_ItemsInRow = 3;

	public CharInfoFeatureGroupVM.FeatureGroupType GroupType => m_GroupType;

	public void SetupChooseModeActions(Action<CharInfoFeatureConsoleView> onClick, Action<CharInfoFeatureConsoleView> onFocus)
	{
		m_WidgetList.Entries?.ForEach(delegate(IBindable e)
		{
			(e as CharInfoFeatureConsoleView)?.SetupChooseModeActions(onClick, onFocus);
		});
	}

	public GridConsoleNavigationBehaviour GetNavigation()
	{
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour();
		if (m_WidgetList.Entries == null)
		{
			gridConsoleNavigationBehaviour.AddRow<ExpandableCollapseMultiButtonPC>(m_ExpandableElement);
			return gridConsoleNavigationBehaviour;
		}
		gridConsoleNavigationBehaviour.SetEntitiesGrid(m_WidgetList.Entries.Select((IBindable e) => e as CharInfoFeatureConsoleView).ToList(), m_ItemsInRow);
		gridConsoleNavigationBehaviour.InsertRow<ExpandableCollapseMultiButtonPC>(0, m_ExpandableElement);
		return gridConsoleNavigationBehaviour;
	}

	public IConsoleEntity GetFirstFeature()
	{
		return m_WidgetList.Entries?.FirstOrDefault((IBindable e) => e is CharInfoFeatureConsoleView) as IConsoleEntity;
	}
}
