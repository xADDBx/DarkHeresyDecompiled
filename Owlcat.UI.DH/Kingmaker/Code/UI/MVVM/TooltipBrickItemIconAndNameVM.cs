using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickItemIconAndNameVM : TooltipBaseBrickVM
{
	public readonly Sprite Icon;

	public readonly string Label;

	public TooltipBrickItemIconAndNameVM(Sprite icon, string label)
	{
		Icon = icon;
		Label = label;
	}
}
