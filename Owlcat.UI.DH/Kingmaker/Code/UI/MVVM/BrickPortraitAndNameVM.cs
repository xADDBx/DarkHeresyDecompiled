using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickPortraitAndNameVM : TooltipBrickVM
{
	public readonly Sprite Icon;

	public readonly string Line;

	public readonly BrickTitleVM BrickTitle;

	public readonly int Difficulty;

	public readonly bool IsUsedSubtypeIcon;

	public readonly PortraitType PortraitType;

	public BrickPortraitAndNameVM(Sprite icon, string line, BrickTitleVM brickTitle = null, int difficulty = 0, bool isUsedSubtypeIcon = false, PortraitType portraitType = PortraitType.Default)
	{
		Icon = icon;
		Line = line;
		PortraitType = portraitType;
		BrickTitle = brickTitle;
		Difficulty = difficulty;
		IsUsedSubtypeIcon = isUsedSubtypeIcon;
	}
}
