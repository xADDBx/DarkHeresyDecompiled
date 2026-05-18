using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityDescriptionVM : TooltipBrickVM
{
	public readonly string Text;

	public readonly bool HasIcon;

	public readonly Sprite Icon;

	public readonly Color Color;

	public BrickAbilityDescriptionVM(string text, Sprite icon = null, Color? iconColor = null)
	{
		Text = text;
		Icon = icon;
		HasIcon = Icon != null;
		Color = iconColor ?? Color.white;
	}
}
