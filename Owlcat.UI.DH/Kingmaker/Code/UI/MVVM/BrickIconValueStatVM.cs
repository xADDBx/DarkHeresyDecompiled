using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickIconValueStatVM : TooltipBrickVM
{
	public readonly TextValueElement Data;

	public readonly Sprite Icon;

	public readonly IconColor IconColor;

	public readonly TextColor TextColor;

	public BrickIconValueStatVM(TextValueElement data, Sprite icon = null, IconColor iconColor = IconColor.Default, TextColor textColor = TextColor.Default)
	{
		Data = data;
		Icon = icon;
		IconColor = iconColor;
		TextColor = textColor;
	}
}
