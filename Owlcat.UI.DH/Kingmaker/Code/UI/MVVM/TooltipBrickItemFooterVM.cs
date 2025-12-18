using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickItemFooterVM : TooltipBrickDoubleTextVM
{
	public readonly Sprite Icon;

	public readonly string AdditionalLine;

	public TooltipBrickItemFooterVM(string leftLine, string rightLine, Sprite icon, string additionalLine)
		: base(leftLine, rightLine, TextAnchor.MiddleLeft, TextAnchor.MiddleRight)
	{
		Icon = icon;
		AdditionalLine = additionalLine;
	}
}
