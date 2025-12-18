using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class QuestNotificationQuestVM : ViewModel
{
	public readonly Quest Quest;

	public readonly QuestNotificationState State;

	public readonly string Title;

	public readonly string Description;

	public QuestNotificationQuestVM(Quest quest, QuestNotificationState state)
	{
		Quest = quest;
		State = state;
		Title = quest.Blueprint.Title;
		Description = ((quest.State == QuestState.Completed && quest.Blueprint.CompletionText.IsSet()) ? quest.Blueprint.CompletionText : quest.Blueprint.Description);
	}

	public void SetCurrentQuest()
	{
		JournalHelper.ChangeCurrentQuest(Quest);
	}
}
