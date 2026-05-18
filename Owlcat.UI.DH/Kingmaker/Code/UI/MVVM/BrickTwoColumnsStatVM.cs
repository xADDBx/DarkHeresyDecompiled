namespace Kingmaker.Code.UI.MVVM;

public class BrickTwoColumnsStatVM : TooltipBrickVM
{
	public readonly StatData LeftStat;

	public readonly StatData RightStat;

	public BrickTwoColumnsStatVM(StatData leftStat, StatData rightStat = null)
	{
		LeftStat = leftStat;
		RightStat = rightStat;
	}
}
