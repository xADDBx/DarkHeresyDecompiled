using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpStatProgressionVM : TooltipBaseBrickVM
{
	public readonly int PointsSpent;

	public TooltipBrickLevelUpStatProgressionVM(int pointsSpent)
	{
		PointsSpent = pointsSpent;
	}
}
