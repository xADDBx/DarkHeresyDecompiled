using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class JournalQuestVM : ViewModel
{
	private readonly ReactiveProperty<bool> m_IsSelected = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsOrderCompleted = new ReactiveProperty<bool>();

	public readonly Quest Quest;

	public readonly List<JournalQuestObjectiveVM> Objectives;

	public readonly string ServiceMessage;

	public readonly string Description;

	public readonly string Title;

	public readonly string CompletionText;

	public readonly string Place;

	public bool IsNew;

	public bool IsCompleted;

	public bool IsUpdated;

	public bool IsPostponed;

	public bool IsFailed;

	private readonly ReactiveCommand<Unit> m_UpdateStatusCommand = new ReactiveCommand<Unit>();

	public readonly bool IsLastChapter;

	public readonly bool IsAffectedByNomos;

	public bool CanCompleteOrder;

	public readonly BlueprintCase RelatedCase;

	private readonly Action<Quest> m_SelectQuestCallback;

	public readonly JournalOrderProfitFactorVM JournalOrderProfitFactorVM;

	private readonly ReactiveCommand<Unit> m_RefreshData = new ReactiveCommand<Unit>();

	public ReadOnlyReactiveProperty<bool> IsSelected => m_IsSelected;

	public ReadOnlyReactiveProperty<bool> IsOrderCompleted => m_IsOrderCompleted;

	public Observable<Unit> UpdateStatusCommand => m_UpdateStatusCommand;

	public bool IsAttention => Objectives.Any((JournalQuestObjectiveVM ob) => ob.IsAttention);

	public bool IsActive
	{
		get
		{
			if (IsFailed || IsCompleted)
			{
				return !IsViewed;
			}
			return true;
		}
	}

	public bool IsRumour => false;

	public bool IsOrder => false;

	public bool IsViewed
	{
		get
		{
			if (Quest.IsViewed)
			{
				return VisibleObjectives.All((QuestObjective x) => x.IsViewed);
			}
			return false;
		}
	}

	public bool QuestIsViewed => Quest.IsViewed;

	public Observable<Unit> RefreshData => m_RefreshData;

	private IEnumerable<QuestObjective> VisibleObjectives => Quest.Objectives.Where((QuestObjective o) => o.IsVisible);

	private IEnumerable<QuestObjective> ActiveObjectives => VisibleObjectives.Where((QuestObjective o) => o.IsVisible && o.IsActive);

	public JournalQuestVM(Quest quest, ReactiveProperty<Quest> selectedQuest = null, Action<Quest> selectQuestCallback = null)
	{
		JournalOrderProfitFactorVM = new JournalOrderProfitFactorVM().AddTo(this);
		Quest = quest;
		Place = Quest.Blueprint.Place;
		Title = Quest.Blueprint.Title;
		Description = Quest.Blueprint.Description;
		CompletionText = ((quest.State == QuestState.Completed) ? ((string)quest.Blueprint.CompletionText) : string.Empty);
		ServiceMessage = Quest.Blueprint.ServiceMessage;
		UpdateStatus(quest);
		IsLastChapter = quest.Blueprint.LastChapter == Game.Instance.Player.Chapter && IsActive;
		IsAffectedByNomos = false;
		ReactiveProperty<bool> isOrderCompleted = m_IsOrderCompleted;
		QuestState state = quest.State;
		isOrderCompleted.Value = state == QuestState.Completed || state == QuestState.Failed;
		m_SelectQuestCallback = selectQuestCallback;
		selectedQuest?.Subscribe(OnSelectedQuestChanged).AddTo(this);
		List<QuestObjective> list = Quest.Objectives.Where((QuestObjective o) => o.IsVisible && o.State != 0 && !o.Blueprint.IsAddendum && !o.Blueprint.IsErrandObjective && !o.Blueprint.IsHidden).ToList();
		list.Sort(Comparison);
		RelatedCase = quest.Blueprint.GetComponent<QuestRelatesToDetectiveCase>()?.Case;
		Objectives = new List<JournalQuestObjectiveVM>();
		if (Objectives == null)
		{
			return;
		}
		foreach (QuestObjective item in list.Where((QuestObjective o) => !o.Blueprint.IsHidden))
		{
			Objectives?.Add(new JournalQuestObjectiveVM(item).AddTo(this));
		}
	}

	private void UpdateStatus(Quest quest, bool forceComplete = false)
	{
		if (forceComplete)
		{
			IsNew = false;
			IsCompleted = true;
			IsUpdated = false;
			IsPostponed = false;
			IsFailed = false;
			return;
		}
		IsNew = quest.State == QuestState.Started;
		IsCompleted = quest.State == QuestState.Completed;
		IsFailed = quest.State == QuestState.Failed;
		IsUpdated = quest.IsViewed && ActiveObjectives.Any((QuestObjective o) => !o.IsViewed) && quest.State != QuestState.Completed && quest.State != QuestState.Failed;
		m_UpdateStatusCommand.Execute(Unit.Default);
		Objectives?.ForEach(delegate(JournalQuestObjectiveVM o)
		{
			o.UpdateState();
		});
	}

	private static int Comparison(QuestObjective o1, QuestObjective o2)
	{
		if (o1.State == o2.State)
		{
			return o2.Order.CompareTo(o1.Order);
		}
		return o1.State.CompareTo(o2.State);
	}

	public void SelectQuest()
	{
		if (!m_IsSelected.Value)
		{
			m_SelectQuestCallback?.Invoke(Quest);
		}
		UpdateStatus(Quest);
	}

	private void OnSelectedQuestChanged(Quest quest)
	{
		m_IsSelected.Value = Quest == quest;
		if (quest != null)
		{
			UpdateStatus(Quest);
		}
	}

	public void CompleteOrder()
	{
		UpdateStatus(Quest, forceComplete: true);
		m_IsOrderCompleted.Value = true;
		EventBus.RaiseEvent(delegate(IUpdateCanCompleteOrderNotificationHandler h)
		{
			h.HandleUpdateCanCompleteOrderNotificationInJournal();
		});
	}
}
