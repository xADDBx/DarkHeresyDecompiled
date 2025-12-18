using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickColonyProjectProgressVM : TooltipBaseBrickVM
{
	public readonly Sprite Icon;

	public readonly string Label;

	public TooltipBrickColonyProjectProgressVM(Sprite icon, string label)
	{
		Icon = icon;
		Label = label;
	}
}
