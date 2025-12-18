using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class NotificationTitleBlockVM : ViewModel
{
	public string Title { get; private set; }

	public QuestNotificationState State { get; private set; }

	public NotificationTitleBlockVM(string title, QuestNotificationState state)
	{
		Title = title;
		State = state;
	}

	public void UpdateState(QuestNotificationState state)
	{
		State = state;
	}
}
