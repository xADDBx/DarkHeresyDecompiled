using Kingmaker.UnitLogic.Parts;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickNonStack : ITooltipBrick
{
	private readonly UnitPartNonStackBonuses m_Bonus;

	public TooltipBrickNonStack(UnitPartNonStackBonuses bonus)
	{
		m_Bonus = bonus;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickNonStackVm(m_Bonus);
	}
}
