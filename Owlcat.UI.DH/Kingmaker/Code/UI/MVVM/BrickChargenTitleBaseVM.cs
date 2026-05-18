using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class BrickChargenTitleBaseVM : TooltipBrickVM
{
	public readonly Sprite Icon;

	public readonly string DisplayName;

	public readonly string Subname;

	protected BrickChargenTitleBaseVM(Sprite icon, string displayName, string subname)
	{
		Icon = icon;
		DisplayName = displayName;
		Subname = subname;
	}
}
