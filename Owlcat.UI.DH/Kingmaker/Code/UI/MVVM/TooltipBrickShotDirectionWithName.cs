using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickShotDirectionWithName : ITooltipBrick
{
	private readonly int m_ShotNumber;

	private readonly int m_DeviationMin;

	private readonly int m_DeviationMax;

	private readonly int m_DeviationValue;

	public TooltipBrickShotDirectionWithName(int shotNumber, int deviationMin, int deviationMax, int currentValue)
	{
		m_ShotNumber = shotNumber;
		m_DeviationMin = deviationMin;
		m_DeviationMax = deviationMax;
		m_DeviationValue = currentValue;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickShotDirectionWithNameVM(m_ShotNumber, m_DeviationMin, m_DeviationMax, m_DeviationValue);
	}
}
