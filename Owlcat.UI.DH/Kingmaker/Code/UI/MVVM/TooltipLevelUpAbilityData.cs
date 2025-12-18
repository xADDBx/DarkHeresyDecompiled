using System.Collections.Generic;
using Assets.Code.View.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipLevelUpAbilityData
{
	public readonly List<TooltipElementStatValueVM> RequiredStats;

	public readonly TooltipBrickIconPattern BrickIconPattern;

	public TooltipLevelUpAbilityData(List<TooltipElementStatValueVM> requiredStats = null, TooltipBrickIconPattern brickIconPattern = null)
	{
		RequiredStats = requiredStats;
		BrickIconPattern = brickIconPattern;
	}
}
