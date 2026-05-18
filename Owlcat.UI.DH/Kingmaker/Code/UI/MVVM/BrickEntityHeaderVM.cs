using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickEntityHeaderVM : TooltipBrickVM
{
	public readonly string MainTitle;

	public readonly Sprite Image;

	public readonly string Title;

	public readonly string LeftLabel;

	public readonly string RightLabel;

	public readonly bool HasUpgrade;

	public BrickEntityHeaderVM(string mainTitle, Sprite image, bool hasUpgrade, string title = null, string leftLabel = null, string rightLabel = null)
	{
		MainTitle = mainTitle;
		Image = image;
		HasUpgrade = hasUpgrade;
		Title = title;
		LeftLabel = leftLabel;
		RightLabel = rightLabel;
	}
}
