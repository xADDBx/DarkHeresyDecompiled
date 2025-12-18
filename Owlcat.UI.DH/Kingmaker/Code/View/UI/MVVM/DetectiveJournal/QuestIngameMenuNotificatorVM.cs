using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Designers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class QuestIngameMenuNotificatorVM : ViewModel, IFullScreenUIHandler, ISubscriber, IQuestObjectiveHandler, IQuestHandler
{
	private readonly ReactiveProperty<bool> m_New = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> IsNew => m_New;

	public ReadOnlyReactiveProperty<bool> IsInCombat => GameUIState.Instance.IsInCombat;

	public QuestIngameMenuNotificatorVM()
	{
		UpdateQuestsMark();
		EventBus.Subscribe(this).AddTo(this);
	}

	private void UpdateQuestsMark()
	{
		m_New.Value = GetFirstNewQuest() != null;
	}

	public void OpenQuestJournalAtFirstNew()
	{
		EventBus.RaiseEvent(delegate(IJournalUIHandler h)
		{
			h.HandleOpenJournal(GetFirstNewQuest());
		});
	}

	private Quest GetFirstNewQuest()
	{
		IEnumerable<Quest> list = GameHelper.Quests.GetList();
		Quest quest = list.FirstOrDefault((Quest q) => !q.IsViewed);
		if (quest != null)
		{
			return quest;
		}
		return list.SelectMany((Quest q) => q.Objectives).FirstOrDefault((QuestObjective o) => o.IsVisible && !o.IsViewed)?.Quest;
	}

	public void HandleQuestObjectiveStarted(QuestObjective objective)
	{
		UpdateQuestsMark();
	}

	public void HandleQuestObjectiveBecameVisible(QuestObjective objective)
	{
		UpdateQuestsMark();
	}

	public void HandleQuestObjectiveCompleted(QuestObjective objective)
	{
		UpdateQuestsMark();
	}

	public void HandleQuestObjectiveFailed(QuestObjective objective)
	{
		UpdateQuestsMark();
	}

	public void HandleQuestStarted(Quest quest)
	{
		UpdateQuestsMark();
	}

	public void HandleQuestCompleted(Quest objective)
	{
		UpdateQuestsMark();
	}

	public void HandleQuestFailed(Quest objective)
	{
		UpdateQuestsMark();
	}

	public void HandleQuestUpdated(Quest objective)
	{
		UpdateQuestsMark();
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		if (!state)
		{
			UpdateQuestsMark();
		}
	}
}
