using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NetLobbyRegionDropdownVM : DropdownItemVM
{
	public readonly string Region;

	public NetLobbyRegionDropdownVM(string text, string region, Sprite icon = null)
		: base(text, icon)
	{
		Region = region;
	}
}
