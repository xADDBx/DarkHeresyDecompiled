using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.HUDNotification.New.Utils;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class QuestNotificationVM : HUDNotificationNewVM
{
	public readonly Quest Quest;

	public readonly QuestNotificationState State;

	public readonly NotificationTitleWithLabelBlockVM TitleBlockVM;

	public readonly NotificationDescriptionBlockVM DescriptionBlockVM;

	public readonly NotificationFooterBlockVM FooterBlockVM;

	public override bool ShouldShow
	{
		get
		{
			if (Quest.IsViewed && Quest.State != QuestState.Completed)
			{
				return Quest.State == QuestState.Failed;
			}
			return true;
		}
	}

	public QuestNotificationVM(Quest quest, QuestNotificationState state)
	{
		Quest = quest;
		State = state;
		string description = ((quest.State == QuestState.Completed && quest.Blueprint.CompletionText.IsSet()) ? quest.Blueprint.CompletionText.Text : quest.Blueprint.Description.Text);
		string questNotificationStateText = UIStrings.Instance.QuestNotificationTexts.GetQuestNotificationStateText(state, quest.Blueprint.Type, quest.Blueprint.Group);
		string text = UIStrings.Instance.QuestNotificationTexts.ToJournal.Text;
		TitleBlockVM = new NotificationTitleWithLabelBlockVM(quest.Blueprint.Title.Text, questNotificationStateText, state);
		if (state == QuestNotificationState.New || quest.State == QuestState.Completed)
		{
			DescriptionBlockVM = new NotificationDescriptionBlockVM(description);
		}
		if (CanShowButton())
		{
			FooterBlockVM = new NotificationFooterBlockVM(text, NotificationUtils.OpenJournal);
		}
		base.OnNotificationShown = delegate
		{
			JournalHelper.ChangeCurrentQuest(quest);
		};
	}

	public void UpdateState(QuestNotificationState state)
	{
		TitleBlockVM.UpdateState(state);
	}
}
