namespace Kingmaker.Code.UI.MVVM;

public class BrickShotDirectionVM : TooltipBrickVM
{
	public readonly int DeviationMin;

	public readonly int DeviationMax;

	public readonly int DeviationValue;

	public BrickShotDirectionVM(int deviationMin, int deviationMax, int deviationValue)
	{
		DeviationMin = deviationMin;
		DeviationMax = deviationMax;
		DeviationValue = deviationValue;
	}
}
