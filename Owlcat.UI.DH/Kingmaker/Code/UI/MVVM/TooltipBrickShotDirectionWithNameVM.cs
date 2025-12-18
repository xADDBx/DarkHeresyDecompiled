using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickShotDirectionWithNameVM : TooltipBaseBrickVM
{
	public readonly int ShotNumber;

	public readonly int DeviationMin;

	public readonly int DeviationMax;

	public readonly int DeviationValue;

	public TooltipBrickShotDirectionWithNameVM(int shotNumber, int deviationMin, int deviationMax, int deviationValue)
	{
		ShotNumber = shotNumber;
		DeviationMin = deviationMin;
		DeviationMax = deviationMax;
		DeviationValue = deviationValue;
	}
}
