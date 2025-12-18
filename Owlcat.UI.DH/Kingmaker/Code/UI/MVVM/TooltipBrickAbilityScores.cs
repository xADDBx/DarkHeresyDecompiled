using Kingmaker.Utility.UnitDescription;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickAbilityScores : ITooltipBrick
{
	private readonly CharInfoAbilityScoresBlockVM m_AbilityScoresBlock;

	public TooltipBrickAbilityScores(UnitDescription.StatsData statsData)
	{
		m_AbilityScoresBlock = new CharInfoAbilityScoresBlockVM(statsData);
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickAbilityScoresVM(m_AbilityScoresBlock);
	}
}
