using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.GameCommands;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class QuestNotificationEntityVM : ViewModel
{
	public readonly Quest Quest;

	public readonly QuestNotificationState State;

	public readonly string QuestName;

	public readonly string Title;

	public readonly string Description;

	public readonly bool IsAddendum;

	public readonly bool IsErrandObjective;

	private readonly ReactiveProperty<QuestNotificationEntityVM> m_AdditionalObjective = new ReactiveProperty<QuestNotificationEntityVM>();

	public ReadOnlyReactiveProperty<QuestNotificationEntityVM> AdditionalObjective => m_AdditionalObjective;

	public QuestNotificationEntityVM(QuestObjective objective, QuestNotificationState state)
	{
		Quest = objective.Quest;
		State = state;
		QuestName = objective.Quest.Blueprint.Title;
		IsAddendum = objective.Blueprint.IsAddendum;
		IsErrandObjective = objective.Blueprint.IsErrandObjective;
		Description = objective.Blueprint.GetDescription();
		Title = objective.Blueprint.GetTitile();
	}

	protected override void OnDispose()
	{
		AdditionalObjective.CurrentValue?.Dispose();
		m_AdditionalObjective.Value = null;
	}

	public void AddObjective(QuestNotificationEntityVM objectiveVM)
	{
		AdditionalObjective.CurrentValue?.Dispose();
		m_AdditionalObjective.Value = objectiveVM;
	}

	public void SetCurrentQuest()
	{
		GameCommandHelper.SetCurrentQuest(Quest);
	}
}
