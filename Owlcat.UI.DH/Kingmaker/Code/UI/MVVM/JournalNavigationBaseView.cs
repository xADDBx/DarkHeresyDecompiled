using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class JournalNavigationBaseView : View<JournalNavigationVM>, ISetCurrentQuestHandler, ISubscriber
{
	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[Header("Navigation Objects")]
	[SerializeField]
	[UsedImplicitly]
	protected WidgetList m_WidgetList;

	[SerializeField]
	private GameObject m_CurrentQuest;

	private bool m_IsInit;

	protected bool ShowCompleted => Game.Instance.Player.UISettings.JournalShowCompletedQuest;

	protected WidgetList WidgetList => m_WidgetList;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		base.ViewModel.SetActiveTab(JournalTab.Quests);
		base.ViewModel.ActiveTab.AsObservable().Subscribe(UpdateActiveTab).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	public void OnShowCompletedToggleChanged(bool value)
	{
		Game.Instance.Player.UISettings.JournalShowCompletedQuest = value;
		DrawEntities();
		switch (JournalHelper.CurrentQuest?.State)
		{
		case null:
		case QuestState.Completed:
		case QuestState.Failed:
			if (!ShowCompleted)
			{
				Quest currentQuest = GetCurrentQuest(null);
				if (!JournalHelper.ChangeCurrentQuest(currentQuest))
				{
					base.ViewModel.SelectQuest(currentQuest);
				}
			}
			break;
		}
	}

	public virtual void DrawEntities()
	{
		m_WidgetList.Clear();
	}

	protected void ScrollToTop()
	{
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			m_ScrollRect.ScrollToTop();
			((RectTransform)m_ScrollRect.content.transform).anchoredPosition = Vector2.zero;
		}).AddTo(this);
	}

	protected void ScrollToRect()
	{
		RectTransform rectTransform = (Game.Instance.IsControllerMouse ? (GetCurrentEntityPC(null)?.transform as RectTransform) : (GetCurrentEntityConsole(null)?.transform as RectTransform));
		if (rectTransform != null && !m_ScrollRect.IsInViewport(rectTransform))
		{
			m_ScrollRect.ScrollToRectCenter(rectTransform, rectTransform);
		}
	}

	private JournalNavigationGroupElementPCView GetCurrentEntityPC(Quest currentQuest)
	{
		List<JournalNavigationGroupElementPCView> list = new List<JournalNavigationGroupElementPCView>();
		if (m_WidgetList.Entries != null)
		{
			foreach (IBindable entry in m_WidgetList.Entries)
			{
				if (entry is JournalNavigationGroupPCView journalNavigationGroupPCView)
				{
					list.AddRange(journalNavigationGroupPCView.WidgetList.Entries.Cast<JournalNavigationGroupElementPCView>());
				}
				else
				{
					list.AddRange(m_WidgetList.Entries.Cast<JournalNavigationGroupElementPCView>());
				}
				if (!list.Any())
				{
					list.AddRange((entry is JournalNavigationGroupPCView journalNavigationGroupPCView2) ? journalNavigationGroupPCView2.WidgetList.Entries.Cast<JournalNavigationGroupElementPCView>() : m_WidgetList.Entries.Cast<JournalNavigationGroupElementPCView>());
				}
			}
		}
		if (currentQuest == null)
		{
			JournalNavigationGroupElementPCView journalNavigationGroupElementPCView = list.FirstOrDefault();
			if (journalNavigationGroupElementPCView != null)
			{
				return journalNavigationGroupElementPCView;
			}
		}
		return list.FirstOrDefault((JournalNavigationGroupElementPCView elementView) => elementView.Quest == currentQuest);
	}

	private JournalNavigationGroupElementConsoleView GetCurrentEntityConsole(Quest currentQuest)
	{
		List<JournalNavigationGroupElementConsoleView> list = new List<JournalNavigationGroupElementConsoleView>();
		if (m_WidgetList.Entries != null)
		{
			foreach (MonoBehaviour entry in m_WidgetList.Entries)
			{
				if (entry is JournalNavigationGroupConsoleView journalNavigationGroupConsoleView)
				{
					list.AddRange(from JournalNavigationGroupElementConsoleView i in journalNavigationGroupConsoleView.WidgetList.Entries
						where i.IsActive
						select i);
				}
				else
				{
					list.AddRange(from JournalNavigationGroupElementConsoleView i in m_WidgetList.Entries
						where i.IsActive
						select i);
				}
				if (!list.Any())
				{
					list.AddRange((entry is JournalNavigationGroupConsoleView journalNavigationGroupConsoleView2) ? journalNavigationGroupConsoleView2.WidgetList.Entries.Cast<JournalNavigationGroupElementConsoleView>() : m_WidgetList.Entries.Cast<JournalNavigationGroupElementConsoleView>());
				}
			}
		}
		if (currentQuest == null)
		{
			JournalNavigationGroupElementConsoleView journalNavigationGroupElementConsoleView = list.FirstOrDefault();
			if (journalNavigationGroupElementConsoleView != null)
			{
				return journalNavigationGroupElementConsoleView;
			}
		}
		return list.FirstOrDefault((JournalNavigationGroupElementConsoleView elementView) => elementView.Quest == currentQuest);
	}

	public Quest GetCurrentQuest(Quest currentQuest)
	{
		if (!Game.Instance.IsControllerMouse)
		{
			return GetCurrentEntityConsole(currentQuest)?.Quest;
		}
		return GetCurrentEntityPC(currentQuest)?.Quest;
	}

	private void UpdateActiveTab(JournalTab activeTab)
	{
		DrawEntities();
		ScrollToTop();
		Quest currentQuest = GetCurrentQuest(JournalHelper.CurrentQuest);
		m_CurrentQuest.SetActive(currentQuest != null);
		base.ViewModel.SelectQuest(currentQuest);
	}

	public void OnPrevActiveTab()
	{
		base.ViewModel.OnPrevActiveTab();
	}

	public void OnNextActiveTab()
	{
		base.ViewModel.OnNextActiveTab();
	}

	void ISetCurrentQuestHandler.HandleSetCurrentQuest(Quest quest)
	{
		m_CurrentQuest.SetActive(quest != null);
	}

	public void HandleElementSelected(IQuestEntity element)
	{
		base.ViewModel.SelectQuest(element.Quest);
	}

	public JournalTab GetActiveTab()
	{
		return base.ViewModel.ActiveTab.CurrentValue;
	}
}
