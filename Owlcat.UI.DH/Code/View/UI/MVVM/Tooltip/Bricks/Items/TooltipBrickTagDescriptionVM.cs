using Owlcat.UI;
using UnityEngine;

namespace Code.View.UI.MVVM.Tooltip.Bricks.Items;

public class TooltipBrickTagDescriptionVM : TooltipBaseBrickVM
{
	public readonly Sprite Icon;

	public readonly Color BgrColor;

	public readonly string TagName;

	public readonly string TagDescription;

	public TooltipBrickTagDescriptionVM(Sprite icon, Color bgrColor, string tagName, string tagDescription)
	{
		Icon = icon;
		BgrColor = bgrColor;
		TagName = tagName;
		TagDescription = tagDescription;
	}
}
