using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickResourceIconAndNameVM : TooltipBaseBrickVM
{
	public readonly Sprite Icon;

	public readonly string Label;

	public TooltipBrickResourceIconAndNameVM(Sprite icon, string label)
	{
		Icon = icon;
		Label = label;
	}
}
