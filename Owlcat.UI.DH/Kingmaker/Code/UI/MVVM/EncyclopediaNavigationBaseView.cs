using System.Collections.Generic;
using Kingmaker.UI.Common;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaNavigationBaseView : View<EncyclopediaNavigationVM>
{
	[Header("Chapters Group Objects")]
	[SerializeField]
	private WidgetList m_ChaptersWidgetList;

	[SerializeField]
	private EncyclopediaNavigationChapterElementBaseView m_ChaptersNavigationViewPrefab;

	[Header("Pages Group Objects")]
	[SerializeField]
	private WidgetList m_PagesWidgetList;

	[SerializeField]
	private EncyclopediaNavigationElementBaseView m_PagesNavigationViewPrefab;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRectChapters;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRectPages;

	[SerializeField]
	private TextMeshProUGUI m_SubTitleText;

	protected override void OnBind()
	{
		DrawChapters();
		base.ViewModel.SelectedChapter.Subscribe(DrawPages).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_ChaptersWidgetList.Clear();
		m_PagesWidgetList.Clear();
	}

	private void DrawChapters()
	{
		EncyclopediaNavigationElementVM[] datas = base.ViewModel.NavigationChapters.ToArray();
		m_ChaptersWidgetList.DrawEntries(datas, m_ChaptersNavigationViewPrefab).AddTo(this);
	}

	private void DrawPages(EncyclopediaNavigationElementVM groupVm)
	{
		m_PagesWidgetList.Clear();
		if (groupVm != null)
		{
			m_SubTitleText.text = "-/ " + groupVm.Title;
			EncyclopediaNavigationElementVM[] datas = groupVm.GetOrCreateChildsVM().ToArray();
			m_PagesWidgetList.DrawEntries(datas, m_PagesNavigationViewPrefab).AddTo(this);
			DelayedInvoker.InvokeInFrames(delegate
			{
				m_ScrollRectPages.ScrollToTop();
			}, 1).AddTo(this);
		}
	}

	public void ScrollMenu(IConsoleEntity entity, bool isChapter)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			if (isChapter)
			{
				m_ScrollRectChapters.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
			}
			else
			{
				m_ScrollRectPages.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
			}
		}
	}

	public List<IConsoleEntity> GetNavigationEntities(bool isChapter)
	{
		List<IConsoleEntity> list = new List<IConsoleEntity>();
		List<IBindable> list2 = (isChapter ? m_ChaptersWidgetList.Entries : m_PagesWidgetList.Entries);
		if (list2 != null)
		{
			foreach (MonoBehaviour item2 in list2)
			{
				if (item2 is EncyclopediaNavigationElementBaseView item)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}
}
