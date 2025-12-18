using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBricksGroupVM : TooltipBaseBrickVM
{
	public readonly TooltipBricksGroupType Type;

	public readonly bool HasBackground;

	public readonly Color? BackgroundColor;

	public readonly TooltipBricksGroupLayoutParams LayoutParams;

	public readonly string Title;

	public TooltipBricksGroupVM(TooltipBricksGroupType type, bool hasBackground, TooltipBricksGroupLayoutParams layoutParams, Color? backgroundColor, string title)
	{
		Type = type;
		HasBackground = hasBackground;
		LayoutParams = layoutParams;
		BackgroundColor = backgroundColor;
		Title = title;
	}
}
