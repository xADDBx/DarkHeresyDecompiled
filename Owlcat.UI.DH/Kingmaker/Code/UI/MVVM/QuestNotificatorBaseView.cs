using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.GameConst;
using ObservableCollections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class QuestNotificatorBaseView : View<QuestNotificatorVM>
{
	private readonly Queue<Action> m_QuestNotificationsQueue = new Queue<Action>();

	private readonly Queue<Action> m_ObjectiveNotificationsQueue = new Queue<Action>();

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private QuestNotificationQuestView m_QuestView;

	private QuestNotificationQuestVM m_CurrentQuest;

	[SerializeField]
	private QuestNotificationObjectivesView m_ObjectiveView;

	[SerializeField]
	private QuestNotificationAddendumView m_AddendumView;

	private QuestNotificationEntityVM m_CurrentObjective;

	[SerializeField]
	private WindowAnimator m_Animator;

	private bool HasQuestNotifications => m_QuestNotificationsQueue.Any();

	private bool HasObjectiveNotifications => m_ObjectiveNotificationsQueue.Any();

	public void Awake()
	{
		m_Animator.Initialize();
		m_QuestView.Initialize();
		m_ObjectiveView.Initialize();
		m_AddendumView.Initialize();
		Hide();
	}

	protected override void OnBind()
	{
		base.ViewModel.QuestEntities.ObserveAdd().Subscribe(delegate(CollectionAddEvent<QuestNotificationQuestVM> q)
		{
			ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
			{
				AddQuestShowEvent(q.Value);
			});
		}).AddTo(this);
		base.ViewModel.ObjectiveEntities.ObserveAdd().Subscribe(delegate(CollectionAddEvent<QuestNotificationEntityVM> q)
		{
			ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
			{
				AddObjectiveShowEvent(q.Value);
			});
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.ForceCloseCommand, delegate
		{
			CheckJournalButtons();
		}).AddTo(this);
		base.ViewModel.IsShowUp.Subscribe(delegate(bool value)
		{
			if (!value)
			{
				base.gameObject.SetActive(value: false);
			}
			else
			{
				UISounds.Instance.Sounds.Notifications.NewQuest.Play();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.ClearCommand, delegate
		{
			m_QuestNotificationsQueue.Clear();
			m_ObjectiveNotificationsQueue.Clear();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	protected void OpenJournal()
	{
		if (Game.Instance.CurrentModeType != GameModeType.GameOver)
		{
			EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
			{
				h.HandleOpenJournal();
			});
		}
		Hide();
	}

	protected void Close()
	{
		Hide();
	}

	protected virtual void CheckJournalButtons()
	{
	}

	protected bool CheckActiveToJournalButtons()
	{
		bool num = RootUIContext.Instance.FullScreenUIType == FullScreenUIType.Journal;
		bool flag = Game.Instance.CurrentModeType == GameModeType.Cutscene;
		bool flag2 = Game.Instance.CurrentModeType == GameModeType.Dialog;
		if (!num && !flag)
		{
			return !flag2;
		}
		return false;
	}

	private void AddQuestShowEvent(QuestNotificationQuestVM quest)
	{
		m_QuestNotificationsQueue.Enqueue(delegate
		{
			QuestNotificationsAction(quest);
		});
		Tick();
	}

	private void QuestNotificationsAction(QuestNotificationQuestVM quest)
	{
		quest.SetCurrentQuest();
		m_CurrentQuest = quest;
		base.gameObject.SetActive(quest.Quest != null);
		m_Title.gameObject.SetActive(value: true);
		m_Title.text = quest.Title;
		m_QuestView.Bind(quest);
		UISounds.Instance.Sounds.Notifications.NewQuest.Play();
	}

	private void AddObjectiveShowEvent(QuestNotificationEntityVM objective)
	{
		if (!string.IsNullOrWhiteSpace((!objective.IsAddendum) ? objective.Title : objective.Description) && !objective.IsErrandObjective)
		{
			MainThreadDispatcher.StartCoroutine(WaitForNotificationClose(objective));
		}
	}

	private IEnumerator WaitForNotificationClose(QuestNotificationEntityVM objective)
	{
		while (base.ViewModel.IsShowUp.CurrentValue && (m_CurrentObjective != null || m_CurrentQuest.State == QuestNotificationState.New))
		{
			yield return null;
		}
		m_ObjectiveNotificationsQueue.Enqueue(delegate
		{
			ObjectiveNotificationsAction(objective);
		});
		Tick();
	}

	private void ObjectiveNotificationsAction(QuestNotificationEntityVM objective)
	{
		objective.SetCurrentQuest();
		m_CurrentObjective = objective;
		m_Title.gameObject.SetActive(value: true);
		m_Title.text = objective.QuestName;
		m_AddendumView.Bind(objective.IsAddendum ? objective : null);
		m_ObjectiveView.Bind(objective.IsAddendum ? null : objective);
	}

	public void Tick()
	{
		if (!base.ViewModel.ForbiddenQuestNotification && (HasQuestNotifications || HasObjectiveNotifications))
		{
			ShowNextNotification();
		}
		if (HasQuestNotifications || HasObjectiveNotifications)
		{
			MainThreadDispatcher.Post(Tick);
		}
	}

	protected virtual void ShowNextNotification()
	{
		CheckJournalButtons();
		base.ViewModel.SetIsShowUp(value: true);
		m_Animator.AppearAnimation(delegate
		{
			StartCoroutine(HideCurrentNotification());
		});
		if (HasQuestNotifications)
		{
			m_QuestNotificationsQueue.Dequeue()();
		}
		else if (HasObjectiveNotifications)
		{
			m_ObjectiveNotificationsQueue.Dequeue()();
		}
	}

	private IEnumerator HideCurrentNotification()
	{
		yield return new WaitForSecondsRealtime(UIConsts.QuestNotificationTime / (float)((!Game.Instance.Controllers.TurnController.TurnBasedModeActive) ? 1 : 2));
		Hide();
	}

	protected virtual void Hide()
	{
		m_Animator.DisappearAnimation(delegate
		{
			m_Title.gameObject.SetActive(value: false);
			if (m_CurrentQuest != null)
			{
				base.ViewModel.HandleQuestShowed(m_CurrentQuest);
				m_CurrentQuest = null;
			}
			if (m_CurrentObjective != null)
			{
				base.ViewModel.HandleObjectiveShowed(m_CurrentObjective);
				m_CurrentObjective = null;
			}
			if (!HasObjectiveNotifications || !HasQuestNotifications)
			{
				base.ViewModel.SetIsShowUp(value: false);
			}
		});
	}
}
