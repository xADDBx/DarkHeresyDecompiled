using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickPFIconAndNameVM : TooltipBaseBrickVM
{
	public readonly Sprite Icon;

	public readonly string Label;

	public TooltipBrickPFIconAndNameVM(Sprite icon, string label)
	{
		Icon = icon;
		Label = label;
	}
}
