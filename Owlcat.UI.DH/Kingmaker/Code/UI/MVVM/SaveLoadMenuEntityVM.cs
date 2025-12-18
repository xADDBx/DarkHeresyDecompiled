using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class SaveLoadMenuEntityVM : SelectionGroupEntityVM
{
	public readonly SaveLoadMode Mode;

	public SaveLoadMenuEntityVM(SaveLoadMode mode)
		: base(allowSwitchOff: false)
	{
		Mode = mode;
	}

	protected override void DoSelectMe()
	{
	}
}
