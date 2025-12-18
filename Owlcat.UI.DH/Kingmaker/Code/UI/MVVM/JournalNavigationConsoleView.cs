using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class JournalNavigationConsoleView : JournalNavigationBaseView
{
	[SerializeField]
	private JournalNavigationGroupConsoleView m_NavigationGroupViewPrefab;

	[SerializeField]
	private JournalNavigationGroupElementConsoleView m_NavigationRumorViewPrefab;

	[SerializeField]
	private JournalNavigationGroupElementConsoleView m_NavigationOrderViewPrefab;

	[Header("Paper")]
	[SerializeField]
	private RectTransform m_PaperTransform;

	public override void DrawEntities()
	{
		base.DrawEntities();
		if (base.ViewModel.ActiveTab.CurrentValue == JournalTab.Quests)
		{
			JournalNavigationGroupVM[] datas = (base.ShowCompleted ? base.ViewModel.NavigationGroups.ToArray() : base.ViewModel.NavigationGroups.Where((JournalNavigationGroupVM q) => q.HasActiveQuests).ToArray());
			base.WidgetList.DrawEntries(datas, m_NavigationGroupViewPrefab);
		}
		m_PaperTransform.SetSiblingIndex(0);
		ScrollToRect();
	}

	public void ScrollMenu(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_ScrollRect.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		List<IConsoleEntity> list = new List<IConsoleEntity>();
		if (base.WidgetList.Entries == null)
		{
			return list;
		}
		foreach (MonoBehaviour entry in base.WidgetList.Entries)
		{
			if (entry is JournalNavigationGroupConsoleView journalNavigationGroupConsoleView)
			{
				list.AddRange(journalNavigationGroupConsoleView.GetSelectableEntities());
			}
			else if (entry is JournalNavigationGroupElementConsoleView item)
			{
				list.Add(item);
			}
		}
		return list;
	}
}
