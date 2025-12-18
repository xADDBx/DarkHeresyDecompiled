using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class FactionVendorInformationVM : ViewModel
{
	public readonly string Location;

	public readonly string Name;

	public readonly MechanicEntity Vendor;

	public FactionVendorInformationVM(string location, string name, MechanicEntity vendor)
	{
		Location = location;
		Name = name;
		Vendor = vendor;
	}

	public FactionVendorInformationVM(string location, string name)
	{
		Location = location;
		Name = name;
	}

	public void StartTrade()
	{
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		int num;
		if (loadedAreaState != null)
		{
			_ = loadedAreaState.Settings.CapitalPartyMode;
			if (true)
			{
				num = (Game.Instance.LoadedAreaState.Settings.CapitalPartyMode ? 1 : 0);
				goto IL_0036;
			}
		}
		num = 0;
		goto IL_0036;
		IL_0036:
		bool flag = (byte)num != 0;
		if (!flag || UtilityNet.IsControlMainCharacterWithWarning())
		{
			Game.Instance.GameCommandQueue.StartTrading(Vendor, flag);
		}
	}
}
