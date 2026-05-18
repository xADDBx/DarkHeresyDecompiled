using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickItemIconAndNameVM : TooltipBrickVM
{
	public readonly Sprite Icon;

	public readonly string Label;

	public BrickItemIconAndNameVM(Sprite icon, string label)
	{
		Icon = icon;
		Label = label;
	}
}
