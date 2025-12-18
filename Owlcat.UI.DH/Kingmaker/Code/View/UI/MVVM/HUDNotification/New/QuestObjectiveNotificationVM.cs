using System.Collections.Generic;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.HUDNotification.New.Utils;
using Kingmaker.Localization;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class QuestObjectiveNotificationVM : HUDNotificationNewVM
{
	public readonly QuestObjective Objective;

	public readonly QuestNotificationState State;

	public readonly NotificationTitleWithLabelBlockVM TitleBlockVM;

	public readonly List<NotificationContentBlockVM> ContentBlockVMs = new List<NotificationContentBlockVM>();

	public readonly NotificationFooterBlockVM FooterBlockVM;

	public override bool ShouldShow
	{
		get
		{
			if (Objective.IsViewed && Objective.State != QuestObjectiveState.Completed)
			{
				return Objective.State == QuestObjectiveState.Failed;
			}
			return true;
		}
	}

	public QuestObjectiveNotificationVM(QuestObjective objective, QuestNotificationState state)
	{
		Objective = objective;
		State = state;
		string text = UIStrings.Instance.QuestNotificationTexts.ToJournal.Text;
		QuestNotificationState state2 = QuestNotificationState.Updated;
		string questNotificationStateText = UIStrings.Instance.QuestNotificationTexts.GetQuestNotificationStateText(state2, objective.Quest.Blueprint.Type, objective.Quest.Blueprint.Group);
		TitleBlockVM = new NotificationTitleWithLabelBlockVM(objective.Quest.Blueprint.Title.Text, questNotificationStateText, state2);
		if (CanShowButton())
		{
			FooterBlockVM = new NotificationFooterBlockVM(text, NotificationUtils.OpenJournal);
		}
		AddObjective(objective, state);
		base.OnNotificationShown = delegate
		{
			JournalHelper.ChangeCurrentQuest(objective.Quest);
		};
	}

	public void AddObjective(QuestObjective objective, QuestNotificationState state)
	{
		LocalizedString titile = objective.Blueprint.GetTitile();
		ContentBlockVMs.Add(new NotificationContentBlockVM(titile, null, state));
	}
}
