using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class ServiceWindowsMenuEntityVM : SelectionGroupEntityVM
{
	public ServiceWindowsType ServiceWindowsType;

	public ServiceWindowsMenuEntityVM(ServiceWindowsType type)
		: base(allowSwitchOff: false)
	{
		ServiceWindowsType = type;
	}

	public void SetAvailable(bool available)
	{
		SetAvailableState(available);
	}

	protected override void DoSelectMe()
	{
	}
}
