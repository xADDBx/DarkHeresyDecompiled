using Code.View.UI.MVVM.Tooltip.Bricks.Items;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickTwoColumnsStatVM : TooltipBaseBrickVM
{
	public readonly StatData LeftStat;

	public readonly StatData RightStat;

	public TooltipBrickTwoColumnsStatVM(StatData leftStat, StatData rightStat)
	{
		LeftStat = leftStat;
		RightStat = rightStat;
	}
}
