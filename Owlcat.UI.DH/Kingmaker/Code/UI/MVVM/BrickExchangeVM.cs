using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickExchangeVM : TooltipBrickVM
{
	public readonly TextValueAddElement Data;

	public readonly string ItemType;

	public readonly Sprite Icon;

	public readonly Color? IconColor;

	public readonly string IconText;

	public readonly BrickElementPalette Type;

	public readonly BrickElementPalette BackgroundType;

	public readonly TooltipBaseTemplate Tooltip;

	public BrickExchangeVM(TextValueAddElement data, string itemType = null, Sprite icon = null, BrickElementPalette type = BrickElementPalette.Normal, BrickElementPalette backgroundType = BrickElementPalette.Normal, TooltipBaseTemplate tooltip = null, Color? iconColor = null, string iconText = null)
	{
		Data = data;
		ItemType = itemType;
		Icon = icon;
		IconColor = iconColor;
		IconText = iconText;
		Type = type;
		BackgroundType = backgroundType;
		Tooltip = tooltip;
	}
}
