using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickTagDescriptionVM : TooltipBrickVM
{
	public readonly Sprite Icon;

	public readonly Color BgrColor;

	public readonly string TagName;

	public readonly string TagDescription;

	public BrickTagDescriptionVM(Sprite icon, Color bgrColor, string tagName, string tagDescription)
	{
		Icon = icon;
		BgrColor = bgrColor;
		TagName = (string.IsNullOrEmpty(tagName) ? "<color=#FFFFFF00>I</color>" : tagName);
		TagDescription = tagDescription;
	}
}
