using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickRate : ITooltipBrick
{
	private readonly TooltipBrickRateVM m_DifficultyVM;

	public TooltipBrickRate(string rateName, int maxRate, int rate)
	{
		m_DifficultyVM = new TooltipBrickRateVM(rateName, maxRate, rate);
	}

	public TooltipBaseBrickVM GetVM()
	{
		return m_DifficultyVM;
	}
}
