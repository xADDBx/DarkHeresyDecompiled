using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickRateVM : TooltipBaseBrickVM
{
	public readonly string RateName;

	public readonly int MaxRate;

	public readonly int Rate;

	public TooltipBrickRateVM(string rateName, int maxRate, int rate)
	{
		RateName = rateName;
		MaxRate = maxRate;
		Rate = rate;
	}
}
