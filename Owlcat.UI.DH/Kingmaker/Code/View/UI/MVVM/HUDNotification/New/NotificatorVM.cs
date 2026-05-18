using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class NotificatorVM : ViewModel, IQuestObjectiveHandler, ISubscriber, IQuestHandler, ICaseStatusChanged, IClueStatusChanged, IClueAddendumStatusChanged, IFullScreenUIHandler
{
	private readonly Dictionary<int, Queue<HUDNotificationNewVM>> m_SingleNotifications = new Dictionary<int, Queue<HUDNotificationNewVM>>();

	private readonly List<CaseNotificationVM> m_DetectiveNotifications = new List<CaseNotificationVM>();

	private const int MainPriority = 0;

	private const int SubPriority = 1;

	private static bool ForbiddenQuestNotification
	{
		get
		{
			if (!(Game.Instance.CurrentModeType == GameModeType.Cutscene) && !LoadingProcess.Instance.IsLoadingScreenActive)
			{
				return RootUIContext.Instance.FullScreenUIType == FullScreenUIType.GroupChanger;
			}
			return true;
		}
	}

	public NotificatorVM()
	{
		m_SingleNotifications[0] = new Queue<HUDNotificationNewVM>();
		m_SingleNotifications[1] = new Queue<HUDNotificationNewVM>();
		EventBus.Subscribe(this).AddTo(this);
	}

	public HUDNotificationNewVM TryGetSingleNotification()
	{
		if (ForbiddenQuestNotification)
		{
			return null;
		}
		foreach (var (_, queue2) in m_SingleNotifications)
		{
			if (queue2.Count > 0)
			{
				HUDNotificationNewVM hUDNotificationNewVM = queue2.Dequeue();
				if (hUDNotificationNewVM.ShouldShow)
				{
					return hUDNotificationNewVM;
				}
			}
		}
		return null;
	}

	public bool TryGetDetectiveNotification(out CaseNotificationVM notification)
	{
		notification = null;
		if (ForbiddenQuestNotification)
		{
			return false;
		}
		notification = m_DetectiveNotifications.FirstOrDefault();
		return notification != null;
	}

	public void HandleQuestObjectiveStarted(QuestObjective objective)
	{
		AddObjectiveNotification(objective, QuestNotificationState.New);
	}

	public void HandleQuestObjectiveBecameVisible(QuestObjective objective)
	{
		AddObjectiveNotification(objective, QuestNotificationState.New);
	}

	public void HandleQuestObjectiveCompleted(QuestObjective objective)
	{
		AddObjectiveNotification(objective, QuestNotificationState.Completed);
	}

	public void HandleQuestObjectiveFailed(QuestObjective objective)
	{
		AddObjectiveNotification(objective, QuestNotificationState.Failed);
	}

	private void AddObjectiveNotification(QuestObjective objective, QuestNotificationState state)
	{
		if (objective.IsVisible && !objective.Blueprint.IsSilentQuestNotification(state))
		{
			HUDNotificationNewVM hUDNotificationNewVM = m_SingleNotifications[1].FirstOrDefault((HUDNotificationNewVM n) => n is QuestObjectiveNotificationVM questObjectiveNotificationVM2 && questObjectiveNotificationVM2.Objective.Quest == objective.Quest);
			if (!objective.Blueprint.IsAddendum && hUDNotificationNewVM is QuestObjectiveNotificationVM questObjectiveNotificationVM)
			{
				questObjectiveNotificationVM.AddObjective(objective, state);
				return;
			}
			HUDNotificationNewVM item = (objective.Blueprint.IsAddendum ? ((HUDNotificationNewVM)new QuestAddendumNotificationVM(objective, state)) : ((HUDNotificationNewVM)new QuestObjectiveNotificationVM(objective, state)));
			m_SingleNotifications[1].Enqueue(item);
		}
	}

	public void HandleQuestStarted(Quest quest)
	{
		AddQuestNotification(quest, QuestNotificationState.New);
	}

	public void HandleQuestCompleted(Quest quest)
	{
		AddQuestNotification(quest, QuestNotificationState.Completed);
	}

	public void HandleQuestFailed(Quest quest)
	{
		AddQuestNotification(quest, QuestNotificationState.Failed);
	}

	public void HandleQuestUpdated(Quest quest)
	{
	}

	private void AddQuestNotification(Quest quest, QuestNotificationState state)
	{
		if (!quest.Blueprint.IsSilentQuestNotification(state))
		{
			if (m_SingleNotifications[0].FirstOrDefault((HUDNotificationNewVM n) => n is QuestNotificationVM questNotificationVM2 && questNotificationVM2.Quest == quest) is QuestNotificationVM questNotificationVM)
			{
				questNotificationVM.UpdateState(state);
				return;
			}
			QuestNotificationVM item = new QuestNotificationVM(quest, state);
			m_SingleNotifications[0].Enqueue(item);
		}
	}

	public void HandleCaseStatusChanged(BlueprintCase blueprint)
	{
		switch (Game.Instance.DetectiveSystem.GetCaseStatus(blueprint))
		{
		case CaseStatus.Opened:
			AddCaseNotification(blueprint);
			break;
		case CaseStatus.None:
		case CaseStatus.Closed:
			break;
		}
	}

	public void HandleClueStatusChanged(BlueprintClue blueprint)
	{
		if (!RootVM.Instance.DialogContext.HasDialog && !blueprint.ParentCase.Blueprint.IsClosed() && !HasOverride(blueprint))
		{
			AddClue(blueprint);
		}
	}

	public void HandleClueAddendumStatusChanged(BlueprintClueAddendum blueprint)
	{
		if (RootVM.Instance.DialogContext.HasDialog || blueprint.ParentCase.Blueprint.IsClosed() || !Game.Instance.DetectiveSystem.HasItemExcludingHidden((BlueprintClue?)blueprint.ParentClue))
		{
			return;
		}
		BlueprintClueAddendum overrideFor = GetOverrideFor(blueprint);
		if (overrideFor != null)
		{
			m_DetectiveNotifications.FirstOrDefault((CaseNotificationVM n) => (bool)n.BlueprintCase == (bool)blueprint)?.RemoveAddendum(overrideFor);
		}
		AddAddendum(blueprint);
	}

	private void AddCaseNotification(BlueprintCase blueprint)
	{
		CaseNotificationVM caseNotificationVM = m_DetectiveNotifications.FirstOrDefault((CaseNotificationVM n) => n.BlueprintCase == blueprint);
		if (caseNotificationVM == null)
		{
			caseNotificationVM = new CaseNotificationVM(blueprint, RemoveNotification);
			m_DetectiveNotifications.Add(caseNotificationVM);
		}
		caseNotificationVM.MarkAsNew();
	}

	private void AddClue(BlueprintClue blueprint)
	{
		BlueprintCase parentCase = (blueprint.ParentCase.Blueprint.IsUnknown() ? null : blueprint.ParentCase.Blueprint);
		CaseNotificationVM caseNotificationVM = m_DetectiveNotifications.FirstOrDefault((CaseNotificationVM n) => n.BlueprintCase == parentCase);
		if (caseNotificationVM == null)
		{
			caseNotificationVM = new CaseNotificationVM(parentCase, RemoveNotification);
			m_DetectiveNotifications.Add(caseNotificationVM);
		}
		caseNotificationVM.AddClue(blueprint);
	}

	private void AddAddendum(BlueprintClueAddendum blueprint)
	{
		BlueprintCase parentCase = (blueprint.ParentCase.Blueprint.IsUnknown() ? null : blueprint.ParentCase.Blueprint);
		CaseNotificationVM caseNotificationVM = m_DetectiveNotifications.FirstOrDefault((CaseNotificationVM n) => n.BlueprintCase == parentCase);
		if (caseNotificationVM == null)
		{
			caseNotificationVM = new CaseNotificationVM(parentCase, RemoveNotification);
			m_DetectiveNotifications.Add(caseNotificationVM);
		}
		caseNotificationVM.AddAddendum(blueprint);
	}

	private bool HasOverride(BlueprintClue clue)
	{
		return m_DetectiveNotifications.FirstOrDefault((CaseNotificationVM n) => n.BlueprintCase == clue.ParentCase.Blueprint)?.Clues.FirstOrDefault((NotificationClueBodyVM c) => c.BlueprintClue.HasOverride(clue) || clue.HasOverride(c.BlueprintClue)) != null;
	}

	private BlueprintClueAddendum GetOverrideFor(BlueprintClueAddendum addendum)
	{
		return m_DetectiveNotifications.FirstOrDefault((CaseNotificationVM n) => n.BlueprintCase == addendum.ParentCase.Blueprint)?.Clues.FirstOrDefault((NotificationClueBodyVM c) => c.BlueprintClue == addendum.ParentClue.Blueprint)?.Addendums.FirstOrDefault((BlueprintClueAddendum a) => a.HasOverride(addendum));
	}

	private void RemoveNotification(CaseNotificationVM caseVM)
	{
		m_DetectiveNotifications.Remove(caseVM);
		caseVM?.Dispose();
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		if (state && fullScreenUIType == FullScreenUIType.DetectiveJournal)
		{
			m_DetectiveNotifications.Clear();
		}
	}
}
