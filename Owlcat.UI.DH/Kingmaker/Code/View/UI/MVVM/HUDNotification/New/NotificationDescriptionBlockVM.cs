using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class NotificationDescriptionBlockVM : ViewModel
{
	public readonly string Description;

	public NotificationDescriptionBlockVM(string description)
	{
		Description = description;
	}
}
