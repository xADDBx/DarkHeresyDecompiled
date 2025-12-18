using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickIconAndNameVM : TooltipBaseBrickVM
{
	public readonly string Line;

	public readonly Sprite Icon;

	public readonly TooltipBrickElementType Type;

	public readonly bool Frame;

	public readonly TooltipBaseTemplate Tooltip;

	public TooltipBrickIconAndNameVM(Sprite icon, string line, TooltipBrickElementType type = TooltipBrickElementType.Medium, bool frame = true, TooltipBaseTemplate tooltip = null)
	{
		Line = line;
		Icon = icon;
		Type = type;
		Frame = frame;
		Tooltip = tooltip;
	}
}
