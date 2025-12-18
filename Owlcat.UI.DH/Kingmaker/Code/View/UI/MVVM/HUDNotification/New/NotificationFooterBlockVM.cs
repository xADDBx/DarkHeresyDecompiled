using System;
using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class NotificationFooterBlockVM : ViewModel
{
	public readonly string ButtonLabel;

	public readonly Action OnButtonClick;

	public NotificationFooterBlockVM(string buttonLabel, Action onButtonClick)
	{
		ButtonLabel = buttonLabel;
		OnButtonClick = onButtonClick;
	}
}
