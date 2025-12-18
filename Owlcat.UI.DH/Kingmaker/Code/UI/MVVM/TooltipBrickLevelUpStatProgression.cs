using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpStatProgression : ITooltipBrick
{
	private readonly int m_PointsSpent;

	public TooltipBrickLevelUpStatProgression(int pointsSpent)
	{
		m_PointsSpent = pointsSpent;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickLevelUpStatProgressionVM(m_PointsSpent);
	}
}
