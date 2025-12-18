using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickShotDirectionVM : TooltipBaseBrickVM
{
	public readonly int DeviationMin;

	public readonly int DeviationMax;

	public readonly int DeviationValue;

	public TooltipBrickShotDirectionVM(int deviationMin, int deviationMax, int deviationValue)
	{
		DeviationMin = deviationMin;
		DeviationMax = deviationMax;
		DeviationValue = deviationValue;
	}
}
