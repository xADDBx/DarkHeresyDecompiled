using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickAbilityTargetVM : TooltipBaseBrickVM
{
	public readonly string Label;

	public readonly string Text;

	public readonly Sprite Icon;

	public readonly TooltipBrickElementType Type;

	public readonly TooltipBaseTemplate Tooltip;

	public TooltipBrickAbilityTargetVM(Sprite icon, string label, string text, TooltipBrickElementType type = TooltipBrickElementType.Medium, TooltipBaseTemplate tooltip = null)
	{
		Label = label;
		Text = text;
		Icon = icon;
		Type = type;
		Tooltip = tooltip;
	}
}
