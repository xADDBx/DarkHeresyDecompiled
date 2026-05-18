using System.Collections.Generic;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipLevelUpAbilityData
{
	public readonly List<TooltipElementStatValueVM> RequiredStats;

	public readonly BrickIconPatternVM BrickIconPattern;

	public TooltipLevelUpAbilityData(List<TooltipElementStatValueVM> requiredStats = null, BrickIconPatternVM brickIconPattern = null)
	{
		RequiredStats = requiredStats;
		BrickIconPattern = brickIconPattern;
	}
}
