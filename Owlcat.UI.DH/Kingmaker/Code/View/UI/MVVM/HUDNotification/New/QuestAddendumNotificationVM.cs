using System.Collections.Generic;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.HUDNotification.New.Utils;
using Kingmaker.Localization;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class QuestAddendumNotificationVM : HUDNotificationNewVM
{
	public readonly QuestObjective Objective;

	public readonly NotificationTitleWithLabelBlockVM TitleBlockVM;

	public readonly List<NotificationContentBlockVM> ContentBlockVMs = new List<NotificationContentBlockVM>();

	public readonly NotificationFooterBlockVM FooterBlockVM;

	public override bool ShouldShow => !Objective.IsViewed;

	public QuestAddendumNotificationVM(QuestObjective objective, QuestNotificationState state)
	{
		Objective = objective;
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
		LocalizedString description = objective.Blueprint.GetDescription();
		ContentBlockVMs.Add(new NotificationContentBlockVM(titile, description, state));
	}
}
