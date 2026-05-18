using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickIconAndNameVM : TooltipBrickVM
{
	public readonly string Text;

	public readonly Sprite Icon;

	public readonly bool Frame;

	public readonly TooltipBaseTemplate Tooltip;

	public BrickIconAndNameVM(string text, Sprite icon, bool frame = true, TooltipBaseTemplate tooltip = null)
	{
		Text = text;
		Icon = icon;
		Frame = frame;
		Tooltip = tooltip;
	}
}
