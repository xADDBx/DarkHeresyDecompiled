using Kingmaker.Code.View.UI.UIUtilities;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityPatternVM : TooltipBrickVM
{
	public readonly Sprite Icon;

	public readonly string Description;

	public readonly UIUtilityItem.UIPatternData PatternData;

	public BrickAbilityPatternVM(Sprite icon, string description, UIUtilityItem.UIPatternData patternData)
	{
		Icon = icon;
		Description = description;
		PatternData = patternData;
	}
}
