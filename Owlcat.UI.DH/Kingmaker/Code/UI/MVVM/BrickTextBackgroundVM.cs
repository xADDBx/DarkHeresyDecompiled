using Kingmaker.Code.View.Bridge.Enums;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickTextBackgroundVM : TooltipBrickVM
{
	public readonly TextEntity Text;

	public readonly TextAnchor TextAnchor;

	public readonly BrickElementPalette Palette;

	public BrickTextBackgroundVM(TextEntity text, TextAnchor textAnchor = TextAnchor.MiddleLeft, BrickElementPalette palette = BrickElementPalette.Normal)
	{
		Text = text;
		TextAnchor = textAnchor;
		Palette = palette;
	}
}
