using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.Tooltip.Bricks;

public class TooltipBrickCantUseVM : TooltipBaseBrickVM
{
	public readonly string CantUseLabel;

	public TooltipBrickCantUseVM(string cantUseLabel)
	{
		CantUseLabel = cantUseLabel;
	}
}
