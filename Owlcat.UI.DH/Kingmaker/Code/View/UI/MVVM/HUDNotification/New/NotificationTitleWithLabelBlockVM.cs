using Kingmaker.Code.View.Bridge.Enums;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class NotificationTitleWithLabelBlockVM : NotificationTitleBlockVM
{
	public string LabelDescription { get; private set; }

	public NotificationTitleWithLabelBlockVM(string title, string labelDescription, QuestNotificationState state)
		: base(title, state)
	{
		LabelDescription = labelDescription;
	}
}
