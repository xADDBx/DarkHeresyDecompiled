using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityTextIconVM : TooltipBrickVM
{
	public readonly string Title;

	public readonly string Description;

	public readonly Sprite Icon;

	public readonly Color IconColor;

	public readonly TooltipConfig TooltipConfig;

	public BrickAbilityTextIconVM(string title, string description, Sprite icon, Color iconColor)
	{
		Title = title;
		Description = description;
		Icon = icon;
		IconColor = iconColor;
		TooltipConfig = new TooltipConfig(InfoCallPCMethod.LeftMouseButton);
	}
}
