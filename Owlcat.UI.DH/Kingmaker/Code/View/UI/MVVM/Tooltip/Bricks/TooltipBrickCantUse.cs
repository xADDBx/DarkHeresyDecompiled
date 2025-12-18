using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.Tooltip.Bricks;

public class TooltipBrickCantUse : ITooltipBrick
{
	private readonly string m_CantUseLabel;

	public TooltipBrickCantUse(string cantUseLabel)
	{
		m_CantUseLabel = cantUseLabel;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickCantUseVM(m_CantUseLabel);
	}
}
