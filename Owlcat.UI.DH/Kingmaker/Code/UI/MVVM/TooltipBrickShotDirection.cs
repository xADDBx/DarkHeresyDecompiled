using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickShotDirection : ITooltipBrick
{
	private readonly int m_DeviationMin;

	private readonly int m_DeviationMax;

	private readonly int m_DeviationValue;

	public TooltipBrickShotDirection(int deviationMin, int deviationMax, int currentValue)
	{
		m_DeviationMin = deviationMin;
		m_DeviationMax = deviationMax;
		m_DeviationValue = currentValue;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickShotDirectionVM(m_DeviationMin, m_DeviationMax, m_DeviationValue);
	}
}
