namespace Kingmaker.Code.UI.MVVM;

public class BrickLevelUpStatProgressionVM : TooltipBrickVM
{
	public readonly int PointsSpent;

	public readonly int StatPerPoint;

	public readonly int PointsTotal;

	public BrickLevelUpStatProgressionVM(int pointsSpent, int statPerPoint, int pointsTotal)
	{
		PointsSpent = pointsSpent;
		StatPerPoint = statPerPoint;
		PointsTotal = pointsTotal;
	}
}
