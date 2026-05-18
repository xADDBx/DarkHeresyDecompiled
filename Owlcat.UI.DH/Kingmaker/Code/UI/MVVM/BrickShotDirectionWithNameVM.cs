namespace Kingmaker.Code.UI.MVVM;

public class BrickShotDirectionWithNameVM : TooltipBrickVM
{
	public readonly int ShotNumber;

	public readonly int DeviationMin;

	public readonly int DeviationMax;

	public readonly int DeviationValue;

	public BrickShotDirectionWithNameVM(int shotNumber, int deviationMin, int deviationMax, int deviationValue)
	{
		ShotNumber = shotNumber;
		DeviationMin = deviationMin;
		DeviationMax = deviationMax;
		DeviationValue = deviationValue;
	}
}
