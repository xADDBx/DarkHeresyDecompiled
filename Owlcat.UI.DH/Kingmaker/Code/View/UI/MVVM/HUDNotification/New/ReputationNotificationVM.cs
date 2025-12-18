using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class ReputationNotificationVM : HUDNotificationNewVM
{
	public readonly NotificationTitleBlockVM TitleBlockVM;

	public readonly NotificationContentBlockVM ContentBlockVM;

	public override bool ShouldShow => true;

	public ReputationNotificationVM(int amount)
	{
		LocalizedString reputationTitle = UIStrings.Instance.NotificationTexts.ReputationTitle;
		LocalizedString reputationDescription = UIStrings.Instance.NotificationTexts.ReputationDescription;
		string description = string.Format(UIStrings.Instance.NotificationTexts.ReputationChanged.Text, amount);
		TitleBlockVM = new NotificationTitleBlockVM(reputationTitle, QuestNotificationState.New);
		ContentBlockVM = new NotificationContentBlockVM(reputationDescription.Text, description, QuestNotificationState.New);
	}
}
