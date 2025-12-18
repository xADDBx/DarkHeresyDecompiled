using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;
using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class NotificationContentBlockVM : ViewModel
{
	public readonly QuestNotificationState State;

	public string Title { get; protected set; }

	public string Description { get; protected set; }

	public NotificationContentBlockVM(LocalizedString title, LocalizedString description, QuestNotificationState state)
	{
		Title = title.Text;
		Description = description?.Text;
		State = state;
	}

	public NotificationContentBlockVM(string title, string description, QuestNotificationState state)
	{
		Title = title;
		Description = description;
		State = state;
	}
}
