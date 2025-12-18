using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickTitleWithIconVM : TooltipBaseBrickVM
{
	public readonly string Name;

	public readonly Sprite Icon;

	public readonly TooltipBaseTemplate Tooltip;

	public TooltipBrickTitleWithIconVM(string name, Sprite icon, TooltipBaseTemplate tooltip)
	{
		Name = name;
		Icon = icon;
		Tooltip = tooltip;
	}
}
