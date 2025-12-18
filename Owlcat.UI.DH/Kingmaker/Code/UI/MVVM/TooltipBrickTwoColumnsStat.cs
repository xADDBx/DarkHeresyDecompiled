using Code.View.UI.MVVM.Tooltip.Bricks.Items;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickTwoColumnsStat : ITooltipBrick
{
	private readonly StatData m_LeftStat;

	private readonly StatData m_RightStat;

	public TooltipBrickTwoColumnsStat(StatData leftStat, StatData rightStat)
	{
		m_LeftStat = leftStat;
		m_RightStat = rightStat;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickTwoColumnsStatVM(m_LeftStat, m_RightStat);
	}
}
