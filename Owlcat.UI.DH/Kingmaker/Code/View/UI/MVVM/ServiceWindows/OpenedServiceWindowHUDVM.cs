using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.ServiceWindows;

public class OpenedServiceWindowHUDVM : ViewModel
{
	public void CloseAll()
	{
		EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
		{
			h.HandleCloseAll();
		});
	}
}
