using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class NotificationVM : ViewModel
{
	private readonly ReactiveCommand<Unit> m_ShowNotificationCommand = new ReactiveCommand<Unit>();

	public Observable<Unit> ShowNotificationCommand => m_ShowNotificationCommand;

	protected void ShowNotification()
	{
		m_ShowNotificationCommand.Execute();
	}
}
